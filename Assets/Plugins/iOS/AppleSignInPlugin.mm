// AppleLoginPlugin.mm
// Unity callbacks:
//   OnLoginSuccess / OnLoginFailed
//   OnLogOutSuccess / OnLogOutFailed / OnSignOutFallback
//   OnAppleCredentialState

#import <Foundation/Foundation.h>
#import <AuthenticationServices/AuthenticationServices.h>
#import <UIKit/UIKit.h>
#import <CommonCrypto/CommonCrypto.h>

#ifdef __cplusplus
extern "C" {
#endif
void UnitySendMessage(const char* obj, const char* method, const char* msg);
UIViewController* UnityGetGLViewController();
#ifdef __cplusplus
}
#endif

// ===================== Globals =====================
static BOOL gIsLogoutOperation = NO;
static BOOL gLogoutInProgress  = NO;
static ASAuthorizationController *gController = nil;

// ===================== Helpers =====================
static void SendUnity(const char* method, NSString *nsmsg) {
    const char* payload = nsmsg ? [nsmsg UTF8String] : "";
    UnitySendMessage("PlatformLoginReceiver", method, payload ? payload : "");
}

static NSString* sha256Hex(NSString *input) {
    if (!input) return @""; 
    NSData *data = [input dataUsingEncoding:NSUTF8StringEncoding];
    uint8_t digest[CC_SHA256_DIGEST_LENGTH];
    CC_SHA256(data.bytes, (CC_LONG)data.length, digest);
    NSMutableString *output = [NSMutableString stringWithCapacity:CC_SHA256_DIGEST_LENGTH * 2];
    for (int i = 0; i < CC_SHA256_DIGEST_LENGTH; i++) [output appendFormat:@"%02x", digest[i]];
    return output;
}

static ASPresentationAnchor GetSafePresentationAnchor(void) {
    UIViewController *glVC = UnityGetGLViewController();
    if (glVC && glVC.view.window) return glVC.view.window;

    for (UIScene *scene in UIApplication.sharedApplication.connectedScenes) {
        if (scene.activationState == UISceneActivationStateForegroundActive &&
            [scene isKindOfClass:UIWindowScene.class]) {
            UIWindowScene *ws = (UIWindowScene *)scene;
            for (UIWindow *win in ws.windows) if (win.isKeyWindow) return win;
            if (ws.windows.firstObject) return ws.windows.firstObject;
        }
    }
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wdeprecated-declarations"
    if (UIApplication.sharedApplication.keyWindow) return UIApplication.sharedApplication.keyWindow;
#pragma clang diagnostic pop
    if (UIApplication.sharedApplication.windows.firstObject) return UIApplication.sharedApplication.windows.firstObject;
    return nil;
}

// ===================== Delegate =====================
@interface AppleSignInDelegate : NSObject<ASAuthorizationControllerDelegate, ASAuthorizationControllerPresentationContextProviding>
@property (nonatomic, copy) NSString *hashedNonce;
@end

@implementation AppleSignInDelegate
- (ASPresentationAnchor)presentationAnchorForAuthorizationController:(ASAuthorizationController *)controller {
    return GetSafePresentationAnchor() ?: [UIWindow new];
}

static inline id JSONNullIfNil(NSString *s) { return s ? s : [NSNull null]; }

- (void)authorizationController:(ASAuthorizationController *)controller
   didCompleteWithAuthorization:(ASAuthorization *)authorization
{
    gController = nil;

    if (gIsLogoutOperation) {
        SendUnity("OnLogOutSuccess", nil);
        gLogoutInProgress = NO;
        return;
    }

    ASAuthorizationAppleIDCredential *cred = (ASAuthorizationAppleIDCredential *)authorization.credential;

    NSString *idTokenStr =
        cred.identityToken ? [[NSString alloc] initWithData:cred.identityToken encoding:NSUTF8StringEncoding] : @"";
    NSString *authCodeStr =
        cred.authorizationCode ? [[NSString alloc] initWithData:cred.authorizationCode encoding:NSUTF8StringEncoding] : @"";

    NSPersonNameComponents *name = cred.fullName;
    NSDictionary *fullNameDict = @{
        @"givenName":   JSONNullIfNil(name.givenName),
        @"familyName":  JSONNullIfNil(name.familyName),
        @"middleName":  JSONNullIfNil(name.middleName),
        @"namePrefix":  JSONNullIfNil(name.namePrefix),
        @"nameSuffix":  JSONNullIfNil(name.nameSuffix),
        @"nickname":    JSONNullIfNil(name.nickname)
    };

    NSMutableArray<NSString *> *scopes = [NSMutableArray array];
    for (ASAuthorizationScope scope in cred.authorizedScopes) {
        if ([scope isEqualToString:ASAuthorizationScopeFullName]) [scopes addObject:@"full_name"];
        else if ([scope isEqualToString:ASAuthorizationScopeEmail]) [scopes addObject:@"email"];
    }

    NSNumber *realUserStatus = @(cred.realUserStatus);

    NSDictionary *payload = @{
        @"idToken":           idTokenStr ?: @"",
        @"authorizationCode": authCodeStr ?: @"",
        @"userId":            JSONNullIfNil(cred.user),
        @"email":             JSONNullIfNil(cred.email),
        @"state":             JSONNullIfNil(cred.state),
        @"fullName":          fullNameDict,
        @"authorizedScopes":  scopes,
        @"realUserStatus":    realUserStatus
    };

    NSError *err = nil;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:payload options:0 error:&err];
    if (!jsonData || err) {
        SendUnity("OnLoginSuccess", idTokenStr ?: @"");
        return;
    }

    NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    SendUnity("OnLoginSuccess", jsonString);
}

- (void)authorizationController:(ASAuthorizationController *)controller
           didCompleteWithError:(NSError *)error
{
    gController = nil;
    NSString *codeStr = [NSString stringWithFormat:@"%ld", (long)error.code];
    if (gIsLogoutOperation) {
        SendUnity("OnLogOutFailed", codeStr);
        gLogoutInProgress = NO;
    } else {
        SendUnity("OnLoginFailed", codeStr);
    }
}
@end

static AppleSignInDelegate* gDelegate = nil;

// ===================== Internals =====================
static void _ensureDelegate(void) { if (!gDelegate) gDelegate = [AppleSignInDelegate new]; }

static void _performAuthRequests(NSArray<ASAuthorizationRequest*> *requests) {
    gController = [[ASAuthorizationController alloc] initWithAuthorizationRequests:requests];
    gController.delegate = gDelegate;
    gController.presentationContextProvider = gDelegate;
    [gController performRequests];
}

// ===================== C API (Unity) =====================
#ifdef __cplusplus
extern "C" {
#endif

void AppleLogin_StartSignIn(void) {
    @autoreleasepool {
        dispatch_async(dispatch_get_main_queue(), ^{
            _ensureDelegate(); gIsLogoutOperation = NO;
            ASAuthorizationAppleIDProvider* provider = [ASAuthorizationAppleIDProvider new];
            ASAuthorizationAppleIDRequest* request  = [provider createRequest];
            request.requestedScopes = @[ASAuthorizationScopeFullName, ASAuthorizationScopeEmail];
            if (@available(iOS 13.0, *)) request.requestedOperation = ASAuthorizationOperationLogin;
            _performAuthRequests(@[request]);
        });
    }
}

void AppleLogin_StartSignInWithNonce(const char* rawNonce) {
    @autoreleasepool {
        dispatch_async(dispatch_get_main_queue(), ^{
            _ensureDelegate(); gIsLogoutOperation = NO;
            NSString *raw    = rawNonce ? [NSString stringWithUTF8String:rawNonce] : @"";
            NSString *hashed = sha256Hex(raw);
            gDelegate.hashedNonce = hashed;

            ASAuthorizationAppleIDProvider* provider = [ASAuthorizationAppleIDProvider new];
            ASAuthorizationAppleIDRequest* request  = [provider createRequest];
            request.requestedScopes = @[ASAuthorizationScopeFullName, ASAuthorizationScopeEmail];
            if (@available(iOS 13.0, *)) { request.requestedOperation = ASAuthorizationOperationLogin; request.nonce = hashed; }
            _performAuthRequests(@[request]);
        });
    }
}

void AppleLogin_Logout(void) {
    @autoreleasepool {
        dispatch_async(dispatch_get_main_queue(), ^{
            _ensureDelegate();
            if (gLogoutInProgress) { return; }
            gLogoutInProgress = YES;

#if __IPHONE_OS_VERSION_MAX_ALLOWED >= 160000
            if (@available(iOS 16.0, *)) {
                gIsLogoutOperation = YES;
                ASAuthorizationAppleIDProvider *provider = [ASAuthorizationAppleIDProvider new];
                ASAuthorizationAppleIDRequest  *request  = [provider createRequest];
                request.requestedOperation = ASAuthorizationOperationLogout;
                _performAuthRequests(@[request]);
                return;
            }
#endif
            NSURL* url = [NSURL URLWithString:UIApplicationOpenSettingsURLString];
            if ([[UIApplication sharedApplication] canOpenURL:url]) {
                [[UIApplication sharedApplication] openURL:url options:@{} completionHandler:nil];
                SendUnity("OnSignOutFallback", @"Opened Settings");
            } else {
                SendUnity("OnLogOutFailed", @"Cannot open Settings");
            }
            gLogoutInProgress = NO;
        });
    }
}

void AppleLogin_CheckCredentialState(const char* userId) {
    @autoreleasepool {
        _ensureDelegate();
        NSString* uid = [NSString stringWithUTF8String:(userId ? userId : "")];
        ASAuthorizationAppleIDProvider* provider = [ASAuthorizationAppleIDProvider new];
        [provider getCredentialStateForUserID:uid completion:^(ASAuthorizationAppleIDProviderCredentialState state, NSError * _Nullable error) {
            NSInteger code = -1;
            if (!error) {
                switch (state) {
                    case ASAuthorizationAppleIDProviderCredentialAuthorized: code = 0; break;
                    case ASAuthorizationAppleIDProviderCredentialRevoked:    code = 1; break;
                    case ASAuthorizationAppleIDProviderCredentialNotFound:   code = 2; break;
#if __IPHONE_15_0
                    case ASAuthorizationAppleIDProviderCredentialTransferred: code = 3; break;
#endif
                    default: code = -1; break;
                }
            }
            NSString* msg = [NSString stringWithFormat:@"%ld", (long)code];
            SendUnity("OnAppleCredentialState", msg);
        }];
    }
}

void AppleLogin_OpenAppSettings(void) {
    @autoreleasepool {
        NSURL* url = [NSURL URLWithString:UIApplicationOpenSettingsURLString];
        if ([[UIApplication sharedApplication] canOpenURL:url]) {
            [[UIApplication sharedApplication] openURL:url options:@{} completionHandler:nil];
        }
    }
}

#ifdef __cplusplus
}
#endif

#import <Foundation/Foundation.h>
#import <GoogleSignIn/GoogleSignIn.h>
#import <AuthenticationServices/AuthenticationServices.h>
#import "UnityInterface.h"

static GIDConfiguration *gidConfig;

extern "C" {

    void GoogleLogin_Init(const char* clientID) {
        if (clientID == NULL) return;
        NSString* nsClientID = [NSString stringWithUTF8String:clientID];
        gidConfig = [[GIDConfiguration alloc] initWithClientID:nsClientID];
    }

    void GoogleLogin_SignIn() {
        UIViewController* vc = UnityGetGLViewController();

        if (gidConfig == nil || vc == nil) {
            UnitySendMessage("GoogleLoginReceiver", "OnGoogleLoginFailed", "CONFIG_OR_VC_NULL");
            return;
        }

        [GIDSignIn.sharedInstance signInWithConfiguration:gidConfig
                                presentingViewController:vc
                                                callback:^(GIDGoogleUser * _Nullable user, NSError * _Nullable error) {

            if (error != nil || user == nil || user.authentication == nil) {
                NSString* errorMsg = error ? [@(error.code) stringValue] : @"UNKNOWN_ERROR";
                UnitySendMessage("GoogleLoginReceiver", "OnGoogleLoginFailed", [errorMsg UTF8String]);
                return;
            }

            NSString* idToken = user.authentication.idToken;

            if (idToken == nil) {
                UnitySendMessage("GoogleLoginReceiver", "OnGoogleLoginFailed", "NO_ID_TOKEN");
                return;
            }

            const char* tokenStr = [idToken UTF8String];
            if (tokenStr == NULL) {
                UnitySendMessage("GoogleLoginReceiver", "OnGoogleLoginFailed", "ID_TOKEN_ENCODING_FAIL");
                return;
            }

            UnitySendMessage("GoogleLoginReceiver", "OnGoogleLoginSuccess", tokenStr);
        }];
    }

    void GoogleLogin_SignOut() {
        [GIDSignIn.sharedInstance signOut];
    }
}

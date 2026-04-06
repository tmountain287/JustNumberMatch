using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using Firebase.Auth;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class FirebaseUserData
{
    public static bool IsLinked { get; private set; }
    public static string UID { get; private set; }
    public static string Email { get; private set; }
    public static string Name { get; private set; }

    public static void Initialize()
    {
        IsLinked = PlayerPrefs.GetInt("IsLinked", 0) == 1;
        UID = PlayerPrefs.GetString("UserUID", "");
        Email = PlayerPrefs.GetString("UserEmail", "");
        Name = PlayerPrefs.GetString("UserName", "");
    }

    public static void DeleteUserInfo()
    {
        IsLinked = false;
        UID = "";
        Email = "";
        Name = "";
        PlayerPrefs.SetInt("IsLinked", 0);
        SaveUserInfo();
    }

    public static void SaveUserInfo(string _uid, string _email, string _name)
    {
        UID = _uid;
        Email = _email;
        Name = _name;
        SaveUserInfo();
    }

    public static void SaveUserInfo()
    {
        PlayerPrefs.SetString("UserUID", UID);
        PlayerPrefs.SetString("UserEmail", Email);
        PlayerPrefs.SetString("UserName", Name);
        PlayerPrefs.Save();
    }
}

public class FirebaseManager : MonoSingletonDont<FirebaseManager>
{
    public FirebaseAuth Auth { get; private set; }

    public bool CanLink { get => Auth != null && Auth.CurrentUser != null; }
    public bool IsLinking { get => Auth != null && Auth.CurrentUser != null && !Auth.CurrentUser.IsAnonymous; }

    public async UniTask<bool> InitializeAsync(CancellationToken ct = default)
    {
        FirebaseUserData.Initialize();

        // 앱 종료/오브젝트 파괴 중에도 Future가 남아있으면 "Future handle still exists..." 경고가 날 수 있어
        // 외부 토큰 + OnDestroy 토큰을 강하게 연결해서 최대한 취소/정리를 유도한다.
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, this.GetCancellationTokenOnDestroy());
        var linkedCt = linkedCts.Token;

        // 1) Dependency 체크
        DependencyStatus status;
        try
        {
            status = await FirebaseApp.CheckAndFixDependenciesAsync()
                                     .AsUniTask()
                                     .AttachExternalCancellation(linkedCt);
        }
        catch (Exception e)
        {
            Debug.LogError($"Firebase dependency check failed: {e}");
            return false;
        }

        if (status != DependencyStatus.Available)
        {
            Debug.LogError("Firebase Init Fail: " + status);
            return false;
        }

        // 2) Firebase 기본 세팅
        //isInitialized = true;

        Auth = FirebaseAuth.DefaultInstance;
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

        FirebaseFirestoreManager.Instance.Initialize();
        try
        {
            await FirestoreDiag.Instance.InitializeAsync(linkedCt);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Firestore] InitializeAsync failed (ignored): {e.Message}");
        }
        Debug.Log("Firebase Init Complete");

        // 3) Push 등록 (※ 이 Register가 내부에서 싱글톤 생성하면, 종료 시 isQuitting 가드 있는 싱글톤이면 안전)
        await FirebasePushReceiver.Instance.RegisterAsync(linkedCt);

        // 4) 익명 로그인
        if (Auth.CurrentUser == null)
        {
            try
            {
                var signInResult = await Auth.SignInAnonymouslyAsync()
                                             .AsUniTask()
                                             .AttachExternalCancellation(linkedCt);
                Debug.Log($"[Auth] Anonymous SignIn Success uid={signInResult.User.UserId}");
            }
            catch (Exception e)
            {
                Debug.LogError("[Auth] Anonymous SignIn Failed: " + e);
                return false;
            }
        }

        var user = Auth.CurrentUser;
        if (user == null)
        {
            Debug.LogError("[Auth] CurrentUser is null after sign-in");
            return false;
        }

        FirebaseUserData.SaveUserInfo(user.UserId, user.Email, user.DisplayName);
        GameAnalyticsHelper.LogLogin("anonymous");
        GameAnalyticsHelper.SetLoginMethod("anonymous");
        GameAnalyticsHelper.LogAppOpen("normal");

        Debug.Log($"Auto Login - uid: {user.UserId}, name: {user.DisplayName ?? "(anonymous)"}");

        // 5) ID Token 필요 시
        try
        {
            var idToken = await user.TokenAsync(true)
                                    .AsUniTask()
                                    .AttachExternalCancellation(linkedCt);
            Debug.Log("Again ID Token: " + idToken);
        }
        catch
        {
            Debug.LogError("ID Token Refresh Fail");
            // 토큰 실패해도 초기화 자체는 성공 처리할지 정책 선택
        }

        return true;
    }

    //IEnumerator CheckAndFixDependencies()
    //{
    //    while (!isInitialized)
    //    {
    //        if (Application.internetReachability != NetworkReachability.NotReachable && !isInitializing)
    //        {
    //            isInitializing = true;
    //            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(async task =>
    //            {
    //                if (task.Result == DependencyStatus.Available)
    //                {
    //                    isInitialized = true;

    //                    Auth = FirebaseAuth.DefaultInstance;
    //                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
    //                    FirebaseFirestoreManager.Instance.Initialize();
    //                    FirestoreDiag.Instance.Initialize();
    //                    Debug.Log("Firebase Init Complete");
    //                    // 🔥 여기서 등록
    //                    FirebasePushReceiver.Instance.Register();

    //                    if (Auth.CurrentUser == null)
    //                    {
    //                        try
    //                        {
    //                            var signInResult = await Auth.SignInAnonymouslyAsync();
    //                            Debug.Log($"[Auth] Anonymous SignIn Success uid={signInResult.User.UserId}");
    //                        }
    //                        catch (Exception e)
    //                        {
    //                            Debug.LogError("[Auth] Anonymous SignIn Failed: " + e);
    //                            return; // 여기서 더 진행하면 뒤에서 CurrentUser null로 터질 수 있음
    //                        }
    //                    }

    //                    FirebaseUser user = Auth.CurrentUser;

    //                    // DisplayName은 익명일 때 null/빈값일 수 있음
    //                    Debug.Log($"Auto Login - uid: {user.UserId}, name: {user.DisplayName ?? "(anonymous)"}");

    //                    // ✅ 3) ID Token 필요하면 여기서
    //                    try
    //                    {
    //                        var idToken = await user.TokenAsync(true);
    //                        Debug.Log("Again ID Token: " + idToken);
    //                        // 서버에 인증용으로 보낼 수 있음
    //                    }
    //                    catch
    //                    {
    //                        Debug.LogError("ID Token Refresh Fail");
    //                    }
    //                }
    //                else
    //                {
    //                    Debug.LogError("Firebase Init Fail: " + task.Result);
    //                }
    //            });
    //        }
    //        yield return new WaitForSeconds(retryInterval);
    //    }
    //}

    public void GoogleSignInWithCredentialAsync(string _token, Action _onLoginSuccess, Action<string> _onLoginFail)
    {
        Credential credential = GoogleAuthProvider.GetCredential(_token, null);

        _ = SignInWithCredentialAsync(credential, _onLoginSuccess, _onLoginFail);
    }

    /// <summary>Apple 로그인 시 iOS에서 사용한 것과 동일한 rawNonce를 넘겨야 Firebase 검증이 성공합니다.</summary>
    public void AppleSignInWithCredentialAsync(string _token, string _rawNonce, Action _onLoginSuccess, Action<string> _onLoginFail)
    {
        string rawNonce = !string.IsNullOrEmpty(_rawNonce) ? _rawNonce : GenerateRawNonce();
        Credential credential = OAuthProvider.GetCredential("apple.com", _token, rawNonce);
        _ = SignInWithCredentialAsync(credential, _onLoginSuccess, _onLoginFail);
    }

    /// <summary>Apple Sign In with Nonce용. iOS 네이티브에서 사용할 nonce 생성 (공개)</summary>
    public static string GenerateRawNonceForApple(int n = 32)
    {
        return GenerateRawNonce(n);
    }

    private async Task SignInWithCredentialAsync(
    Credential credential,
    Action onLoginSuccess,
    Action<string> onLoginFail)
    {
        // 로컬 함수: AuthResult/ FirebaseUser 둘 다 안전하게 저장
        void SaveUser(FirebaseUser u)
        {
            if (u == null)
            {
                onLoginFail?.Invoke("FirebaseUser is null");
                return;
            }

            FirebaseUserData.SaveUserInfo(u.UserId, u.Email, u.DisplayName);
        }

        try
        {
            if (Auth == null)
            {
                onLoginFail?.Invoke("Auth is null");
                return;
            }

            if (credential == null)
            {
                onLoginFail?.Invoke("Credential is null");
                return;
            }

            var user = Auth.CurrentUser; // ✅ 로컬로 고정

            // =========================
            // 1) 익명 유저면: Link 시도
            // =========================
            if (user != null && user.IsAnonymous)
            {
                Debug.Log("[Auth] Link start (anonymous -> provider)");
                Debug.Log("1");

                // ✅ 타임아웃(예: 15초)
                var linkTask = user.LinkWithCredentialAsync(credential);
                var completed = await Task.WhenAny(linkTask, Task.Delay(15000));

                if (completed != linkTask)
                {
                    onLoginFail?.Invoke("LinkWithCredentialAsync timeout");
                    return;
                }

                // 여기서 fault면 catch로 감
                var linkRes = await linkTask;

                Debug.Log("2");
                Debug.Log($"[Auth] Link success uid={linkRes.User.UserId}");

                SaveUser(linkRes.User);
                GameAnalyticsHelper.LogSignUp("google_or_apple");
                GameAnalyticsHelper.SetLoginMethod(FirebaseManager.Instance.IsLinking ? "google_or_apple" : "anonymous");
                onLoginSuccess?.Invoke();
                return;
            }

            // =========================
            // 2) 익명이 아니면: SignIn
            // =========================
            Debug.Log("[Auth] SignInWithCredential start");

            var signInTask = Auth.SignInWithCredentialAsync(credential);
            var completed2 = await Task.WhenAny(signInTask, Task.Delay(15000));

            if (completed2 != signInTask)
            {
                onLoginFail?.Invoke("SignInWithCredentialAsync timeout");
                return;
            }

            var signInRes = await signInTask;
            Debug.Log($"[Auth] SignIn success uid={signInRes.UserId}");

            SaveUser(signInRes);
            GameAnalyticsHelper.LogLogin("google_or_apple");
            GameAnalyticsHelper.SetLoginMethod("google_or_apple");
            onLoginSuccess?.Invoke();
        }
        // ✅ Link 전용 예외: "This credential is already associated with a different user account."
        catch (FirebaseAccountLinkException ale)
        {
            Debug.Log($"[Auth] FirebaseAccountLinkException: {ale}");

            // 이 케이스는 Link 불가 → 그 구글 계정으로 로그인 전환
            try
            {
                Debug.Log("[Auth] AccountLinkException -> switching to SignInWithCredential");
                var r = await Auth.SignInWithCredentialAsync(credential);

                SaveUser(r);
                GameAnalyticsHelper.LogLogin("google_or_apple");
                GameAnalyticsHelper.SetLoginMethod("google_or_apple");
                onLoginSuccess?.Invoke();
            }
            catch (Exception ex2)
            {
                Debug.Log($"[Auth] Switch sign-in failed: {ex2}");
                onLoginFail?.Invoke($"Credential already in use + switch failed: {ex2.Message}");
            }
        }
        catch (FirebaseException fe)
        {
            var code = (AuthError)fe.ErrorCode;
            Debug.Log($"[Auth] FirebaseException code={code} msg={fe.Message}\n{fe}");

            // ✅ 이미 다른 계정에 연결된 구글이면 "전환 로그인" 시도
            if (code == AuthError.CredentialAlreadyInUse)
            {
                try
                {
                    Debug.Log("[Auth] CredentialAlreadyInUse -> switching to SignInWithCredential");
                    var r = await Auth.SignInWithCredentialAsync(credential);

                    SaveUser(r);
                    GameAnalyticsHelper.LogLogin("google_or_apple");
                    GameAnalyticsHelper.SetLoginMethod("google_or_apple");
                    onLoginSuccess?.Invoke();
                    return;
                }
                catch (Exception ex2)
                {
                    Debug.Log($"[Auth] Switch sign-in failed: {ex2}");
                    onLoginFail?.Invoke($"CredentialAlreadyInUse + switch failed: {ex2.Message}");
                    return;
                }
            }

            onLoginFail?.Invoke(code.ToString());
        }
        catch (Exception ex)
        {
            Debug.Log($"[Auth] Exception: {ex}");
            onLoginFail?.Invoke(ex.Message);
        }
    }


    private static readonly char[] _pool = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();
    private static string GenerateRawNonce(int n = 32)
    {
        var data = new byte[n];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(data);
        var sb = new StringBuilder(n);
        foreach (var b in data) sb.Append(_pool[b % _pool.Length]);
        return sb.ToString();
    }
    private static string Sha256(string s)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    public void LogOut()
    {
        GameAnalyticsHelper.LogLogout();
        FirebaseUserData.DeleteUserInfo();

        if (Auth == null || Auth.CurrentUser == null)
        {
            Debug.Log("로그인된 유저가 없습니다. 로그아웃 생략.");
        }
        else
        {
            Auth.SignOut();
        }
    }   
}

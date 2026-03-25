using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class FirebaseManager : MonoSingletonDont<FirebaseManager>
{
    public FirebaseAuth Auth { get; private set; }

    private bool isInitialized = false;
    private bool isInitializing = false;
    private float retryInterval = 5f;

    private bool init = false;

    public void Initialize()
    {
        if (init) return;

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        StartCoroutine(CheckAndFixDependencies());
#endif
        init = true;
    }

    IEnumerator CheckAndFixDependencies()
    {
        while (!isInitialized)
        {
            if (Application.internetReachability != NetworkReachability.NotReachable && !isInitializing)
            {
                isInitializing = true;
                FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.Result == DependencyStatus.Available)
                    {
                        isInitialized = true;

                        Auth = FirebaseAuth.DefaultInstance;

                        Debug.Log("Firebase Init Complete");
                        // 🔥 여기서 등록
                        FirebasePushReceiver.Instance.Register();

                        if (Auth != null)
                        {
                            FirebaseUser user = Auth.CurrentUser;
                            Debug.Log("Auto Login - User: " + Auth.CurrentUser.DisplayName);

                            user.TokenAsync(true).ContinueWithOnMainThread(tokenTask =>
                            {
                                if (tokenTask.IsCompletedSuccessfully)
                                {
                                    //Token = tokenTask.Result;
                                    //이건 firebase 토큰............
                                    Debug.Log("Again ID Token: " + tokenTask.Result);

                                    // 서버에 인증용으로 보낼 수 있음
                                }
                                else
                                {
                                    Debug.LogError("ID Token Refesh Fail");
                                }
                            });
                        }
                        else
                        {
                            Debug.Log("Need Login.");
                            // StartGoogleLogin(); // 자동 로그인 실패 시 로그인 시작할 수도 있음
                        }
                    }
                    else
                    {
                        Debug.LogError("Firebase Init Fail: " + task.Result);
                    }
                });
            }
            yield return new WaitForSeconds(retryInterval);
        }
    }

    public void GoogleSignInWithCredentialAsync(string _token)
    {
        if (Auth == null || Auth.CurrentUser == null)
        {
            return;
        }

        Credential credential = GoogleAuthProvider.GetCredential(_token, null);

        Auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                FirebaseUser user = task.Result;

                Debug.Log($"Firebase Login Success - UID: {user.UserId}, Name: {user.DisplayName}, EMAIL: {user.Email}");
            }
            else
            {
                Debug.Log("Firebase Login Fail: " + task.Exception);
            }
        });
    }

    public void AppleSignInWithCredentialAsync(string _token)
    {
        if (Auth == null || Auth.CurrentUser == null)
        {
            return;
        }

        var rawNonce = GenerateRawNonce();
        var nonceSha256 = Sha256(rawNonce);

        Credential credential = OAuthProvider.GetCredential("apple.com", _token, rawNonce);

        Auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                FirebaseUser user = task.Result;

                Debug.Log($"Firebase Login Success - UID: {user.UserId}, Name: {user.DisplayName}, EMAIL: {user.Email}");               
            }
            else
            {
                Debug.Log("Firebase Login Fail: " + task.Exception);
            }
        });
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

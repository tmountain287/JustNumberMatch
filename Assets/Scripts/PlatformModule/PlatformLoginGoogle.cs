using Firebase.Auth;
using System;
using UnityEngine;

public abstract class PlatformLoginGoogle : PlatformLoginBase
{    
    protected string webClientId = "788294993689-oqhpc6hpcj6utskqe9k45jk8illfp0i6.apps.googleusercontent.com";
    protected string iosClientId = "788294993689-oqhpc6hpcj6utskqe9k45jk8illfp0i6.apps.googleusercontent.com";

    public override void Initialize()
    {
        
    }

    public override void OnLoginSuccess(string _json)
    {
        try
        {
            var dto = JsonUtility.FromJson<GoogleSignInAccountDTO>(_json);

            // 안전 로그 (토큰은 절대 풀로그 하지 마세요)
            Debug.Log($"Google login OK: email={dto.email}, name={dto.displayName}");

            PlatformLoginReceiver.Instance.Token = dto.idToken;

            //PlatformLoginReceiver.Instance.SaveUserInfo(
            //    dto.idToken,
            //    dto.id ?? string.Empty,
            //    dto.email ?? string.Empty,
            //    dto.familyName
            //);

            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                onLoginSuccess?.Invoke();
            });

            FirebaseManager.Instance.GoogleSignInWithCredentialAsync(dto.idToken, ()=>
            {
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    onLoginSuccess?.Invoke();
                });
            }, (error)=>
            {
                onLoginFail?.Invoke(error);
            });
        }
        catch (Exception ex)
        {
            Debug.Log($"OnLoginSuccess parse failed: {ex}");
            onLoginFail?.Invoke("");
        }
    }

    public override void LogOut(Action _onSuccess = null, Action<string> _onFail = null)
    {
        FirebaseManager.Instance.LogOut();
    }
}

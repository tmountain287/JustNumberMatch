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

            // onLoginSuccess는 Firebase 자격 증명 로그인 성공 후 한 번만 호출한다.
            // (이전에는 Google 토큰 수신 직후 + Firebase 성공 시 두 번 호출되어 UserInfoPopup.AsyncLinkAccount가 중복 실행되고,
            //  실패 시 "계정연동에 실패했습니다" 메시지 박스가 두 번 뜰 수 있음.)

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

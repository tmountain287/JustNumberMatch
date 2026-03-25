#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
public class AppleFullNameDto
{
    public string givenName;
    public string familyName;
    public string middleName;
    public string namePrefix;
    public string nameSuffix;
    public string nickname;
}

[Serializable]
public class AppleSignInDto
{
    public string idToken;
    public string authorizationCode;
    public string userId;
    public string email;          // Hide My Email이면 프록시/혹은 재로그인 시 null
    public string state;
    public AppleFullNameDto fullName;
    public string[] authorizedScopes;
    public int realUserStatus;    // 0 unknown, 1 likelyReal, 2 supported
}

public class PlatformLoginApple : PlatformLoginBase
{
    [DllImport("__Internal")] private static extern void AppleLogin_StartSignIn();
    [DllImport("__Internal")] private static extern void AppleLogin_Logout();
    [DllImport("__Internal")] private static extern void AppleLogin_CheckCredentialState(string userId);
    [DllImport("__Internal")] private static extern void AppleLogin_OpenAppSettings();    
    
    public override void Initialize()
    {
       
    }

    public override bool StartLogin(Action _onSuccess, Action<string> _onFail)
    {
        if (base.StartLogin(_onSuccess, _onFail))
        {
            AppleLogin_StartSignIn();
        }

        return false;
    }

    public override void OnLoginSuccess(string _json)
    {
        try
        {
            var dto = JsonUtility.FromJson<AppleSignInDto>(_json);

            // 토큰은 로그 금지
            Debug.Log($"Apple login OK: userId={dto.userId}, email={(dto.email ?? "null")}");

            // 필요 로직: 서버 검증/저장
            PlatformLoginReceiver.Instance.SaveUserInfo(
                dto.idToken,
                dto.userId ?? string.Empty,
                dto.email ?? string.Empty,
                BuildDisplayName(dto.fullName)
            );

            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                onLoginSuccess?.Invoke();
            });

            FirebaseManager.Instance.AppleSignInWithCredentialAsync(dto.idToken);
        }
        catch (Exception ex)
        {
            Debug.Log($"OnLoginSuccess parse failed: {ex}");
            onLoginFail?.Invoke("");
        }        
    }

    public override void LogOut(Action _onSuccess = null, Action<string> _onFail = null)
    {
        base.LogOut(()=>
        {
            FirebaseManager.Instance.LogOut();
            PlatformLoginReceiver.Instance.DeleteUserInfo();
            _onSuccess?.Invoke();
        }, _onFail);
        AppleLogin_Logout();
    }

    private string BuildDisplayName(AppleFullNameDto n)
    {
        if (n == null) return string.Empty;
        // 필요에 맞게 조합
        if (!string.IsNullOrEmpty(n.familyName) || !string.IsNullOrEmpty(n.givenName))
            return $"{n.familyName}{n.givenName}";
        if (!string.IsNullOrEmpty(n.nickname))
            return n.nickname;
        return string.Empty;
    }
}
#endif
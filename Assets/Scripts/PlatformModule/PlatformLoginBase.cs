using System;
using UnityEngine;

[Serializable]
public class GoogleSignInAccountDTO
{
    public string idToken;
    public string email;
    public string displayName;
    public string givenName;
    public string familyName;
    public string id;              // Google user id (sub)
    public string serverAuthCode;  // 옵션
    public string photoUrl;        // 옵션
}

public abstract class PlatformLoginBase
{
    protected Action onLoginSuccess = null;
    protected Action<string> onLoginFail = null;

    protected Action onLogOutSuccess = null;
    protected Action<string> onLogOutFail = null;

    public virtual void Initialize() { }
    
    public virtual bool StartLogin(Action _onSuccess, Action<string> _onFail)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            _onFail("온라인 상태에서만\n계정 연동이 가능합니다.\n네트워크 연결 후\n다시 진행해 주세요.");
            return false;
        }
//#if !UNITY_EDITOR
//        if (FirebaseManager.Instance.Auth == null)
//        {
//            _onFail("초기화 실패로 지금은 계정연동을\n할수 없습니다..\n문제가 계속 되면 고객센터에\n문의해주세요.");
//            return false;
//        }
//#endif
        onLoginSuccess = () => _onSuccess.Invoke();
        onLoginFail = (error) => _onFail.Invoke(error);
        return true;
    }

    public virtual void OnLoginSuccess(string _json)
    {
       
    }

    public virtual void OnLoginFailed(string _error)
    {
        Debug.Log("Login Fail Code: " + _error);

        UnityMainThreadDispatcher.Instance.Enqueue((error) =>
        {
            onLoginFail?.Invoke(error);
        }, _error);
    }

    public virtual void OnLogOutSuccess()
    {
        Debug.Log("Logout Success");
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            onLogOutSuccess?.Invoke();
        });
    }

    public virtual void OnLogOutFailed(string _error)
    {
        Debug.Log("Logout Fail Code: " + _error);

        UnityMainThreadDispatcher.Instance.Enqueue((error) =>
        {
            onLogOutFail?.Invoke(error);
        }, _error);
    }

    public virtual void LogOut(Action _onSuccess = null, Action<string> _onFail = null)
    {
        onLogOutSuccess = () => _onSuccess?.Invoke();
        onLogOutFail = (error) => _onFail?.Invoke(error);
    }
}

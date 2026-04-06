using System;
using UnityEngine;
public class PlatformLoginReceiver : MonoSingletonDont<PlatformLoginReceiver>
{
#if UNITY_ANDROID && !UNITY_EDITOR
    private PlatformLoginBase platformLogin = new PlatformLoginGoogleAOS();
#elif UNITY_IOS && !UNITY_EDITOR
    private PlatformLoginBase platformLogin = new PlatformLoginApple();
#else
    private PlatformLoginBase platformLogin = new PlatformLoginGoogleStandard();
#endif

    public string Token { get; set; }

    public bool IsLinking { get => !string.IsNullOrEmpty(PlayerPrefs.GetString("UserUID", "")); }

    public void Initialize()
    {
        platformLogin.Initialize();
    }

    public void StartLogin(Action _onSuccess, Action<string> _onFail)
    {
        platformLogin.StartLogin(_onSuccess, _onFail);
    }

    public void OnLoginSuccess(string _token)
    {
        Debug.Log(_token);
        platformLogin.OnLoginSuccess(_token);
    }

    public void OnLoginFailed(string _error)
    {
        Debug.Log(_error);
        platformLogin.OnLoginFailed(_error);
    }

    public void OnLogOutSuccess()
    {
        Debug.Log("OnLogOutSuccess");
        platformLogin.OnLogOutSuccess();
    }

    public void OnLogOutFailed(string _error)
    {
        Debug.Log(_error);
        platformLogin.OnLogOutFailed(_error);
    }

    public void OnSignOutFallback(string _error)
    {
        Debug.Log("OnSignOutFallback");
        Debug.Log(_error);
    }

    public void LogOut(Action _onSuccess = null, Action<string> _onFail = null)
    {
        platformLogin.LogOut(()=> _onSuccess?.Invoke(), (error)=>_onFail?.Invoke(error));
    }

    public void Log(string _message)
    {

    }
}
#if UNITY_ANDROID
using System;
using UnityEngine;

public class PlatformLoginGoogleAOS : PlatformLoginGoogle
{
    public override void Initialize()
    {
        base.Initialize();

        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass plugin = new AndroidJavaClass("com.yourcompany.googlelogin.GoogleLoginPlugin");

            // ✅ activity, webClientId 전달
            plugin.CallStatic("init", activity, webClientId);
        }
    }

    public override bool StartLogin(Action _onSuccess, Action<string> _onFail)
    {
        if (base.StartLogin(_onSuccess, _onFail))
        {
            AndroidJavaClass plugin = new AndroidJavaClass("com.yourcompany.googlelogin.GoogleLoginPlugin");
            plugin.CallStatic("signIn");
        }

        return false;
    }

    public override void LogOut(Action _onSuccess = null, Action<string> _onFail = null)
    {
        base.LogOut(_onSuccess, _onFail);
        using (var plugin = new AndroidJavaClass("com.yourcompany.googlelogin.GoogleLoginPlugin"))
        {
            plugin.CallStatic("signOut");
        }
    }
}
#endif

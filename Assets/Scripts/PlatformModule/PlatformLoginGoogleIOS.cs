#if UNITY_IOS
using System;
using System.Runtime.InteropServices;

public class PlatformLoginGoogleIOS : PlatformLoginGoogle
{
    [DllImport("__Internal")]
    private static extern void GoogleLogin_Init(string clientID);

    [DllImport("__Internal")]
    private static extern void GoogleLogin_SignIn();

    [DllImport("__Internal")]
    private static extern void GoogleLogin_SignOut();

    public override void Initialize()
    {
        base.Initialize();

        GoogleLogin_Init(iosClientId);
    }

    public override bool StartLogin(Action _onSuccess, Action<string> _onFail)
    {
        if (base.StartLogin(_onSuccess, _onFail))
        {
            GoogleLogin_SignIn();
        }

        return false;
    }

    public override void LogOut(Action _onSuccess = null, Action<string> _onFail = null)
    {        
        GoogleLogin_SignOut();
    }
}
#endif
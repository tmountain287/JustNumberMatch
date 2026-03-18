#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Threading.Tasks;

public class PlatformLoginGoogleStandard : PlatformLoginGoogle
{
    private string clientSecret = "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";
    private string redirectUri = "http://localhost:7000";

    public override bool StartLogin(Action _onSuccess, Action<string> _onFail)
    {
        if (base.StartLogin(_onSuccess, _onFail))
        {
            _ = StartGoogleAuth();
        }

        return false;
    }

    private async Task StartGoogleAuth()
    {
        var authHelper = new GoogleAuthHelper();
        var result = await authHelper.GetGoogleUserAsync(
            clientId: webClientId,
            clientSecret: clientSecret,
            redirectUri: redirectUri
        );
        
        Debug.Log(result.idToken);

        if (!string.IsNullOrEmpty(result.idToken))
        {
            var dto = new GoogleSignInAccountDTO
            {
                idToken = result.idToken,
                id = result.userId,
                email = result.email,
                displayName = result.name,
                photoUrl = result.picture,       // 없다면 null 그대로
            };

            Debug.Log(result.idToken);

            PlatformLoginReceiver.Instance.Token = result.idToken;

            FirebaseManager.Instance.GoogleSignInWithCredentialAsync(result.idToken, () =>
            {
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    onLoginSuccess?.Invoke();
                });
            }, (error) =>
            {
                onLoginFail?.Invoke(error);
            });
        }
        else
        {
            UnityMainThreadDispatcher.Instance.Enqueue((error) =>
            {
                onLoginFail?.Invoke(error);
            }, "fail");
        }
    }
}
#endif

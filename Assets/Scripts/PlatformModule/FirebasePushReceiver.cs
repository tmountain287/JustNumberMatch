using Firebase;
using Firebase.Messaging;
using UnityEngine;

public class FirebasePushReceiver : MonoSingletonDont<FirebasePushReceiver>
{
    public string PushToken { get; private set; } = string.Empty;
#if UNITY_IOS
    public async void Register()
    {        await FirebaseMessaging.RequestPermissionAsync();
#else
    public void Register()
    { 
#endif
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;
        Debug.Log("FirebasePushReceiver: FCM 이벤트 등록 완료");
    }

    public void Unregister()
    {
        FirebaseMessaging.TokenReceived -= OnTokenReceived;
        FirebaseMessaging.MessageReceived -= OnMessageReceived;
    }

    private void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        PushToken = token.Token;
        Debug.Log($"[FCM] Token received: {PushToken}");
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        string messageId = e?.Message?.MessageId ?? "";
        string campaign = e?.Message?.Data?.ContainsKey("campaign") == true ? e.Message.Data["campaign"] : null;
        GameAnalyticsHelper.LogPushReceived(messageId, campaign);
        Debug.Log($"[FCM] Push received from: {e.Message.From}");
        if (e.Message.Notification != null)
        {
            Debug.Log($"[FCM] Title: {e.Message.Notification.Title}");
            Debug.Log($"[FCM] Body: {e.Message.Notification.Body}");
        }
    }
}
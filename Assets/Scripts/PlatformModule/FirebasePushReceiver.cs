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
        Debug.Log($"[FCM] Push received from: {e.Message.From}");
        if (e.Message.Notification != null)
        {
            Debug.Log($"[FCM] Title: {e.Message.Notification.Title}");
            Debug.Log($"[FCM] Body: {e.Message.Notification.Body}");
        }
    }
}
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Extensions;
using Firebase.Messaging;
using System.Threading;
using UnityEngine;

public class FirebasePushReceiver : MonoSingletonDont<FirebasePushReceiver>
{
    public string PushToken { get; private set; } = string.Empty;

    public async UniTask RegisterAsync(CancellationToken ct = default)
    {
#if UNITY_IOS
        // iOS permission 요청도 Future 기반이라, 종료/파괴 시 취소되도록 await + cancellation 적용
        await FirebaseMessaging.RequestPermissionAsync()
            .AsUniTask()
            .AttachExternalCancellation(ct);
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

    protected override void OnDestroy()
    {
        Unregister();
        base.OnDestroy();
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
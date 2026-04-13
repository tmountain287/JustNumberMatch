using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common.Manager;

#if UNITY_ANDROID
using Unity.Notifications.Android;
using UnityEngine.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif

public enum LocalPushType
{
    DailyComeback,   // 오늘 미접속 시 오후 1시 재접속 유도
    EnergyFull,      // 에너지 가득 참
    BuildComplete    // 어떤 제작/건설 완료
}

public class LocalPushManager : MonoSingletonDont<LocalPushManager>
{
    [Header("Android 채널 설정")]
    [SerializeField] private string androidChannelId = "local_channel";
    [SerializeField] private string androidChannelName = "Local Notifications";
    [SerializeField] private string androidChannelDescription = "Local push notifications";

    // DailyComeback은 날짜별 고정 ID(Android) / 날짜 문자열(iOS)로 예약·취소 — 앱 재시작 후에도 오늘 예약 취소 가능
#if UNITY_ANDROID
    private readonly Dictionary<LocalPushType, int> androidIds = new();
#elif UNITY_IOS
    private readonly Dictionary<LocalPushType, string> iosIds = new();
#endif

    [Header("재접속 푸시 - 매일 지정 시각, 접속한 날만 제외")]
    [SerializeField] private int dailyComebackHour = 13;
    [SerializeField] private int dailyComebackMinute = 0;
    [SerializeField] private int dailyComebackScheduleDays = 30; // 내일부터 N일치 매일 예약 (iOS 64개 제한 고려)

    public void Initialize()
    {
#if UNITY_ANDROID
        CreateAndroidChannel();
        RequestAndroidNotificationPermission();
#elif UNITY_IOS
        // 첫 실행 시 네트워크 없어도 알림 권한 팝업이 뜨도록 Firebase와 무관하게 여기서 요청
        StartCoroutine(RequestIOSNotificationPermission());
#endif
        RefreshDailyComebackPush();
        PlayerPrefsManager.Instance.AddChangeEvent(PrefsKey.PUSH, OnPushSettingChanged);
        PlayerPrefsManager.Instance.AddChangeEvent(PrefsKey.Language, OnLanguageChanged);
    }

#if UNITY_ANDROID
    /// <summary>Android 13(API 33) 이상에서 알림 권한 런타임 요청. 첫 설치 후 첫 실행 시 시스템 팝업이 뜨도록 함.</summary>
    private void RequestAndroidNotificationPermission()
    {
        const string postNotifications = "android.permission.POST_NOTIFICATIONS";
        if (!Permission.HasUserAuthorizedPermission(postNotifications))
            Permission.RequestUserPermission(postNotifications);
    }
#elif UNITY_IOS
    /// <summary>iOS 알림 권한 요청. 네트워크/Firebase 초기화와 무관하게 앱 시작 직후 팝업이 뜨도록 함.</summary>
    private IEnumerator RequestIOSNotificationPermission()
    {
        var authOption = AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound;
        using (var req = new AuthorizationRequest(authOption, true))
        {
            while (!req.IsFinished)
                yield return null;
            if (Debug.isDebugBuild)
                Debug.Log($"[LocalPush] iOS Notification Authorization finished: granted={req.Granted}, error={req.Error}");
        }
    }
#endif

    private void OnPushSettingChanged()
    {
        RefreshDailyComebackPush();
    }

    private void OnLanguageChanged()
    {
        if (IsPushEnabled())
            RefreshDailyComebackPush();
    }

    private void OnDestroy()
    {
        if (PlayerPrefsManager.Instance != null)
        {
            PlayerPrefsManager.Instance.RemoveChangeEvent(PrefsKey.PUSH, OnPushSettingChanged);
            PlayerPrefsManager.Instance.RemoveChangeEvent(PrefsKey.Language, OnLanguageChanged);
        }
    }

    /// <summary>
    /// 앱 실행 시 호출: 오늘 날짜 재접속 로컬 푸시 예약을 취소한 뒤, 내일부터 매일 지정 시각 N일치만 다시 예약.
    /// </summary>
    public void RefreshDailyComebackPush()
    {
        if (!IsPushEnabled())
        {
            Cancel(LocalPushType.DailyComeback);
            return;
        }

        CancelDailyComebackScheduledInWindow();
        ScheduleDailyComebackEveryDay();
    }

    private bool IsPushEnabled()
    {
        return PlayerPrefsManager.Instance.GetPlayerPrefsValue(PrefsKey.PUSH, 1) > 0;
    }

    /// <summary>
    /// 내일부터 매일 지정 시각(기본 13:00)에 알림 예약. 오늘은 <see cref="CancelDailyComebackScheduledInWindow"/>에서 취소만 함.
    /// </summary>
    private void ScheduleDailyComebackEveryDay()
    {
        int days = Mathf.Clamp(dailyComebackScheduleDays, 1, 63); // iOS 64개 제한
        var (title, body) = GetLocalizedMessage(LocalPushType.DailyComeback);

        for (int i = 1; i <= days; i++)
        {
            DateTime targetDate = DateTime.Now.Date.AddDays(i);
            DateTime targetTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, dailyComebackHour, dailyComebackMinute, 0);

#if UNITY_ANDROID
            var notification = new AndroidNotification
            {
                Title = title,
                Text = body,
                FireTime = targetTime
            };

            AndroidNotificationCenter.SendNotificationWithExplicitID(notification, androidChannelId, GetDailyComebackAndroidIdForDate(targetDate));

#elif UNITY_IOS
            TimeSpan interval = targetTime - DateTime.Now;
            if (interval.TotalSeconds < 1)
                interval = TimeSpan.FromSeconds(1);

            var trigger = new iOSNotificationTimeIntervalTrigger
            {
                TimeInterval = interval,
                Repeats = false
            };

            string identifier = GetDailyComebackIosIdentifierForDate(targetDate);

            var notification = new iOSNotification
            {
                Identifier = identifier,
                Title = title,
                Body = body,
                ShowInForeground = false,
                Trigger = trigger
            };

            iOSNotificationCenter.ScheduleNotification(notification);
#endif
        }
    }

    /// <summary>날짜당 고정 Android 알림 ID — 재시작 후에도 동일 날짜 슬롯 취소 가능.</summary>
    private static int GetDailyComebackAndroidIdForDate(DateTime date)
    {
        var d = date.Date;
        return 900_000_000 + d.Year * 10_000 + d.Month * 100 + d.Day;
    }

#if UNITY_IOS
    private static string GetDailyComebackIosIdentifierForDate(DateTime date)
    {
        var d = date.Date;
        return $"local_push_DailyComeback_{d:yyyy_MM_dd}";
    }
#endif

    /// <summary>
    /// 오늘 0일차부터 N일차까지(재예약 대상 구간 + 오늘) 예약된 DailyComeback을 모두 취소. 오늘 앱을 켠 경우 오늘 오후 푸시가 남지 않도록 함.
    /// </summary>
    private void CancelDailyComebackScheduledInWindow()
    {
#if UNITY_ANDROID || UNITY_IOS
        int days = Mathf.Clamp(dailyComebackScheduleDays, 1, 63);
        DateTime today = DateTime.Now.Date;
#if UNITY_ANDROID
        for (int i = 0; i <= days; i++)
        {
            int id = GetDailyComebackAndroidIdForDate(today.AddDays(i));
            AndroidNotificationCenter.CancelScheduledNotification(id);
            AndroidNotificationCenter.CancelNotification(id);
        }
#elif UNITY_IOS
        for (int i = 0; i <= days; i++)
        {
            string sid = GetDailyComebackIosIdentifierForDate(today.AddDays(i));
            iOSNotificationCenter.RemoveScheduledNotification(sid);
            iOSNotificationCenter.RemoveDeliveredNotification(sid);
        }
        for (int i = 1; i <= 63; i++)
        {
            string legacy = $"{GetIosIdentifier(LocalPushType.DailyComeback)}_{i}";
            iOSNotificationCenter.RemoveScheduledNotification(legacy);
            iOSNotificationCenter.RemoveDeliveredNotification(legacy);
        }
#endif
#endif
    }

#if UNITY_ANDROID
    /// <summary>AndroidManifest의 default_notification_channel_id와 동일해야 FCM 원격 푸시가 표시됨 (API 26+)</summary>
    private const string FcmDefaultChannelId = "default_channel_id";

    private void CreateAndroidChannel()
    {
        var channel = new AndroidNotificationChannel
        {
            Id = androidChannelId,
            Name = androidChannelName,
            Description = androidChannelDescription,
            Importance = Importance.Default
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);

        // FCM 원격 푸시용 채널: 매니페스트 meta-data와 id 일치 필요 (없으면 Android 8+에서 푸시 미표시)
        var fcmChannel = new AndroidNotificationChannel
        {
            Id = FcmDefaultChannelId,
            Name = "알림",
            Description = "원격 알림",
            Importance = Importance.High
        };
        AndroidNotificationCenter.RegisterNotificationChannel(fcmChannel);
    }
#endif

    // ──────────────────────────────────────
    // 언어 / 메시지 (LocalizationPushTable 사용)
    // ──────────────────────────────────────

    /// <summary>
    /// LocalizationPushTable에서 푸시 메시지 조회
    /// </summary>
    private (string title, string body) GetLocalizedMessage(LocalPushType type)
    {
        switch (type)
        {
            case LocalPushType.DailyComeback:
                string title = LocalizationManager.Instance.GetText("DailyComebackTitle", LocalUIType.Push);
                string body = LocalizationManager.Instance.GetText("DailyComebackBody", LocalUIType.Push);
                if (string.IsNullOrEmpty(title)) title = "The fun you missed is waiting!";
                if (string.IsNullOrEmpty(body)) body = "One more matchstick puzzle?";
                return (title, body);

            case LocalPushType.EnergyFull:
                return ("Your energy is full!", "Jump back in and keep playing!");

            case LocalPushType.BuildComplete:
                return ("Build complete!", "Open the game to see the result.");
        }

        return ("Notification", "You have a new message.");
    }

    // ──────────────────────────────────────
    // 예약 / 취소 공통 로직
    // ──────────────────────────────────────

    /// <summary>
    /// 같은 타입으로 다시 예약하면 이전 예약은 취소하고 새로 걸어줌.
    /// 오늘/내일 특정 시각(hour:minute)에 1회 알림.
    /// </summary>
    public void ScheduleLocalPushAt(LocalPushType type, int hour, int minute)
    {
        DateTime now = DateTime.Now;

        DateTime targetTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
        if (targetTime <= now)
            targetTime = targetTime.AddDays(1);

        var (title, body) = GetLocalizedMessage(type);

        Cancel(type);   // ← 같은 타입 예약 있으면 먼저 취소

#if UNITY_ANDROID
        var notification = new AndroidNotification
        {
            Title = title,
            Text = body,
            FireTime = targetTime
        };

        int id = AndroidNotificationCenter.SendNotification(notification, androidChannelId);
        androidIds[type] = id;

#elif UNITY_IOS
        TimeSpan interval = targetTime - now;
        if (interval.TotalSeconds < 1)
            interval = TimeSpan.FromSeconds(1);

        var trigger = new iOSNotificationTimeIntervalTrigger
        {
            TimeInterval = interval,
            Repeats = false
        };

        string identifier = GetIosIdentifier(type);

        var notification = new iOSNotification
        {
            Identifier = identifier,
            Title = title,
            Body = body,
            ShowInForeground = false,
            Trigger = trigger
        };

        iOSNotificationCenter.ScheduleNotification(notification);
        iosIds[type] = identifier;
#endif
    }

    /// <summary>
    /// 지금 기준 N초 뒤에 1회 알림.
    /// 같은 타입으로 여러 번 호출해도 항상 하나만 유지.
    /// </summary>
    public void ScheduleLocalPushAfterSeconds(LocalPushType type, int seconds)
    {
        if (seconds < 1) seconds = 1;

        DateTime now = DateTime.Now;
        DateTime targetTime = now.AddSeconds(seconds);

        var (title, body) = GetLocalizedMessage(type);

        Cancel(type);   // ← 먼저 취소

#if UNITY_ANDROID
        var notification = new AndroidNotification
        {
            Title = title,
            Text = body,
            FireTime = targetTime
        };

        int id = AndroidNotificationCenter.SendNotification(notification, androidChannelId);
        androidIds[type] = id;

#elif UNITY_IOS
        var trigger = new iOSNotificationTimeIntervalTrigger
        {
            TimeInterval = TimeSpan.FromSeconds(seconds),
            Repeats = false
        };

        string identifier = GetIosIdentifier(type);

        var notification = new iOSNotification
        {
            Identifier = identifier,
            Title = title,
            Body = body,
            Trigger = trigger
        };

        iOSNotificationCenter.ScheduleNotification(notification);
        iosIds[type] = identifier;
#endif
    }

    /// <summary>
    /// 해당 타입의 예약/표시된 알림을 취소
    /// </summary>
    public void Cancel(LocalPushType type)
    {
#if UNITY_ANDROID
        if (type == LocalPushType.DailyComeback)
        {
            CancelDailyComebackScheduledInWindow();
        }
        else if (androidIds.TryGetValue(type, out int id))
        {
            AndroidNotificationCenter.CancelScheduledNotification(id);
            AndroidNotificationCenter.CancelNotification(id);
            androidIds.Remove(type);
        }
#elif UNITY_IOS
        if (type == LocalPushType.DailyComeback)
        {
            CancelDailyComebackScheduledInWindow();
        }
        else
        {
            string identifier = GetIosIdentifier(type);
            iOSNotificationCenter.RemoveScheduledNotification(identifier);
            iOSNotificationCenter.RemoveDeliveredNotification(identifier);
            iosIds.Remove(type);
        }
#endif
    }

#if UNITY_IOS
    private string GetIosIdentifier(LocalPushType type)
    {
        // 타입마다 고정된 Identifier 사용 → 같은 타입이면 항상 덮어쓰기/취소가능
        return $"local_push_{type}";
    }
#endif
}

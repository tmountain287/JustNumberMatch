using Common.Manager;
using UnityEngine;

public enum HapticType
{
    Light,
    Medium,
    Heavy
}

public class HapticManager : MonoSingletonDont<HapticManager>
{ 
    public void Play(HapticType type = HapticType.Light)
    {
        if (PlayerPrefsManager.Instance.GetPlayerPrefsValue(PrefsKey.VIBRATION, 1)==0)
            return;

        if (!SystemInfo.supportsVibration)
            return;

#if UNITY_ANDROID && !UNITY_EDITOR
        PlayAndroid(type);
#elif UNITY_IOS && !UNITY_EDITOR
        //PlayIOS(type);
#else
        // 에디터나 기타 플랫폼에서는 진동 없음 (필요하면 Debug.Log 정도)
        // Debug.Log($"Haptic: {type}");
#endif
    }

    // 성공용 간편 함수
    public void PlaySuccess()
    {
        Play(HapticType.Light);
    }

    // 오답용 넣고 싶으면 이거 호출하게 사용
    public void PlayError()
    {
        Play(HapticType.Medium);
    }

    #region ANDROID

    private void PlayAndroid(HapticType type)
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext"))
            using (AndroidJavaObject vibrator = context.Call<AndroidJavaObject>("getSystemService", "vibrator"))
            {
                if (vibrator == null)
                    return;

                long durationMs;
                int amplitude; // 1~255

                switch (type)
                {
                    case HapticType.Light:
                        durationMs = 15;
                        amplitude = 80;
                        break;
                    case HapticType.Medium:
                        durationMs = 25;
                        amplitude = 160;
                        break;
                    case HapticType.Heavy:
                        durationMs = 35;
                        amplitude = 255;
                        break;
                    default:
                        durationMs = 20;
                        amplitude = 120;
                        break;
                }

                using (AndroidJavaClass vibrationEffectClass =
                       new AndroidJavaClass("android.os.VibrationEffect"))
                {
                    int defaultAmplitude = vibrationEffectClass.GetStatic<int>("DEFAULT_AMPLITUDE");
                    if (amplitude <= 0)
                        amplitude = defaultAmplitude;

                    AndroidJavaObject vibrationEffect =
                        vibrationEffectClass.CallStatic<AndroidJavaObject>(
                            "createOneShot",
                            durationMs,
                            amplitude
                        );

                    vibrator.Call("vibrate", vibrationEffect);
                }
            }
        }
        catch
        {
            // 실패 시 그냥 기본 진동
            Handheld.Vibrate();
        }
    }

    #endregion

    #region IOS

    // iOS는 네이티브 플러그인 연결하면 더 좋지만
    // 일단 기본 Handheld.Vibrate로만 처리하게 해둘게.
    // 나중에 원하면 UIImpactFeedbackGenerator 쓰는 네이티브 코드까지 만들어줄게.

    private void PlayIOS(HapticType type)
    {
        Handheld.Vibrate();
    }

    #endregion
}

using GoogleMobileAds.Api;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GoogleAdManager : MonoSingletonDont<GoogleAdManager>
{
    private UnityEvent onOpenEvent = new();
    private UnityEvent onCloseEvent = new();

    private string interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712"; //서비스가 아니면 테스트 값
    private string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917"; //서비스가 아니면 테스트 값

    private bool isInitialized = false;
    private bool isInitializing = false;
    private float retryInterval = 5f;

    private InterstitialAdWrapper interstitialAd;
    public RewardAdWrapper rewardedAd;

    private bool init = false;

    public void Initialize(Action _onOpen = null, Action _onClose = null)
    {
        if (init) return;

#if SERVICE && (!UNITY_EDITOR)
        GoogleAdsConfig config = Resources.Load<GoogleAdsConfig>("GoogleAdsConfig");
        PlatformAdID platformAd = config.GetAdsID();
        if (platformAd != null) 
        {
            interstitialAdUnitId = platformAd.InterstitialAdUnitId;
            rewardedAdUnitId = platformAd.RewardedAdUnitId;
        }
#endif
        UnityMainThreadDispatcher dispatcher = UnityMainThreadDispatcher.Instance;
        StartCoroutine(CheckAndInitializeAdMob());

        onOpenEvent.AddListener(() =>
        {
            _onOpen?.Invoke();
        });

        onCloseEvent.AddListener(() =>
        {
            _onClose?.Invoke();
        });
        init = true;
    }

    IEnumerator CheckAndInitializeAdMob()
    {
        while (!isInitialized)
        {
            if (Application.internetReachability != NetworkReachability.NotReachable && !isInitializing)
            {
                isInitializing = true;
                MobileAds.Initialize(initStatus =>
                {
                    Debug.Log("AdMob Initialize done");
                    isInitialized = true;
                    interstitialAd = new InterstitialAdWrapper(interstitialAdUnitId, () => onOpenEvent?.Invoke(), () => onCloseEvent?.Invoke());
                    rewardedAd = new RewardAdWrapper(rewardedAdUnitId, () => onOpenEvent?.Invoke(), () => onCloseEvent?.Invoke());

                    RequestInterstitialAd();
                    RequestRewardedAd();
                });
            }
            yield return new WaitForSeconds(retryInterval);
        }
    }

    public void RequestInterstitialAd() => interstitialAd?.LoadAd();
    public void RequestRewardedAd() => rewardedAd?.LoadAd();

    public void ShowInterstitialAd() => interstitialAd?.ShowAd(() =>
    {
        onOpenEvent?.Invoke();
    });

    public void ShowRewardedAd(Action<string> onSuccess = null, Action onStop = null, Action<string> onFail = null)
    {
        StartCoroutine(ShowRewardedAdCoroutine((adater) => { onSuccess?.Invoke(adater); }, () => { onStop?.Invoke(); }, (error) => { onFail?.Invoke(error); }));
    }

    private IEnumerator ShowRewardedAdCoroutine(Action<string> onSuccess, Action onStop, Action<string> onFail)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            onFail?.Invoke("온라인 상태에서만\n광고 보기가 가능합니다.\n네트워크 연결 후\n다시 시도해 주세요.");
            yield break;
        }

        float timeout = 5f;
        float timer = 0f;

        while (rewardedAd == null && timer < timeout)
        {
            Debug.Log("[Rewarded] Ad Stanby...");
            yield return new WaitForSeconds(0.5f);
            timer += 0.5f;
        }

        if (rewardedAd != null)
        {
            rewardedAd?.ShowAd(() =>
            {
                onOpenEvent?.Invoke();
            },
            (adapter) =>
            {
                onSuccess?.Invoke(adapter);
            },
            () =>
            {
                onStop?.Invoke();
            },
            (error) =>
            {
                onFail?.Invoke(error);
            });
        }
        else
        {
            onFail?.Invoke("광고 초기화 안됨");
        }
    }
}
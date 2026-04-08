using Common.Manager;
using Common.UI;
using GoogleMobileAds.Api;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GoogleAdManager : MonoSingletonDont<GoogleAdManager>
{
    /// <summary>스크린 픽셀 기준 배너 높이(없으면 0). Canvas는 <see cref="BannerCanvasBottomInset"/> 등에서 scaleFactor로 변환.</summary>
    public static event Action<float> BannerBottomInsetPixelsChanged;

    private UnityEvent onOpenEvent = new();
    private UnityEvent onCloseEvent = new();

    // 비-SERVICE: https://developers.google.com/admob/unity/test-ads (Android / iOS 샘플 단위 ID가 다름)
    private string interstitialAdUnitId = Application.platform == RuntimePlatform.IPhonePlayer
        ? "ca-app-pub-3940256099942544/4411468910"
        : "ca-app-pub-3940256099942544/1033173712";
    private string rewardedAdUnitId = Application.platform == RuntimePlatform.IPhonePlayer
        ? "ca-app-pub-3940256099942544/1712485313"
        : "ca-app-pub-3940256099942544/5224354917";
    // 표준 배너 샘플 ID: https://developers.google.com/admob/unity/banner
    private string bannerAdUnitId = Application.platform == RuntimePlatform.IPhonePlayer
        ? "ca-app-pub-3940256099942544/2435281174"
        : "ca-app-pub-3940256099942544/9214589741";

    private bool isInitialized = false;
    private bool isInitializing = false;
    private float retryInterval = 5f;

    private InterstitialAdWrapper interstitialAd;
    public RewardAdWrapper rewardedAd;
    private BannerAdWrapper bannerAd;
    private BaseUI.Type lastUiTypeForBanner = BaseUI.Type.INTRO;
    private bool useBannerAds = true;

    private bool init = false;

    /// <summary>마지막으로 알려진 배너 하단 높이(스크린 픽셀). 레이아웃 조회용. 배너 없음·숨김·광고제거면 0.</summary>
    private float _lastBannerInsetScreenPixels;

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
            if (!string.IsNullOrEmpty(platformAd.BannerAdUnitId))
                bannerAdUnitId = platformAd.BannerAdUnitId;
            else
            {
                // 비어 있으면 필드 초기화에 있는 Google 샘플 배너 ID(플랫폼별)로 요청 — 배너만 비활성화하지 않음
                Debug.LogWarning("[GoogleAdManager] SERVICE: BannerAdUnitId가 비어 있어 Google 샘플(테스트) 배너 단위로 요청합니다. 스토어 출시 전에 AdMob에서 배너 단위를 만들고 GoogleAdsConfig에 넣으세요.");
            }
        }
#endif
        UserDataManager.OnValueAdsFreeChanged += OnAdsFreeChanged;
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
            // 에디터는 internetReachability가 NotReachable인 경우가 많아 초기화가 영원히 안 됨. 실제 기기만 막는다.
            bool networkOk = Application.internetReachability != NetworkReachability.NotReachable;
#if UNITY_EDITOR
            networkOk = true;
#endif
            if (networkOk && !isInitializing)
            {
                isInitializing = true;
                MobileAds.Initialize(initStatus =>
                {
                    isInitialized = true;
                    interstitialAd = new InterstitialAdWrapper(interstitialAdUnitId, () => onOpenEvent?.Invoke(), () => onCloseEvent?.Invoke());
                    rewardedAd = new RewardAdWrapper(rewardedAdUnitId, () => onOpenEvent?.Invoke(), () => onCloseEvent?.Invoke());
                    if (useBannerAds)
                        bannerAd = new BannerAdWrapper(bannerAdUnitId, AdPosition.Bottom, ApplyBannerVisibility);

                    RequestInterstitialAd();
                    RequestRewardedAd();
                    ApplyBannerVisibility();
                });
            }
            yield return new WaitForSeconds(retryInterval);
        }
    }

    public void RequestInterstitialAd() => interstitialAd?.LoadAd();
    public void RequestRewardedAd() => rewardedAd?.LoadAd();
    public void RequestBannerRetry() => bannerAd?.RetryLoadAfterFailure();

    public void UpdateBannerForCurrentUI(BaseUI.Type uiType)
    {
        lastUiTypeForBanner = uiType;
        ApplyBannerVisibility();
    }

    private void OnAdsFreeChanged() => ApplyBannerVisibility();

    /// <summary>메인 메뉴·스테이지 선택 등 배너를 띄울 화면. 실제 플로우는 <see cref="EnterStep"/> 에서 LOBBY가 아니라 STAGE 로 진입합니다.</summary>
    private static bool ShouldShowBannerForUi(BaseUI.Type uiType) =>
        uiType == BaseUI.Type.LOBBY || uiType == BaseUI.Type.STAGE;

    /// <summary>배너 로드 직후 SDK가 알려주는 높이(스크린 픽셀). UI 여백은 <see cref="NotifyBannerBottomInsetPixels"/>로 통일.</summary>
    public void NotifyBannerBottomInsetPixels(float heightPixels)
    {
        if (!isInitialized || bannerAd == null || UserDataManager.IsAdsFree || !ShouldShowBannerForUi(lastUiTypeForBanner))
            heightPixels = 0f;
        heightPixels = Mathf.Max(0f, heightPixels);
        _lastBannerInsetScreenPixels = heightPixels;
        BannerBottomInsetPixelsChanged?.Invoke(heightPixels);
    }

    /// <summary>현재 UI 레이아웃에 쓸 배너 하단 높이(스크린 픽셀). <see cref="BannerUiLayout"/>와 동기화됩니다.</summary>
    public float GetBannerInsetScreenPixelsForLayout() => _lastBannerInsetScreenPixels;

    private void ApplyBannerVisibility()
    {
        if (!isInitialized || bannerAd == null)
        {
            NotifyBannerBottomInsetPixels(0f);
            return;
        }

        if (UserDataManager.IsAdsFree)
        {
            bannerAd.Destroy();
            NotifyBannerBottomInsetPixels(0f);
            return;
        }

        //if (ShouldShowBannerForUi(lastUiTypeForBanner))
            bannerAd.EnsureLoadedAndVisible();
        //else
        //{
        //    bannerAd.Hide();
        //    NotifyBannerBottomInsetPixels(0f);
        //}
    }

    protected override void OnDestroy()
    {
        UserDataManager.OnValueAdsFreeChanged -= OnAdsFreeChanged;
        bannerAd?.Destroy();
        NotifyBannerBottomInsetPixels(0f);
        base.OnDestroy();
    }

    public void ShowInterstitialAd() => interstitialAd?.ShowAd(() =>
    {
        onOpenEvent?.Invoke();
    });

    public void ShowRewardedAd(Action<string> onSuccess = null, Action onStop = null, Action<string> onFail = null, string placement = "unknown")
    {
        StartCoroutine(ShowRewardedAdCoroutine((adater) => { onSuccess?.Invoke(adater); }, () => { onStop?.Invoke(); }, (error) => { onFail?.Invoke(error); }, placement));
    }

    private IEnumerator ShowRewardedAdCoroutine(Action<string> onSuccess, Action onStop, Action<string> onFail, string placement)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            string message = LocalizationManager.Instance.GetText("OfflineAd");
            onFail?.Invoke(message);
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
            GameAnalyticsHelper.LogRewardedAdStart(placement ?? "unknown");
            rewardedAd?.ShowAd(() =>
            {
                onOpenEvent?.Invoke();
            },
            (adapter) =>
            {
                GameAnalyticsHelper.LogRewardedAdComplete(placement ?? "unknown", "gold");
                onSuccess?.Invoke(adapter);
            },
            () =>
            {
                GameAnalyticsHelper.LogRewardedAdSkip(placement ?? "unknown");
                onStop?.Invoke();
            },
            (error) =>
            {
                onFail?.Invoke(error);
            });
        }
        else
        {
            string message = LocalizationManager.Instance.GetText("FailAdLoad");
            onFail?.Invoke(message);
        }
    }
}
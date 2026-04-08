using GoogleMobileAds.Api;
using System;
using UnityEngine;

// 표준 배너(320×50): https://developers.google.com/admob/unity/banner
public class BannerAdWrapper
{
    private readonly string adUnitId;
    private readonly AdPosition position;
    private readonly Action onLoadCompletedRefresh;

    private BannerView bannerView;
    private bool isLoading;
    private bool hasLoadedOnce;

    public BannerAdWrapper(string adUnitId, AdPosition position, Action onLoadCompletedRefresh)
    {
        this.adUnitId = adUnitId;
        this.position = position;
        this.onLoadCompletedRefresh = onLoadCompletedRefresh;
    }

    public void EnsureLoadedAndVisible()
    {
#if !UNITY_EDITOR
        if (Application.internetReachability == NetworkReachability.NotReachable)
            return;
#endif

        if (bannerView == null)
            CreateBanner();

        if (!hasLoadedOnce && !isLoading)
            TryLoad();
        else if (hasLoadedOnce)
        {
            bannerView?.Show();
            if (bannerView != null && GoogleAdManager.Instance != null)
                GoogleAdManager.Instance.NotifyBannerBottomInsetPixels(GetBannerHeightPixelsForLayout());
        }
    }

    public void Hide()
    {
        bannerView?.Hide();
    }

    public void RetryLoadAfterFailure()
    {
        if (bannerView == null)
            return;
        if (!isLoading)
            TryLoad();
    }

    public void Destroy()
    {
        UnregisterEvents();
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }

        isLoading = false;
        hasLoadedOnce = false;
    }

    private void CreateBanner()
    {
        bannerView = new BannerView(adUnitId, AdSize.Banner, position);
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        bannerView.OnBannerAdLoaded += OnBannerAdLoaded;
        bannerView.OnBannerAdLoadFailed += OnBannerAdLoadFailed;
    }

    private void UnregisterEvents()
    {
        if (bannerView == null)
            return;

        bannerView.OnBannerAdLoaded -= OnBannerAdLoaded;
        bannerView.OnBannerAdLoadFailed -= OnBannerAdLoadFailed;
    }

    private void TryLoad()
    {
        if (isLoading || bannerView == null)
            return;

        isLoading = true;
        bannerView.LoadAd(new AdRequest());
    }

    private void OnBannerAdLoaded()
    {
        isLoading = false;
        hasLoadedOnce = true;
        if (bannerView != null && GoogleAdManager.Instance != null)
            GoogleAdManager.Instance.NotifyBannerBottomInsetPixels(GetBannerHeightPixelsForLayout());
        onLoadCompletedRefresh?.Invoke();
    }

    /// <summary>
    /// AdPosition.Bottom 기준으로 화면 하단에서 위로 잡아먹는 높이(스크린 픽셀에 가깝게).
    /// SDK가 0을 주는 경우(에디터 등) 레이아웃용으로 대략값을 씁니다.
    /// </summary>
    private float GetBannerHeightPixelsForLayout()
    {
        if (bannerView == null)
            return 0f;
        float h = bannerView.GetHeightInPixels();
#if UNITY_EDITOR
        if (h <= 0f)
            h = Mathf.RoundToInt(50f * Screen.dpi / 160f);
        if (h <= 0f)
            h = 90f;
#else
        if (h <= 0f)
            h = Mathf.RoundToInt(50f * Screen.dpi / 160f);
#endif
        return h;
    }

    private void OnBannerAdLoadFailed(LoadAdError error)
    {
        isLoading = false;
        Debug.Log($"[Banner] Load Fail: {error.GetMessage()}");
        if (GoogleAdManager.Instance != null)
            GoogleAdManager.Instance.Invoke(nameof(GoogleAdManager.RequestBannerRetry), 10f);
    }
}

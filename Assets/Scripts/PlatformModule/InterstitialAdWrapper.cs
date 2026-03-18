using GoogleMobileAds.Api;
using System;
using UnityEngine;

public class InterstitialAdWrapper  : BaseAdWrapper
{
    private InterstitialAd interstitialAd;
    private Action onOpenedWrapper;
    private Action onClosedWrapper;

    public InterstitialAdWrapper (string adUnitId, Action onAdOpened, Action onAdClosed)
        : base(adUnitId, onAdOpened, onAdClosed) { }

    public override void LoadAd()
    {
        if (isLoading) return;
        isLoading = true;

        if (interstitialAd != null)
        {
            //if (onOpenedWrapper != null)
            //    interstitialAd.OnAdFullScreenContentOpened -= onOpenedWrapper;
            if (onClosedWrapper != null)
                interstitialAd.OnAdFullScreenContentClosed -= onClosedWrapper;
        }

        InterstitialAd.Load(adUnitId, new AdRequest(), (ad, error) =>
        {
            isLoading = false;

            if (error != null)
            {
                Debug.Log($"[Interstitial] Load Fail: {error.GetMessage()}");
                GoogleAdManager.Instance.Invoke(nameof(GoogleAdManager.Instance.RequestInterstitialAd), 10f);
                return;
            }

            interstitialAd = ad;
            Debug.Log($"[Interstitial]{adUnitId} Load Success!");

            //onOpenedWrapper = () =>
            //{
            //    Debug.Log("[Interstitial] 광고 열림");
            //    OnAdOpened();
            //};

            onClosedWrapper = () =>
            {
                Debug.Log("[Interstitial] Ad Close");
                OnAdClosed();
            };

            //interstitialAd.OnAdFullScreenContentOpened += onOpenedWrapper;
            interstitialAd.OnAdFullScreenContentClosed += onClosedWrapper;
        });
    }

    public override void ShowAd(Action _onOpen = null, Action<string> _onSuccess = null, Action _onStop = null, Action<string> _onFail = null)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("NetworkReachability.NotReachable");
            _onFail?.Invoke("온라인 상태에서만\n광고 보기가 가능합니다.\n네트워크 연결 후\n다시 시도해 주세요.");
            return;
        }

        onStop = () => _onStop?.Invoke();

        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            _onOpen?.Invoke();
            interstitialAd.Show();
            _onSuccess?.Invoke(LogAdNetwork(interstitialAd));
        }
        else
        {
            Debug.Log("[Interstitial] Not Ready");
            _onFail?.Invoke("광고 준비가 안되었습니다.\n잠시 후 다시 시도해 주세요.");
        }
    }

    protected override void OnAdClosed()
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            interstitialAd = null;
            onStop?.Invoke();
            base.OnAdClosed();
        });
    }

    private string LogAdNetwork(InterstitialAd ad)
    {
        var responseInfo = ad.GetResponseInfo();
        if (responseInfo == null)
        {
            return "Unknown";
        }

        string adapter = responseInfo.GetMediationAdapterClassName();
        Debug.Log($"Mediation Adapter: {adapter}");

        if (adapter == null)
        {
            return "Unknown";
        }

        Debug.Log($"Mediation Adapter: {adapter}");

        if (adapter.Contains("unity", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("✅ Unity Ads 로 광고 노출됨");
            return "UnityAds";
        }
        else if (adapter.Contains("admob", System.StringComparison.OrdinalIgnoreCase) ||
                 adapter.Contains("google", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("✅ Google AdMob 로 광고 노출됨");
            return "AdMob";
        }
        else
        {
            Debug.Log($"⚠️ 기타 네트워크: {adapter}");
            return adapter;
        }
    }
}

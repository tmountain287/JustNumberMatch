using GoogleMobileAds.Api;
using UnityEngine;
using System.Collections;
using System;

public class RewardAdWrapper : BaseAdWrapper
{
    private RewardedAd rewardedAd;
    private Action onOpenedWrapper;
    private Action onClosedWrapper;

    private Action<string> onSuccess;
    private Action<string> onFailed;

    private Reward currentReward;

    public RewardAdWrapper(string adUnitId, Action onAdOpened, Action onAdClosed)
        : base(adUnitId, onAdOpened, onAdClosed) { }

    public override void LoadAd()
    {
        currentState = "로딩중";
        if (isLoading) return;
        isLoading = true;

        if (rewardedAd != null)
        {
            //if (onOpenedWrapper != null)
            //    rewardedAd.OnAdFullScreenContentOpened -= onOpenedWrapper;
            if (onClosedWrapper != null)
                rewardedAd.OnAdFullScreenContentClosed -= onClosedWrapper;
        }

        RewardedAd.Load(adUnitId, new AdRequest(), (ad, error) =>
        {
            isLoading = false;

            if (error != null)
            {
                currentState = "[Rewarded] Load Fail";
                Debug.Log($"[Rewarded] LoadFail: {error.GetMessage()}");
                GoogleAdManager.Instance.Invoke(nameof(GoogleAdManager.Instance.RequestRewardedAd), 10f);
                return;
            }

            rewardedAd = ad;            
            Debug.Log($"[Rewarded]{adUnitId} Load Success!");
            currentState = "[Rewarded] 광고 로드 성공";

            //onOpenedWrapper = () =>
            //{
            //    Debug.Log("[Rewarded] 광고 열림");
            //    OnAdOpened();
            //};

            onClosedWrapper = () =>
            {
                Debug.Log("[Rewarded] Close");
                OnAdClosed();
            };

            //rewardedAd.OnAdFullScreenContentOpened += onOpenedWrapper;
            rewardedAd.OnAdFullScreenContentClosed += onClosedWrapper;
        });
    }

    public override void ShowAd(Action _onOpen = null, Action<string> _onSuccess = null, Action _onClose = null, Action<string> _onFail = null)
    {
        onSuccess = (adater) => _onSuccess?.Invoke(adater);
        onStop = () => _onClose?.Invoke();
        onFailed = (error) => _onFail?.Invoke(error);
        GoogleAdManager.Instance.StartCoroutine(ShowRewardedAdCoroutine(_onOpen));
    }

    private IEnumerator ShowRewardedAdCoroutine(Action _onOpen)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            OnRewardFail("온라인 상태에서만\n광고 보기가 가능합니다.\n네트워크 연결 후\n다시 시도해 주세요.");
            yield break;
        }

        float timeout = 5f;
        float timer = 0f;

        while (rewardedAd == null && timer < timeout)
        {
            currentState = "[Rewarded] 광고 대기 중";
            Debug.Log("[Rewarded] Ad Stanby...");
            yield return new WaitForSeconds(0.5f);
            timer += 0.5f;
        }

        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            _onOpen?.Invoke();
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"[Rewarded] Reward: {reward.Amount} {reward.Type}");
                currentReward = reward;
            });
        }
        else
        {
            Debug.Log("[Rewarded] Not Ready (Time Out)");
            currentState = "[Rewarded] 광고 준비 안됨";
            OnRewardFail("광고 준비가 안되었습니다.\n잠시 후 다시 시도해 주세요.");
        }
    }

    private void OnRewardFail(string _error)
    {
        UnityMainThreadDispatcher.Instance.Enqueue((error) =>
        {
            onFailed?.Invoke(error);
        }, _error);
    }


    protected override void OnAdClosed()
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            if (currentReward != null)
            {
                onSuccess?.Invoke(LogAdNetwork(rewardedAd));
            }
            else
            {
                onStop?.Invoke();
            }
            currentReward = null;
            rewardedAd = null;
            base.OnAdClosed();
        });
    }

    private string LogAdNetwork(RewardedAd ad)
    {
        var responseInfo = ad.GetResponseInfo();
        if (responseInfo == null)
        {
            return "Unknown";
        }

        string adapter = responseInfo.GetMediationAdapterClassName();

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
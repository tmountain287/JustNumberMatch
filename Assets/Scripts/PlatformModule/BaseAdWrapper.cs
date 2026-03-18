using GoogleMobileAds.Api;
using System;
using UnityEngine;

public abstract class BaseAdWrapper
{
    protected string adUnitId;
    protected bool isLoading = false;

    private Action onAdOpened = null;
    private Action onAdClosed = null;

    protected Action onStop = null;

    public string currentState = "";

    public BaseAdWrapper(string adUnitId, Action onAdOpened, Action onAdClosed)
    {
        this.adUnitId = adUnitId;
        this.onAdOpened = onAdOpened;
        this.onAdClosed = onAdClosed;
    }

    public abstract void LoadAd();
    public abstract void ShowAd(Action _onOpen = null, Action<string> _onSuccess = null, Action _onClose = null, Action<string> _onFail = null);

    protected virtual void OnAdOpened()
    {
        onAdOpened?.Invoke();
    }

    protected virtual void OnAdClosed()
    {
        onAdClosed?.Invoke();
        LoadAd(); // 광고 닫히면 다시 로드
    }
}
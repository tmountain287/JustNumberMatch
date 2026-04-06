using Common.Manager;
using Common.UI;
using DG.Tweening;
using JustOneMatch.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : MonoSingleton<UIManager>
{
    #region Inspector Fields
    [SerializeField] private List<BaseUI> uiList = new();
    [SerializeField] private GameObject loading = null;
    [SerializeField] private GameObject backAds = null;
    [SerializeField] private GameObject block = null;
    [SerializeField] private RewardEffectUI rewardEffectUI = null;
    #endregion

    private TopUI topUI = null;

    private Coroutine deferredPremiumSalePopupCoroutine;

    public bool IsLoading { get { return loading.activeSelf; } }

    public BaseUI.Type CurrentUIType { get => currentUIType; set => currentUIType = value; }
    public TopUI TopUI { get => topUI; set => topUI = value; }

    private BaseUI.Type currentUIType = BaseUI.Type.INTRO;

    //private void Start()
    //{
    //    SceneManager.sceneLoaded += OnSceneLoaded;
    //}

    //private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    ShowUI(BaseUI.Type.LOBBY);
    //}

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // 씬이 종료될 때 모든 트윈 제거
        DOTween.KillAll();
       // SceneManager.sceneLoaded -= OnSceneLoaded;
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    private bool canFullScreen = false;
#endif

    public void ActivateForSeconds(float duration)
    {
        StartCoroutine(ActivateRoutine(duration));
    }

    private IEnumerator ActivateRoutine(float duration)
    {
        block.SetActive(true);
        yield return new WaitForSeconds(duration);
        block.SetActive(false);
    }

    public void ShowLoading()
    {
        loading.SetActive(true);
    }

    public void HideLoading()
    {
        loading.SetActive(false);
    } 

    public T GetUI<T>(BaseUI.Type _type) where T : BaseUI
    {
        return uiList.Where(x => x.UIType == _type).FirstOrDefault() as T;
    }


    public bool ShowUI(BaseUI.Type _type)
    {
        if(CurrentUIType != _type)
        {
            uiList.ForEach(x => x.SetUI(_type));
            CurrentUIType = _type;
            GameAnalyticsHelper.LogScreenView("Screen_" + _type.ToString(), _type.ToString());
            GoogleAdManager.Instance.UpdateBannerForCurrentUI(_type);

            if (_type == BaseUI.Type.STAGE)
            {
                if (deferredPremiumSalePopupCoroutine != null)
                    StopCoroutine(deferredPremiumSalePopupCoroutine);
                deferredPremiumSalePopupCoroutine = StartCoroutine(DeferredPremiumSalePopupRoutine());
            }

            return true;
        }
        return false;        
    }

    IEnumerator DeferredPremiumSalePopupRoutine()
    {
        yield return null;

        float waited = 0f;
        while (PopupManager.Instance != null && PopupManager.Instance.OpenPopupCount > 0 && waited < 12f)
        {
            yield return null;
            waited += Time.unscaledDeltaTime;
        }

        yield return new WaitForSecondsRealtime(0.15f);

        UserDataManager.TryShowPremiumSalePopupOnFirstEligibleSession();
        deferredPremiumSalePopupCoroutine = null;
    }

    public void ShowRewardedAd(Action<string> _onSuccess, Action _onFail = null, string placement = "unknown")
    {
        ShowLoading();
        GoogleAdManager.Instance.ShowRewardedAd((adapter) =>
        {
            HideLoading();
            UserDataManager.ResetInterstitialCondition();
            _onSuccess?.Invoke(adapter);
        },
        () => { HideLoading(); },
        (str) =>
        {
            HideLoading();
            PopupManager.Instance.OpenMessageBoxPopup("", str);
            _onFail?.Invoke();
        },
        placement);
    }

    public void OnUI(BaseUI.Type _type, bool _isOn)
    {
        BaseUI baseUI = uiList.Where(x => x.UIType == _type).FirstOrDefault();
        if (baseUI != null)
        {
            baseUI.gameObject.SetActive(_isOn);
        }
    }

    public void SetBackAds(bool _flag)
    {
        backAds.SetActive(_flag);
    }

    public void OnEffect(ItemType _itemType, int _amount, Vector3 _startPosition, Transform _targetPoint,
        Action _firsArrivedAction = null, Action _arrivedAction = null)
    {
        rewardEffectUI.OnEffect(_itemType, _amount, _startPosition, _targetPoint,
        _firsArrivedAction, _arrivedAction);
    }



    //    public void ForceFullScreen()
    //    {
    //#if UNITY_WEBGL && !UNITY_EDITOR
    //        if (!Screen.fullScreen)
    //        {
    //             if (Application.isMobilePlatform)
    //            {
    //                canFullScreen = true;
    //                Application.ExternalEval("ForceFullScreen()");
    //            }
    //        }
    //#endif
    //    }

    //#if UNITY_WEBGL && !UNITY_EDITOR
    //    private void Update()
    //    {
    //        if(!canFullScreen)
    //            return;

    //        if (!Screen.fullScreen)
    //        {
    //            if (Application.isMobilePlatform)
    //            {
    //                if (Input.touchCount > 0)
    //                {
    //                    // 첫 번째 터치 이벤트 가져오기
    //                    Touch touch = Input.GetTouch(0);

    //                    // 터치가 시작될 때 (TouchPhase.Began) AFunction 호출
    //                    if (touch.phase == TouchPhase.Began)
    //                    {
    //                        Application.ExternalEval("ForceFullScreen()");
    //                    }
    //                }
    //            }
    //        }
    //    }
    //#endif
}
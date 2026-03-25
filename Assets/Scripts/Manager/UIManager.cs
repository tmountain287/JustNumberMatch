using Common.Manager;
using Common.UI;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoSingleton<UIManager>
{
    #region Inspector Fields
    [SerializeField] private List<BaseUI> uiList = new();
    [SerializeField] private GameObject loading = null;
    [SerializeField] private GameObject backAds = null;
    #endregion

    public bool IsLoading { get { return loading.activeSelf; } }

    public BaseUI.Type CurrentUIType { get => currentUIType; set => currentUIType = value; }

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
    public void ShowLoading()
    {
        loading.SetActive(true);
    }

    public void HideLoading()
    {
        loading.SetActive(false);
    } 

    public BaseUI GetUI(BaseUI.Type _type)
    {
        return uiList.Where(x => x.UIType == _type).FirstOrDefault();
    }


    public bool ShowUI(BaseUI.Type _type)
    {
        if(CurrentUIType != _type)
        {
            uiList.ForEach(x => x.SetUI(_type));
            CurrentUIType = _type;
            return true;
        }
        return false;        
    }

    public void ShowRewardedAd(Action<string> _onSuccess, Action _onFail = null)
    {
        ShowLoading();
        GoogleAdManager.Instance.ShowRewardedAd((adapter) =>
        {
            CNetDocument.InGame.GamePlayCount = 0;
            HideLoading();
            _onSuccess?.Invoke(adapter);
        },
        () =>
        {
            HideLoading();
        },
        (str) =>
        {
            HideLoading();
            PopupManager.Instance.OpenMessageBoxPopup("알림", str);
            _onFail?.Invoke();
        });
    }

    public void ShowRewardedAdOnFail(Action<string> _onSuccess, Action _onFail = null)
    {
        ShowLoading();
        GoogleAdManager.Instance.ShowRewardedAd((adapter) =>
        {
            HideLoading();
            _onSuccess?.Invoke(adapter);
        },
        () =>
        {
            HideLoading();
        },
        (str) =>
        {
            HideLoading();
            _onFail?.Invoke();
        });
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
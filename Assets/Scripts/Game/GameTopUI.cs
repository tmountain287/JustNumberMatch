using Common.Manager;
using Common.UI;
using DG.Tweening;
using JustOneMatch.UI;
using System;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using Util;

public class GameTopUI : MonoBehaviour
{
    [SerializeField] private GameUI gameUI;
    [SerializeField] private SkillItemButton hintButton = null;   
    [SerializeField] private SkillItemButton changeButton = null;
   

    [SerializeField] private Button restartButton = null;
    [SerializeField] private Button exitButton = null;

    [SerializeField] private ClearStageState clearStateState = null;
    [SerializeField] private GameTimer gameTimer = null;
    [SerializeField] private InfiniteSliderTimer infiniteSliderTimer = null;

    [SerializeField] private SliderTimer sliderTimer = null;    

    [SerializeField] private RectTransform rightRect = null;
    [SerializeField] private RectTransform leftRect = null;
    [SerializeField] private RectTransform centerRect = null;

    [SerializeField] private Button goldAddButton = null;

    [SerializeField] private ObjectPool comboEffectObjectPool;

    public SkillItemButton HintButton => hintButton;
    public SkillItemButton ChangeButton => changeButton;

    public Button RestartButton => restartButton;
    
    public Button ExitButton => exitButton;

    public ClearStageState ClearStageState => clearStateState;

    public GameTimer GameTimer => gameTimer;

    public SliderTimer SliderTimer => sliderTimer;

    public InfiniteSliderTimer InfiniteSliderTimer => infiniteSliderTimer;

    public Button GoldAddButton => goldAddButton;

    /// <summary>서바이벌 모드: comboEffectObjectPool에서 이펙트를 꺼내 PlayCombo 재생 후 끝나면 풀에 반환</summary>
    public void PlayComboEffectFromPool(float gauge01)
    {
        if (comboEffectObjectPool == null) return;
        GameObject go = comboEffectObjectPool.GetObjectFromPool();
        var effect = go.GetComponent<ComboTextEffect>();
        if (effect == null)
        {
            comboEffectObjectPool.ReturnObjectToPool(go);
            return;
        }
        effect.PlayCombo(gauge01, () => comboEffectObjectPool.ReturnObjectToPool(go));
    }

    private Action exitAction = null;

    private IGameTopUIPresenter presenter;
    public void SetExitAction(Action act) => exitAction = act;

    void Start()
    {
        //#if UNITY_EDITOR || TEST
        //        debugButton.onClick.AddListener(() =>
        //        {
        //            UIManager.Instance.OnUI(BaseUI.Type.DEBUG, true);
        //        });
        //        debugButton.enabled = true;
        //#else
        //            debugButton.enabled = false;
        //#endif
        comboEffectObjectPool.CreateObjectPool();
        restartButton.onClick.AddListener(() =>
        {
            GameMgr.Instance.ReStage();
        });

        

        exitButton.onClick.AddListener(() =>
        {
            exitAction?.Invoke();
        });
    }

    private void OnEnable()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    private void OnDisable()
    {
        rightRect.anchoredPosition = new Vector2(1000, rightRect.anchoredPosition.y);
        leftRect.anchoredPosition = new Vector2(-1000, leftRect.anchoredPosition.y);
    }

    public void SetGameMode(GameModeType gameMode)
    {
        presenter?.OnDetach();

        switch (gameMode)
        {
            case GameModeType.STAGE:
                presenter = new StageTopPresenter();
                break;
            case GameModeType.BOSS_STAGE:
                presenter = new BossStageTopPresenter();
                break;
            case GameModeType.TIME_ATTACK:
                presenter = new TimeAttackTopPresenter();
                break;
            case GameModeType.SURVIVAL_MODE:
                presenter = new InfiniteTopPresenter();
                break;
        }

        MoveRight(true);
        MoveLeft(true);

        presenter?.OnAttach(this);
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    public void ReStage()
    {
        presenter?.OnReStage();
    }

    //public void RefreshButtons()
    //{
    //    if(HintButton.gameObject.activeSelf)
    //    {
    //        bool hasHint = UserDataManager.GetItemCount(ItemType.Hint) > 0;

    //        if (!hasHint)
    //        {
    //            HintButton.gameObject.SetActive(false);
    //            hintAdButton.gameObject.SetActive(true);
    //        }
    //    }

    //    MoveRight(true);
    //}

    private void MoveRight(bool _flag)
    {
        float x = _flag ? 0 : 1000f;

        rightRect.DOAnchorPosX(x, 0.2f)
              .SetEase(_flag ? Ease.OutBack : Ease.InBack);
    }

    private void MoveLeft(bool _flag)
    {
        float x = _flag ? 0 : -1000f;

        leftRect.DOAnchorPosX(x, 0.2f)
              .SetEase(_flag ? Ease.OutBack : Ease.InBack);
    }
}
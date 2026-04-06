using Common.Manager;
using JustOneMatch.UI;

public sealed class BossStageTopPresenter : TopUIPresenterBase
{
    public override void OnAttach(GameTopUI v)
    {
        base.OnAttach(v);

        // 표시/비표시
        SetActive(V.RestartButton, true);
        SetActive(V.ChangeButton, true);        
        SetActive(V.HintButton, true);
        SetActive(v.ClearStageState, true);
        SetActive(v.GameTimer, false);
        SetActive(v.SliderTimer, true);
        SetActive(v.InfiniteSliderTimer, false);

        v.GoldAddButton.gameObject.SetActive(false);

        // 종료 액션
        V.SetExitAction(() =>
        {
            GameMgr.Instance.StopAnySession();
            UIManager.Instance.ShowUI(Common.UI.BaseUI.Type.STAGE);
            PopupManager.Instance.OpenPopup<StagePopup>()
                .Initialize(GameMgr.Instance.CurrentTableData.difficultyType,
                            GameMgr.Instance.CurrentTableData.stage);
        });

        V.ChangeButton.SetButton(GameModeType.BOSS_STAGE, () =>
        {
            //MoveRight(false);
            GameMgr.Instance.UseChangeItem(false);
        }, () =>
        {
            UIManager.Instance.ShowRewardedAd((adapter) =>
            {
                GameMgr.Instance.UseChangeItem(true);
            }, null, "change");
        });

        V.ChangeButton.SetEnable(true);

        V.HintButton.SetButton(GameModeType.BOSS_STAGE, () =>
        {
            V.HintButton.SetEnable(false);
            GameMgr.Instance.UseHintItem(false);
        },
        () =>
        {
            UIManager.Instance.ShowRewardedAd((adapter) =>
            {
                V.HintButton.SetEnable(false);
                GameMgr.Instance.UseHintItem(true);
            }, null, "hint");
        });

        V.HintButton.SetEnable(true);
    }

    public override void OnReStage()
    {
        base.OnReStage();
        V.HintButton.SetEnable(true);
    }
}

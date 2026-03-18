using Common.Manager;
using JustOneMatch.UI;

public sealed class StageTopPresenter : TopUIPresenterBase
{
    public override void OnAttach(GameTopUI v)
    {
        base.OnAttach(v);

        SetActive(V.HintButton, true);
        SetActive(V.ChangeButton, false);
        SetActive(V.RestartButton, true);
        SetActive(v.ClearStageState, false);
        SetActive(v.GameTimer, false);
        SetActive(v.SliderTimer, false);
        SetActive(v.InfiniteSliderTimer, false);

        v.GoldAddButton.gameObject.SetActive(true);

        // 종료 액션
        V.SetExitAction(() =>
        {
            GameMgr.Instance.StopAnySession();
            UIManager.Instance.ShowUI(Common.UI.BaseUI.Type.STAGE);
            PopupManager.Instance.OpenPopup<StagePopup>()
                .Initialize(GameMgr.Instance.CurrentTableData.difficultyType,
                            GameMgr.Instance.CurrentTableData.stage);
        });
        
        V.HintButton.SetButton(GameModeType.STAGE, () =>
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

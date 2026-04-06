public sealed class InfiniteTopPresenter : TopUIPresenterBase
{
    public override void OnAttach(GameTopUI v)
    {
        base.OnAttach(v);

        // 표시/비표시
        SetActive(V.RestartButton, true);

        SetActive(V.ChangeButton, true);

        // 힌트 버튼은 정책에 따라 숨김
        SetActive(V.HintButton, true);
        SetActive(v.ClearStageState, false);
        SetActive(v.GameTimer, false);
        SetActive(v.SliderTimer, false);
        SetActive(v.InfiniteSliderTimer, true);

        v.GoldAddButton.gameObject.SetActive(false);

        // 종료 액션 - 서바이벌 모드는 StageUI로 복귀
        V.SetExitAction(() =>
        {
            GameMgr.Instance.StopAnySession();
            UIManager.Instance.ShowUI(Common.UI.BaseUI.Type.STAGE);
        });

        V.ChangeButton.SetButton(GameModeType.TIME_ATTACK, () =>
        {
            //MoveRight(false);
            GameMgr.Instance.UseChangeItem(false);
        }, null, 3);

        V.ChangeButton.SetEnable(true);

        V.HintButton.SetButton(GameModeType.TIME_ATTACK, () =>
        {
            V.HintButton.SetEnable(false);
            GameMgr.Instance.UseHintItem(false);
        },
        null, 3);

        V.HintButton.SetEnable(true);


        //// (선택) 타임어택 상태 이벤트 연결
        //if (V.TimeAttackState != null)
        //{
        //    V.TimeAttackState.OnRemainChanged += OnRemainChanged;
        //    V.TimeAttackState.OnClearedChanged += OnClearedChanged;
        //}
    }

    public override void OnReStage()
    {
        base.OnReStage();
        V.HintButton.SetEnable(true);
        V.ChangeButton.SetEnable(true);
        V.HintButton.SetCount();
        V.ChangeButton.SetCount();
    }

    public override void OnDetach()
    {
        //if (V?.TimeAttackState != null)
        //{
        //    V.TimeAttackState.OnRemainChanged -= OnRemainChanged;
        //    V.TimeAttackState.OnClearedChanged -= OnClearedChanged;
        //}
    }

    private void OnRemainChanged(float seconds)
    {
        // 필요 시 상단 타이머 텍스트/애니메이션 업데이트 (GameUI에 위임해도 OK)
        // V.gameUI.SetTimer(seconds); 같은 식으로
    }

    private void OnClearedChanged(int cleared, int total)
    {
        // 상단 진행바/텍스트 업데이트 (V.gameUI 연동)
    }
}

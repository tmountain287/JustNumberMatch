using Common.UI;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public sealed class EnterStep : IIntroFlowStep
{
    public string Name => "Enter";
    public float Weight => 0.40f;

    public bool CanRun(IntroFlowContext ctx)
    {
        return true;
    }

    public async UniTask<FlowResult> RunAsync(IntroFlowContext ctx)
    {
        ctx.SetTextKey?.Invoke("Entering the Game");

        // ✅ 남은 만큼 채워서 100% 만들고 애니메이션 끝까지 대기
        await ctx.Progress.SetAsync(1f, ProgressFlow.GetStepAnimDuration(Weight));

        GameAnalyticsHelper.LogLobbyEntered();
        // 접속 시점에 다음날 13시 재접속 푸시 예약 (푸시 ON일 때만)
        LocalPushManager.Instance.RefreshDailyComebackPush();

        Debug.Log("111111111111111111111111111111");
        // 여기서 다음 씬/로비 진입/페이드 등
        InGameManager.Instance.GameInit();
        UIManager.Instance.ShowUI(BaseUI.Type.STAGE);

        return FlowResult.Continue;
    }
}

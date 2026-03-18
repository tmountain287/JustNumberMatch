using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface ILobbyFlowStep
{
    string Name { get; }
    float Weight { get; }                 // 0~1 누적 비율
    bool CanRun(LobbyFlowContext ctx);
    UniTask<FlowResult> RunAsync(LobbyFlowContext ctx);
}

public enum FlowResult
{
    Continue,   // 다음 단계 진행
    Stop        // 여기서 멈춤 (유지보수/강제업데이트 등)
}

public sealed class LobbyFlowContext
{
    public LobbyUI UI { get; }
    public ProgressFlow Progress { get; }
    public Action<string> SetTextKey { get; }

    public LobbyFlowContext(LobbyUI ui, ProgressFlow progress, Action<string> setTextKey)
    {
        UI = ui;
        Progress = progress;
        SetTextKey = setTextKey;
    }
}

public sealed class LobbyFlowRunner
{
    private readonly List<ILobbyFlowStep> steps;

    public LobbyFlowRunner(IEnumerable<ILobbyFlowStep> steps)
    {
        this.steps = new List<ILobbyFlowStep>(steps);
    }

    public async UniTask RunAsync(LobbyFlowContext ctx)
    {
        foreach (var step in steps)
        {
            if (!step.CanRun(ctx))
                continue;

            var result = await step.RunAsync(ctx);

            // ✅ 실행된 Step만 progress 반영 + 완료까지 대기
            await ctx.Progress.AddAsync(step.Weight, 0.2f);

            if (result == FlowResult.Stop)
                return;
        }
    }

}

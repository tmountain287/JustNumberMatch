using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IIntroFlowStep
{
    string Name { get; }
    float Weight { get; }                 // 0~1 누적 비율
    bool CanRun(IntroFlowContext ctx);
    UniTask<FlowResult> RunAsync(IntroFlowContext ctx);
}

public enum FlowResult
{
    Continue,   // 다음 단계 진행
    Stop        // 여기서 멈춤 (유지보수/강제업데이트 등)
}

public sealed class IntroFlowContext
{
    public IntroUI UI { get; }
    public ProgressFlow Progress { get; }
    public Action<string> SetTextKey { get; }

    public IntroFlowContext(IntroUI ui, ProgressFlow progress, Action<string> setTextKey)
    {
        UI = ui;
        Progress = progress;
        SetTextKey = setTextKey;
    }
}

public sealed class IntroFlowRunner
{
    private readonly List<IIntroFlowStep> steps;

    public IntroFlowRunner(IEnumerable<IIntroFlowStep> steps)
    {
        this.steps = new List<IIntroFlowStep>(steps);
    }

    public async UniTask RunAsync(IntroFlowContext ctx)
    {
        foreach (var step in steps)
        {
            if (!step.CanRun(ctx))
                continue;

            var result = await step.RunAsync(ctx);

            // ✅ 실행된 Step만 progress 반영 + 완료까지 대기
            float animSec = ProgressFlow.GetStepAnimDuration(step.Weight);
            await ctx.Progress.AddAsync(step.Weight, animSec);

            if (result == FlowResult.Stop)
                return;
        }
    }

}

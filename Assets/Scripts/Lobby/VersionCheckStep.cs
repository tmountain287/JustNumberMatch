using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

public sealed class VersionCheckStep : ILobbyFlowStep
{
    public string Name => "VersionCheck";
    public float Weight => 0.10f;

    public bool CanRun(LobbyFlowContext ctx)
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    public async UniTask<FlowResult> RunAsync(LobbyFlowContext ctx)
    {
        ctx.SetTextKey?.Invoke("Checking version");

        // 시작에 살짝 움직임(선택)
        ctx.Progress.Add(Weight * 0.2f, 0.15f);

        VersionInfo info = await CheckVersionAsync();
        string current = Application.version;
        string serverVersion = info?.android_latest ?? info?.android_min ?? "";

        if (info == null)
        {
            GameAnalyticsHelper.LogVersionCheckResult("fail", current, serverVersion);
            return FlowResult.Continue;
        }

        if (info.maintenance)
        {
            Debug.Log("유지보수 모드");
            GameAnalyticsHelper.LogVersionCheckResult("maintenance", current, serverVersion);
            return FlowResult.Stop;
        }

        if (IsOlder(current, info.android_min))
        {
            Debug.Log("강제 업데이트");
            GameAnalyticsHelper.LogVersionCheckResult("force_update", current, serverVersion);
            return FlowResult.Stop;
        }

        if (IsOlder(current, info.android_latest))
        {
            Debug.Log("업데이트 권유");
            GameAnalyticsHelper.LogVersionCheckResult("update_available", current, serverVersion);
            return FlowResult.Continue;
        }

        Debug.Log("버전 OK");
        GameAnalyticsHelper.LogVersionCheckResult("ok", current, serverVersion);
        // 남은 80% 채우는 건 Runner가 Weight 처리하지만,
        // 위에서 20%를 더했으니 Weight * 0.2를 미리 쓴 셈.
        // 이 경우 Runner가 Weight를 또 더하면 초과될 수 있어서
        // "Step 내부에서 Add하지 말고" Runner에게만 맡기는게 더 깔끔.
        // -> 그래서 위의 20% Add는 원하면 제거하거나, Weight를 0으로 두고 내부에서 다 처리하는 방식으로 통일 추천.

        return FlowResult.Continue;
    }

    private UniTask<VersionInfo> CheckVersionAsync()
    {
        var tcs = new UniTaskCompletionSource<VersionInfo>();

        NetworkManager.Instance.CheckVersion(
            onSuccess: info => tcs.TrySetResult(info),
            onFail: err =>
            {
                Debug.LogWarning(err);
                tcs.TrySetResult(null);
            });

        return tcs.Task;
    }

    private bool IsOlder(string current, string target)
    {
        try { return new Version(current) < new Version(target); }
        catch { return false; }
    }
}

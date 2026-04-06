using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

public sealed class VersionCheckStep : ILobbyFlowStep
{
    public string Name => "VersionCheck";
    public float Weight => 0.18f;

    public bool CanRun(LobbyFlowContext ctx)
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    public async UniTask<FlowResult> RunAsync(LobbyFlowContext ctx)
    {
        ctx.SetTextKey?.Invoke("Checking version");

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

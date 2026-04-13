using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

public sealed class VersionCheckStep : IIntroFlowStep
{
    public string Name => "VersionCheck";
    public float Weight => 0.18f;

    public bool CanRun(IntroFlowContext ctx)
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    public async UniTask<FlowResult> RunAsync(IntroFlowContext ctx)
    {
        ctx.SetTextKey?.Invoke("Checking version");

        VersionInfo info = await CheckVersionAsync();
        string current = Application.version;
        string serverVersion = info?.android_latest ?? info?.android_min ?? "";

        if (info == null)
        {
            return FlowResult.Continue;
        }

        if (info.maintenance)
        {
            Debug.Log("유지보수 모드");
            return FlowResult.Stop;
        }

        if (IsOlder(current, info.android_min))
        {
            Debug.Log("강제 업데이트");
            return FlowResult.Stop;
        }

        if (IsOlder(current, info.android_latest))
        {
            Debug.Log("업데이트 권유");
            return FlowResult.Continue;
        }

        Debug.Log("버전 OK");
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

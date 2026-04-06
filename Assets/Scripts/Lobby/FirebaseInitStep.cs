using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class FirebaseInitStep : ILobbyFlowStep
{
    public string Name => "FirebaseInit";
    public float Weight => 0.18f;

    public bool CanRun(LobbyFlowContext ctx)
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    public async UniTask<FlowResult> RunAsync(LobbyFlowContext ctx)
    {
        ctx.SetTextKey?.Invoke("Checking account");

        // 실제 초기화를 기다리는 형태(권장)
        bool ok = await FirebaseManager.Instance.InitializeAsync(ctx.UI.GetCancellationTokenOnDestroy());
        if (!ok)
        {
            Debug.LogWarning("Firebase init failed. Continue with limited features.");
            // 실패해도 로비는 진행시키는 정책이면 Continue
        }

        return FlowResult.Continue;
    }
}

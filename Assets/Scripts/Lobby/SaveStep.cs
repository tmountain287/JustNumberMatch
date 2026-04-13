using Common.Manager;
using Cysharp.Threading.Tasks;
using UI.Popup;
using Newtonsoft.Json;
using UnityEngine;

public sealed class SaveStep : IIntroFlowStep
{
    public string Name => "Save";
    public float Weight => 0.24f;

    public bool CanRun(IntroFlowContext ctx)
    {
        if(FirebaseManager.Instance!=null)
            return FirebaseManager.Instance.IsLinking && Application.internetReachability != NetworkReachability.NotReachable;
        return false;
    }

    static void LogFirstDiff(string a, string b)
    {
        if (a == null || b == null)
        {
            Debug.Log($"null compare: a={(a == null)} b={(b == null)}");
            return;
        }

        int min = Mathf.Min(a.Length, b.Length);
        for (int i = 0; i < min; i++)
        {
            if (a[i] != b[i])
            {
                Debug.Log($"DIFF at {i}: a='{a[i]}' (0x{((int)a[i]):X4}) vs b='{b[i]}' (0x{((int)b[i]):X4})");
                Debug.Log($"a snippet: {a.Substring(Mathf.Max(0, i - 10), Mathf.Min(50, a.Length - Mathf.Max(0, i - 10)))}");
                Debug.Log($"b snippet: {b.Substring(Mathf.Max(0, i - 10), Mathf.Min(50, b.Length - Mathf.Max(0, i - 10)))}");
                return;
            }
        }

        if (a.Length != b.Length)
            Debug.Log($"Same prefix, different length: a={a.Length} b={b.Length}");
        else
            Debug.Log("Strings are identical (Ordinal)");
    }

    public async UniTask<FlowResult> RunAsync(IntroFlowContext ctx)
    {
        async UniTask ContinueAfterSaveAsync()
        {
            // ctx.SetTextKey?.Invoke("VerifyingPurchaseRecovery");
            UserDataManager.RefreshAdsRewardGold();
            await NetworkManager.Instance.RecoveryPendingItem();
            await NetworkManager.Instance.SaveUserDataAsync(false);
        }

        Debug.Log(SystemInfo.deviceUniqueIdentifier);

        var result = await NetworkManager.Instance.SaveUserDataAsync(false);

        // 정책: null이면 멈출지/계속할지
        if (result == null)
            return FlowResult.Continue; // 또는 Stop

        string encryptedLocal = SecurePlayerPrefs.Encrypt(UserDataManager.UserData);

        if (result.Type == SaveResultType.Success)
        {
            await ContinueAfterSaveAsync();
            return FlowResult.Continue;
        }

        if (result.Type == SaveResultType.PermissionDenied && result.ConflictRecord != null)
        {
            LogFirstDiff(result.ConflictRecord, encryptedLocal);

            UserData beforeData =
                    JsonConvert.DeserializeObject<UserData>(SecurePlayerPrefs.Decrypt(result.ConflictRecord));

            string strBefore = JsonConvert.SerializeObject(beforeData);
            string strCurrent = JsonConvert.SerializeObject(UserDataManager.UserData);

            if (strBefore != strCurrent)
            {
                var tcs = new UniTaskCompletionSource();

                PopupManager.Instance.OpenPopup<SelectUserPopup>()
                    .Initialize(beforeData, UserDataManager.UserData, async (isCurrent) =>
                    {
                        await ContinueAfterSaveAsync();
                        tcs.TrySetResult();
                    });

                await tcs.Task;
                return FlowResult.Continue;
            }
            
            await ContinueAfterSaveAsync();
            return FlowResult.Continue;
        }
        
        await ContinueAfterSaveAsync();
        return FlowResult.Continue;
    }
}

using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class DailyIconWidget : MonoBehaviour
{
    [SerializeField] private string iconId = "DailyBonus";
    [SerializeField] private GameObject root;
    [SerializeField] private Text timerText;

    private DailyTimedIconController c;
    private CancellationTokenSource cts;

    void OnEnable()
    {
        c = DailyIconRegistry.Get(iconId);
        Loop().Forget();
    }

    void OnDisable()
    {
        cts?.Cancel();
    }

    private async UniTaskVoid Loop()
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();

        while (!cts.IsCancellationRequested)
        {
            bool active = c.IsActive();
            root.SetActive(active);

            if (active)
            {
                var t = c.GetRemainingTime();
                timerText.text = $"{(int)t.TotalHours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
            }

            await UniTask.Delay(1000, cancellationToken: cts.Token);
        }
    }
}

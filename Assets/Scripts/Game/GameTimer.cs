using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private Text timeText = null;

    [Header("Update Rate (Hz)")]
    [SerializeField, UnityEngine.Range(2, 60)] private int uiFps = 10; // UI 갱신 빈도(문자열 GC 절약)

    private SolveRunTimer timer;
    private float nextUiUpdate;

    public void SetTimer(SolveRunTimer _timer)
    {
        timer = _timer;
        if (timer != null)
        {
            timer.OnFinishedMs -= OnFinished;
            timer.OnFinishedMs += OnFinished;
        }
    }

    private void OnDisable()
    {
        if (timer != null)
            timer.OnFinishedMs -= OnFinished;
    }

    private void Update()
    {
        if (timer == null || timeText == null) return;

        if (Time.unscaledTime >= nextUiUpdate)
        {
            nextUiUpdate = Time.unscaledTime + (1f / uiFps);

            long ms = timer.IsRunning ? timer.ElapsedMs : timer.LastSolveMs;
            timeText.text = ms.FormatFromMs();
        }
    }

    private void OnFinished(SolveRunResultMs r)
    {
        if (timeText != null)
            timeText.text = r.TotalMs.FormatFromMs();
    }

    public void SetRemain(long remainMs)
    {
        timeText.text = remainMs.FormatFromMs();
    }
}

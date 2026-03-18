using System;
using UnityEngine;
using UnityEngine.UI;

public class MidnightCountdown : MonoBehaviour
{
    [SerializeField] private Text timerText;

    public Action onFinished; // 0초 도달 시 실행될 이벤트

    private TimeSpan remaining;

    private void Start()
    {
        UpdateRemainingTime();
        StartCoroutine(CountdownRoutine());
    }

    private void UpdateRemainingTime()
    {
        DateTime now = DateTime.Now;

        // 오늘 24:00 == 내일 00:00
        DateTime midnight = now.Date.AddDays(1);

        remaining = midnight - now;
    }

    private System.Collections.IEnumerator CountdownRoutine()
    {
        while (remaining.TotalSeconds > 0)
        {
            // 표시
            timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
                remaining.Hours, remaining.Minutes, remaining.Seconds);

            yield return new WaitForSeconds(1f);

            remaining = remaining.Subtract(TimeSpan.FromSeconds(1));
        }

        // 마지막 00:00:00 표시
        timerText.text = "00:00:00";

        // 이벤트 발생
        onFinished?.Invoke();

        Debug.Log("자정 카운트 완료!");
    }
}

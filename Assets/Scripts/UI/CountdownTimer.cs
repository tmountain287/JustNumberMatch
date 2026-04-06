using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class CountdownTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage;   // fillAmount 1 → 0
    [SerializeField] private Text timeText;     // 남은 1초 단위 텍스트 표시

    [Header("타이머")]
    [SerializeField] private float totalTime = 10f;

    public Action OnTimeOver;   // 0초 되었을 때 호출되는 이벤트

    private float remainTime;
    private bool isRunning;

    private void OnDisable()
    {
        fillImage.fillAmount = 1;
        timeText.text = totalTime.ToString();
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isRunning)
            return;

        remainTime -= Time.deltaTime;

        // 0 미만으로 내려가지 않게 고정
        if (remainTime < 0f)
            remainTime = 0f;

        UpdateUI();

        // 종료 체크
        if (remainTime <= 0f)
        {
            isRunning = false;
            OnTimeOver?.Invoke();
        }
    }

    public void SetTimer(float time, Action onTimeOver)
    {
        totalTime = time;
        remainTime = time;
        OnTimeOver = onTimeOver;
        isRunning = false;

        fillImage.fillAmount = 1;
        timeText.text = totalTime.ToString();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 타이머 시작
    /// </summary>
    public void StartTimer()
    {
        isRunning = true;
        UpdateUI();
    }

    /// <summary>
    /// UI 갱신 (fillAmount + 1초 단위 텍스트)
    /// </summary>
    private void UpdateUI()
    {
        // fillAmount 1 → 0
        fillImage.fillAmount = remainTime / totalTime;

        // 남은 시간 텍스트 (1초 단위 반올림)
        int displaySec = Mathf.CeilToInt(remainTime);
        timeText.text = displaySec.ToString();
    }
}

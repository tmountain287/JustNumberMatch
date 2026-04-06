using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

public class SliderTimer : MonoBehaviour
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private Slider slider;
    [SerializeField] private List<RectTransform> starRects;
    [SerializeField] private List<Image> starImages; // ★★★ 넣어두기

    public bool IsTimeUp { get => remainTime == 0; }

    private float totalTime;   // 최대 시간 (게이지 풀일 때)
    private float remainTime;  // 실제 남은 시간
    private Sequence timerSeq;
    /// <summary>각 별이 사라지는 남은 시간 임계(최대 시간 대비 0~100%).</summary>
    private List<int> starThresholdPercent = new();
    private List<bool> starConsumed = new();

    public System.Action OnTimeOver; // 다 떨어졌을 때 콜백(있으면)

    public int RemainingStarCount
    {
        get => starConsumed.Count(x => x == false);
    }


    /// <param name="time">최대 시간(초)</param>
    /// <param name="starThresholdPercents">별별 임계(0~100). 남은 시간이 time×%/100 이하일 때 해당 별 소모</param>
    public void InitTimer(float time, List<int> starThresholdPercents)
    {
        starThresholdPercent = starThresholdPercents;
        totalTime = time;
        remainTime = time;

        starConsumed.Clear();

        foreach (var img in starImages)
        {
            img.gameObject.SetActive(true);
            starConsumed.Add(false);
        }

        for (int i = 0; i < starThresholdPercent.Count; i++)
        {
            float p = Mathf.Clamp(starThresholdPercent[i], 0f, 100f);
            float w = rect.rect.width * (p / 100f);
            starRects[i].anchoredPosition = new Vector2(w, 0);
        }

        if (timerSeq != null && timerSeq.IsActive())
            timerSeq.Kill();

        slider.value = 1f;
    }

    // 처음 시작
    public void StartTimer()
    {
        StartCountdownSequence();
    }

    // 시간 추가 (게이지가 스르륵 차오르면서 다시 감소)
    public void AddTime(float addSec, float fillDuration = 0.25f)
    {
        // 이미 끝났으면 무시할지, 부활시킬지는 취향인데 일단 끝나면 무시
        if (remainTime <= 0f)
            return;

        if (timerSeq != null && timerSeq.IsActive())
            timerSeq.Kill();

        // 현재 위치 기준으로 남은 시간 재계산
        remainTime = slider.value * totalTime;

        // 남은 시간에 추가
        remainTime += addSec;
        // totalTime 이상 못 올라가게 하고 싶으면:
        remainTime = Mathf.Clamp(remainTime, 0f, totalTime);

        float currentValue = slider.value;
        float targetValue = remainTime / totalTime;

        timerSeq = DOTween.Sequence();

        // 1) 현재 value -> targetValue 까지 "채워지는" 트윈
        timerSeq.Append(
            slider.DOValue(targetValue, fillDuration)
                  .SetEase(Ease.OutCubic)
        );

        // 2) 다시 targetValue -> 0으로 줄어드는 카운트다운 트윈
        timerSeq.Append(
            slider.DOValue(0f, remainTime)
                  .SetEase(Ease.Linear)
                  .OnUpdate(() =>
                  {
                      // 슬라이더 비율 기준으로 남은 시간 계속 갱신
                      remainTime = slider.value * totalTime;
                  })
        );

        timerSeq.OnComplete(() =>
        {
            remainTime = 0f;
            slider.value = 0f;
            OnTimeOver?.Invoke();
        });
    }

    private void StartCountdownSequence()
    {
        if (timerSeq != null && timerSeq.IsActive())
            timerSeq.Kill();

        timerSeq = DOTween.Sequence();

        // 처음 시작은 그냥 1 -> 0 카운트다운만
        timerSeq.Append(
            slider.DOValue(0f, remainTime)
                  .SetEase(Ease.Linear)
                  .OnUpdate(() =>
                  {
                      remainTime = slider.value * totalTime;

                      // ⭐ 별 체크 로직 (남은 시간 ≤ 최대시간×%)
                      for (int i = 0; i < starThresholdPercent.Count; i++)
                      {
                          float p = Mathf.Clamp(starThresholdPercent[i], 0f, 100f);
                          float thresholdSec = totalTime * (p / 100f);
                          if (!starConsumed[i] && remainTime <= thresholdSec)
                          {
                              starConsumed[i] = true;
                              if (i < starImages.Count)
                                  PlayStarDisappearTween(starImages[i]);  // ⭐ 여기!
                          }
                      }
                  })
        );

        timerSeq.OnComplete(() =>
        {
            remainTime = 0f;
            slider.value = 0f;
            OnTimeOver?.Invoke();
        });
    }

    public void PlayStarDisappearTween(Image star)
    {
        star.transform.localScale = Vector3.one;

        Sequence seq = DOTween.Sequence();

        seq.Join(
            star.transform.DOScale(1.6f, 0.35f)   // 살짝 커지면서
                .SetEase(Ease.OutBack)
        );
        seq.Join(
            star.DOFade(0f, 0.35f)   // 알파 0까지 감소
        );

        seq.OnComplete(() =>
        {
            star.gameObject.SetActive(false); // 사라지면 비활성화
            star.color = new Color(star.color.r, star.color.g, star.color.b, 1f); // 알파 되돌리기 (나중에 재사용용)
            star.transform.localScale = Vector3.one; // 스케일도 초기화
        });
    }

    public void Pause()
    {
        if (timerSeq != null && timerSeq.IsActive())
            timerSeq.Pause();
    }

    public void Resume()
    {
        if (timerSeq != null && timerSeq.IsActive())
            timerSeq.Play();
    }
}

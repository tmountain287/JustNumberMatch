using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Common.UI;

public class InfiniteSliderTimer : MonoBehaviour
{
    [Header("Main Timer")]
    [SerializeField] private Slider mainSlider;

    public bool IsTimeUp => remainTime <= 0f;

    private float totalTime;
    private float remainTime;
    private float elapsedTime;

    private float decaySpeed = 1f;
    private Sequence mainSeq;

    [Header("Main Timer Speed")]
    [SerializeField] private float speedGrowthPerSec = 0.1f; // 시간 경과에 따른 가속도

    [Header("Combo Gauge")]
    [SerializeField] private Slider comboSlider;
    [SerializeField] private bool useComboGauge = true;

    // 난이도별 콤보 유지 시간
    [SerializeField] private float comboTimeEasy = 3f;
    [SerializeField] private float comboTimeNormal = 2.5f;
    [SerializeField] private float comboTimeHard = 2f;

    [SerializeField] private Text comboObj = null;
    [SerializeField] private Text comboText;

    private float comboTotalTime;
    private float comboRemainTime;
    private bool isComboRunning;
    private bool comboAlreadyNotified;

    [Header("Score UI")]
    [SerializeField] private NumberIncrement scoreText;

    // ★ Pause 상태
    private bool isPaused = false;
    public bool IsPaused => isPaused; // 필요하면 외부에서 읽기용

    public System.Action OnTimeOver;        // 메인 타이머 끝났을 때
    public System.Action OnComboTimeOver;   // 콤보 게이지 끝났을 때(콤보 끊기)

    public float RemainTime => remainTime;
    public float ComboRemainTime => comboRemainTime;
    /// <summary>콤보 게이지 현재 값 0~1 (콤보 이펙트 연출용)</summary>
    public float ComboGauge01 => (useComboGauge && comboTotalTime > 0f) ? Mathf.Clamp01(comboRemainTime / comboTotalTime) : 0f;
    public float GetElapsedPlayTime() => elapsedTime;

    private void ResetMainSeq()
    {
        if (mainSeq != null && mainSeq.IsActive())
            mainSeq.Kill();
        mainSeq = null;
    }

    private void Update()
    {
        // ★ 일시정지 중이면 타이머/콤보 갱신 안 함
        if (isPaused)
            return;

        UpdateMainTimer();
        UpdateComboGauge();
    }

    // =================== Main Timer ===================

    private void UpdateMainTimer()
    {
        if (IsTimeUp) return;

        elapsedTime += Time.deltaTime;

        // 점점 빨라지는 감소 속도
        decaySpeed = 1f + elapsedTime * speedGrowthPerSec;

        remainTime -= Time.deltaTime * decaySpeed;
        if (remainTime < 0f) remainTime = 0f;

        if (mainSlider != null && totalTime > 0f)
            mainSlider.value = remainTime / totalTime;

        if (remainTime <= 0f)
            OnTimeOver?.Invoke();
    }

    public void StartTimer(float startSec)
    {
        totalTime = startSec;
        remainTime = startSec;
        elapsedTime = 0f;
        decaySpeed = 1f;

        if (mainSlider != null)
            mainSlider.value = 1f;

        ResetMainSeq();
    }

    public void AddTime(float addSec, float fillDur = 0.4f)
    {
        float before = remainTime;
        remainTime = Mathf.Min(remainTime + addSec, totalTime);
        float after = remainTime;

        ResetMainSeq();

        if (mainSlider != null && totalTime > 0f)
        {
            mainSeq = DOTween.Sequence();
            mainSeq.Append(DOVirtual.Float(
                before / totalTime,
                after / totalTime,
                fillDur,
                v => mainSlider.value = v
            ).SetEase(DG.Tweening.Ease.OutCubic));
        }
    }

    public void ForceSetRemainTime(float sec)
    {
        remainTime = Mathf.Clamp(sec, 0f, totalTime);
        if (mainSlider != null && totalTime > 0f)
            mainSlider.value = remainTime / totalTime;
    }

    // =================== Combo Gauge ===================

    private void UpdateComboGauge()
    {
        if (!useComboGauge || !isComboRunning)
            return;

        comboRemainTime -= Time.deltaTime;
        if (comboRemainTime < 0f)
            comboRemainTime = 0f;

        if (comboSlider != null && comboTotalTime > 0f)
            comboSlider.value = comboRemainTime / comboTotalTime;

        // 한 번만 콜백 보내기
        if (comboRemainTime <= 0f && !comboAlreadyNotified)
        {
            comboAlreadyNotified = true;
            isComboRunning = false;

            // 🔥 콤보 게이지가 다 떨어지면 콤보 0으로 초기화
            SetCombo(0);

            OnComboTimeOver?.Invoke();
        }
    }


    /// <summary>
    /// 난이도에 따라 콤보 게이지를 새로 시작 (문제 클리어 시마다 호출 추천)
    /// </summary>
    public void StartComboGaugeByDifficulty(DifficultyType difficulty)
    {
        float sec = difficulty switch
        {
            DifficultyType.Easy => comboTimeEasy,
            DifficultyType.Normal => comboTimeNormal,
            DifficultyType.Hard => comboTimeHard,
            _ => comboTimeEasy,
        };

        StartComboGauge(sec);
    }

    /// <summary>
    /// 특정 시간(sec)으로 콤보 게이지 시작
    /// </summary>
    public void StartComboGauge(float sec)
    {
        if (!useComboGauge || comboSlider == null)
            return;

        comboTotalTime = Mathf.Max(0.01f, sec);
        comboRemainTime = comboTotalTime;
        isComboRunning = true;
        comboAlreadyNotified = false;

        comboSlider.gameObject.SetActive(true);
        comboSlider.value = 1f;
    }

    public void SetCombo(int combo)
    {
        if (comboText == null)
            return;
        
        comboText.text = $"x{combo}"; ;
        comboText.gameObject.SetActive(combo > 0);

        comboObj.color = combo > 0 ? Color.white : Color.gray;
        //// 예전처럼 숨기고 싶으면 아래 주석 복원
        //if (combo <= 0)
        //{
        //    comboText.gameObject.SetActive(false);
        //    comboText.text = "";
        //}
        //else
        //{
        //    comboText.gameObject.SetActive(true);
        //    comboText.text = combo.ToString();
        //}
    }

    /// <summary>
    /// 강제로 콤보 종료(콤보 끊을 때)
    /// </summary>
    public void StopComboGauge(bool invokeCallback = false)
    {
        isComboRunning = false;
        comboRemainTime = 0f;
        comboAlreadyNotified = true;
        // comboWarningActive = false;

        if (comboSlider != null)
            comboSlider.value = 0f;

        //  StopComboWarningFX();

        // 🔥 콤보 텍스트 숨기기
        SetCombo(0);

        if (invokeCallback)
            OnComboTimeOver?.Invoke();
    }

    // =================== Score Text ===================

    public void SetScore(long score, bool _isAni = true)
    {
        if (scoreText != null)
            scoreText.SetNumber(score, _isAni);
    }

    // =================== Pause / Resume ===================

    /// <summary>
    /// 메인 타이머 + 콤보 게이지 둘 다 일시정지
    /// </summary>
    public void Pause()
    {
        if (isPaused) return;
        isPaused = true;

        // ⛔ 여기서 mainSeq.Pause()는 더 이상 호출하지 않는다.
        // AddTime 연출은 그대로 재생되게 놔둠.
    }

    /// <summary>
    /// 메인 타이머 + 콤보 게이지 시간 감소 다시 진행
    /// </summary>
    public void Resume()
    {
        if (!isPaused) return;
        isPaused = false;

        // ⛔ 여기서 mainSeq.Play()도 호출하지 않는다.
    }
}

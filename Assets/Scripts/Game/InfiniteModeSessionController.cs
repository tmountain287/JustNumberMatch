using UnityEngine;
using Common.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JustOneMatch.UI;
using Common.UI;

public sealed class InfiniteModeSessionController : GameSessionController
{
    [Header("타이머 설정")]
    [SerializeField] private float startTimeSec = 120f;
    [SerializeField] private float maxTimeSec = 120f;

    [Header("난이도별 시간 보상")]
    [SerializeField] private float addEasy = 0.8f;
    [SerializeField] private float addNormal = 1.3f;
    [SerializeField] private float addHard = 2.0f;

    [Header("난이도 증가 기준")]
    [SerializeField] private int normalUnlock = 10;
    [SerializeField] private int hardUnlock = 30;

    [Header("점수 - 난이도별 기본점")]
    [SerializeField] private int baseScoreEasy = 100;
    [SerializeField] private int baseScoreNormal = 250;
    [SerializeField] private int baseScoreHard = 500;

    [Header("점수 - 콤보 보너스 (콤보 1당 추가 배율, 예: 0.08 = 8%)")]
    [SerializeField, Range(0.02f, 0.2f)] private float comboBonusPerStep = 0.08f;
    [SerializeField, Range(1f, 5f)] private float maxComboMultiplier = 3f;

    [Header("중복 방지")]
    [SerializeField] private int recentExcludeCount = 10;

    private InfiniteSliderTimer timer;
    private DifficultyType curDifficulty;

    private EquationTableData curEquation;
    private List<EquationTableData> recentEquations = new List<EquationTableData>();

    private int solvedCount;
    private int currentCombo;
    private int maxCombo;
    private long score;

    private int continueCount = 0;

    public override void ReadySession(IStageSequence seq)
    {
        continueCount = 0;
        GameMgr.Instance.OnStageCleared += OnStageCleared;

        timer = GameMgr.Instance.GameUI.GameTopUI.InfiniteSliderTimer;
        timer.OnTimeOver = OnTimeOver;
        

        timer.OnComboTimeOver = () =>
        {
            currentCombo = 0;
            timer.SetCombo(0);
        };

        solvedCount = 0;
        currentCombo = 0;
        maxCombo = 0;
        score = 0;
        recentEquations.Clear();
        timer.SetScore(score, false);
        timer.SetCombo(0);
        curDifficulty = DifficultyType.Easy;
        curEquation = TableDataManager.Instance.TableEquationData.GetRandomEquation(curDifficulty);
    }

    public override void StartSession()
    {       
        StartCoroutine(StartSessionFlow());
    }

    private IEnumerator StartSessionFlow()
    {
        UIManager.Instance.ActivateForSeconds(1.7f);
        GameMgr.Instance.SetEquation(curEquation);
        timer.StartTimer(startTimeSec);
        timer.StartComboGaugeByDifficulty(curEquation.difficultyType);
        timer.Pause();
        yield return new WaitForSeconds(1.7f);
        timer.Resume();
        
    }

    public override void StopSession()
    {
        GameMgr.Instance.OnStageCleared -= OnStageCleared;

        if (timer != null)
            timer.OnTimeOver = null;
    }

    private void OnStageCleared()
    {
        if (timer.IsTimeUp) return;

        solvedCount++;
        currentCombo++;
        float gauge01 = timer.ComboGauge01;
        timer.SetCombo(currentCombo);
        maxCombo = Mathf.Max(maxCombo, currentCombo);

        AddScoreForClear(curDifficulty, currentCombo);

        float bonus = GetTimeBonus(curDifficulty);
        timer.AddTime(bonus);

        if (timer.RemainTime > maxTimeSec)
            timer.ForceSetRemainTime(maxTimeSec);

        curDifficulty = DecideNextDifficulty();

        AddToRecent(curEquation);
        curEquation = TableDataManager.Instance.TableEquationData.GetRandomEquation(curDifficulty, recentEquations);
        timer.StartComboGaugeByDifficulty(curEquation.difficultyType);

        GameMgr.Instance.GameUI.GameTopUI.PlayComboEffectFromPool(gauge01);

        StartCoroutine(StageClearFlow());
    }

    private IEnumerator StageClearFlow()
    {
        UIManager.Instance.ActivateForSeconds(2.0f);
        timer.Pause();
        GameMgr.Instance.GameUI.OnCongrats();
        yield return new WaitForSeconds(2);
        GameMgr.Instance.SetEquation(curEquation);
        yield return new WaitForSeconds(1.7f);
        GameMgr.Instance.GameUI.GameTopUI.ReStage();
        timer.Resume();
    }

    public override void ChangeSequence()
    {
        // 혹시 돌고 있던 코루틴 있으면 정리
        StopAllCoroutines();

        // 매치스틱 원위치
        GameMgr.Instance.ReStoreMatchStick();

        // 타이머 / 콤보 / 점수 초기화
        if (timer != null)
        {
            timer.Pause();                // 혹시 돌고있으면 일단 정지
            timer.StartTimer(startTimeSec);  // 처음 시간으로 리셋

            timer.SetCombo(0);
            timer.OnComboTimeOver = () =>
            {
                currentCombo = 0;
                timer.SetCombo(0);
            };

            score = 0;
            timer.SetScore(score, false);
        }

        solvedCount = 0;
        currentCombo = 0;
        maxCombo = 0;

        // 난이도/문제 다시 뽑기
        curDifficulty = DifficultyType.Easy;
        AddToRecent(curEquation);
        curEquation = TableDataManager.Instance.TableEquationData.GetRandomEquation(curDifficulty, recentEquations);

        // 새 문제 세팅 + 콤보 게이지 시작
        GameMgr.Instance.SetEquation(curEquation);
        if (timer != null)
            timer.StartComboGaugeByDifficulty(curEquation.difficultyType);

        // 필요하다면 여기서 다시 진행 시작 (타이머 재생)
        if (timer != null)
            timer.Resume();
    }

    private void AddToRecent(EquationTableData eq)
    {
        if (eq == null) return;
        recentEquations.Add(eq);
        while (recentEquations.Count > recentExcludeCount)
            recentEquations.RemoveAt(0);
    }

    private DifficultyType DecideNextDifficulty()
    {
        if (solvedCount < normalUnlock)
            return DifficultyType.Easy;

        if (solvedCount < hardUnlock)
        {
            return UnityEngine.Random.value < 0.6f ?
                DifficultyType.Easy : DifficultyType.Normal;
        }

        float r = UnityEngine.Random.value;
        if (r < 0.2f) return DifficultyType.Easy;
        if (r < 0.7f) return DifficultyType.Normal;
        return DifficultyType.Hard;
    }

    private void OnTimeOver()
    {
        GameMgr.Instance.OnStageCleared -= OnStageCleared;
        GameMgr.Instance.ReStoreMatchStick();

        float playSec = timer.GetElapsedPlayTime();
        timer.Pause();

        Action resultAction = () =>
        {
            GameAnalyticsHelper.LogSurvivalSessionEnd(score, solvedCount, playSec);
            long bestScore = UserDataManager.UserData.infiniteBestScore;
            bool isNewRecord = UserDataManager.UpdateInfiniteRecord(score);            

            PopupManager.Instance.OpenPopup<InfiniteResultPopup>()
                .Initialize(score,
                    bestScore,
                    isNewRecord,
                    () => GameMgr.Instance.StartInfiniteMode(),
                    () => UIManager.Instance.ShowUI(BaseUI.Type.STAGE)
                );
        };

        if (continueCount == 0)
        {
            PopupManager.Instance.OpenPopup<InfiniteAdsPopup>().Initialize(() =>
            {
                UIManager.Instance.ShowRewardedAd((adapter) =>
                {
                    continueCount++;
                    GameMgr.Instance.OnStageCleared += OnStageCleared;

                    // "게이지 50%" = 시작 시간의 50% 만큼 시간 추가
                    float reviveTime = startTimeSec * 0.5f;
                    timer.ForceSetRemainTime(reviveTime);
                    timer.StartComboGaugeByDifficulty(curEquation.difficultyType);
                    timer.Resume();
                }, () => { resultAction?.Invoke(); }, "continue");
            }, () =>
            {
                resultAction?.Invoke();
            });
        }
        else
        {
            resultAction?.Invoke();
        }
    }

    /// <summary>난이도 기본점 × 콤보 배율로 한 번에 점수 추가 (자연스러운 서바이벌 점수)</summary>
    private void AddScoreForClear(DifficultyType diff, int combo)
    {
        int baseScore = diff switch
        {
            DifficultyType.Easy => baseScoreEasy,
            DifficultyType.Normal => baseScoreNormal,
            DifficultyType.Hard => baseScoreHard,
            _ => baseScoreEasy
        };

        // 콤보 1 = 1배, 콤보 N = 1 + (N-1) * comboBonusPerStep (최대 maxComboMultiplier)
        float comboMultiplier = 1f + (combo - 1) * comboBonusPerStep;
        comboMultiplier = Mathf.Min(comboMultiplier, maxComboMultiplier);

        long earned = (long)(baseScore * comboMultiplier);
        score += earned;
        timer.SetScore(score);
    }

    private float GetTimeBonus(DifficultyType d)
    {
        return d switch
        {
            DifficultyType.Easy => addEasy,
            DifficultyType.Normal => addNormal,
            DifficultyType.Hard => addHard,
            _ => addEasy
        };
    }
}

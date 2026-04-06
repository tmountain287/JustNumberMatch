using Common.Manager;
using Common.UI;
using GoogleMobileAds.Api;
using JustOneMatch.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public readonly struct SolveRunResultMs
{
    public readonly long TotalMs;        // 마지막 정답 시점까지 총 경과 시간(ms)
    public readonly double AvgMs;          // 문제 1개 평균 시간(ms)
    public readonly long MaxMs;          // 가장 오래 걸린 문제(ms)
    public readonly long MinMs;          // 가장 빨리 푼 문제(ms)
    public readonly long[] PerProblemMs;   // 각 문제별 소요 시간(ms)

    public SolveRunResultMs(long totalMs, double avgMs, long maxMs, long minMs, long[] perProblemMs)
    {
        TotalMs = totalMs;
        AvgMs = avgMs;
        MaxMs = maxMs;
        MinMs = minMs;
        PerProblemMs = perProblemMs ?? Array.Empty<long>();
    }

    public override string ToString()
        => $"Total={TotalMs}ms, Avg={AvgMs:F1}ms, Max={MaxMs}ms, Min={MinMs}ms, Count={PerProblemMs.Length}";
}

public sealed class SolveRunTimer
{
    private readonly Stopwatch _sw = new Stopwatch();
    private readonly List<long> _perProblemMs = new List<long>();

    private long _lastSplitMs = 0; // 직전 스플릿 시각(ms)
    private long _lastSolveMs = 0; // 마지막 정답 시각(ms)
    private long _externAddedMs = 0; // 백그라운드 등 외부에서 더해준 시간(ms)

    // 항상 "실제 게임에서 흐른 시간" = 스톱워치 + 외부추가
    private long NowMs => _sw.ElapsedMilliseconds + _externAddedMs;

    public bool IsRunning => _sw.IsRunning;
    public long ElapsedMs => NowMs;     // 표시용
    public long LastSolveMs => _lastSolveMs; // 총 시간(ms) - Finish에서 사용

    public event Action<long> OnSolvedMs;
    public event Action<SolveRunResultMs> OnFinishedMs;

    public void StartRun()
    {
        _perProblemMs.Clear();
        _lastSplitMs = 0;
        _lastSolveMs = 0;
        _externAddedMs = 0;

        _sw.Reset();
        _sw.Start();
    }

    public void Pause() { if (_sw.IsRunning) _sw.Stop(); }
    public void Resume() { if (!_sw.IsRunning) _sw.Start(); }

    public void Reset()
    {
        _sw.Reset();
        _perProblemMs.Clear();
        _lastSplitMs = 0;
        _lastSolveMs = 0;
        _externAddedMs = 0;
    }

    /// <summary>타임가드 등으로 측정한 백그라운드 시간(ms)을 더해준다.</summary>
    public void AddExternalElapsed(long ms)
    {
        if (ms <= 0) return;
        _externAddedMs += ms;
    }

    public void MarkSolved()
    {
        if (!_sw.IsRunning) return;

        long tMs = NowMs;
        long delta = Math.Max(0, tMs - _lastSplitMs);

        _perProblemMs.Add(delta);
        _lastSplitMs = tMs;
        _lastSolveMs = tMs;

        OnSolvedMs?.Invoke(delta);
    }

    public SolveRunResultMs Finish()
    {
        long total = _lastSolveMs > 0 ? _lastSolveMs : NowMs;

        _sw.Stop();

        int n = _perProblemMs.Count;
        double avg = (n > 0) ? _perProblemMs.Average(x => (double)x) : 0.0;
        long max = (n > 0) ? _perProblemMs.Max() : 0;
        long min = (n > 0) ? _perProblemMs.Min() : 0;

        var result = new SolveRunResultMs(total, avg, max, min, _perProblemMs.ToArray());
        OnFinishedMs?.Invoke(result);
        return result;
    }
}

public sealed class TimeAttackSessionController : GameSessionController
{
    private const string KEY_TIMEATTACK_SESSION = "TIMEATTACK_SESSION";

    private IStageSequence seq;
    private SolveRunTimer timer = new();

    private DifficultyType difficultyType;

    public override void ReadySession(IStageSequence sequence)
    {
        seq = sequence;
        StartSession();
    }

    public override void StartSession()
    {  
        GameMgr.Instance.OnStageCleared += OnStageCleared;

        seq.Reset();
        seq.MoveNext();                 // 단일이든 리스트든 현재 1판을 가리킴
        GameMgr.Instance.SetEquation(seq.Current);
        difficultyType = seq.Current.difficultyType;
        GameMgr.Instance.GameUI.GameTopUI.ClearStageState.SetTimeAttackState(seq.Count);
        GameMgr.Instance.GameUI.GameTopUI.GameTimer.SetTimer(timer);

        TimeGuardManager.Instance.SaveNow(KEY_TIMEATTACK_SESSION);
        timer.StartRun();
    }

    public override void ChangeSequence()
    {
        var listSeq = seq as ListStageSequence;
        if (listSeq == null)
        {
            UnityEngine.Debug.LogWarning("[Change] ListStageSequence 아님");
            return;
        }

        var currentList = listSeq.List; // 현재 3개 (혹은 count개)
        if (currentList == null || currentList.Count == 0)
        {
            UnityEngine.Debug.LogWarning("[Change] 현재 문제 리스트 비어있음");
            return;
        }

        // 현재 스테이지가 리스트에서 몇 번째인지 찾기
        int currentIndex = currentList.IndexOf(seq.Current);
        if (currentIndex < 0)
        {
            // 못 찾으면 일단 0번으로
            currentIndex = 0;
        }

        // 현재 3개(혹은 count개)를 excludeList로 넘겨서, 같은 범위에서 1개만 새로 뽑기
        var newSeq = GameMgr.Instance.BuildRandomRangeFrom(
            difficultyType,           
            1,
            currentList) as ListStageSequence;

        if (newSeq == null || newSeq.List == null || newSeq.List.Count == 0)
        {
            UnityEngine.Debug.LogWarning("[Change] 교체 후보 없음");
            return;
        }

        var newStage = newSeq.List[0];

        // 현재 인덱스에 새 문제로 교체
        currentList[currentIndex] = newStage;

        // seq.Current가 리스트를 바라보고 있다면, 이 교체만으로도 내부 참조가 바뀜.
        // 바로 새 스테이지로 UI 갱신
        GameMgr.Instance.SetEquation(newStage);
    }
  
    public override void StopSession()
    {
        //gm.StageCleared -= OnStageCleared;
        GameMgr.Instance.OnStageCleared -= OnStageCleared;

    }

    private void OnStageCleared()
    {
        GameMgr.Instance.GameUI.GameTopUI.ClearStageState.OnIcon(seq.Index);
        timer.MarkSolved();

        if (!seq.MoveNext()) // 10개 모두 끝!
        {
            SolveRunResultMs result = timer.Finish();

            var record = UserDataManager.SetTimeAttackRecord(difficultyType, result.TotalMs);
            int clearCount = result.PerProblemMs?.Length ?? 0;
            float durationSec = result.TotalMs / 1000f;
            GameAnalyticsHelper.LogTimeAttackComplete(difficultyType.ToString().ToLower(), 0, clearCount, durationSec, record.Item1);
            StartCoroutine(ResultFlow(result, record.Item1, record.Item2));
        }
        else
        {
            StartCoroutine(StageClearFlow());
        }
    }

    private IEnumerator StageClearFlow()
    {
        UIManager.Instance.ActivateForSeconds(2.0f);
        timer.Pause();
        GameMgr.Instance.GameUI.OnCongrats();
        yield return new WaitForSeconds(2);
        GameMgr.Instance.SetEquation(seq.Current);
        yield return new WaitForSeconds(1.7f);
        GameMgr.Instance.GameUI.GameTopUI.ReStage();
        timer.Resume();
    }

    private IEnumerator ResultFlow(SolveRunResultMs _result, bool _isBest, long _preBest)
    {
        UIManager.Instance.ActivateForSeconds(1.7f);
        yield return new WaitForSeconds(0);
       // bool reward = UserDataManager.ClearStage(seq.Current);
        
        PopupManager.Instance.OpenPopup<TimeAttackResultPopup>().Initialize(difficultyType, _result, _isBest, _preBest, () =>
        {
            int needCount = ConfigData.NeedTimeAttckTicketCountDic[difficultyType];
            if (UserDataManager.GetItemCount(ItemType.TimeAttackTicket) < needCount)
            {
                PopupManager.Instance.OpenMessageBoxPopup("", LocalizationManager.Instance.GetText("NotEnoughTicket"));
            }
            else
            {
                UserDataManager.SubItemCount(ItemType.TimeAttackTicket, needCount);
                UserDataManager.Save(_onComplete: ()=>
                {
                    GameMgr.Instance.StartTimeAttack(difficultyType);
                });                
            }
        },
        () =>
        {
            UIManager.Instance.ShowUI(BaseUI.Type.TIMEATTACT);
        });
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            // 타이머가 실제로 돌고 있을 때만 저장
            if (timer != null && timer.IsRunning && seq != null)
            {
                TimeGuardManager.Instance.SaveNow(KEY_TIMEATTACK_SESSION);
                timer.Pause();    // 스톱워치는 멈추고
            }
        }
        else
        {
            // 복귀 시: 방금까지의 경과 ms를 타이머에 더해줌
            if (timer != null && !timer.IsRunning && seq != null)
            {
                bool suspicious;
                long elapsedMs = TimeGuardManager.Instance.GetElapsedMs(KEY_TIMEATTACK_SESSION, out suspicious);
                double sec = elapsedMs / 1000.0;

                UnityEngine.Debug.Log(
                    $"[TimeAttack] 앱 백그라운드 동안 지난 시간: {sec:F1}초 (조작의심: {suspicious})"
                );

                // 조작 의심일 때 별도 처리하고 싶으면 여기서
                // if (suspicious) { 강제 실패 처리 / 팝업 등 }

                // 정상/의심 여부와 상관없이 "흐른 시간"은 타이머에 반영
                timer.AddExternalElapsed(elapsedMs);
                timer.Resume();
            }
        }
    }
}
using Common.Manager;
using JustOneMatch.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public sealed class CountdownSolveRunTimer
{
    private readonly Stopwatch _sw = new Stopwatch();
    private readonly List<long> _perProblemMs = new List<long>();
    private long _lastSplitMs = 0;
    private long _lastSolveMs = 0;

    private MonoBehaviour _host;
    private Coroutine _tickRoutine;

    public long LimitMs { get; private set; }    // 총 제한(ms)
    public long ElapsedMs => _sw.ElapsedMilliseconds;
    public long RemainingMs => Math.Max(0, LimitMs - ElapsedMs);
    public bool IsRunning => _sw.IsRunning;
    public bool IsTimeUp => RemainingMs <= 0;

    public event Action<long> OnSolvedMs;               // 방금 푼 문제 소요(ms)
    public event Action<SolveRunResultMs> OnFinishedMs; // 런 종료 시 결과
    public event Action<long> OnTickMs;                 // UI 갱신용 남은 시간(ms)
    public event Action OnTimeUp;                       // 시간 초과 순간

    /// <summary>호스트(MonoBehaviour)는 코루틴 틱용으로 필요</summary>
    public void StartRun(MonoBehaviour host, long limitMs, float tickIntervalSec = 0.0001f)
    {
        _host = host;
        LimitMs = Math.Max(0, limitMs);

        _perProblemMs.Clear();
        _lastSplitMs = 0;
        _lastSolveMs = 0;
        _sw.Reset();
        _sw.Start();

        StopTickRoutineIfAny();
        if (_host != null && tickIntervalSec > 0f)
            _tickRoutine = _host.StartCoroutine(TickLoop(tickIntervalSec));
    }

    public void Pause()
    {
        if (_sw.IsRunning) _sw.Stop();
    }

    public void Resume()
    {
        if (!_sw.IsRunning) _sw.Start();
    }

    public void Reset()
    {
        StopTickRoutineIfAny();
        _sw.Reset();
        _perProblemMs.Clear();
        _lastSplitMs = 0;
        _lastSolveMs = 0;
        LimitMs = 0;
    }

    public void MarkSolved()
    {
        if (!_sw.IsRunning) return;

        long tMs = _sw.ElapsedMilliseconds;
        long delta = Math.Max(0, tMs - _lastSplitMs);

        _perProblemMs.Add(delta);
        _lastSplitMs = tMs;
        _lastSolveMs = tMs;

        OnSolvedMs?.Invoke(delta);

        // 혹시 이 시점에서 시간초과면 즉시 종료
        if (IsTimeUp)
        {
            OnTimeUp?.Invoke();
            Finish(); // 결과 발행
        }
    }

    public SolveRunResultMs Finish()
    {
        StopTickRoutineIfAny();

        long cappedElapsed = Math.Min(_sw.ElapsedMilliseconds, LimitMs);
        _sw.Stop();

        int n = _perProblemMs.Count;
        double avg = (n > 0) ? _perProblemMs.Average(x => (double)x) : 0.0;
        long max = (n > 0) ? _perProblemMs.Max() : 0;
        long min = (n > 0) ? _perProblemMs.Min() : 0;

        var result = new SolveRunResultMs(
            totalMs: cappedElapsed,
            avgMs: avg,
            maxMs: max,
            minMs: min,
            perProblemMs: _perProblemMs.ToArray()
        );

        OnFinishedMs?.Invoke(result);
        return result;
    }

    private IEnumerator TickLoop(float tickIntervalSec)
    {
        var wait = new WaitForSecondsRealtime(tickIntervalSec);
        while (true)
        {
            if (_sw.IsRunning)
            {
                long remain = RemainingMs;
                OnTickMs?.Invoke(remain);

                if (remain <= 0)
                {
                    OnTimeUp?.Invoke();
                    Finish();  // 자동 종료
                    yield break;
                }
            }
            yield return wait;
        }
    }

    private void StopTickRoutineIfAny()
    {
        if (_host != null && _tickRoutine != null)
        {
            _host.StopCoroutine(_tickRoutine);
            _tickRoutine = null;
        }
    }
}

public sealed class BossStageSessionController : GameSessionController
{
    private IStageSequence seq;  
    private DifficultyType difficultyType;

    public override void ReadySession(IStageSequence sequence)
    {
        seq = sequence;
        GameMgr.Instance.OnStageCleared += OnStageCleared;

        seq.Reset();
        seq.MoveNext();


        difficultyType = seq.Current.difficultyType;

        // 난이도별 보스 최대 제한 시간(초)
        float bossMaxSec = ConfigData.BossTimeDic[difficultyType];

        // UI에 카운트다운 타이머 연결 (남은 시간 표시)
        GameMgr.Instance.GameUI.GameTopUI.ClearStageState.SetTimeAttackState(seq.Count); // 기존 API 유지
        //GameMgr.Instance.GameUI.GameTopUI.ClearStageState.BindCountdown(timer); // 남은 시간 바인딩(메서드 추가 가정)
        GameMgr.Instance.GameUI.GameTopUI.SliderTimer.InitTimer(bossMaxSec, ConfigData.BossTimeStarDic[difficultyType]);

    }

    public override void StartSession()
    {
        GameMgr.Instance.SetEquation(seq.Current);
        GameMgr.Instance.GameUI.GameTopUI.SliderTimer.StartTimer();
        GameMgr.Instance.GameUI.GameTopUI.SliderTimer.OnTimeOver = () =>
        {
            //tartCoroutine(ResultFlow_TimeUp());
            GameMgr.Instance.ReStoreMatchStick();
            GameMgr.Instance.GameUI.GameTopUI.SliderTimer.OnTimeOver = null;
            PopupManager.Instance.OpenPopup<StageFailedPopup>().Initialize(()=>GameMgr.Instance.StartStageMode(StageTableData));
        };
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
            StageTableData.randomStartID,
            StageTableData.randomEndID,
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
        GameMgr.Instance.GameUI.GameTopUI.SliderTimer.OnTimeOver = null;
        GameMgr.Instance.OnStageCleared -= OnStageCleared;
    }

    private void OnStageCleared()
    {
        GameMgr.Instance.GameUI.GameTopUI.ClearStageState.OnIcon(seq.Index);       

        if (GameMgr.Instance.GameUI.GameTopUI.SliderTimer.IsTimeUp) return; // 타임업이면 ResultFlow_TimeUp에서 처리됨

        if (!seq.MoveNext()) // 모든 문제 종료(시간 내 클리어)
        {
            GameMgr.Instance.GameUI.GameTopUI.SliderTimer.Pause();

            StageComplete();
        }
        else
        {
            StartCoroutine(StageClearFlow());
        }
    }

    private IEnumerator StageClearFlow()
    {
        UIManager.Instance.ActivateForSeconds(2.0f);
        GameMgr.Instance.GameUI.GameTopUI.SliderTimer.Pause();
        GameMgr.Instance.GameUI.OnCongrats();
        yield return new WaitForSeconds(2);
        GameMgr.Instance.SetEquation(seq.Current);
        yield return new WaitForSeconds(1.7f);
        GameMgr.Instance.GameUI.GameTopUI.ReStage();
        GameMgr.Instance.GameUI.GameTopUI.SliderTimer.Resume();
    }

    private void StageComplete()
    {
        int level = UserDataManager.Level;
        int xp = UserDataManager.XP;

        GameMgr.Instance.GameUI.GoldStateBox.SetBlockAutoUpdate(true);
        GameMgr.Instance.GameUI.SkillItemButtonList.ForEach(x => x.SetBlockAutoUpdate(true));

        var clearResult = UserDataManager.ClearStage(StageTableData, GameMgr.Instance.GameUI.GameTopUI.SliderTimer.RemainingStarCount);

        Action nextAction = null;
        Action exitAction = null;
        Action popupAction = null;

        popupAction = () =>
        {
            PopupManager.Instance.OpenPopup<ResultPopup>().Initialize(clearResult.Item1, StageTableData,  clearResult.Item2, level, xp, clearResult.Item3, GameMgr.Instance.GameUI.GameTopUI.SliderTimer.RemainingStarCount, () =>
            {
                GameMgr.Instance.StartStageMode(StageTableData);
            }, nextAction, exitAction);
        };

        StageTableData nextPlayable = StageNextPlayableHelper.FindNextPlayableStage(StageTableData);
        if (nextPlayable != null)
        {
            nextAction = () => GameMgr.Instance.StartStageMode(nextPlayable);
        }
        
        exitAction = () =>
        {
            UIManager.Instance.ShowUI(Common.UI.BaseUI.Type.STAGE);
            PopupManager.Instance.OpenPopup<StagePopup>().Initialize(StageTableData.difficultyType, StageTableData.id);
        };
        

        if (clearResult.Item1)
        {
            GameAnalyticsHelper.LogBossStageComplete(difficultyType.ToString().ToLower(), StageTableData.id, true);
            GameAnalyticsHelper.SetMaxStageCleared(UserDataManager.UserData.clearStageInfoDic[StageTableData.difficultyType]);
            UserDataManager.Save(_onComplete: () =>
            {
                popupAction.Invoke();
            });
        }
        else
        {
            popupAction.Invoke();
        }
    }

    private IEnumerator ResultFlow_TimeUp()
    {
        yield return new WaitForSeconds(1f);

        //// 타임업 전용 팝업(없다면 기존 팝업에 "Time Up" 플래그로 처리)
        //PopupManager.Instance.OpenPopup<TimeAttackResultPopup>().InitializeTimeUp(
        //    result,
        //    onRetry: () =>
        //    {
        //        int needCount = ConfigData.NeedTimeAttckTicketCountDic[difficultyType];
        //        if (UserDataManager.GetItemCount(ItemType.TimeAttackTicket) < needCount)
        //        {
        //            PopupManager.Instance.OpenMessageBoxPopup("알림", "티켓이 부족합니다.");
        //        }
        //        else
        //        {
        //            UserDataManager.SubItemCount(ItemType.TimeAttackTicket, needCount);
        //            UserDataManager.Save(true);
        //            GameMgr.Instance.StartTimeAttack(difficultyType);
        //        }
        //    },
        //    onExit: () =>
        //    {
        //        UIManager.Instance.ShowUI(BaseUI.Type.TIMEATTACT);
        //    }
        //);
    }
}


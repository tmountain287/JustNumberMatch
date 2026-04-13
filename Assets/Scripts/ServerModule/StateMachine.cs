using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    private static readonly Dictionary<EGoStopEventType, ServerModuleState> _states = new()
    {
        { EGoStopEventType.EGSEVT_FIRE_MATCH, new FireMatchState() },
        { EGoStopEventType.EGSEVT_INVALID_GAME, new InvalidGameState() },
        { EGoStopEventType.EGSEVT_NEXT_LEVEL, new NextLevelState() },

        { EGoStopEventType.EGSEVT_INITIALIZE, new InitializeState() },
        { EGoStopEventType.EGSEVT_SHUFFLE_CARD, new ShuffleState() },
        { EGoStopEventType.EGSEVT_DIVIDE_CARD, new DivideCardState() },
        { EGoStopEventType.EGSEVT_CHANGE_TURN, new ChangeTurnState() },
        { EGoStopEventType.EGSEVT_CHANGE_BOARD_CARD, new ChangeBoardCardState() },

        { EGoStopEventType.EGSEVT_SKILL_CARD, new SkillCardState () },

        { EGoStopEventType.EGSEVT_GENERAL_HIT_CARD, new GneneralHitCardState () },
        { EGoStopEventType.EGSEVT_BOMB_HIT_CARD, new BombHitCardState() },
        { EGoStopEventType.EGSEVT_DDADDAK_BOMB_HIT_CARD, new DdaddakBombHitCardState() },
        { EGoStopEventType.EGSEVT_BBUKSWEEP_HIT_CARD, new BbukSweepHitCardState() },
        { EGoStopEventType.EGSEVT_BONUS_HIT_CARD, new BonusHitCardState() },
        { EGoStopEventType.EGSEVT_EMPTY_BOMB_HIT_CARD, new EmptyBombHitCardState() },        
        { EGoStopEventType.EGSEVT_SELECT_SHAKE_CARD, new SelectShakeCardState() },
        { EGoStopEventType.EGSEVT_SHAKE, new ShakeState() },

        { EGoStopEventType.EGSEVT_GENERAL_FLIP_CARD, new GeneralFlipCardState() },
        { EGoStopEventType.EGSEVT_BBUK_FLIP_CARD, new BbukFlipCardState() },
        { EGoStopEventType.EGSEVT_BBUKSWEEP_FLIP_CARD, new BbukSweepFlipCardState() },
        { EGoStopEventType.EGSEVT_BONUS_FLIP_CARD, new BonusFlipCardState() },
        { EGoStopEventType.EGSEVT_DDADDK_FLIP_CARD, new DdaDakFlipCardState() },
        { EGoStopEventType.EGSEVT_JJOK_FLIP_CARD, new JJokFlipCardState() },
        { EGoStopEventType.EGSEVT_SELECT_CARD, new SelectCardState() },
        { EGoStopEventType.EGSEVT_BOARDSWEEP, new BoardSweepState() },
        { EGoStopEventType.EGSEVT_DRAG_CARD, new DragCardState() },

        { EGoStopEventType.EGSEVT_BBUK_REWARD, new BbukRewardState() },

        { EGoStopEventType.EGSEVT_SELECT_GUKJUN_CARD, new SelectGukjunState() },
        { EGoStopEventType.EGSEVT_GUKJUN_TO_PEE, new GukjunToPeeState() },

        { EGoStopEventType.EGSEVT_NAGARI, new NagariState() },
        { EGoStopEventType.EGSEVT_BOARD_PRESIDENT, new BoardPresidentState() },
        { EGoStopEventType.EGSEVT_SELECT_PRESIDENT, new SelectPresidentState() },
        { EGoStopEventType.EGSEVT_PLAYER_PRESIDENT, new PlayerPresidentState() },
        { EGoStopEventType.EGSEVT_SAME_PLAYER_PRESIDENT, new SamePlayerPresidentState() },
        { EGoStopEventType.EGSEVT_RESULT, new ResultState() },
        { EGoStopEventType.EGSEVT_BANKRUPTCY, new BankruptcyState() },
        { EGoStopEventType.EGSEVT_REVIVAL, new RevivalState() },

        { EGoStopEventType.EGSEVT_SELECT_GOSTOP, new SelectGoState() },
        { EGoStopEventType.EGSEVT_GO, new GoState() },
        { EGoStopEventType.EGSEVT_STOP, new StopState() },

        { EGoStopEventType.EGSEVT_ROUND_MISSION, new RoundMissionState() },
        { EGoStopEventType.EGSEVT_ROUND_MISSION_SUCCESS, new RoundMissionSuccessState() },
        { EGoStopEventType.EGSEVT_ROUND_MISSION_FAIL, new RoundMissionFailState() },
    };

    private ServerModuleState _currentState;
    private Coroutine coroutine = null;
    //private void Start()
    //{
    //    _currentState = _states[EGoStopEventType.EGSEVT_INITIALIZE];
    //    StartCoroutine(HandleEvent(EGoStopEventType.EGSEVT_INITIALIZE));
    //}

    public bool IsCurrentState(EGoStopEventType eventType)
    {
        return _states.TryGetValue(eventType, out var state) && _currentState == state;
    }

    public void SetState(EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        Debug.Log(eventType.ToString());
        StopCoroutine();
        if (_states.TryGetValue(eventType, out var newState))
        {
            _currentState = newState;
            coroutine = StartCoroutine(HandleEvent(eventType, goStopEvent));
        }
        else
        {
            Debug.Log("Unknown event");
        }
    }

    public IEnumerator HandleEvent(EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        yield return StartCoroutine(_currentState.Handle(this, eventType, goStopEvent));
    }

    public void StopCoroutine()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusHitCardState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_BONUS_HIT_CARD)
        {
            // 보너스 카드 내기 로직 추가
            CNetDocument.InGame?.PushEvent(goStopEvent);
            yield return new WaitForSeconds(GameConstants.HitDuration + GameConstants.FlipDragDuration + GameConstants.HitBonusDelay + GameConstants.MoveDuration + 0.3f);
            machine.SetState(EGoStopEventType.EGSEVT_CHANGE_TURN);
        }
        yield return null;
    }
}

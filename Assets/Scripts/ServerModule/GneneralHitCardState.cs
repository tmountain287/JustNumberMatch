using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GneneralHitCardState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_GENERAL_HIT_CARD)
        {
            CNetDocument.InGame?.PushEvent(goStopEvent);
            yield return new WaitForSeconds(GameConstants.HitToNextDuration);
            InGameManager.Instance.FlipCard();
        }
        yield return null;
    }
}

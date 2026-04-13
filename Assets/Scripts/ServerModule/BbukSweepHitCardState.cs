using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BbukSweepHitCardState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_BBUKSWEEP_HIT_CARD)
        {
            CNetDocument.InGame?.PushEvent(goStopEvent);
            //뻑먹기 연출
            yield return new WaitForSeconds(1f);
            InGameManager.Instance.FlipCard();
        }
        yield return null;
    }
}

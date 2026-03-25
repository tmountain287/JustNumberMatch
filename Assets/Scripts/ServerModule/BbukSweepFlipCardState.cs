using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BbukSweepFlipCardState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_BBUKSWEEP_FLIP_CARD)
        {
            CNetDocument.InGame?.PushEvent(goStopEvent);
            //뻑 먹는 연출 타임
            yield return new WaitForSeconds(1f);
            machine.SetState(EGoStopEventType.EGSEVT_SELECT_CARD);
        }
        yield return null;
    }
}

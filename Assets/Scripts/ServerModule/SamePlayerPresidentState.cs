using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SamePlayerPresidentState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_SAME_PLAYER_PRESIDENT)
        {          
            CNetDocument.InGame?.PushEvent(goStopEvent);
            yield return new WaitForSeconds(2f);
            machine.SetState(EGoStopEventType.EGSEVT_NAGARI);
        }
    }
}
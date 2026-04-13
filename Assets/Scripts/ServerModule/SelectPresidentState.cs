using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SelectPresidentState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_SELECT_PRESIDENT)
        {          
            CNetDocument.InGame?.PushEvent(goStopEvent);
        }
        yield return null;
    }
}
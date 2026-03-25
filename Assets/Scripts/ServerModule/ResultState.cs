using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ResultState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_RESULT)
        {
            CNetDocument.InGame?.PushEvent(goStopEvent);
        }
        yield return null;
    }
}
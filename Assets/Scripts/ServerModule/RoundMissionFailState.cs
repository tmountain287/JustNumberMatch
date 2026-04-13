using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoundMissionFailState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_ROUND_MISSION_FAIL)
        {          
            CNetDocument.InGame?.PushEvent(goStopEvent);

            yield return new WaitForSeconds(1f);
        }
        yield return null;
    }
}
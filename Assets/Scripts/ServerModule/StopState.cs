using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StopState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_STOP)
        {          
            CNetDocument.InGame?.PushEvent(goStopEvent);

            yield return new WaitForSeconds(GameConstants.STOP_DURATION);
            InGameManager.Instance.OnResult();
        }
        yield return null;
    }
}
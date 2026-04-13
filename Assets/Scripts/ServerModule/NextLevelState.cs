using System.Collections;
using UnityEngine;

public class NextLevelState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_NEXT_LEVEL)
        {
            yield return new WaitForSeconds(0.3f);
            CNetDocument.InGame?.PushEvent(goStopEvent);
        }
        yield return null;
    }
}


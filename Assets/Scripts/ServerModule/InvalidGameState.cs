using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InvalidGameState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_INVALID_GAME)
        {
            nxInvalidGameEvent _event = new();
            CNetDocument.InGame?.PushEvent(_event);
        }
        yield return null;
    }
}

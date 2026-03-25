using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RevivalState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_REVIVAL)
        {
            nxRevivalEvent _event = new();
            CNetDocument.InGame?.PushEvent(_event);
        }
        yield return null;
    }
}
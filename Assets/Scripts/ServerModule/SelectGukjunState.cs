using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SelectGukjunState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_SELECT_GUKJUN_CARD)
        {
            CNetDocument.InGame?.PushEvent(goStopEvent);
            //yield return new WaitForSeconds(1f);            
        }
        yield return null;
    }
}
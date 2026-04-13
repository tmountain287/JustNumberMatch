using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTurnState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_CHANGE_TURN)
        {
            Debug.Log("Turn Changed!");
            nxChangeTurnEvent _event = new nxChangeTurnEvent();
            _event.iTurnMatchSlotIndex = InGameManager.Instance.CurrentTurnSlotIndex;
            CNetDocument.InGame?.PushEvent(_event);
        }
        yield return null;
    }
}

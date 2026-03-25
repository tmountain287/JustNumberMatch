using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NagariState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_NAGARI)
        {
            nxNagariEvent _event = new();
            CNetDocument.InGame?.PushEvent(_event);
            yield return new WaitForSeconds(2.5f);
            InGameManager.Instance.IsNagari = true;
            InGameManager.Instance.WinMultipleCount = 0;
            InGameManager.Instance.CurrentTurnSlotIndex = InGameManager.Instance.CurrentFirstSlotIndex;
            machine.SetState(EGoStopEventType.EGSEVT_INITIALIZE);
        }
        yield return null;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SelectCardState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_SELECT_CARD)
        {
            InGameManager.Instance.SendInGameData();
            if (InGameManager.Instance.SelectCardQueue.Count > 0)
            {
                nxSelectCardEvent _event = new();
                _event.aSelectBoardSlot = InGameManager.Instance.SelectCardQueue.Dequeue();
                
                CNetDocument.InGame?.PushEvent(_event);
            }
            else
            {
                //yield return new WaitForSeconds(1f);
                machine.SetState(EGoStopEventType.EGSEVT_BOARDSWEEP);
            }
        }
        yield return null;
    }
}
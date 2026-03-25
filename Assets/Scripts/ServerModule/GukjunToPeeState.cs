using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GukjunToPeeState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_GUKJUN_TO_PEE)
        {
            CNetDocument.InGame?.PushEvent(goStopEvent);

            yield return new WaitForSeconds(GameConstants.MoveDuration + GameConstants.MoveToNextDuration);

            nxGukJunToPeeEvent _event = (nxGukJunToPeeEvent)goStopEvent;

            if (_event.iMatchSolotIndex == InGameManager.Instance.CurrentTurnSlotIndex)
                InGameManager.Instance.CheckGo();
            else
            {
                InGameManager.Instance.ChangeTurnSlot();
                machine.SetState(EGoStopEventType.EGSEVT_CHANGE_TURN);
            }

        }
        yield return null;
    }
}
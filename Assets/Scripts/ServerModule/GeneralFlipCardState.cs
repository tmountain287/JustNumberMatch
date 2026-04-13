using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralFlipCardState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_GENERAL_FLIP_CARD)
        {
            CNetDocument.InGame?.PushEvent(goStopEvent);

            yield return new WaitForSeconds(GameConstants.FlipToNextDuration);


            if (InGameManager.Instance.DragCardList.Count > 0)
            {
                machine.SetState(EGoStopEventType.EGSEVT_SELECT_CARD);
            }
            else
            {
                machine.SetState(EGoStopEventType.EGSEVT_DRAG_CARD);
            }

        }
        yield return null;
    }
}
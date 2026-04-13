using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GoState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_GO)
        {
            CNetDocument.InGame?.PushEvent(goStopEvent);

            //고 연출 타임
            yield return new WaitForSeconds(1f);

            NxCard nxCard = InGameManager.Instance.CurrentOtherPlayer.CollectCards.GetCard(CardSubType.GUKJUN);

            InGameManager.Instance.CurrentOtherPlayer.CollectCards.Log();

            if (nxCard != null && !nxCard.IsLock)
            {
                nxSelectGukJunCardEvent _event = new();
                _event.iMatchSolotIndex = InGameManager.Instance.CurrentOtherPlayer.slotIndex;
                machine.SetState(EGoStopEventType.EGSEVT_SELECT_GUKJUN_CARD, _event);
            }
            else
            {
                InGameManager.Instance.ChangeTurnSlot();
                machine.SetState(EGoStopEventType.EGSEVT_CHANGE_TURN);
            }
        }
        yield return null;
    }
}
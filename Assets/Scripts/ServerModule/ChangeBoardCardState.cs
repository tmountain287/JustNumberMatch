using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ChangeBoardCardState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_CHANGE_BOARD_CARD)
        {
            Debug.Log("board Changed!");

            NxCard card = InGameManager.Instance.BoardCards.GetCard(CardMainType.JOCKER);

            if(card == null)
            {
                machine.SetState(EGoStopEventType.EGSEVT_BOARD_PRESIDENT);
            }
            else
            {
                nxChangeBoardCardEvent _event = new nxChangeBoardCardEvent();
                _event.DragInfo = new();
                _event.DragInfo = card.Sn;

                InGameManager.Instance.BoardCards.Remove(card);

                int flipCardIndex = InGameManager.Instance.CardDeck.GetCard();

                _event.FlipCard = flipCardIndex;

                InGameManager.Instance.BoardCards.Add(_event.FlipCard);

                PlayerData playerData = InGameManager.Instance.PlayerDataList.Where(x => x.slotIndex == InGameManager.Instance.CurrentFirstSlotIndex).FirstOrDefault();
                InGameManager.Instance.BoardCards.Remove(card);
                playerData.CollectCards.Add(card);

                CNetDocument.InGame?.PushEvent(_event);

                yield return new WaitForSeconds(1f);
                machine.SetState(EGoStopEventType.EGSEVT_CHANGE_BOARD_CARD);
            }
        }
        yield return null;
    }
}

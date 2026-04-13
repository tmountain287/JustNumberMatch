using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuffleState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_SHUFFLE_CARD)
        {
            InGameManager.Instance.CardDeck.ShuffleDeck();
            Debug.Log("Cards Shuffled!");
            CNetDocument.InGame?.PushEvent(new nxShuffleCardEvent());

            yield return new WaitForSeconds(1f);
            machine.SetState(EGoStopEventType.EGSEVT_DIVIDE_CARD);
        }
    }
}

using System.Collections;
using UnityEngine;

public class DivideCardState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_DIVIDE_CARD)
        {
            Debug.Log("Cards Divided!");

            nxDivideCardEvent _event = new nxDivideCardEvent();

            //if(k==1)
            //{
            //    _event.aBoardHolder = new() { 33, 1, 30, 31, 40, 41, 49, 48 };// InGameManager.Instance.CardDeck.GetCard(8);

            //    //////new() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

            //    _event.aPlayerHolder0 = new(){ 0, 1, 2, 3, 25, 22, 18, 20, 49, 48};// InGameManager.Instance.CardDeck.GetCard(10);
            //    _event.aPlayerHolder1 = new() { 4, 5, 6, 7, 9, 10, 18, 20, 3, 10 };// InGameManager.Instance.CardDeck.GetCard(10);                
            //}
            //else
            
            _event.aBoardHolder = InGameManager.Instance.CardDeck.GetCard(8);

            _event.aPlayerHolder0 = InGameManager.Instance.CardDeck.GetCard(10);
            _event.aPlayerHolder1 = InGameManager.Instance.CardDeck.GetCard(10);


            //k++;
            InGameManager.Instance.SetDivideCards(_event.aBoardHolder, _event.aPlayerHolder0, _event.aPlayerHolder1);

            CNetDocument.InGame?.PushEvent(_event);
            yield return new WaitForSeconds(1f);
            machine.SetState(EGoStopEventType.EGSEVT_CHANGE_BOARD_CARD);
        }
    }

    //private List<nxCard> MakeOderNxCards(List<CardData> cardList)
    //{
    //    List<nxCard> nxCards = new List<nxCard>();
    //    for (int i = 0; i < cardList.Count; i++)
    //    {
    //        nxCards.Add(new nxCard(cardList[i].Index, i, 0));
    //    }

    //    return nxCards;
    //}

    //private List<nxCard> ConvertToNxCards(List<CardData> cardList)
    //{
    //    List<nxCard> nxCards = new List<nxCard>();
    //    Dictionary<int, int> subIndexToMainType = new Dictionary<int, int>();
    //    int mainTypeCounter = 0;

    //    foreach (var card in cardList.OrderBy(c => c.Index))
    //    {
    //        if (!subIndexToMainType.ContainsKey(card.SubIndex))
    //        {
    //            subIndexToMainType[card.SubIndex] = mainTypeCounter++;
    //        }

    //        nxCards.Add(new nxCard(card.Index, subIndexToMainType[card.SubIndex], -1));
    //    }

    //    int subTypeValue = 0;
    //    foreach (var card in nxCards.OrderByDescending(c => c.iSn))
    //    {
    //        card.HolderSlot.iSubSlot = subTypeValue++;
    //    }

    //    return nxCards;
    //}
}

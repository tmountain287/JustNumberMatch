using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DragCardState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_DRAG_CARD)
        {
            if (goStopEvent != null)
            {
                nxDragCardEvent nxDragCardEvent = (nxDragCardEvent)goStopEvent;
                CNetDocument.InGame?.PushEvent(goStopEvent);

                int jockerCount = nxDragCardEvent.aDragInfo
                    .Select(index => CardDataInfo.CardDataInfoList.FirstOrDefault(card => card.Index == index))
                    .Count(card => card != null && card.MainType == CardMainType.JOCKER);

                if (nxDragCardEvent.collectionTypeList.Count > 0)
                {
                    float totalAniTime = CollectionDataInfo.CollectionDataInfoList
                                        .Where(data => nxDragCardEvent.collectionTypeList.Contains(data.collectionType))
                                        .Sum(data => data.aniTime);
                    yield return new WaitForSeconds(totalAniTime + (nxDragCardEvent.aDragInfo.Count * GameConstants.MoveDuration));
                }
                else
                    yield return new WaitForSeconds(GameConstants.NoneDragDuration + (nxDragCardEvent.aDragInfo.Count * GameConstants.MoveDuration) + (nxDragCardEvent.aStealInfo.Count * GameConstants.StealDuration));
            }
            else
            {
                nxDragCardEvent nxDragCardEvent = new();
                CNetDocument.InGame?.PushEvent(nxDragCardEvent);
                yield return new WaitForSeconds(GameConstants.NoneDragDuration);
            }

            bool has = false;
            NxCard nxCard = InGameManager.Instance.CurrentTurnPlayer.CollectCards.GetCard(CardSubType.GUKJUN);

            if (nxCard != null && !nxCard.IsLock)
            {
                has = true;
            }

            //막장일때 국준으로 나면 자동이동????
            //if(InGameManager.Instance.CurrentTurnPlayer.HandCards.Count == 0 && !InGameManager.Instance.CurrentTurnPlayer.CanGo && InGameManager.Instance.CurrentTurnPlayer.CanGukjunToGo && has)
            //{                
            //    nxCard.ChangeToDoublePee();
            //    nxGukJunToPeeEvent _event = new();
            //    _event.iMatchSolotIndex = InGameManager.Instance.CurrentTurnPlayer.slotIndex;
            //    _event.iUse = true;
            //    InGameManager.Instance.SendInGameData();
            //    CNetDocument.InGame?.PushEvent(_event);
            //    yield return new WaitForSeconds(1.0f);
            //    InGameManager.Instance.CheckGo();                
            //}            
            //else 
            if ((InGameManager.Instance.CurrentTurnPlayer.CanGo && has) || InGameManager.Instance.CurrentTurnPlayer.CanGukjunToGo || (InGameManager.Instance.PlayerDataList.Any(x => x.goCount > 0) && has))// && InGameManager.Instance.CurrentTurnPlayer.HandCards.Count != 0))
            {
                nxSelectGukJunCardEvent _event = new();
                _event.iMatchSolotIndex = InGameManager.Instance.CurrentTurnSlotIndex;
                machine.SetState(EGoStopEventType.EGSEVT_SELECT_GUKJUN_CARD, _event);
                //yield return new WaitForSeconds(1f);
            }
            else
            {
                InGameManager.Instance.CheckGo();
            }
        }
        yield return null;
    }
}

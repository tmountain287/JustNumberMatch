using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillCardState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_SKILL_CARD)
        {
            CNetDocument.InGame?.PushEvent(goStopEvent);
            nxSkillCardEvent nxDragCardEvent = (nxSkillCardEvent)goStopEvent;

            yield return new WaitForSeconds(3.0f);

            if (nxDragCardEvent.collectionTypeList.Count > 0)
            {
                float totalAniTime = CollectionDataInfo.CollectionDataInfoList
                                    .Where(data => nxDragCardEvent.collectionTypeList.Contains(data.collectionType))
                                    .Sum(data => data.aniTime);
                yield return new WaitForSeconds(totalAniTime);
            }
            else
                yield return new WaitForSeconds(GameConstants.NoneDragDuration + (nxDragCardEvent.aStealInfo.Count * GameConstants.StealDuration));
            machine.SetState(EGoStopEventType.EGSEVT_CHANGE_TURN);
        }
        yield return null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyBombHitCardState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_EMPTY_BOMB_HIT_CARD)
        {
            // 폭탄 후 빈 카드 내기 로직 추가
            CNetDocument.InGame?.PushEvent(goStopEvent);
            yield return new WaitForSeconds(0.2f);
            InGameManager.Instance.FlipCard();
        }
        yield return null;
    }
}

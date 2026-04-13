using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombHitCardState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_BOMB_HIT_CARD)
        {
            // 폭탄으로 카드 내기 로직 추가
            CNetDocument.InGame?.PushEvent(goStopEvent);

            //GameConstants.HitDuration * 3
            //폭탄 연출 타임
            yield return new WaitForSeconds(1f);
            InGameManager.Instance.FlipCard();
        }
        yield return null;
    }
}

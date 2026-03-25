using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_SHAKE)
        {
            // 흔들기 로직 추가
            var _event = (nxShakeEvent)goStopEvent;
            CNetDocument.InGame?.PushEvent(goStopEvent);
            yield return new WaitForSeconds(2f);
            InGameManager.Instance.SendReqHitCard(_event.HitCard, false);
            //machine.SetState(EGoStopEventType.EGSEVT_CHANGE_TURN);
        }
        yield return null;
    }
}

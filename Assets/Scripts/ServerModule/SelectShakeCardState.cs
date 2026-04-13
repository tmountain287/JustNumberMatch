using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectShakeCardState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_SELECT_SHAKE_CARD)
        {
            // 흔들기 선택 로직 추가
            CNetDocument.InGame?.PushEvent(goStopEvent);
        }
        yield return null;
    }
}

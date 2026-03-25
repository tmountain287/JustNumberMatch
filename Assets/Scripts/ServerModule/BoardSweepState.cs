using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSweepState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_BOARDSWEEP)
        {
            if(InGameManager.Instance.IsBoardSweep())
            {
                InGameManager.Instance.StealCount++;
                nxBoardSweepEvent _event = new();
                CNetDocument.InGame?.PushEvent(_event);
                //판쓸 연출 타임
                yield return new WaitForSeconds(1f);
            }

            InGameManager.Instance.DragCards();
        }
        yield return null;
    }
}
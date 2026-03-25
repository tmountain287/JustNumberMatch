using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BbukRewardState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_BBUK_REWARD)
        {
            nxBBukRewardEvent nxBBukRewardEvent = (nxBBukRewardEvent)goStopEvent;
            InGameManager.Instance.IsBbukReward = true;
            CNetDocument.InGame?.PushEvent(goStopEvent);

            if (nxBBukRewardEvent.isBankruptcy && InGameManager.Instance.CurrentTurnSlotIndex == 0)
            {
                UserDataManager.ApplyMatchResultMoney(InGameManager.Instance.CurrentTurnSlotIndex == 0, nxBBukRewardEvent.iBBukReward);
                InGameManager.Instance.SendInGameData();
            }
            else
            {
                UserDataManager.ApplyMatchResultMoney(InGameManager.Instance.CurrentTurnSlotIndex == 0, nxBBukRewardEvent.iBBukReward, ()=>
                {
                    DOVirtual.DelayedCall(2.0f, () =>
                    {
                        InGameManager.Instance.SendReqBbukReward();
                    });
                });
                InGameManager.Instance.SendInGameData();
            }
        }
        yield return null;
    }
}
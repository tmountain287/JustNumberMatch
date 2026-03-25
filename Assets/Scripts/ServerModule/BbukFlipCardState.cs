using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BbukFlipCardState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_BBUK_FLIP_CARD)
        {
            InGameManager.Instance.SendInGameData();
            
            CNetDocument.InGame?.PushEvent(goStopEvent);

            //뻑 연출 타임
            yield return new WaitForSeconds(1f);

            if(InGameManager.Instance.CurrentTurnPlayer.bbukCount == 10 - InGameManager.Instance.CurrentTurnPlayer.HandCards.Count)
            {
                long resultMoney = InGameManager.Instance.PointValue * 7 * InGameManager.Instance.CurrentTurnPlayer.bbukCount;

                nxBBukRewardEvent _event = new nxBBukRewardEvent();
                _event.iBBukCount = InGameManager.Instance.CurrentTurnPlayer.bbukCount;

                _event.isBankruptcy = InGameManager.Instance.CurrentOtherPlayer.Money.Value <= resultMoney;
                _event.iBBukReward = _event.isBankruptcy ? InGameManager.Instance.CurrentOtherPlayer.Money.Value : resultMoney;

                InGameManager.Instance.CurrentOtherPlayer.Money.Value = InGameManager.Instance.CurrentOtherPlayer.Money.Value - _event.iBBukReward;
                InGameManager.Instance.CurrentTurnPlayer.Money.Value = InGameManager.Instance.CurrentTurnPlayer.Money.Value + _event.iBBukReward;
                
                machine.SetState(EGoStopEventType.EGSEVT_BBUK_REWARD, _event);
            }
            else
            {
                if (InGameManager.Instance.CurrentTurnPlayer.bbukCount == 3)
                {
                    InGameManager.Instance.OnResult(false);
                }
                else
                {
                    machine.SetState(EGoStopEventType.EGSEVT_SELECT_CARD);
                }
            }
        }
        yield return null;
    }
}
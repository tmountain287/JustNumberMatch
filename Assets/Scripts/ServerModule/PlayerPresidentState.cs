using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerPresidentState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_PLAYER_PRESIDENT)
        {
            nxPlayerPresidentEvent _event = (nxPlayerPresidentEvent)goStopEvent;
            CNetDocument.InGame?.PushEvent(goStopEvent);
            yield return new WaitForSeconds(2f);

            InGameManager.Instance.CurrentFirstSlotIndex = _event.iMatchSlotIndex;

            nxResultEvent _resultEvent = new();
            _resultEvent.netResultInfo = new();
            _resultEvent.netResultInfo.winSlotIndex = _event.iMatchSlotIndex;
            _resultEvent.netResultInfo.pointValue = InGameManager.Instance.PointValue;
            _resultEvent.netResultInfo.score = 7;
            
            _resultEvent.netResultInfo.multiPle = (InGameManager.Instance.IsNagari ? 2 : 1) * InGameManager.Instance.FireMatchMultiple * InGameManager.Instance.NextWinMultiple;

            long resultMoney = InGameManager.Instance.PointValue * _resultEvent.netResultInfo.score * _resultEvent.netResultInfo.multiPle;

            _resultEvent.netResultInfo.isBankruptcy = InGameManager.Instance.CurrentOtherPlayer.Money.Value <= resultMoney;
            _resultEvent.netResultInfo.reward = _resultEvent.netResultInfo.isBankruptcy ? InGameManager.Instance.CurrentOtherPlayer.Money.Value : resultMoney;            
            _resultEvent.netResultInfo.isPresident = true;

            InGameManager.Instance.CurrentOtherPlayer.Money.Value = InGameManager.Instance.CurrentOtherPlayer.Money.Value - _resultEvent.netResultInfo.reward;
            InGameManager.Instance.CurrentTurnPlayer.Money.Value = InGameManager.Instance.CurrentTurnPlayer.Money.Value + _resultEvent.netResultInfo.reward;

            bool isNextFire = false;

            if (_resultEvent.netResultInfo.winSlotIndex == 1 && _resultEvent.netResultInfo.isBankruptcy)
            {
                //내가 파산
                UserDataManager.UserData.gage = 0;
                InGameManager.Instance.MyPlayer.GaugeValue.Value = UserDataManager.UserData.gage;
                InGameManager.Instance.FireMatchMultiple = 1;
            }
            else
            {
                if (InGameManager.Instance.FireMatchMultiple > 1 && InGameManager.Instance.UseFireTicket == false)  //어우
                {
                    UserDataManager.UserData.gage = 0;
                    InGameManager.Instance.MyPlayer.GaugeValue.Value = UserDataManager.UserData.gage;
                    InGameManager.Instance.FireMatchMultiple = 1;
                }
                else
                {
                    UserDataManager.AddGauge(ConfigData.FireGaugeAddValue);
                    InGameManager.Instance.MyPlayer.GaugeValue.Value = UserDataManager.UserData.gage;
                    isNextFire = InGameManager.Instance.MyPlayer.GaugeValue.Value >= ConfigData.FireGaugeMax;
                }
            }

            _resultEvent.netResultInfo.isNextFire = isNextFire;

            UserDataManager.SetResult(_event.iMatchSlotIndex == 0, _resultEvent.netResultInfo.reward);
            InGameManager.Instance.SendInGameData();
            InGameManager.Instance.UseFireTicket = false;
            machine.SetState(EGoStopEventType.EGSEVT_RESULT, _resultEvent);            
        }
    }
}
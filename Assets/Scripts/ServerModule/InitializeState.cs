using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializeState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_INITIALIZE)
        {
            InGameManager.Instance.GameMissionData = null;
            CNetDocument.InGame.CurrentFirstIndex = InGameManager.Instance.CurrentFirstSlotIndex;
            CNetDocument.InGame.CurrentTurnIndex = InGameManager.Instance.CurrentTurnSlotIndex;
            InGameManager.Instance.Clear();

            nxInitEvent _event = new();
            _event.iWinMultiple = InGameManager.Instance.WinMultipleCount;
            _event.isNagari = InGameManager.Instance.IsNagari;
            _event.iFireMatchMultiple = InGameManager.Instance.FireMatchMultiple;
            CNetDocument.InGame?.PushEvent(_event);
            Debug.Log("Game Initialized!");

            if (UserDataManager.OtherMoney <= 0)
            {
                yield return null;
                InGameManager.Instance.SendReqNextLevel();
            }
            else if (UserDataManager.Money <= 0)
            {
                yield return null;                
                machine.SetState(EGoStopEventType.EGSEVT_BANKRUPTCY);
            }
            else if(UserDataManager.UserData.gage >= ConfigData.FireGaugeMax && InGameManager.Instance.FireMatchMultiple == 1 && !UserDataManager.UserData.isOtherFirstMatch)
            {
                yield return null;
                nxFireMatchEvent _fireEvent = new();
                _fireEvent.characterID1 = InGameManager.Instance.MyPlayer.characterID;
                _fireEvent.characterID2 = InGameManager.Instance.OtherPlayer.characterID;
                _fireEvent.useTicket = false;
                machine.SetState(EGoStopEventType.EGSEVT_FIRE_MATCH, _fireEvent);
            }           
            else
            {
                yield return new WaitForSeconds(0.5f);
                machine.SetState(EGoStopEventType.EGSEVT_SHUFFLE_CARD);
            }            
        }
    }
}
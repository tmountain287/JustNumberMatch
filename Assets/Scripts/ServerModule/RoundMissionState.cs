using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoundMissionState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_ROUND_MISSION)
        {
            if (UserDataManager.Level >= ConfigData.MissionOpenLevel)
            {
                InGameManager.Instance.MakeMissionData();

                nxRoundMissionEvent _event = new();
                _event.nxRoundMission = new();
                _event.nxRoundMission.aMissionCard = InGameManager.Instance.GameMissionData.cardSnList;
                _event.nxRoundMission.iMultiple = InGameManager.Instance.GameMissionData.multiple;
                CNetDocument.InGame?.PushEvent(_event);

                yield return new WaitForSeconds(2f);
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
            machine.SetState(EGoStopEventType.EGSEVT_CHANGE_TURN);
        }
        yield return null;
    }
}
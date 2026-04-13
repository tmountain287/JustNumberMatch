using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoardPresidentState : ServerModuleState
{
    public override IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null)
    {
        if (eventType == EGoStopEventType.EGSEVT_BOARD_PRESIDENT)
        {
            List<NxCard> cards = InGameManager.Instance.BoardCards.GetWithSameSubIndex();

            if (cards != null)
            {
                Debug.Log("EGSEVT_BOARD_PRESIDENT !");
                nxBoardPresidentEvent _event = new nxBoardPresidentEvent();
                _event.aPresidentCard = cards.Select(x => x.Sn).ToList();
                CNetDocument.InGame?.PushEvent(_event);
                yield return new WaitForSeconds(1f);
                machine.SetState(EGoStopEventType.EGSEVT_NAGARI);
            }
            else
            {
                List<NxCard> player1 = InGameManager.Instance.MyPlayer.HandCards.GetWithSameSubIndex();
                List<NxCard> player2 = InGameManager.Instance.OtherPlayer.HandCards.GetWithSameSubIndex();

                if (player1 != null && player2 != null)
                {
                    nxSamePlayerPresidentEvent _event = new();
                    _event.aPresidentCard1 = player1.Select(x => x.Sn).ToList();
                    _event.aPresidentCard2 = player2.Select(x => x.Sn).ToList();
                    machine.SetState(EGoStopEventType.EGSEVT_SAME_PLAYER_PRESIDENT, _event);
                }
                else
                {
                    if (player1 != null)
                    {
                        nxSelectPresidentEvent _event = new();
                        _event.aPresidentCard = player1.Select(x => x.Sn).ToList();
                        _event.iMatchSlotIndex = 0;
                        CNetDocument.InGame?.PushEvent(_event);
                    }
                    else if (player2 != null)
                    {
                        nxSelectPresidentEvent _event = new();
                        _event.aPresidentCard = player2.Select(x => x.Sn).ToList();
                        _event.iMatchSlotIndex = 1;
                        CNetDocument.InGame?.PushEvent(_event);
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.5f);
                        machine.SetState(EGoStopEventType.EGSEVT_ROUND_MISSION);
                    }
                }
            }
        }
        yield return null;
    }
}
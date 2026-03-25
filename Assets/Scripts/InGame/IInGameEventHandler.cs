using System;
using System.Collections.Generic;
using System.Linq;

namespace InGame.Event
{
    //public interface IInGameEventHandler
    //{
    //    //void HandleTopCardDrawStartEvent(object _sender, nxGoStopEvent _event);
    //    //void HandleTopCardSelectEvent(object _sender, nxGoStopEvent _event);
    //    //void HandleTopCardLeaderEvent(object _sender, nxGoStopEvent _event);

    //    void HandleFireMatchEvent(object _sender, nxGoStopEvent _event);
    //    void HandleNextLevelEvent(object _sender, nxGoStopEvent _event);
    //    void HandleInitEvent(object _sender, nxGoStopEvent _event);
    //    void HandleChangeTurnEvent(object _sender, nxGoStopEvent _event);
    //    void HandleShuffleCardEvent(object _sender, nxGoStopEvent _event);
    //    void HandleDivedeCardEvent(object _sender, nxGoStopEvent _event);
    //    void HandleChangeBoardCardEvent(object _sender, nxGoStopEvent _event);

    //    void HandleGeneralHitCardEvent(object _sender, nxGoStopEvent _event);
    //    void HandleBombHitCardEvent(object _sender, nxGoStopEvent _event);
    //    void HandleBBucksweepHitCardEvent(object _sender, nxGoStopEvent _event);
    //    void HandleBonusHitCardEvent(object _sender, nxGoStopEvent _event);
    //    void HandleEmptyBombHitCardEvent(object _sender, nxGoStopEvent _event);        
    //    void HandleSelectShakeCardEvent(object _sender, nxGoStopEvent _event);
    //    void HandleShakeEvent(object _sender, nxGoStopEvent _event);

    //    void HandleGeneralFlipCardEvent(object _sender, nxGoStopEvent _event);
    //    void HandleBBuckFlipCardEvent(object _sender, nxGoStopEvent _event);
    //    void HandleBBucksweepFlipCardEvent(object _sender, nxGoStopEvent _event);
    //    void HandleBonusFlipCardEvent(object _sender, nxGoStopEvent _event);
    //    void HandleDDaddkFlipCardEvent(object _sender, nxGoStopEvent _event);
    //    void HandleJJokFlipCardEvent(object _sender, nxGoStopEvent _event);
    //    void HandleSelectCardEvent(object _sender, nxGoStopEvent _event);
    //    void HandleBoardsweepEvent(object _sender, nxGoStopEvent _event);

    //    void HandleDragCardEvent(object _sender, nxGoStopEvent _event);
    //    //void HandleSelectGukjunCardEvent(object _sender, nxGoStopEvent _event);
    //    //void HandleGukjunToPeeEvent(object _sender, nxGoStopEvent _event);

    //    void HandleSelectGostopEvent(object _sender, nxGoStopEvent _event); 
    //    void HandleGoEvent(object _sender, nxGoStopEvent _event);
    //    void HandleStopEvent(object _sender, nxGoStopEvent _event);

    //    void HandleNagariEvent(object _sender, nxGoStopEvent _event);
    //    void HandleBoardPresidentEvent(object _sender, nxGoStopEvent _event);
    //    void HandlePlayerPresidentEvent(object _sender, nxGoStopEvent _event);
    //    void HandleSamePlayerPresidentEvent(object _sender, nxGoStopEvent _event);

    //    void HandleResultEvent(object _sender, nxGoStopEvent _event);
    //    void HandleRoundMissionEvent(object _sender, nxGoStopEvent _event);
    //    void HandleRoundMissionSuccessEvent(object _sender, nxGoStopEvent _event);
    //    void HandleRoundMissionFailEvent(object _sender, nxGoStopEvent _event);
    //    //void HandleNfyAutoModeEvent(object _sender, nxGoStopEvent _event);
    //}


    public abstract class CInGameEventHadler// : IInGameEventHandler
    {
        private Queue<nxGoStopEvent> eventQueue = new();
        private Dictionary<EGoStopEventType, EventHandler<nxGoStopEvent>> eventHandlers = new();

        public void OnEventCompleted()
        {
            //UnityEngine.Debug.Log("Complete");
            //eventCompleted = true;
        }

        public CInGameEventHadler()
        {
            //eventCompleted = true;
            // 이벤트 타입과 핸들러를 매핑
            //AddEventHandler(EGoStopEventType.EGSEVT_TOPCARD_DRAW_START, HandleTopCardDrawStartEvent);
            //AddEventHandler(EGoStopEventType.EGSEVT_TOPCARD_SELECT, HandleTopCardSelectEvent);
            //AddEventHandler(EGoStopEventType.EGSEVT_TOPCARD_LEADER, HandleTopCardLeaderEvent);

            AddEventHandler(EGoStopEventType.EGSEVT_FIRE_MATCH, HandleFireMatchEvent); 
            AddEventHandler(EGoStopEventType.EGSEVT_NEXT_LEVEL, HandleNextLevelEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_INITIALIZE, HandleInitEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_CHANGE_TURN, HandleChangeTurnEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_SHUFFLE_CARD, HandleShuffleCardEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_DIVIDE_CARD, HandleDivedeCardEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_CHANGE_BOARD_CARD, HandleChangeBoardCardEvent);

            AddEventHandler(EGoStopEventType.EGSEVT_SKILL_CARD, HandleSkillCardEvent);

            AddEventHandler(EGoStopEventType.EGSEVT_GENERAL_HIT_CARD, HandleGeneralHitCardEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_BOMB_HIT_CARD, HandleBombHitCardEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_BBUKSWEEP_HIT_CARD, HandleBBucksweepHitCardEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_BONUS_HIT_CARD, HandleBonusHitCardEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_EMPTY_BOMB_HIT_CARD, HandleEmptyBombHitCardEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_SELECT_SHAKE_CARD, HandleSelectShakeCardEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_SHAKE, HandleShakeEvent);

            AddEventHandler(EGoStopEventType.EGSEVT_GENERAL_FLIP_CARD, HandleGeneralFlipCardEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_BBUK_FLIP_CARD, HandleBBuckFlipCardEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_BBUKSWEEP_FLIP_CARD, HandleBBucksweepFlipCardEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_BONUS_FLIP_CARD, HandleBonusFlipCardEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_DDADDK_FLIP_CARD, HandleDDaddkFlipCardEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_JJOK_FLIP_CARD, HandleJJokFlipCardEvent);

            AddEventHandler(EGoStopEventType.EGSEVT_SELECT_CARD, HandleSelectCardEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_BOARDSWEEP, HandleBoardsweepEvent);

            AddEventHandler(EGoStopEventType.EGSEVT_DRAG_CARD, HandleDragCardEvent);
            //AddEventHandler(EGoStopEventType.EGSEVT_SELECT_GUKJUN_CARD, HandleSelectGukjunCardEvent);
            //AddEventHandler(EGoStopEventType.EGSEVT_GUKJUN_TO_PEE, HandleGukjunToPeeEvent);

            AddEventHandler(EGoStopEventType.EGSEVT_SELECT_GOSTOP, HandleSelectGostopEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_GO, HandleGoEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_STOP, HandleStopEvent);

            AddEventHandler(EGoStopEventType.EGSEVT_NAGARI, HandleNagariEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_BOARD_PRESIDENT, HandleBoardPresidentEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_PLAYER_PRESIDENT, HandlePlayerPresidentEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_SAME_PLAYER_PRESIDENT, HandleSamePlayerPresidentEvent);       
            
            AddEventHandler(EGoStopEventType.EGSEVT_RESULT, HandleResultEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_BANKRUPTCY, HandleBankruptcyEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_REVIVAL, HandleRevivalEvent);

            AddEventHandler(EGoStopEventType.EGSEVT_ROUND_MISSION, HandleRoundMissionEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_ROUND_MISSION_SUCCESS, HandleRoundMissionSuccessEvent);
            AddEventHandler(EGoStopEventType.EGSEVT_ROUND_MISSION_FAIL, HandleRoundMissionFailEvent);
            //AddEventHandler(EGoStopEventType.EGSEVT_NFY_AUTOMODE, HandleNfyAutoModeEvent);
        }
        //public abstract void HandleTopCardDrawStartEvent(object _sender, nxGoStopEvent _event);
        //public abstract void HandleTopCardSelectEvent(object _sender, nxGoStopEvent _event);
        //public abstract void HandleTopCardLeaderEvent(object _sender, nxGoStopEvent _event);
        
        public abstract void HandleFireMatchEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleNextLevelEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleInitEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleChangeTurnEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleShuffleCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleDivedeCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleChangeBoardCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleSkillCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleGeneralHitCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleBombHitCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleBBucksweepHitCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleBonusHitCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleEmptyBombHitCardEvent(object _sender, nxGoStopEvent _event);
        
        public abstract void HandleSelectShakeCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleShakeEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleGeneralFlipCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleBBuckFlipCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleBBucksweepFlipCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleBonusFlipCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleDDaddkFlipCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleJJokFlipCardEvent(object _sender, nxGoStopEvent _event);

        public abstract void HandleSelectCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleBoardsweepEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleDragCardEvent(object _sender, nxGoStopEvent _event);
        //public abstract void HandleSelectGukjunCardEvent(object _sender, nxGoStopEvent _event);
        //public abstract void HandleGukjunToPeeEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleSelectGostopEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleGoEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleStopEvent(object _sender, nxGoStopEvent _event);

        public abstract void HandleNagariEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleBoardPresidentEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandlePlayerPresidentEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleSamePlayerPresidentEvent(object _sender, nxGoStopEvent _event);
        
        public abstract void HandleResultEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleBankruptcyEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleRevivalEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleRoundMissionEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleRoundMissionSuccessEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleRoundMissionFailEvent(object _sender, nxGoStopEvent _event);
        //public abstract void HandleNfyAutoModeEvent(object _sender, nxGoStopEvent _event);

        //public abstract void HandleEndEvent(object _sender, nxPokerEvent _event);

        public void AddEventHandler(EGoStopEventType eventType, EventHandler<nxGoStopEvent> handler)
        {
            if (eventHandlers.ContainsKey(eventType))
            {
                eventHandlers[eventType] += handler;
            }
            else
            {
                eventHandlers.Add(eventType, handler);
            }
        }

        public void RemoveEventHandler(EGoStopEventType eventType, EventHandler<nxGoStopEvent> handler)
        {
            if (eventHandlers.ContainsKey(eventType))
            {
                eventHandlers[eventType] -= handler;
            }
        }

        public void RemoveAllEventHandlers()
        {
            foreach (var eventType in eventHandlers.Keys.ToList())
            {
                eventHandlers[eventType] = null;
            }

            eventHandlers.Clear();
            eventHandlers = null;
        }

        public void PushEvent(nxGoStopEvent _evPokerEvent)
        {
            eventQueue.Enqueue(_evPokerEvent);
        }

        public void ProcessEvent()
        {
            while (true)
            {
                if (eventQueue.Count == 0) // || eventCompleted == false)
                    return;

                nxGoStopEvent pokerEvent = eventQueue.Dequeue();

                if (eventHandlers.TryGetValue(pokerEvent.GetGoStopType(), out var eventHandler))
                {
                    string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"); // 밀리초까지 포함
                    UnityEngine.Debug.Log($"[{currentTime}] {pokerEvent.GetGoStopType()}");
                    //eventCompleted = false;
                    eventHandler?.Invoke(this, pokerEvent);
                }
                else
                {
                    // Handle unknown event type or log an error
                    UnityEngine.Debug.Log("Unhandled event type: " + pokerEvent.GetGoStopType());
                }
            }
        }

        public void ClearEvent()
        {
            eventQueue.Clear();
        }
    }
}
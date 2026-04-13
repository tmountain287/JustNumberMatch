using UnityEngine;

namespace InGame.Event
{
    public abstract class InGameEventHadlerMono : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            if (CNetDocument.InGame != null)
            {
                AddEventHandler();
            }
        }

        protected virtual void OnDisable()
        {
            if (CNetDocument.InGame != null)
            {
                RemoveEventHandler();
            }
        }

        protected virtual void AddEventHandler()
        {
            //CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_TOPCARD_DRAW_START, HandleTopCardDrawStartEvent);
            //CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_TOPCARD_SELECT, HandleTopCardSelectEvent);
            //CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_TOPCARD_LEADER, HandleTopCardLeaderEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_FIRE_MATCH, HandleFireMatchEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_INVALID_GAME, HandleInvalidGameEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_NEXT_LEVEL, HandleNextLevelEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_INITIALIZE, HandleInitEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_CHANGE_TURN, HandleChangeTurnEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_SHUFFLE_CARD, HandleShuffleCardEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_DIVIDE_CARD, HandleDivedeCardEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_CHANGE_BOARD_CARD, HandleChangeBoardCardEvent);

            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_SKILL_CARD, HandleSkillCardEvent);

            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_GENERAL_HIT_CARD, HandleGeneralHitCardEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_BOMB_HIT_CARD, HandleBombHitCardEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_BBUKSWEEP_HIT_CARD, HandleBBucksweepHitCardEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_BONUS_HIT_CARD, HandleBonusHitCardEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_EMPTY_BOMB_HIT_CARD, HandleEmptyBombHitCardEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_SELECT_SHAKE_CARD, HandleSelectShakeCardEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_SHAKE, HandleShakeEvent);

            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_GENERAL_FLIP_CARD, HandleGeneralFlipCardEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_BBUK_FLIP_CARD, HandleBBuckFlipCardEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_BBUKSWEEP_FLIP_CARD, HandleBBucksweepFlipCardEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_BONUS_FLIP_CARD, HandleBonusFlipCardEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_DDADDK_FLIP_CARD, HandleDDaddkFlipCardEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_JJOK_FLIP_CARD, HandleJJokFlipCardEvent);

            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_BBUK_REWARD, HandleBbukRewardEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_BBUK_REWARD_CONFIRM, HandleBbukRewardConfirmEvent);

            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_SELECT_CARD, HandleSelectCardEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_BOARDSWEEP, HandleBoardsweepEvent);

            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_DRAG_CARD, HandleDragCardEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_SELECT_GUKJUN_CARD, HandleSelectGukjunCardEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_GUKJUN_TO_PEE, HandleGukjunToPeeEvent);

            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_SELECT_GOSTOP, HandleSelectGostopEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_GO, HandleGoEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_STOP, HandleStopEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_NAGARI, HandleNagariEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_BOARD_PRESIDENT, HandleBoardPresidentEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_SELECT_PRESIDENT, HandleSelectPresidentEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_PLAYER_PRESIDENT, HandlePlayerPresidentEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_SAME_PLAYER_PRESIDENT, HandleSamePlayerPresidentEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_RESULT, HandleResultEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_BANKRUPTCY, HandleBankruptcyEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_REVIVAL, HandleRevivalEvent);

            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_DDADDAK_BOMB_HIT_CARD, HandleDDaddakBombHitCardEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_ROUND_MISSION, HandleRoundMissionEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_ROUND_MISSION_SUCCESS, HandleRoundMissionSuccessEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_ROUND_MISSION_FAIL, HandleRoundMissionFailEvent);
            CNetDocument.InGame.AddEventHandler(EGoStopEventType.EGSEVT_NFY_AUTOMODE, HandleNfyAutoModeEvent);
        }

        protected virtual void RemoveEventHandler()
        {
            //CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_TOPCARD_DRAW_START, HandleTopCardDrawStartEvent);
            //CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_TOPCARD_SELECT, HandleTopCardSelectEvent);
            //CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_TOPCARD_LEADER, HandleTopCardLeaderEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_FIRE_MATCH, HandleFireMatchEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_INVALID_GAME, HandleInvalidGameEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_NEXT_LEVEL, HandleNextLevelEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_INITIALIZE, HandleInitEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_CHANGE_TURN, HandleChangeTurnEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_SHUFFLE_CARD, HandleShuffleCardEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_DIVIDE_CARD, HandleDivedeCardEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_CHANGE_BOARD_CARD, HandleChangeBoardCardEvent);

            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_GENERAL_HIT_CARD, HandleGeneralHitCardEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_BOMB_HIT_CARD, HandleBombHitCardEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_BBUKSWEEP_HIT_CARD, HandleBBucksweepHitCardEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_BONUS_HIT_CARD, HandleBonusHitCardEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_EMPTY_BOMB_HIT_CARD, HandleEmptyBombHitCardEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_SELECT_SHAKE_CARD, HandleSelectShakeCardEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_SHAKE, HandleShakeEvent);

            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_GENERAL_FLIP_CARD, HandleGeneralFlipCardEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_BBUK_FLIP_CARD, HandleBBuckFlipCardEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_BBUKSWEEP_FLIP_CARD, HandleBBucksweepFlipCardEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_BONUS_FLIP_CARD, HandleBonusFlipCardEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_DDADDK_FLIP_CARD, HandleDDaddkFlipCardEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_JJOK_FLIP_CARD, HandleJJokFlipCardEvent);

            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_BBUK_REWARD, HandleBbukRewardEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_BBUK_REWARD_CONFIRM, HandleBbukRewardConfirmEvent);

            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_SELECT_CARD, HandleSelectCardEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_BOARDSWEEP, HandleBoardsweepEvent);

            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_DRAG_CARD, HandleDragCardEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_SELECT_GUKJUN_CARD, HandleSelectGukjunCardEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_GUKJUN_TO_PEE, HandleGukjunToPeeEvent);

            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_SELECT_GOSTOP, HandleSelectGostopEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_GO, HandleGoEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_STOP, HandleStopEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_NAGARI, HandleNagariEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_BOARD_PRESIDENT, HandleBoardPresidentEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_SELECT_PRESIDENT, HandleSelectPresidentEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_PLAYER_PRESIDENT, HandlePlayerPresidentEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_SAME_PLAYER_PRESIDENT, HandleSamePlayerPresidentEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_RESULT, HandleResultEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_BANKRUPTCY, HandleBankruptcyEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_REVIVAL, HandleRevivalEvent);

            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_DDADDAK_BOMB_HIT_CARD, HandleDDaddakBombHitCardEvent);            
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_ROUND_MISSION, HandleRoundMissionEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_ROUND_MISSION_SUCCESS, HandleRoundMissionSuccessEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_ROUND_MISSION_FAIL, HandleRoundMissionFailEvent);
            CNetDocument.InGame.RemoveEventHandler(EGoStopEventType.EGSEVT_NFY_AUTOMODE, HandleNfyAutoModeEvent);
            //CNetDocument.InGame.RemoveInGameRoundEventHandler(EHoldemEventType.EHEVT_END, HandleEndEvent);
        }


        //public abstract void HandleTopCardDrawStartEvent(object _sender, nxGoStopEvent _event);
        //public abstract void HandleTopCardSelectEvent(object _sender, nxGoStopEvent _event);
        //public abstract void HandleTopCardLeaderEvent(object _sender, nxGoStopEvent _event);

        public abstract void HandleFireMatchEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleInvalidGameEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleNextLevelEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleInitEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleChangeTurnEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleShuffleCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleDivedeCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleChangeBoardCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleSkillCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleGeneralHitCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleBombHitCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleDDaddakBombHitCardEvent(object _sender, nxGoStopEvent _event);
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

        public abstract void HandleBbukRewardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleBbukRewardConfirmEvent(object _sender, nxGoStopEvent _event);

        public abstract void HandleSelectCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleBoardsweepEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleDragCardEvent(object _sender, nxGoStopEvent _event);

        public abstract void HandleSelectGukjunCardEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleGukjunToPeeEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleSelectGostopEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleGoEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleStopEvent(object _sender, nxGoStopEvent _event);

        public abstract void HandleNagariEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleBoardPresidentEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleSelectPresidentEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandlePlayerPresidentEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleSamePlayerPresidentEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleResultEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleBankruptcyEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleRevivalEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleRoundMissionEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleRoundMissionSuccessEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleRoundMissionFailEvent(object _sender, nxGoStopEvent _event);
        public abstract void HandleNfyAutoModeEvent(object _sender, nxGoStopEvent _event);
    }
}
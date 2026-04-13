using InGame.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System;

public class CEventQueue
{
    public void Push(nxGoStopEvent _evGoStop) { m_aEventQueue.Enqueue(_evGoStop); }
    public nxGoStopEvent Pop() { return m_aEventQueue.Dequeue(); }

    public bool IsEmpty() { return m_aEventQueue.Count == 0 ? true : false; }

    private Queue<nxGoStopEvent> m_aEventQueue = new Queue<nxGoStopEvent>();
}


public class CInGame : CInGameEventHadler
{
    public CEventHadler<EGoStopEventType> IngameRoundEventHandler { get; set; } = new();

    private EGoStopEventType m_eEventType = EGoStopEventType.EGSEVT_INITIALIZE;

    public EGoStopEventType EventType
    {
        get { return m_eEventType; }
        set
        {
            m_eEventType = value;
            IngameRoundEventHandler.TriggerEvent(value);
        }
    }

    public netRoundMission NXRoundMission { get; set; } = null;

    public List<CInGamePlayer> InGamePlayerList { get; set; } = new();

    public CInGamePlayer CurrentTurnPlayer => InGamePlayerList.Where(x => x.PlayerData.slotIndex == CurrentTurnIndex).FirstOrDefault();
    public CInGamePlayer CurrentOtherPlayer => InGamePlayerList.Where(x => x.PlayerData.slotIndex != CurrentTurnIndex).FirstOrDefault();

    public int CurrentTurnIndex { get; set; } = -1;
    public int CurrentFirstIndex { get; set; } = -1;

    public NxCardList BoardCards { get; set; } = new();
    public ObservableProperty<long> PointValue { get; set; } = new(0);

    //public UnityEvent<CInGamePlayer> OnAddPlayerEvent { get; set; } = new();
    //public UnityEvent<int> OnLeavePlayerEvent { get; set; } = new();

    public event Action OnChangeGameEvent = null;

    public int WinMultipleCount { get; set; } = 0;
    public int FireMatchMultiple { get; set; } = 1;
    public bool IsNagari { get; set; } = false;

    public int GamePlayCount { get; set; } = 0;
    public bool IsNextAd { get; set; } = false;

    public CInGame(long _pointValue, List<PlayerData> _playerDataList)
    {
        PointValue.Value = _pointValue;
        _playerDataList.ForEach(player => AddPlayer(player));
    }

    public void ChangeInGame(long _pointValue, List<PlayerData> _playerDataList)
    {
        PointValue.Value = _pointValue;
        InGamePlayerList?.Clear();
        _playerDataList.ForEach(player => AddPlayer(player));
        OnChangeGameEvent?.Invoke();
    }

    public void Reset()
    {
        //InGamePlayerList?.ForEach(player => player.Clear());

        InGamePlayerList?.Clear();
        InGamePlayerList = null;

        //OnAddPlayerEvent.RemoveAllListeners();
        //OnLeavePlayerEvent.RemoveAllListeners();

        RemoveAllEventHandlers();
    }

    public void UpdateGameData(NxCardList _boardCards, List<PlayerData> _playerDatas)
    {
        BoardCards = _boardCards;

        for (int i = 0; i < _playerDatas.Count; i++)
        {
            CInGamePlayer player = InGamePlayerList.Where(x => x.PlayerData.slotIndex == _playerDatas[i].slotIndex).FirstOrDefault();
            if (player != null)
            {
                player.SetPlayerData(_playerDatas[i]);
            }
        }
    }

    #region HandlerEvent
    //public override void HandleTopCardDrawStartEvent(object _sender, nxGoStopEvent _event)
    //{
    //    nxTopCardDrawStartEvent handleEvent = (nxTopCardDrawStartEvent)_event;
    //}

    //public override void HandleTopCardSelectEvent(object _sender, nxGoStopEvent _event)
    //{
    //    nxTopCardSelectEvent handleEvent = (nxTopCardSelectEvent)_event;
    //}

    //public override void HandleTopCardLeaderEvent(object _sender, nxGoStopEvent _event)
    //{
    //    nxTopCardLeaderEvent handleEvent = (nxTopCardLeaderEvent)_event;
    //    CurrentFirstIndex = handleEvent.iTurnMatchSlotIndex;

    //    Debug.Log(CurrentFirstIndex);
    //}

    public override void HandleFireMatchEvent(object _sender, nxGoStopEvent _event)
    {
        
    }

    public override void HandleNextLevelEvent(object _sender, nxGoStopEvent _event)
    {
        nxNextLevelEvent handleEvent = (nxNextLevelEvent)_event;
        PointValue.Value = handleEvent.poingValue;
        CurrentOtherPlayer.PlayerData.Refresh(handleEvent.playerData.name, handleEvent.playerData.characterID, handleEvent.playerData.isFirst, handleEvent.playerData.slotIndex, handleEvent.playerData.Money.Value);
    }

    public override void HandleInitEvent(object _sender, nxGoStopEvent _event)
    {
        nxInitEvent handleEvent = (nxInitEvent)_event;
        WinMultipleCount = handleEvent.iWinMultiple;
        IsNagari = handleEvent.isNagari;
        FireMatchMultiple = handleEvent.iFireMatchMultiple;
    }

    public override void HandleChangeTurnEvent(object _sender, nxGoStopEvent _event)
    {
        nxChangeTurnEvent handleEvent = (nxChangeTurnEvent)_event;
        CurrentTurnIndex = handleEvent.iTurnMatchSlotIndex;
    }

    public override void HandleShuffleCardEvent(object _sender, nxGoStopEvent _event)
    {
        // 기본 구현
    }

    public override void HandleDivedeCardEvent(object _sender, nxGoStopEvent _event)
    {

    }

    public override void HandleChangeBoardCardEvent(object _sender, nxGoStopEvent _event)
    {

    }

    public override void HandleSkillCardEvent(object _sender, nxGoStopEvent _event)
    {
        //nxGeneralHitCardEvent handleEvent = (nxGeneralHitCardEvent)_event;
    }

    public override void HandleGeneralHitCardEvent(object _sender, nxGoStopEvent _event)
    {
        nxGeneralHitCardEvent handleEvent = (nxGeneralHitCardEvent)_event;
    }

    public override void HandleBombHitCardEvent(object _sender, nxGoStopEvent _event)
    {
        //nxBombHitCardEvent handleEvent = (nxBombHitCardEvent)_event;
        //MoveHitCardToBoard(handleEvent.aHitCard);

        //handleEvent.aToPlayerSlot.ForEach(x => Debug.Log(x.iSn));

        //CurrentTurnPlayer.PlayerCardList.AddRange(handleEvent.aToPlayerSlot);
    }

    public override void HandleBBucksweepHitCardEvent(object _sender, nxGoStopEvent _event)
    {
        //nxBBukSweepHitCardEvent handleEvent = (nxBBukSweepHitCardEvent)_event;
        //MoveHitCardToBoard(handleEvent.HitCard);
    }

    public override void HandleBonusHitCardEvent(object _sender, nxGoStopEvent _event)
    {
        //nxBonusHitCardEvent handleEvent = (nxBonusHitCardEvent)_event;
        //MoveHitCardToDrag(handleEvent.HitCard);
        //CurrentTurnPlayer.PlayerCardList.Add(handleEvent.FlipCard);
        //StealCard()
    }

    public override void HandleEmptyBombHitCardEvent(object _sender, nxGoStopEvent _event)
    {
        nxEmptyBombHitCardEvent handleEvent = (nxEmptyBombHitCardEvent)_event;
        //nxCard nxCard = BoardCards.Where(x => x.HolderSlot == handleEvent.DragInfo.FromSlot).FirstOrDefault();
    }

    public override void HandleSelectShakeCardEvent(object _sender, nxGoStopEvent _event)
    {
        nxSelectShakeCardEvent handleEvent = (nxSelectShakeCardEvent)_event;
    }

    public override void HandleShakeEvent(object _sender, nxGoStopEvent _event)
    {
        nxShakeEvent handleEvent = (nxShakeEvent)_event;
    }

    public override void HandleGeneralFlipCardEvent(object _sender, nxGoStopEvent _event)
    {
        nxGeneralFlipCardEvent handleEvent = (nxGeneralFlipCardEvent)_event;
        //if (handleEvent.bSelectFilp)
        //{
        //    return;
        //}

        //BoardCards.Add(handleEvent.FlipCard);
    }

    public override void HandleBBuckFlipCardEvent(object _sender, nxGoStopEvent _event)
    {
        nxBBukFlipCardEvent handleEvent = (nxBBukFlipCardEvent)_event;
        //   BoardCards.Add(handleEvent.FlipCard);
    }

    public override void HandleBBucksweepFlipCardEvent(object _sender, nxGoStopEvent _event)
    {
        nxBBukSweepFlipCardEvent handleEvent = (nxBBukSweepFlipCardEvent)_event;
        //   BoardCards.Add(handleEvent.FlipCard);
    }

    public override void HandleBonusFlipCardEvent(object _sender, nxGoStopEvent _event)
    {
        nxBonusFlipCardEvent handleEvent = (nxBonusFlipCardEvent)_event;
        // BoardCards.Add(handleEvent.FlipCard);
    }

    public override void HandleDDaddkFlipCardEvent(object _sender, nxGoStopEvent _event)
    {
        nxDDaDDakFlipCardEvent handleEvent = (nxDDaDDakFlipCardEvent)_event;
        //  BoardCards.Add(handleEvent.FlipCard);
    }

    public override void HandleJJokFlipCardEvent(object _sender, nxGoStopEvent _event)
    {
        nxJJokFlipCardEvent handleEvent = (nxJJokFlipCardEvent)_event;
        //  BoardCards.Add(handleEvent.FlipCard);
    }

    public override void HandleSelectCardEvent(object _sender, nxGoStopEvent _event)
    {
        nxSelectCardEvent handleEvent = (nxSelectCardEvent)_event;
    }

    public override void HandleBoardsweepEvent(object _sender, nxGoStopEvent _event)
    {
        nxBoardSweepEvent handleEvent = (nxBoardSweepEvent)_event;
    }

    public override void HandleDragCardEvent(object _sender, nxGoStopEvent _event)
    {
        nxDragCardEvent handleEvent = (nxDragCardEvent)_event;

        if (handleEvent.missionResult != null && handleEvent.missionResult.isSuccess == false)
        {
            NXRoundMission = null;
        }
    }

    //public override void HandleSelectGukjunCardEvent(object _sender, nxGoStopEvent _event)
    //{
    //    nxSelectGukJunCardEvent handleEvent = (nxSelectGukJunCardEvent)_event;
    //}

    //public override void HandleGukjunToPeeEvent(object _sender, nxGoStopEvent _event)
    //{
    //    nxGukJunToPeeEvent handleEvent = (nxGukJunToPeeEvent)_event;
    //}

    public override void HandleSelectGostopEvent(object _sender, nxGoStopEvent _event)
    {
        nxSelectGoStopEvent handleEvent = (nxSelectGoStopEvent)_event;
    }

    public override void HandleGoEvent(object _sender, nxGoStopEvent _event)
    {
        nxGoEvent handleEvent = (nxGoEvent)_event;
    }

    public override void HandleStopEvent(object _sender, nxGoStopEvent _event)
    {
        nxStopEvent handleEvent = (nxStopEvent)_event;
    }

    public override void HandleNagariEvent(object _sender, nxGoStopEvent _event)
    {
        nxNagariEvent handleEvent = (nxNagariEvent)_event;
    }

    public override void HandleBoardPresidentEvent(object _sender, nxGoStopEvent _event)
    {
        nxBoardPresidentEvent handleEvent = (nxBoardPresidentEvent)_event;
    }

    public override void HandlePlayerPresidentEvent(object _sender, nxGoStopEvent _event)
    {
        nxPlayerPresidentEvent handleEvent = (nxPlayerPresidentEvent)_event;
    }

    public override void HandleSamePlayerPresidentEvent(object _sender, nxGoStopEvent _event)
    {
        nxSamePlayerPresidentEvent handleEvent = (nxSamePlayerPresidentEvent)_event;
    }

    public override void HandleRoundMissionEvent(object _sender, nxGoStopEvent _event)
    {
        nxRoundMissionEvent handleEvent = (nxRoundMissionEvent)_event;
        NXRoundMission = handleEvent.nxRoundMission;
    }

    public override void HandleResultEvent(object _sender, nxGoStopEvent _event)
    {
        IsNextAd = false;
        //if (UserDataManager.UserData.isPremium)
        //    return;

        //if(ConfigData.AdOpenLevel <= UserDataManager.Level)
        //{
        //    GamePlayCount++;
        //    if(GamePlayCount >= ConfigData.AdOpenCount)
        //    {
        //        IsNextAd = true;
        //        GamePlayCount = 0;
        //    }
        //}
    }

    public override void HandleBankruptcyEvent(object _sender, nxGoStopEvent _event)
    {
        
    }

    public override void HandleRevivalEvent(object _sender, nxGoStopEvent _event)
    {

    }

    public override void HandleRoundMissionSuccessEvent(object _sender, nxGoStopEvent _event)
    {
        nxRoundMissionSuccessEvent handleEvent = (nxRoundMissionSuccessEvent)_event;
    }

    public override void HandleRoundMissionFailEvent(object _sender, nxGoStopEvent _event)
    {
        nxRoundMissionFailEvent handleEvent = (nxRoundMissionFailEvent)_event;
        NXRoundMission = null;
    }
    #endregion

    //추후
    public void AddPlayer(PlayerData _playerData)
    {
        CInGamePlayer inGamePlayer = new(_playerData);
        InGamePlayerList.Add(inGamePlayer);
        //OnAddPlayerEvent.Invoke(inGamePlayer);
    }

    public void RemovePlayer(int _iSlotIndex)
    {
        //OnLeavePlayerEvent.Invoke(_iSlotIndex);
        InGamePlayerList = InGamePlayerList.Where(x => x.PlayerData.slotIndex != _iSlotIndex).ToList();
    }

    public CInGamePlayer FindInGamePlayer(int _iSlotIndex)
    {
        return InGamePlayerList.Where(x => x.PlayerData.slotIndex == _iSlotIndex).FirstOrDefault();
    }

    public CInGamePlayer FindInGamePlayerOther(int _iSlotIndex)
    {
        return InGamePlayerList.Where(x => x.PlayerData.slotIndex != _iSlotIndex).FirstOrDefault();
    }

    public bool IsMissionCard(int _iSn)
    {
        if (NXRoundMission == null)
            return false;

        if (_iSn == -1)
            return false;

        return NXRoundMission.aMissionCard.Any(x => x == _iSn);
    }

    //public bool IsMissionCard(int _iSn)
    //{
    //    if (_iSn == -1)
    //        return false;

    //    return NXRoundMission.aMissionCard.Any(x => x.iSn == _iSn);
    //}
}

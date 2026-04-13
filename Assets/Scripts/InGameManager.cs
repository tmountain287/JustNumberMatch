using Common.Manager;
using DG.Tweening;
using UI.Popup;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class GameMissionData
{
    public List<int> cardSnList = new List<int>();
    public int multiple = 0;

    public GameMissionData(List<int> _cardSnList, int _multiple)
    {
        cardSnList = _cardSnList;
        multiple = _multiple;
    }
}

public class InGameManager : MonoSingleton<InGameManager>
{
    public CardDeck CardDeck { get; set; } = new();
    public NxCardList BoardCards { get; set; } = new();
    public List<PlayerData> PlayerDataList { get; set; } = new();
    public Dictionary<int, List<int>> bbukList = new();
    public int CurrentTurnSlotIndex { get; set; } = 0;
    public int CurrentFirstSlotIndex { get; set; } = 0;
    public Queue<List<int>> SelectCardQueue { get; set; } = new();
    public NxCardList DragCardList { get; set; } = new();

    private NxCard hitCard = null;
    public int StealCount { get; set; } = 0;
    public GameMissionData GameMissionData { get; set; } = null;

    public List<CollectionData> CollectionWarnList = new();
    public long PointValue { get; set; } = 0;
    public int WinMultipleCount { get; set; } = 0;
    public int NextWinMultiple { get => (int)Mathf.Pow(2, WinMultipleCount); }
    public int FireMatchMultiple { get; set; } = 1;
    public bool IsNagari { get; set; } = false;
    public bool IsFireMatch { get; set; } = true;

    public bool UseFireTicket { get; set; } = false;

    public long CurrentLoseMoney { get; set; } = 0;

    public int UseStealItemCount { get; set; } = 0;

    [SerializeField] private StateMachine gameStateMachine = null;

    public PlayerData CurrentTurnPlayer
    {
        get => PlayerDataList.Where(x => x.slotIndex == CurrentTurnSlotIndex).FirstOrDefault();
    }

    public PlayerData CurrentOtherPlayer
    {
        get => PlayerDataList.Where(x => x.slotIndex != CurrentTurnSlotIndex).FirstOrDefault();
    }

    public PlayerData MyPlayer
    {
        get => PlayerDataList.Where(x => x.slotIndex == 0).FirstOrDefault();
    }

    public PlayerData OtherPlayer
    {
        get => PlayerDataList.Where(x => x.slotIndex != 0).FirstOrDefault();
    }

    public PlayerData GetPlayer(int _slotIndex)
    {
        return PlayerDataList.Where(x => x.slotIndex == _slotIndex).FirstOrDefault();
    }

    public static List<int> GetRandomCardList()
    {
        System.Random random = new();
        int chance = random.Next(100); // 0~99

        if (chance < 20)
        {
            // 20% 확률로 미션 카드
            var mission = MissionDataInfo.MissionDataList[random.Next(MissionDataInfo.MissionDataList.Count)];
            return mission.CardList;
        }
        else
        {
            // 80% 확률로 직접 선택
            var cardList = new List<CardData>();

            // KWANG 2장 조합
            var kwangs = CardDataInfo.CardDataInfoList
                .Where(card => card.MainType == CardMainType.KWANG)
                .ToList();

            // MUNG에서 GUKJUN 제외
            var mungs = CardDataInfo.CardDataInfoList
                .Where(card => card.MainType == CardMainType.MUNG && card.SubType != CardSubType.GUKJUN)
                .ToList();

            var ddis = CardDataInfo.CardDataInfoList
                .Where(card => card.MainType == CardMainType.DDI)
                .ToList();

            bool useKwang = random.Next(100) < 10; // 50% 확률로 KWANG 또는 MUNG+DDI

            if (useKwang && kwangs.Count >= 2)
            {
                cardList = kwangs.OrderBy(_ => random.Next()).Take(2).ToList();
            }
            else if (mungs.Count >= 1 && ddis.Count >= 1)
            {
                cardList.Add(mungs[random.Next(mungs.Count)]);
                cardList.Add(ddis[random.Next(ddis.Count)]);
            }
            else
            {
                // fallback: 미션카드 하나 뽑기
                var mission = MissionDataInfo.MissionDataList[random.Next(MissionDataInfo.MissionDataList.Count)];
                return mission.CardList;
            }

            cardList = cardList.OrderBy(x => x.Index).ToList();

            return cardList.Select(card => card.Index).ToList();
        }
    }

    public void MakeMissionData()
    {
        GameMissionData = new GameMissionData(GetRandomCardList(), UnityEngine.Random.Range(2, 3));
    }

    public void GameClear()
    {        
        CNetDocument.ClearInGameEvent();
        gameStateMachine.StopCoroutine();
        gameStateMachine.SetState(EGoStopEventType.EGSEVT_INITIALIZE);
    }

    public void GameInit()
    {        
        PointValue = TableDataManager.Instance.TableLevelData.GetLevelTableData(UserDataManager.Level).pointValue;

        PlayerData playerData1 = new(UserDataManager.NickName, UserDataManager.UserData.profileIndex, true, 0, UserDataManager.Money, UserDataManager.UserData.gage);

        CharacterData characterTableData = TableDataManager.Instance.TableCharacterData.GetLevelTableDataByLevel(UserDataManager.Level);

        PlayerData playerData2 = new(characterTableData.name, characterTableData.id, false, 1, UserDataManager.OtherMoney);

        PlayerDataList.Add(playerData1);
        PlayerDataList.Add(playerData2);
        
        CurrentTurnSlotIndex = 0;
        CurrentFirstSlotIndex = 0;  //선은 이기면 바꿔줌       

        CNetDocument.InGame = new(PointValue, PlayerDataList);
    }

    public void RestartGameReady()
    {
        CNetDocument.ClearInGameEvent();
        gameStateMachine.StopCoroutine();
        PointValue = TableDataManager.Instance.TableLevelData.GetLevelTableData(UserDataManager.Level).pointValue;

        PlayerData playerData1 = new(UserDataManager.NickName, UserDataManager.UserData.profileIndex, true, 0, UserDataManager.Money, UserDataManager.UserData.gage);

        CharacterData characterTableData = TableDataManager.Instance.TableCharacterData.GetLevelTableDataByLevel(UserDataManager.Level);

        PlayerData playerData2 = new(characterTableData.name, characterTableData.id, false, 1, UserDataManager.OtherMoney);

        PlayerDataList.Clear();
        PlayerDataList.Add(playerData1);
        PlayerDataList.Add(playerData2);

        CurrentTurnSlotIndex = 0;
        CurrentFirstSlotIndex = 0;  //선은 이기면 바꿔줌       

        CNetDocument.InGame.ChangeInGame(PointValue, PlayerDataList);
    }

    public void GameStart()
    {
        gameStateMachine.SetState(EGoStopEventType.EGSEVT_INITIALIZE);
    }

    public void GameLeave()
    {
        gameStateMachine = null;
    }

    private void Update()
    {
        CNetDocument.UpdateInGame();
    }

    public void SendInGameData()
    {
        CNetDocument.InGame.UpdateGameData(BoardCards, PlayerDataList);
    }

    public void ChangeTurnSlot()
    {
        CurrentTurnSlotIndex = CurrentTurnSlotIndex == 0 ? 1 : 0;
    }

    public void Clear()
    {
        UseStealItemCount = 0;
        CollectionWarnList.Clear();
        BoardCards.Clear();
        PlayerDataList.ForEach(player => player.Clear());
        SendInGameData();
    }

    public void SetDivideCards(List<int> _boardCardList, List<int> _cardList1, List<int> _cardList2)
    {
        BoardCards.SetCards(_boardCardList);
        PlayerData player1 = PlayerDataList.Where(x => x.slotIndex == CurrentFirstSlotIndex).FirstOrDefault();
        if (player1 != null)
        {
            player1.HandCards.SetCards(_cardList1);
        }

        PlayerData player2 = PlayerDataList.Where(x => x.slotIndex != CurrentFirstSlotIndex).FirstOrDefault();
        if (player2 != null)
        {
            player2.HandCards.SetCards(_cardList2);
        }

        SendInGameData();
    }

    public List<int> GetIndicesForTotal(List<int> a, int total)
    {
        List<int> indices = new List<int>();
        int sum = 0;

        // Step 1: 뒤에서부터 가능한 모든 조합을 탐색
        for (int start = a.Count - 1; start >= 0; start--)
        {
            sum = 0;
            indices.Clear();

            for (int i = start; i >= 0; i--)
            {
                sum += a[i];
                indices.Add(i);

                if (sum == total)
                {
                    // 정답 찾음 → 뒤에서부터 시작했으니 가장 적절한 조합
                    return indices;
                }

                if (sum > total)
                    break; // 이 조합은 넘침 → 다음 시도
            }
        }

        // Step 2: 정확히 못 찾으면 초과하는 최소 조합 반환 (기존 로직처럼)
        sum = 0;
        indices.Clear();
        for (int i = a.Count - 1; i >= 0; i--)
        {
            sum += a[i];
            indices.Add(i);
            if (sum >= total)
                break;
        }

        return indices;
    }

    public Tuple<bool, List<NxCard>> GetStealPlayerPee(int _total)
    {
        List<NxCard> temp = new();
        List<NxCard> list = CurrentOtherPlayer.CollectCards.GetCards(CardMainType.PEE)
            .Concat(CurrentOtherPlayer.CollectCards.GetCards(CardMainType.JOCKER))
            .OrderBy(card =>
            {
                return card.CardData.SubType switch
                {
                    CardSubType.THREE_PEE => 3,
                    CardSubType.DOUBLE_PEE => 2,
                    _ => 1
                };
            }).ToList();

        if (_total == 0)
            return new(false, temp);

        // 최적 조합 탐색 (합이 정확히 맞는 경우)
        for (int start = 0; start < list.Count; start++)
        {
            int sum = 0;
            temp.Clear();

            for (int i = start; i < list.Count; i++)
            {
                int v = list[i].CardData.SubType switch
                {
                    CardSubType.THREE_PEE => 3,
                    CardSubType.DOUBLE_PEE => 2,
                    _ => 1
                };

                if (sum + v > _total)
                    break;

                sum += v;
                temp.Add(list[i]);

                if (sum == _total)
                    return new(false, temp);
            }
        }

        // 부족하지만 최대한 근접한 조합
        temp.Clear();
        int finalSum = 0;
        foreach (var card in list)
        {
            int v = card.CardData.SubType switch
            {
                CardSubType.THREE_PEE => 3,
                CardSubType.DOUBLE_PEE => 2,
                _ => 1
            };

            finalSum += v;
            temp.Add(card);
            if (finalSum >= _total)
                break;
        }

        return new(finalSum < _total, temp);
    }


    public void SendReqSelectBoardCard(int _iSn)
    {
        DragCardList.Add(BoardCards.GetCard(_iSn));
        gameStateMachine.SetState(EGoStopEventType.EGSEVT_SELECT_CARD);
    }

    bool isHitBoardCard = false;

    public void SendReqHitCard(int _iSn, bool _shakeCheck = true)
    {
        isHitBoardCard = false;
        StealCount = 0;
        SelectCardQueue.Clear();
        DragCardList.Clear();

        hitCard = CurrentTurnPlayer.HandCards.GetCard(_iSn);

        List<NxCard> sameBoadCards = BoardCards.GetCardsEx(hitCard.SubIndex, CardMainType.JOCKER);
        int sameBoardCount = sameBoadCards.Count;

        List<NxCard> sameHandCards = CurrentTurnPlayer.HandCards.GetCards(hitCard.SubIndex);
        int sameHandCount = _shakeCheck ? sameHandCards.Count : 1;

        if (hitCard.CardData.MainType == CardMainType.BOMB)
        {
            nxEmptyBombHitCardEvent _event = new();
            _event.BombCard = hitCard.Sn;
            CurrentTurnPlayer.HandCards.Remove(hitCard.Sn);
            gameStateMachine.SetState(EGoStopEventType.EGSEVT_EMPTY_BOMB_HIT_CARD, _event);

        }
        else if (hitCard.CardData.MainType == CardMainType.JOCKER)
        {
            int flpCardIndex = CardDeck.GetCard();
            nxBonusHitCardEvent _event = new();
            _event.HitCard = hitCard.Sn;
            _event.FlipCard = CurrentTurnPlayer.slotIndex == 0 ? flpCardIndex : -1;

            CurrentTurnPlayer.HandCards.Remove(hitCard);
            CurrentTurnPlayer.CollectCards.Add(hitCard);
            CurrentTurnPlayer.HandCards.Add(new NxCard(flpCardIndex));

            Tuple<bool, List<NxCard>> stealData = GetStealPlayerPee(1);

            List<NxCard> stealCards = stealData.Item2;

            if (stealCards.Count > 0)
            {
                _event.StealCardToDragInfo = stealCards[0].Sn;
                CurrentOtherPlayer.CollectCards.Remove(stealCards[0]);
                CurrentTurnPlayer.CollectCards.Add(stealCards[0]);
            }

            if (stealData.Item1)
            {
                LockGukjun();
            }

            gameStateMachine.SetState(EGoStopEventType.EGSEVT_BONUS_HIT_CARD, _event);
        }
        else
        {
            if (sameHandCount >= 3)
            {
                //보드에 카드가 있으면 폭탄
                if (sameBoardCount == 1)
                {
                    CurrentTurnPlayer.shakeCount++;

                    nxBombHitCardEvent _event = new();
                    _event.aHitCard = sameHandCards.Select(x => x.Sn).ToList();

                    _event.aToPlayerSlot.Add(50);
                    _event.aToPlayerSlot.Add(50);
                    _event.shakeCount = CurrentTurnPlayer.shakeCount;

                    DragCardList.Add(sameBoadCards);
                    DragCardList.Add(sameHandCards);

                    BoardCards.Add(sameHandCards);

                    CurrentTurnPlayer.HandCards.Remove(sameHandCards);
                    CurrentTurnPlayer.HandCards.Add(new NxCard(50));
                    CurrentTurnPlayer.HandCards.Add(new NxCard(50));

                    gameStateMachine.SetState(EGoStopEventType.EGSEVT_BOMB_HIT_CARD, _event);

                    StealCount++;
                    isHitBoardCard = true;
                }
                //보드에 카드가 없으면 흔들것인가 선택
                else if (sameBoardCount == 0)
                {
                    nxSelectShakeCardEvent _event = new();
                    _event.aShakeCard = sameHandCards.Select(x => x.Sn).ToList();

                    gameStateMachine.SetState(EGoStopEventType.EGSEVT_SELECT_SHAKE_CARD, _event);
                }
            }
            else if (sameHandCount == 2 && sameBoardCount == 2) //스몰폭탄
            {
                nxDDaDDkBombHitCardEvent _event = new();
                _event.aHitCard = sameHandCards.Select(x => x.Sn).ToList();
                _event.aToPlayerSlot.Add(50);

                DragCardList.Add(sameBoadCards);
                DragCardList.Add(sameHandCards);

                BoardCards.Add(sameHandCards);

                CurrentTurnPlayer.HandCards.Remove(sameHandCards);
                CurrentTurnPlayer.HandCards.Add(new NxCard(50));

                gameStateMachine.SetState(EGoStopEventType.EGSEVT_DDADDAK_BOMB_HIT_CARD, _event);
                StealCount++;
                isHitBoardCard = true;
            }
            else
            {
                CurrentTurnPlayer.HandCards.Remove(hitCard);
                BoardCards.Add(hitCard);

                if (sameBoardCount == 3) //뻑을 먹음
                {
                    nxBBukSweepHitCardEvent _event = new();

                    DragCardList.Add(BoardCards.GetCards(hitCard.SubIndex));

                    _event.HitCard = hitCard.Sn;
                    _event.bSelf = CurrentTurnPlayer.BbukList.Any(x => x == hitCard.SubIndex);
                    StealCount += (_event.bSelf ? 2 : 1);
                    gameStateMachine.SetState(EGoStopEventType.EGSEVT_BBUKSWEEP_HIT_CARD, _event);
                    isHitBoardCard = true;
                }
                else
                {
                    if (sameBoardCount == 0) //빈곳에 쳐야함
                    {

                    }
                    else
                    {
                        DragCardList.Add(hitCard);
                        if (sameBoardCount == 2)
                        {
                            if (sameBoadCards.All(x => x.CardData.MainType == CardMainType.PEE && x.CardData.SubType == CardSubType.NORMAL))
                            {
                                DragCardList.Add(sameBoadCards[^1]);
                            }
                            else
                            {
                                SelectCardQueue.Enqueue(sameBoadCards.Select(x => x.Sn).ToList());
                            }
                        }
                        else
                        {
                            sameBoadCards.ForEach(card => DragCardList.Add(card));
                        }
                        isHitBoardCard = true;
                    }

                    nxGeneralHitCardEvent _event = new();
                    _event.HitCard = hitCard.Sn;

                    gameStateMachine.SetState(EGoStopEventType.EGSEVT_GENERAL_HIT_CARD, _event);
                }
            }
        }
        SendInGameData();
    }

    //private int kkk = -1;

    public void FlipCard()
    {
        //kkk++;
        //NxCard flipCard = kkk == 0 ? new(48) : (      kkk == 1 ? new(1) : new(CardDeck.GetCard()) );
        //NxCard flipCard = kkk == 1 ? new(hitCard.Sn) : new(CardDeck.GetCard());
        NxCard flipCard = new(CardDeck.GetCard());
        //NxCard flipCard = kkk==1 ? new(48) : new(CardDeck.GetCard());

        List<NxCard> sameBoadCards = BoardCards.GetCardsEx(flipCard.SubIndex, CardMainType.JOCKER);
        int sameBoardCount = sameBoadCards.Count;

        BoardCards.Add(flipCard);

        if (flipCard.CardData.MainType == CardMainType.JOCKER)
        {
            nxBonusFlipCardEvent _event = new();
            _event.FlipCard = flipCard.Sn;
            _event.SubIndex = hitCard.SubIndex;

            flipCard.SubIndex = hitCard.SubIndex;

            DragCardList.Add(flipCard);
            gameStateMachine.SetState(EGoStopEventType.EGSEVT_BONUS_FLIP_CARD, _event);
        }
        else
        {
            if (flipCard.SubIndex == hitCard.SubIndex) //뒤집은 카드가 낸 카드랑 짝이 맞음
            {
                if (CurrentTurnPlayer.HandCards.Count > 0)
                {
                    if (sameBoardCount == 1) //쪽
                    {
                        Debug.Log("쪽");
                        StealCount++;

                        nxJJokFlipCardEvent _event = new();
                        _event.FlipCard = flipCard.Sn;

                        DragCardList.Add(sameBoadCards);
                        DragCardList.Add(flipCard);

                        gameStateMachine.SetState(EGoStopEventType.EGSEVT_JJOK_FLIP_CARD, _event);
                    }
                    else if (sameBoardCount == 2)  //뻑
                    {
                        Debug.Log("뻑");
                        DragCardList.Clear();

                        CurrentTurnPlayer.bbukCount++;

                        nxBBukFlipCardEvent _event = new();
                        _event.FlipCard = flipCard.Sn;
                        _event.iBBukCount = CurrentTurnPlayer.bbukCount;

                        //뻑카운트 첫뻑
                        //작업
                        CurrentTurnPlayer.BbukList.Add(hitCard.SubIndex);
                        gameStateMachine.SetState(EGoStopEventType.EGSEVT_BBUK_FLIP_CARD, _event);
                    }
                    else //따닥
                    {
                        Debug.Log("따닥");
                        SelectCardQueue.Clear();

                        nxDDaDDakFlipCardEvent _event = new();
                        _event.FlipCard = flipCard.Sn;


                        DragCardList.Add(BoardCards.GetCards(flipCard.SubIndex));
                        StealCount++;

                        gameStateMachine.SetState(EGoStopEventType.EGSEVT_DDADDK_FLIP_CARD, _event);
                    }
                }
                else
                {
                    if (sameBoardCount == 1) //마지막 쪽
                    {
                        nxGeneralFlipCardEvent _event = new();
                        _event.FlipCard = flipCard.Sn;
                        _event.isDragCard = true;

                        DragCardList.Add(sameBoadCards);
                        DragCardList.Add(flipCard);

                        gameStateMachine.SetState(EGoStopEventType.EGSEVT_GENERAL_FLIP_CARD, _event);
                    }
                    else if (sameBoardCount == 2)  //마지막 뻑
                    {
                        nxGeneralFlipCardEvent _event = new();
                        _event.FlipCard = flipCard.Sn;
                        _event.isDragCard = true;

                        DragCardList.Add(BoardCards.GetCards(flipCard.SubIndex));
                        DragCardList.Remove(flipCard);

                        gameStateMachine.SetState(EGoStopEventType.EGSEVT_GENERAL_FLIP_CARD, _event);
                    }
                    else //마지막 따닥
                    {
                        SelectCardQueue.Clear();
                        nxGeneralFlipCardEvent _event = new();
                        _event.FlipCard = flipCard.Sn;
                        _event.isDragCard = true;
                        DragCardList.Add(BoardCards.GetCards(flipCard.SubIndex));
                        gameStateMachine.SetState(EGoStopEventType.EGSEVT_GENERAL_FLIP_CARD, _event);
                    }
                }
            }
            else  //낸 카드랑 다른 카드임
            {
                if (sameBoardCount == 3) //뻑 먹음
                {
                    Debug.Log("뻑스윕");
                    nxBBukSweepFlipCardEvent _event = new();
                    _event.FlipCard = flipCard.Sn;
                    _event.bSelf = CurrentTurnPlayer.BbukList.Any(x => x == flipCard.SubIndex);

                    DragCardList.Add(BoardCards.GetCards(flipCard.SubIndex));

                    StealCount += (_event.bSelf ? 2 : 1);

                    gameStateMachine.SetState(EGoStopEventType.EGSEVT_BBUKSWEEP_FLIP_CARD, _event);
                }
                else
                {
                    nxGeneralFlipCardEvent _event = new();
                    _event.FlipCard = flipCard.Sn;

                    if (sameBoardCount == 1) //한장 맞음
                    {
                        Debug.Log("한장맞음");
                        DragCardList.Add(BoardCards.GetCards(flipCard.SubIndex));
                        _event.isGoodStart = CurrentTurnPlayer.HandCards.Count == 9 && isHitBoardCard;
                    }
                    else if (sameBoardCount == 2) // 같은 카드가 두장이 있음
                    {
                        Debug.Log("두장있음");

                        DragCardList.Add(flipCard);
                        if (sameBoadCards.All(x => x.CardData.MainType == CardMainType.PEE && x.CardData.SubType == CardSubType.NORMAL))
                        {
                            DragCardList.Add(sameBoadCards[^1]);
                        }
                        else
                        {
                            SelectCardQueue.Enqueue(sameBoadCards.Select(x => x.Sn).ToList());
                        }
                        _event.isGoodStart = CurrentTurnPlayer.HandCards.Count == 9 && isHitBoardCard;
                    }
                    
                    _event.isDragCard = DragCardList.Count > 0;

                    gameStateMachine.SetState(EGoStopEventType.EGSEVT_GENERAL_FLIP_CARD, _event);
                }
            }
        }

        SendInGameData();
    }

    public void DragCards()
    {
        if (DragCardList.Count > 0 || StealCount > 0)
        {
            DragCardList.Log();
            StealCount += DragCardList.Cards.Count(x => x.CardData.MainType == CardMainType.JOCKER);

            Tuple<bool, List<NxCard>> stealData = GetStealPlayerPee(StealCount);

            List<NxCard> stealCards = stealData.Item2;

            if (stealData.Item1)
                LockGukjun();

            nxDragCardEvent _event = new();
            _event.aDragInfo = DragCardList.SnList;
            _event.aStealInfo = stealCards.Select(x => x.Sn).ToList();

            BoardCards.Remove(DragCardList.Cards);

            CollectionWarnList.ForEach(x =>
            {
                bool has = DragCardList.Cards.Any(card => x.all.Contains(card.Sn));
                if (has)
                    _event.removeWarnList.Add(x.collectionType);
            });

            CollectionWarnList.RemoveAll(data => _event.removeWarnList.Contains(data.collectionType));

            List<CollectionData> collectionTypeList = CollectionDataInfo.GetCollectionTypeList(DragCardList.SnList, CurrentTurnPlayer.CollectCards.SnList, CurrentOtherPlayer.CollectCards.SnList, CurrentTurnPlayer.HandCards.Count > 0);

            collectionTypeList.ForEach(x => Debug.Log(x.collectionType.ToString()));

            CollectionWarnList.AddRange(collectionTypeList.Where(x => x.checkOther).ToList());

            _event.collectionTypeList = collectionTypeList.Select(x => x.collectionType).ToList();

            CurrentTurnPlayer.CollectCards.Add(DragCardList.Cards);
            CurrentTurnPlayer.CollectCards.Add(stealCards);

            CurrentOtherPlayer.CollectCards.Remove(stealCards);

            CurrentTurnPlayer.CollectionTypeList = CollectionDataInfo.GetCollectionTypeList(CurrentTurnPlayer.CollectCards.SnList);

            CurrentTurnPlayer.CollectCards.Log();
            CurrentOtherPlayer.CollectCards.Log();
            
            if(GameMissionData != null && CurrentTurnPlayer.MissionMultiple == 1)
            {
                bool isMissionCard = DragCardList.SnList.Any(val => GameMissionData.cardSnList.Contains(val));

                if (isMissionCard)
                {
                    if (GameMissionData.cardSnList.All(val => CurrentTurnPlayer.CollectCards.SnList.Contains(val)))
                    {
                        //미션 성공
                        CurrentTurnPlayer.MissionMultiple = GameMissionData.multiple;

                        _event.missionResult = new();
                        _event.missionResult.isSuccess = true;
                        _event.missionResult.iMatchSlotIndex = CurrentTurnSlotIndex;

                    }
                    else
                    {
                        if (CurrentTurnPlayer.CollectCards.SnList.Any(val => GameMissionData.cardSnList.Contains(val)) &&
                            CurrentOtherPlayer.CollectCards.SnList.Any(val => GameMissionData.cardSnList.Contains(val)))
                        {
                            GameMissionData = null;
                            _event.missionResult = new();
                            _event.missionResult.isSuccess = false;
                        }
                    }
                }
            }
            
            SendInGameData();
            gameStateMachine.SetState(EGoStopEventType.EGSEVT_DRAG_CARD, _event);
        }
        else
        {
            gameStateMachine.SetState(EGoStopEventType.EGSEVT_DRAG_CARD);
        }
    }
   

    public void SendSkillCard(CardMainType _mainType, bool _useItem)
    {
        List<NxCard> stealCards = new();

        int stealCount = 0;// TableDataManager.Instance.TableStealItemData.GetRandomCount((StealItemType)_mainType);

        if (_mainType == CardMainType.PEE)
        {
            Tuple<bool, List<NxCard>> stealData = GetStealPlayerPee(stealCount);
            stealCards = stealData.Item2;
        }
        else
        {
            stealCards = CurrentOtherPlayer.CollectCards.GetLastElements(_mainType, stealCount);
        }

        //stealCards = CurrentOtherPlayer.CollectCards.GetLastElements(_mainType, stealCount);

        //if (_mainType == CardMainType.PEE)
        //{
        //    stealCards.AddRange(CurrentOtherPlayer.CollectCards.GetCards(CardMainType.JOCKER));
        //}


        if (stealCards.Count == 0)
            return;

        UseStealItemCount++;
        if (_useItem)
        {
            StealType stealType = (StealType)(_mainType + 1);
            UserDataManager.UseStealItem(stealType);
        }
        else
        {
            // 광고 경유 뺏기 사용 — 아이템 소모 없이 1회 사용권 부여
            StealType stealType = (StealType)(_mainType + 1);
        }

        List<int> stealCardsSnList = stealCards.Select(x => x.Sn).ToList();

        nxSkillCardEvent _event = new();
        _event.aStealInfo = stealCardsSnList;
        _event.mainType = _mainType;
        CollectionWarnList.ForEach(x =>
        {
            bool has = stealCards.Any(card => x.all.Contains(card.Sn));
            if (has)
                _event.removeWarnList.Add(x.collectionType);
        });

        CollectionWarnList.RemoveAll(data => _event.removeWarnList.Contains(data.collectionType));

        List<CollectionData> collectionTypeList = CollectionDataInfo.GetCollectionTypeList(stealCardsSnList, CurrentTurnPlayer.CollectCards.SnList, CurrentOtherPlayer.CollectCards.SnList, CurrentTurnPlayer.HandCards.Count > 0);

        collectionTypeList.ForEach(x => Debug.Log(x.collectionType.ToString()));

        CollectionWarnList.AddRange(collectionTypeList.Where(x => x.checkOther).ToList());

        _event.collectionTypeList = collectionTypeList.Select(x => x.collectionType).ToList();
        
        CurrentTurnPlayer.CollectCards.Add(stealCards);

        CurrentOtherPlayer.CollectCards.Remove(stealCards);

        CurrentTurnPlayer.CollectionTypeList = CollectionDataInfo.GetCollectionTypeList(CurrentTurnPlayer.CollectCards.SnList);

        CurrentTurnPlayer.CollectCards.Log();
        CurrentOtherPlayer.CollectCards.Log();

        if (GameMissionData != null && CurrentTurnPlayer.MissionMultiple == 1)
        {
            bool isMissionCard = stealCardsSnList.Any(val => GameMissionData.cardSnList.Contains(val));

            if (isMissionCard)
            {
                if (GameMissionData.cardSnList.All(val => CurrentTurnPlayer.CollectCards.SnList.Contains(val)))
                {
                    //미션 성공
                    CurrentTurnPlayer.MissionMultiple = GameMissionData.multiple;
                    CurrentOtherPlayer.MissionMultiple = 1;
                    _event.missionResult = new();
                    _event.missionResult.isSuccess = true;
                    _event.missionResult.iMatchSlotIndex = CurrentTurnSlotIndex;
                }
                else
                {
                    if (CurrentTurnPlayer.CollectCards.SnList.Any(val => GameMissionData.cardSnList.Contains(val)) &&
                        CurrentOtherPlayer.CollectCards.SnList.Any(val => GameMissionData.cardSnList.Contains(val)))
                    {
                        CurrentOtherPlayer.MissionMultiple = 1;
                        CurrentTurnPlayer.MissionMultiple = 1;
                        GameMissionData = null;
                        _event.missionResult = new();
                        _event.missionResult.isSuccess = false;
                    }
                }
            }
        }

        SendInGameData();
        gameStateMachine.SetState(EGoStopEventType.EGSEVT_SKILL_CARD, _event);
    }

    public void SendReqShakeResult(bool _flag)
    {
        if (!_flag)
        {
            SendReqHitCard(hitCard.Sn, false);
        }
        else
        {
            CurrentTurnPlayer.shakeCount++;

            nxShakeEvent _event = new();
            List<NxCard> sameHandCards = CurrentTurnPlayer.HandCards.GetCards(hitCard.SubIndex);
            _event.HitCard = hitCard.Sn;
            _event.aShakeCard = sameHandCards.Select(x => x.Sn).ToList();
            _event.shakeCount = CurrentTurnPlayer.shakeCount;
            gameStateMachine.SetState(EGoStopEventType.EGSEVT_SHAKE, _event);
        }
    }

    public void SendReqSelectPresident(bool _flag)
    {
        if (!_flag)
        {
            //그만 치기 리절트 화면
            List<NxCard> player1 = MyPlayer.HandCards.GetWithSameSubIndex();
            List<NxCard> player2 = OtherPlayer.HandCards.GetWithSameSubIndex();

            nxPlayerPresidentEvent _event = new();

            if (player1 != null)
            {
                _event.aPresidentCard = player1.Select(x => x.Sn).ToList();
                _event.iMatchSlotIndex = 0;

            }
            else if (player2 != null)
            {
                _event.aPresidentCard = player2.Select(x => x.Sn).ToList();
                _event.iMatchSlotIndex = 1;
            }

            gameStateMachine.SetState(EGoStopEventType.EGSEVT_PLAYER_PRESIDENT, _event);
        }
        else
        {
            //계속치기
            gameStateMachine.SetState(EGoStopEventType.EGSEVT_ROUND_MISSION);
        }
    }

    //public void SendReqUseFiretTicket()
    //{
    //    UseFireTicket = true;
    //    UserDataManager.UseFireTicket();
    //    UserDataManager.Save(true, () =>
    //    {
    //        nxFireMatchEvent _fireEvent = new();
    //        _fireEvent.characterID1 = MyPlayer.characterID;
    //        _fireEvent.characterID2 = OtherPlayer.characterID;
    //        _fireEvent.useTicket = true;
    //        FireMatchMultiple = ConfigData.FireMaxMultiple;
    //        gameStateMachine.SetState(EGoStopEventType.EGSEVT_FIRE_MATCH, _fireEvent);
    //    });
    //}

    public void SendReqNextGame(bool _multiple, bool _useAds)
    {
        IsNagari = false;
        FireMatchMultiple = 1;
        CurrentFirstSlotIndex = CurrentTurnSlotIndex;

        bool isSave = false;

        if (_multiple)
        {
            if (!_useAds)
            {
                if (WinMultipleCount == 3)
                {
                    int gold = (int)Mathf.Pow(2, WinMultipleCount - 1) * ConfigData.NextMultipleGold + ConfigData.NextMultipleGold;
                    UserDataManager.SubGold(gold);
                }
                else
                {
                    int gold = (int)Mathf.Pow(2, WinMultipleCount) * ConfigData.NextMultipleGold;
                    UserDataManager.SubGold(gold);
                }

                isSave = true;
            }

            WinMultipleCount++;
            WinMultipleCount = Mathf.Min(3, WinMultipleCount);
        }
        else
        {
            WinMultipleCount = 0;
        }

        UserDataManager.WinningStreakClear();

        if(isSave)
        {
            UIManager.Instance.ShowLoading();
            UserDataManager.Save(true, () =>
            {
                UIManager.Instance.HideLoading();
                if (UserDataManager.Money <= 0)
                {
                    gameStateMachine.SetState(EGoStopEventType.EGSEVT_BANKRUPTCY);
                }
                else
                {
                    gameStateMachine.SetState(EGoStopEventType.EGSEVT_INITIALIZE);
                }
            });
        }
        else
        {
            if (UserDataManager.Money <= 0)
            {
                gameStateMachine.SetState(EGoStopEventType.EGSEVT_BANKRUPTCY);
            }
            else
            {
                gameStateMachine.SetState(EGoStopEventType.EGSEVT_INITIALIZE);
            }
        }
    }

    public void SendReqInvalidGame()
    {
        CurrentOtherPlayer.Money.Value = CurrentOtherPlayer.Money.Value + CurrentLoseMoney;
        CurrentTurnPlayer.Money.Value = CurrentTurnPlayer.Money.Value - CurrentLoseMoney;

        CurrentTurnSlotIndex = InGameManager.Instance.CurrentFirstSlotIndex;

        UserDataManager.RestoreResult(CurrentLoseMoney);
        SendInGameData();

        gameStateMachine.SetState(EGoStopEventType.EGSEVT_INVALID_GAME);
    }

    public void SendReqInvalidGameConfirm()
    {
        gameStateMachine.SetState(EGoStopEventType.EGSEVT_INITIALIZE);
    }

    public void SendReqRevival(bool _isAd)
    {
        //gameStateMachine.SetState(EGoStopEventType.EGSEVT_INITIALIZE);
        CurrentFirstSlotIndex = CurrentTurnSlotIndex;
        long revivalMoney = _isAd ? ConfigData.AdRevivalMoney : ConfigData.RevivalMoney;
        long prevMoney = UserDataManager.Money;
        UserDataManager.Money = revivalMoney;
        
        MyPlayer.Money.Value = UserDataManager.Money;
        SendInGameData();
        UIManager.Instance.ShowLoading();
        UserDataManager.Save(true, () =>
        {
            UIManager.Instance.HideLoading();
            gameStateMachine.SetState(EGoStopEventType.EGSEVT_REVIVAL);
        });
    }

    public void SendReqNextLevel()
    {
        CNetDocument.InGame?.PushEvent(new nxBBukRewardConfirmEvent());
        UserDataManager.LevelUp();

        CharacterData data = TableDataManager.Instance.TableCharacterData.GetLevelTableDataByLevel(UserDataManager.Level);
        PlayerData playerData = new(data.name, data.id, false, 1, UserDataManager.OtherMoney);

        Action nextLevelAction = () =>
        {
            PointValue = TableDataManager.Instance.TableLevelData.GetLevelTableData(UserDataManager.Level).pointValue;

            nxNextLevelEvent _event = new();
            _event.iLevel = UserDataManager.Level;
            _event.poingValue = PointValue;
            _event.playerData = playerData;

            OtherPlayer.Refresh(data.name, data.id, false, 1, UserDataManager.OtherMoney);
            IsNagari = false;
            WinMultipleCount = 0;
            FireMatchMultiple = 1;
            gameStateMachine.SetState(EGoStopEventType.EGSEVT_NEXT_LEVEL, _event);
        };
    }

    public void SendReqNextLevelConfirm()
    {
        CurrentFirstSlotIndex = 0;
        CurrentTurnSlotIndex = CurrentFirstSlotIndex;
        gameStateMachine.SetState(EGoStopEventType.EGSEVT_INITIALIZE);
    }

    public void SendReqFireMatchConfirm(bool _multiple = false, bool _useGold = false)
    {
        if (UseFireTicket)
        {
            gameStateMachine.SetState(EGoStopEventType.EGSEVT_INITIALIZE);
        }
        else
        {
            //FireMatchMultiple = _multiple ? ConfigData.FireMaxMultiple : 2;
            UserDataManager.Gage = 0;
            if (_useGold)
            {
                UserDataManager.SubGold(ConfigData.FireMutilpleGold);
            }

            UIManager.Instance.ShowLoading();
            UserDataManager.Save(true, () =>
            {
                UIManager.Instance.HideLoading();
                gameStateMachine.SetState(EGoStopEventType.EGSEVT_INITIALIZE);
            });
        }
        //MyPlayer.GaugeValue.Value = UserDataManager.UserData.gage;        
    }

    public void SendReqSelectGoStop(bool _flag)
    {
        if (_flag)
        {
            CurrentTurnPlayer.goCount++;

            nxGoEvent _event = new();
            _event.iGoCount = CurrentTurnPlayer.goCount;
            CurrentTurnPlayer.MaxScore = CurrentTurnPlayer.CollectionScore;
            SendInGameData();
            gameStateMachine.SetState(EGoStopEventType.EGSEVT_GO, _event);
        }
        else
        {
            nxStopEvent _event = new();
            gameStateMachine.SetState(EGoStopEventType.EGSEVT_STOP, _event);
        }
    }

    public void SendReqSelectGukjunToPee(int _slotIndex, bool _flag)
    {
        PlayerData player = GetPlayer(_slotIndex);

        if (player != null)
        {
            NxCard card = player.CollectCards.GetCard(CardSubType.GUKJUN);
            if (_flag)
            {
                card.ChangeToDoublePee();
            }
            else
            {
                card.IsLock = true;
            }
        }
        SendInGameData();
        nxGukJunToPeeEvent _event = new();
        _event.iMatchSolotIndex = _slotIndex;
        _event.iUse = _flag;
        gameStateMachine.SetState(EGoStopEventType.EGSEVT_GUKJUN_TO_PEE, _event);
    }

    public bool IsBbukReward { get; set; } = false;

    public void SendReqBbukReward()
    {
        CNetDocument.InGame?.PushEvent(new nxBBukRewardConfirmEvent());
        if (IsBbukReward)
        {
            IsBbukReward = false;
            if(UserDataManager.Money == 0)
            {
                gameStateMachine.SetState(EGoStopEventType.EGSEVT_BANKRUPTCY);
            }
            else
            {
                if (CurrentTurnPlayer.bbukCount == 3)
                {                    
                    OnResult(false, 1.5f);
                }
                else
                {
                    DOVirtual.DelayedCall(0.5f, () =>
                    {
                        gameStateMachine.SetState(EGoStopEventType.EGSEVT_DRAG_CARD);
                    });
                }
            }
        }
    }

    private void LockGukjun()
    {
        NxCard nxCard = CurrentOtherPlayer.CollectCards.GetCard(CardSubType.GUKJUN);

        if (nxCard != null && !nxCard.IsLock)
        {
            nxCard.IsLock = true;
            nxGukJunToPeeEvent _event = new();
            _event.iMatchSolotIndex = CurrentOtherPlayer.slotIndex;
            _event.iUse = false;
            SendInGameData();
            CNetDocument.InGame?.PushEvent(_event);
        }
    }

    public void OnResult(bool _checkGukjun = true, float _delay = 0.0f)
    {
        StartCoroutine(OnResultCourotine(_checkGukjun, _delay));
    }

    IEnumerator OnResultCourotine(bool _checkGukjun = true, float _delay = 0.0f)
    {
        yield return new WaitForSeconds(_delay);

        if (_checkGukjun)
        {
            NxCard nxCard = CurrentOtherPlayer.CollectCards.GetCard(CardSubType.GUKJUN);

            if (nxCard != null && !nxCard.IsLock && CurrentOtherPlayer.PeeCount > 0)
            {
                nxCard.ChangeToDoublePee();
                nxGukJunToPeeEvent _event = new();
                _event.iMatchSolotIndex = CurrentOtherPlayer.slotIndex;
                _event.iUse = true;
                SendInGameData();
                CNetDocument.InGame?.PushEvent(_event);
                yield return new WaitForSeconds(GameConstants.MoveDuration + GameConstants.MoveToNextDuration);
            }
        }

        int skillMultiple = 1;

        //마지막으로 친사람이 선이 되고 현재턴은 그냥 안바까도 됨
        nxResultEvent _resultEvent = new();
        _resultEvent.netResultInfo = new();
        _resultEvent.netResultInfo.winSlotIndex = CurrentTurnSlotIndex;
        _resultEvent.netResultInfo.pointValue = PointValue;
        _resultEvent.netResultInfo.score = CurrentTurnPlayer.bbukCount == 3 ? 7 : CurrentTurnPlayer.CollectionScore;
        _resultEvent.netResultInfo.isNextFire = UserDataManager.UserData.gage >= ConfigData.FireGaugeMax;
        _resultEvent.netResultInfo.multiPle = (CurrentTurnPlayer.bbukCount == 3) ? (IsNagari ? 2 : 1) * FireMatchMultiple * NextWinMultiple : 
                                            (IsNagari ? 2 : 1)
                                            * CurrentTurnPlayer.MungMultiple
                                            * NextWinMultiple
                                            * CurrentTurnPlayer.ShakeMultiple
                                            * CurrentTurnPlayer.GoMultiple
                                            * CurrentTurnPlayer.MissionMultiple
                                            * ((CurrentOtherPlayer.IsKwangBak && CurrentTurnPlayer.KwangCount >= 3) ? 2 : 1)
                                            * ((CurrentOtherPlayer.IsPeeBak && CurrentTurnPlayer.PeeScore > 0) ? 2 : 1)
                                            * (CurrentOtherPlayer.IsGoBak ? 2 : 1)
                                            * FireMatchMultiple
                                            * skillMultiple;
        _resultEvent.netResultInfo.skillMultiple = skillMultiple;

        _resultEvent.netResultInfo.is3Bbuk = CurrentTurnPlayer.bbukCount == 3;

        long resultMoney = _resultEvent.netResultInfo.pointValue * _resultEvent.netResultInfo.score * _resultEvent.netResultInfo.multiPle;

        //if (CurrentTurnSlotIndex == 0)
        //    resultMoney = (long)(resultMoney * (decimal)(1 + UserDataManager.SkillDataDic[SkillType.MONEY_UP] * 0.01));

        _resultEvent.netResultInfo.isBankruptcy = CurrentOtherPlayer.Money.Value <= resultMoney;
        _resultEvent.netResultInfo.reward = _resultEvent.netResultInfo.isBankruptcy ? CurrentOtherPlayer.Money.Value : resultMoney;

        CurrentOtherPlayer.Money.Value = CurrentOtherPlayer.Money.Value - _resultEvent.netResultInfo.reward;
        CurrentTurnPlayer.Money.Value = CurrentTurnPlayer.Money.Value + _resultEvent.netResultInfo.reward;

        bool isNextFire = false;

        if(CurrentTurnSlotIndex == 1 && _resultEvent.netResultInfo.isBankruptcy)
        {
            //내가 파산
            UserDataManager.UserData.gage = 0;
            MyPlayer.GaugeValue.Value = UserDataManager.UserData.gage;
            FireMatchMultiple = 1;
        }
        else
        {
            if(FireMatchMultiple > 1 && UseFireTicket == false) //어우
            {
                UserDataManager.UserData.gage = 0;
                MyPlayer.GaugeValue.Value = UserDataManager.UserData.gage;
                FireMatchMultiple = 1;
            }
            else
            {
                UserDataManager.AddGauge(ConfigData.FireGaugeAddValue);
                MyPlayer.GaugeValue.Value = UserDataManager.UserData.gage;
                isNextFire = MyPlayer.GaugeValue.Value >= ConfigData.FireGaugeMax;
            }
        }

        _resultEvent.netResultInfo.isNextFire = isNextFire;

        bool isPlayerWin = CurrentTurnSlotIndex == 0;
        int finalScore = _resultEvent.netResultInfo.score * _resultEvent.netResultInfo.multiPle;
        UserDataManager.SetResult(isPlayerWin, _resultEvent.netResultInfo.reward, finalScore, CurrentTurnPlayer.goCount);
        CurrentLoseMoney = !isPlayerWin ? _resultEvent.netResultInfo.reward : 0;

        // Game Analytics — 게임 결과 + 재화 변동
        string opponentName = PlayerDataList.Count > 1 ? PlayerDataList[1].name : null;
        int opponentCharId = PlayerDataList.Count > 1 ? PlayerDataList[1].characterID : 0;
        SendInGameData();
        UseFireTicket = false;
        gameStateMachine.SetState(EGoStopEventType.EGSEVT_RESULT, _resultEvent);
    }

    public bool IsBoardSweep()
    {
        return CurrentTurnPlayer.HandCards.Count != 0 && DragCardList.Count == BoardCards.Count;
    }

    public void CheckGo()
    {
        //int totalScore = InGameManager.Instance.CurrentTurnPlayer.CollectionScore;
        if (CurrentTurnPlayer.CanGo)
        {
            CurrentTurnPlayer.HandCards.Log();
            if (CurrentTurnPlayer.HandCards.Count > 0)
            {
                nxSelectGoStopEvent _event = new();
                _event.iGoCount = CurrentTurnPlayer.goCount + 1;

                int skillMultiple = 1;

                

                int multiPle = (CurrentTurnPlayer.bbukCount == 3) ? 1 : CurrentTurnPlayer.MungMultiple
                                            * (IsNagari ? 2 : 1)
                                            * NextWinMultiple
                                            * CurrentTurnPlayer.ShakeMultiple
                                            * CurrentTurnPlayer.GoMultiple
                                            * CurrentTurnPlayer.MissionMultiple
                                            * ((CurrentOtherPlayer.IsKwangBak && CurrentTurnPlayer.KwangCount >= 3) ? 2 : 1)
                                            * ((CurrentOtherPlayer.IsRealPeeBak && CurrentTurnPlayer.PeeScore > 0) ? 2 : 1)
                                            * (CurrentOtherPlayer.IsGoBak ? 2 : 1)
                                            * FireMatchMultiple
                                            * skillMultiple;
                
                    
                long resultMoney = CurrentTurnPlayer.CollectionScore * multiPle * PointValue;
                //if (CurrentTurnSlotIndex == 0)
                //    resultMoney = (long)(resultMoney * (decimal)(1 + UserDataManager.SkillDataDic[SkillType.MONEY_UP] * 0.01));

                _event.isbankruptcy = CurrentOtherPlayer.Money.Value <= resultMoney;

                _event.nTotalMoney = _event.isbankruptcy ? CurrentOtherPlayer.Money.Value : resultMoney;
                gameStateMachine.SetState(EGoStopEventType.EGSEVT_SELECT_GOSTOP, _event);
            }
            else
            {
                nxStopEvent _event = new();
                gameStateMachine.SetState(EGoStopEventType.EGSEVT_STOP, _event);
                //OnResult(_delay: 0.5f);
            }
        }
        else
        {
            if (CurrentTurnPlayer.HandCards.Count == 0 && CurrentOtherPlayer.HandCards.Count == 0)
            {
                gameStateMachine.SetState(EGoStopEventType.EGSEVT_NAGARI);
            }
            else
            {
                ChangeTurnSlot();
                gameStateMachine.SetState(EGoStopEventType.EGSEVT_CHANGE_TURN);
            }
        }
    }

    public void SendReqProfileChange(int _characterIndex)
    {
        MyPlayer.characterID = _characterIndex;
    }

    public void SendReqNickNameChange(string _nickName)
    {
        MyPlayer.name = _nickName;
    }
}
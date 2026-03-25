using Common.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GameConstants
{
    public static readonly float[] HIT_DURATION = { 0.11f, 0.09f, 0.07f };
    public static readonly float[] FLIP_DURATION = { 0.11f, 0.09f, 0.07f };
    public static readonly float[] DRAG_DURAION = { 0.12f, 0.08f, 0.05f };
    public static readonly float[] STEAL_DURAION = { 0.12f, 0.08f, 0.05f };
    public static readonly float[] MOVE_DURAION = { 0.13f, 0.08f, 0.05f };
    public static readonly float[] FLIP_MOVE_DURATION = { 0.11f, 0.09f, 0.07f };
    public static readonly float[] FLIP_DRAG_DURATION = { 0.11f, 0.09f, 0.07f };

    public static readonly float[] MOVE_TO_NEXT = { 0.9f, 0.7f, 0.5f };
    public static readonly float[] HIT_BONUS_DELAY = { 0.06f, 0.04f, 0.02f };
    public static readonly float[] HIT_TO_NEXT = { 0.6f, 0.4f, 0.2f };
    public static readonly float[] FLIP_TO_NEXT = { 0.8f, 0.6f, 0.4f };
    public static readonly float[] NONE_DRAG = { 0.14f, 0.12f, 0.1f };

    public static readonly float STOP_DURATION = 3.0f;

    public static int PrefsValue { get => PlayerPrefsManager.Instance.GetPlayerPrefsValue(PrefsKey.GAME_SPEED, 2); }

    public static float HitDuration { get => HIT_DURATION[PrefsValue]; }
    public static float FlipDuration { get => FLIP_DURATION[PrefsValue]; }
    public static float DragDuration { get => DRAG_DURAION[PrefsValue]; }
    public static float StealDuration { get => STEAL_DURAION[PrefsValue]; }
    public static float MoveDuration { get => MOVE_DURAION[PrefsValue]; }
    public static float HitToNextDuration { get => HitDuration + HIT_TO_NEXT[PrefsValue]; }
    public static float FlipToNextDuration { get => FlipDuration + FLIP_TO_NEXT[PrefsValue]; }
    public static float MoveToNextDuration { get => MOVE_TO_NEXT[PrefsValue]; }
    public static float NoneDragDuration { get => NONE_DRAG[PrefsValue]; }

    public static float FlipMoveDuration { get => FLIP_MOVE_DURATION[PrefsValue]; }
    public static float FlipDragDuration { get => FLIP_DRAG_DURATION[PrefsValue]; }
    public static float HitBonusDelay { get => HIT_BONUS_DELAY[PrefsValue]; }
}

public enum CollectionType
{
    NONE = -1,
    BI_KWANG_3 = 0,
    KWANG_3 = 1,
    KWANG_4 = 2,
    KWANG_5 = 3,
    GODORI = 4,
    CHUNGDAN = 5,
    HONGDAN = 6,
    CHODAN = 7,
    GODORI_WARN = 8,
    CHUNGDAN_WARN = 9,
    HONGDAN_WARN = 10,
    CHODAN_WARN = 11,
    HUMMING = 12,
}

public class CollectionData
{
    public string name;
    public CardMainType mainType;
    public CollectionType collectionType;
    public List<int> all = new();
    public int essential = -1;
    public int essentialCount = 0;
    public int score = 0;
    public int group = -1;
    public bool checkOther = false;
    public float aniTime = 1.0f;

    public CollectionData(CollectionType _collectionType, string _name, CardMainType _mainType = CardMainType.NONE, List<int> _all = null, int _essential = -1, int _essentialCount = 0, int _score = 0, int _group = -1, bool _checkOther = false, float _aniTime = 1.0f)
    {
        collectionType = _collectionType;
        name = _name;
        mainType = _mainType;
        all = _all;
        essential = _essential;
        essentialCount = _essentialCount;
        group = _group;
        score = _score;
        checkOther = _checkOther;
        aniTime = _aniTime;
    }
}

public class CollectionDataInfo
{
    public static List<CollectionData> CollectionDataInfoList { get => collectionDataInfoList; }

    private static readonly List<CollectionData> collectionDataInfoList = new()
    {
        new(CollectionType.KWANG_5, "오광", CardMainType.KWANG,  new List<int>{0, 8, 28, 40, 44}, -1, 5, 15, 0, _aniTime:3),
        new(CollectionType.KWANG_4, "사광",CardMainType.KWANG,  new List<int>{0, 8, 28, 40, 44}, -1, 4, 4, 0, _aniTime:1.333f),
        new(CollectionType.KWANG_3, "삼광", CardMainType.KWANG, new List<int>{0, 8, 28, 40}, -1, 3, 3, 0, _aniTime:1.333f),
        new(CollectionType.BI_KWANG_3, "비삼광", CardMainType.KWANG, new List<int>{0, 8, 28, 40, 44}, 44, 3, 2, 0, _aniTime:1.333f),

        new(CollectionType.GODORI, "고도리",CardMainType.MUNG, new List<int>{4, 12, 29 }, -1, 3, 5, _aniTime:3),
        new(CollectionType.CHUNGDAN, "청단",CardMainType.DDI,  new List<int>{21, 33, 37 }, -1, 3, 3, _aniTime:1.333f),
        new(CollectionType.HONGDAN, "홍단",CardMainType.DDI, new List<int>{1, 5, 9 }, -1, 3, 3, _aniTime:1.333f),
        new(CollectionType.CHODAN, "초단",CardMainType.DDI, new List<int>{13, 17, 25 }, -1, 3, 3, _aniTime:1.333f),
        new(CollectionType.GODORI_WARN, "고도리비상",CardMainType.MUNG, new List<int>{4, 12, 29 }, -1, 2, 0, -1, true, _aniTime:1f),
        new(CollectionType.CHUNGDAN_WARN, "청단비상",CardMainType.DDI, new List<int>{21, 33, 37 }, -1, 2, 0, -1, true, _aniTime:1f),
        new(CollectionType.HONGDAN_WARN, "홍단비상",CardMainType.DDI, new List<int>{1, 5, 9 }, -1, 2, 0, -1, true, _aniTime:1f),
        new(CollectionType.CHODAN_WARN, "초단비상",CardMainType.DDI, new List<int>{13, 17, 25 }, -1, 2, 0, -1, true, _aniTime:1f),

        new(CollectionType.HUMMING, "",CardMainType.NONE, _aniTime:1.333f),
    };

    public static List<CollectionType> GetCollectionTypeList(List<int> _myCollection)
    {
        List<CollectionType> temp = new();
        Dictionary<int, CollectionType> groupMap = new(); // 그룹번호 -> 최고 타입 추적용

        for (int i = 0; i < collectionDataInfoList.Count; i++)
        {
            var data = collectionDataInfoList[i];

            if (data.mainType == CardMainType.NONE || data.score == 0)
                continue;

            int matchCount = data.all.Count(id => _myCollection.Contains(id));
            if (matchCount != data.essentialCount)
                continue;

            if (data.group != -1)
            {
                if (groupMap.TryGetValue(data.group, out var existingType))
                {
                    if ((int)data.collectionType > (int)existingType)
                    {
                        groupMap[data.group] = data.collectionType; // 더 높은 타입으로 교체
                    }
                    // 더 낮은 건 무시
                }
                else
                {
                    groupMap.Add(data.group, data.collectionType);
                }
            }
            else
            {
                if (!temp.Contains(data.collectionType))
                {
                    temp.Add(data.collectionType);
                }
            }
        }

        // groupMap에 모인 최고 타입들을 temp에 추가
        foreach (var type in groupMap.Values)
        {
            temp.Add(type);
        }

        return temp;
    }

    public static List<CollectionData> GetCollectionTypeList(List<int> _drag, List<int> _myCollection, List<int> _otherCollection, bool _includeWarn)
    {
        List<CollectionData> temp = new();
        List<int> combined = _drag.Concat(_myCollection).ToList();
        for (int i = 0; i < _drag.Count; i++)
        {
            for (int j = 0; j < collectionDataInfoList.Count; j++)
            {
                var data = collectionDataInfoList[j];

                if (data.mainType == CardMainType.NONE)
                    continue;

                if (data.all.Any(x => x == _drag[i]) &&
                   (data.essential == -1 || (data.essential != -1 && data.all.Any(x => x == data.essential))))
                {
                    int matchCount = data.all.Count(id => combined.Contains(id));
                    if (data.essentialCount == matchCount)
                    {
                        if (data.group != -1)
                        {
                            var existing = temp.FirstOrDefault(t => t.group == data.group);
                            if (existing != null)
                            {
                                if ((int)data.collectionType > (int)existing.collectionType)
                                {
                                    temp.Remove(existing);
                                    temp.Add(data);
                                }

                                // 현재 것이 낮거나 같으면 무시
                                continue;
                            }

                            // 그룹은 있으나 기존에 없으면 추가 조건 검토
                            if (!data.checkOther)
                            {
                                temp.Add(data);
                            }
                            else if (_includeWarn)
                            {
                                int otherCount = data.all.Count(id => _otherCollection.Contains(id));
                                if (otherCount == 0)
                                {
                                    temp.Add(data);
                                }
                            }
                        }
                        else // group == -1 인 경우
                        {
                            if (temp.Any(t => t.collectionType == data.collectionType))
                                continue;

                            if (!data.checkOther)
                            {
                                temp.Add(data);
                            }
                            else if (_includeWarn)
                            {
                                int otherCount = data.all.Count(id => _otherCollection.Contains(id));
                                if (otherCount == 0)
                                {
                                    temp.Add(data);
                                }
                            }
                        }
                    }
                }
            }
        }
        return temp;
    }
}

public enum CardMainType
{
    NONE = -1,
    KWANG = 0,
    MUNG = 1,
    DDI = 2,
    PEE = 3,
    JOCKER = 4,
    BOMB = 5,
}

public enum CardSubType
{
    NORMAL,
    BI,
    DOUBLE_PEE,
    THREE_PEE,
    CHUNG,
    HONG,
    CHODAN,
    GODORI,
    GUKJUN,
}

public class CardData
{
    public int Index { get; set; }
    public int Group { get; set; }
    public string Name { get; set; }
    public CardSubType SubType { get; set; }
    public CardMainType MainType { get; set; }
    public int Grade { get; set; }

    public int MainIndex => MainType == CardMainType.JOCKER ? 3 : (int)MainType;

    public CardData(int _index, int _group, CardMainType _type, CardSubType _subType, int _grade)
    {
        Index = _index;
        Group = _group;
        MainType = _type;
        SubType = _subType;
        Grade = _grade;
    }

    public CardData(CardData _cardData)
    {
        Index = _cardData.Index;
        Group = _cardData.Group;
        MainType = _cardData.MainType;
        SubType = _cardData.SubType;
        Grade = _cardData.Grade;
    }
}

public class CardDataInfo
{
    public static List<CardData> CardDataInfoList { get => cardDataInfoList; }

    private static readonly List<CardData> cardDataInfoList = new()
    {
        new CardData(0, 1, CardMainType.KWANG, CardSubType.NORMAL, 0),
        new CardData(1, 1, CardMainType.DDI, CardSubType.HONG, 2),
        new CardData(2, 1, CardMainType.PEE, CardSubType.NORMAL, 2),
        new CardData(3, 1, CardMainType.PEE, CardSubType.NORMAL, 2),

        new CardData(4, 2, CardMainType.MUNG, CardSubType.GODORI, 2),
        new CardData(5, 2, CardMainType.DDI, CardSubType.HONG, 2),
        new CardData(6, 2, CardMainType.PEE, CardSubType.NORMAL, 2),
        new CardData(7, 2, CardMainType.PEE, CardSubType.NORMAL, 2),

        new CardData(8, 3, CardMainType.KWANG, CardSubType.NORMAL, 0),
        new CardData(9, 3, CardMainType.DDI, CardSubType.HONG, 2),
        new CardData(10, 3, CardMainType.PEE, CardSubType.NORMAL, 2),
        new CardData(11, 3, CardMainType.PEE, CardSubType.NORMAL, 2),

        new CardData(12, 4, CardMainType.MUNG, CardSubType.GODORI, 2),
        new CardData(13, 4, CardMainType.DDI, CardSubType.CHODAN, 2),
        new CardData(14, 4, CardMainType.PEE, CardSubType.NORMAL, 2),
        new CardData(15, 4, CardMainType.PEE, CardSubType.NORMAL, 2),

        new CardData(16, 5, CardMainType.MUNG, CardSubType.NORMAL, 3),
        new CardData(17, 5, CardMainType.DDI, CardSubType.CHODAN, 2),
        new CardData(18, 5, CardMainType.PEE, CardSubType.NORMAL, 2),
        new CardData(19, 5, CardMainType.PEE, CardSubType.NORMAL, 2),

        new CardData(20, 6, CardMainType.MUNG, CardSubType.NORMAL, 3),
        new CardData(21, 6, CardMainType.DDI, CardSubType.CHUNG, 2),
        new CardData(22, 6, CardMainType.PEE, CardSubType.NORMAL, 2),
        new CardData(23, 6, CardMainType.PEE, CardSubType.NORMAL, 2),

        new CardData(24, 7, CardMainType.MUNG, CardSubType.NORMAL, 3),
        new CardData(25, 7, CardMainType.DDI, CardSubType.CHODAN, 2),
        new CardData(26, 7, CardMainType.PEE, CardSubType.NORMAL, 2),
        new CardData(27, 7, CardMainType.PEE, CardSubType.NORMAL, 2),

        new CardData(28, 8, CardMainType.KWANG, CardSubType.NORMAL, 0),
        new CardData(29, 8, CardMainType.MUNG, CardSubType.GODORI, 2),
        new CardData(30, 8, CardMainType.PEE, CardSubType.NORMAL, 2),
        new CardData(31, 8, CardMainType.PEE, CardSubType.NORMAL, 2),

        new CardData(32, 9, CardMainType.MUNG, CardSubType.GUKJUN, 0),
        new CardData(33, 9, CardMainType.DDI, CardSubType.CHUNG, 2),
        new CardData(34, 9, CardMainType.PEE, CardSubType.NORMAL, 2),
        new CardData(35, 9, CardMainType.PEE, CardSubType.NORMAL, 2),

        new CardData(36, 10, CardMainType.MUNG, CardSubType.NORMAL, 3),
        new CardData(37, 10, CardMainType.DDI, CardSubType.CHUNG, 2),
        new CardData(38, 10, CardMainType.PEE, CardSubType.NORMAL, 2),
        new CardData(39, 10, CardMainType.PEE, CardSubType.NORMAL, 2),

        new CardData(40, 11, CardMainType.KWANG, CardSubType.NORMAL, 0),
        new CardData(41, 11, CardMainType.PEE, CardSubType.DOUBLE_PEE, 0),
        new CardData(42, 11, CardMainType.PEE, CardSubType.NORMAL, 2),
        new CardData(43, 11, CardMainType.PEE, CardSubType.NORMAL, 2),

        new CardData(44, 12, CardMainType.KWANG, CardSubType.BI, 1),
        new CardData(45, 12, CardMainType.MUNG, CardSubType.NORMAL, 3),
        new CardData(46, 12, CardMainType.DDI, CardSubType.NORMAL, 3),
        new CardData(47, 12, CardMainType.PEE, CardSubType.DOUBLE_PEE, 0),

        new CardData(48, 13, CardMainType.JOCKER, CardSubType.DOUBLE_PEE, 0),
        new CardData(49, 13, CardMainType.JOCKER, CardSubType.THREE_PEE, 0),
        new CardData(50, 13, CardMainType.BOMB, CardSubType.NORMAL, 3),

        new CardData(52, 9, CardMainType.PEE, CardSubType.DOUBLE_PEE, 0),   //9 더블피 피로이동하면 인덱스가 변경
    };
}

public class CardDeck
{
    private List<int> cardList = new List<int>();
    private Stack<int> cardStack = new();

    private System.Random random = new System.Random();

    public CardDeck()
    {
        for (int i = 0; i < 50; i++)
        {
            cardList.Add(i);
        }
    }

    public void ShuffleDeck()
    {
        for (int i = cardList.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (cardList[i], cardList[j]) = (cardList[j], cardList[i]);
        }
        cardStack.Clear();
        cardList.ForEach(card => cardStack.Push(card));
    }

    public int GetCard()
    {
        if (cardStack.Count == 0)
            return -1;

        return cardStack.Pop();
    }

    public List<int> GetCard(int _count)
    {
        List<int> list = new();

        for (int i = 0; i < _count; i++)
        {
            if (cardStack.Count == 0)
            {
                return null;
            }
            list.Add(cardStack.Pop());
        }

        return list;
    }
}

public class MissionData
{
    public List<int> CardList { get; set; } = new();
    public string Name { get; set; } = "";

    public MissionData(List<int> _cardList, string _name)
    {
        CardList = _cardList;
        Name = _name;
    }
}

public class MissionDataInfo
{
    public static List<MissionData> MissionDataList { get => missionDataList; }

    private static readonly List<MissionData> missionDataList = new()
    {
        new MissionData(new List<int>{0, 8, 28}, "3광"),
        new MissionData(new List<int>{0, 8, 40}, "3광"),
        new MissionData(new List<int>{0, 8, 44}, "비3광"),
        new MissionData(new List<int>{0, 28, 40}, "3광"),
        new MissionData(new List<int>{0, 28, 44}, "비3광"),
        new MissionData(new List<int>{0, 40, 44}, "비3광"),
        new MissionData(new List<int>{8, 28, 40}, "3광"),
        new MissionData(new List<int>{8, 28, 44}, "비3광"),
        new MissionData(new List<int>{8, 40, 44}, "비3광"),
        new MissionData(new List<int>{28, 40, 44}, "비3광"),

        new MissionData(new List<int>{4, 12, 29 }, "고도리"),
        new MissionData(new List<int>{21, 33, 37 }, "청단"),
        new MissionData(new List<int>{1, 5, 9 }, "홍단"),
        new MissionData(new List<int>{13, 17, 25 }, "초단"),
    };
}

public enum EFlipType
{
    NONE = 0,
    BBUK = 1,
    BBUK_GET = 2,
    BONUS = 3,
    DDADAK = 4,
    JJOK = 5,
}


public enum EGoStopEventType
{
    EGSEVT_FIRE_MATCH,
    EGSEVT_NEXT_LEVEL,
    EGSEVT_INVALID_GAME,
    EGSEVT_INITIALIZE,

    EGSEVT_CHANGE_TURN,             // 차례변경

    EGSEVT_SHUFFLE_CARD,            // 패섞기

    EGSEVT_DIVIDE_CARD,             // 패나누기
    EGSEVT_CHANGE_BOARD_CARD,       // Board Card를 교체하기

    EGSEVT_SKILL_CARD,              // 스킬 쓰기

    // Hit
    EGSEVT_GENERAL_HIT_CARD,        // 일반적인 카드 내기
    EGSEVT_BOMB_HIT_CARD,           // 폭탄으로 카드 내기
    EGSEVT_DDADDAK_BOMB_HIT_CARD,   // 두장으로 폭탄으로 카드 내기
    EGSEVT_BBUKSWEEP_HIT_CARD,      // 뻑스윕으로 카드 내기
    EGSEVT_BONUS_HIT_CARD,          // 보너스 카드 내기
    EGSEVT_EMPTY_BOMB_HIT_CARD,     // 폭탄후 빈카드 내기

    EGSEVT_SELECT_SHAKE_CARD,       // 흔들기 선택
    EGSEVT_SHAKE,                   // 흔들기

    // Flip
    EGSEVT_GENERAL_FLIP_CARD,       // 일반적인 카드 플립
    EGSEVT_BBUK_FLIP_CARD,          // 뻑으로 카드 플립
    EGSEVT_BBUKSWEEP_FLIP_CARD,     // 뻑스윕으로 카드 플립
    EGSEVT_BONUS_FLIP_CARD,         // 보너스 카드 플립
    EGSEVT_DDADDK_FLIP_CARD,        // 따닥으로 카드 플립
    EGSEVT_JJOK_FLIP_CARD,          // 쪽으로 카드 플립

    EGSEVT_BBUK_REWARD,
    EGSEVT_BBUK_REWARD_CONFIRM,

    EGSEVT_SELECT_CARD,             // 먹을 카드 선택
    EGSEVT_BOARDSWEEP,              // 판쓸

    // Drag
    EGSEVT_DRAG_CARD,               // 먹은 카드 갖고 오기

    EGSEVT_SELECT_GUKJUN_CARD,      // 국준을 피로 쓸지 선택
    EGSEVT_GUKJUN_TO_PEE,           // 국준을 피로 사용

    // Game Result
    EGSEVT_SELECT_GOSTOP,           // 고스톱 선택
    EGSEVT_GO,                      // 고!
    EGSEVT_STOP,                    // 스톱!

    EGSEVT_NAGARI,                  // 나가리
    EGSEVT_BOARD_PRESIDENT,         // 보드 총통
    EGSEVT_SELECT_PRESIDENT,        // 플레이어 총통
    EGSEVT_PLAYER_PRESIDENT,        // 플레이어 총통
    EGSEVT_SAME_PLAYER_PRESIDENT,   // 동시 총통

    EGSEVT_ROUND_MISSION,           // 수행할 라운드 미션
    EGSEVT_ROUND_MISSION_SUCCESS,   // 라운드 미션 성공
    EGSEVT_ROUND_MISSION_FAIL,      // 라운드 미션 실패

    EGSEVT_NFY_AUTOMODE,            // 자동치기 통지

    EGSEVT_RESULT,
    EGSEVT_BANKRUPTCY,
    EGSEVT_REVIVAL,

    EGSEVT_END
}
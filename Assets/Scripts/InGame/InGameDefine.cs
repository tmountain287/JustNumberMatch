using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class nxGoStopEvent
{
    public nxGoStopEvent() { }
    public abstract EGoStopEventType  GetGoStopType();
}

public class nxFireMatchEvent : nxGoStopEvent
{
    public nxFireMatchEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_FIRE_MATCH;
    
    public int characterID1;
    public int characterID2;
    public bool useTicket;
}

public class nxNextLevelEvent : nxGoStopEvent
{
    public nxNextLevelEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_NEXT_LEVEL;

    public int iLevel = 0;
    public long poingValue = 0;
    public PlayerData playerData;
}

public class nxInvalidGameEvent : nxGoStopEvent
{
    public nxInvalidGameEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_INVALID_GAME;
}

public class nxInitEvent : nxGoStopEvent
{
    public nxInitEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_INITIALIZE;
    public int iWinMultiple = 0;
    public bool isNagari = false;
    public int iFireMatchMultiple = 0;
}

// 차례 변경
public class nxChangeTurnEvent : nxGoStopEvent
{
    public nxChangeTurnEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_CHANGE_TURN;
    public int iTurnMatchSlotIndex = -1;
}

// 패섞기
public class nxShuffleCardEvent : nxGoStopEvent
{
    public nxShuffleCardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_SHUFFLE_CARD;
}

// 패나누기
public class nxDivideCardEvent : nxGoStopEvent
{
    public nxDivideCardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_DIVIDE_CARD;
    public List<int> aBoardHolder = null;
    public List<int> aPlayerHolder0 = null;
    public List<int> aPlayerHolder1 = null;
}

// Board 카드를 교체한다
public class nxChangeBoardCardEvent : nxGoStopEvent
{
    public nxChangeBoardCardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_CHANGE_BOARD_CARD;
    public int DragInfo = -1;  // 갖고올 보너스 패
    public int FlipCard = -1;      // Flip한 카드
}

/*
** Hit
*/

public class nxSkillCardEvent : nxGoStopEvent
{
    public nxSkillCardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_SKILL_CARD;
    public CardMainType mainType = CardMainType.NONE;
    public List<int> aStealInfo = new();
    public List<CollectionType> collectionTypeList = new();
    public List<CollectionType> removeWarnList = new();
    public netMissionResult missionResult = null;
}

// 일반적인 카드 내기
public class nxGeneralHitCardEvent : nxGoStopEvent
{
    public nxGeneralHitCardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_GENERAL_HIT_CARD;
    public int HitCard = -1;
}

// 폭탄으로 카드 내기
public class nxBombHitCardEvent : nxGoStopEvent
{
    public nxBombHitCardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_BOMB_HIT_CARD;
    public List<int> aHitCard = new();       // 폭탄으로 낸 카드
    public List<int> aToPlayerSlot = new();  // 폭탄 후 채울 카드
    public int shakeCount = 0;
}

// 두장으로 폭탄
public class nxDDaDDkBombHitCardEvent : nxGoStopEvent
{
    public nxDDaDDkBombHitCardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_DDADDAK_BOMB_HIT_CARD;
    public List<int> aHitCard = new();        // 폭탄으로 낸 카드
    public List<int> aToPlayerSlot = new();   // 폭탄 후 채울 카드
}

// 뻑스윕으로 카드 내기
public class nxBBukSweepHitCardEvent : nxGoStopEvent
{
    public nxBBukSweepHitCardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_BBUKSWEEP_HIT_CARD;
    public int HitCard = -1;       // 낸 카드
    public bool bSelf = false;              // 자뻑
}

// 보너스 카드 내기
public class nxBonusHitCardEvent : nxGoStopEvent
{
    public nxBonusHitCardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_BONUS_HIT_CARD;
    public int HitCard = -1;
    public int FlipCard = -1;      // 뒤집은 카드
    public int StealCardToDragInfo = -1; // 스틸 카드 DragInfo
}

// 폭탄후 빈카드 내기
public class nxEmptyBombHitCardEvent : nxGoStopEvent
{
    public nxEmptyBombHitCardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_EMPTY_BOMB_HIT_CARD;
    public int BombCard = -1;           // 폭탄으로 인해서 플레이어가 낸 빈 카드
}

// 흔들기 선택
public class nxSelectShakeCardEvent : nxGoStopEvent
{
    public nxSelectShakeCardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }
    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_SELECT_SHAKE_CARD;
    public List<int> aShakeCard = null;      // 흔들기 선택 리스트
}

// 흔들기
public class nxShakeEvent : nxGoStopEvent
{
    public nxShakeEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_SHAKE;
    public int HitCard = -1;
    public List<int> aShakeCard = null;      // 흔들카드 리스트
    public int shakeCount = 0;                // 몇번 흔들었는지
}

/*
** Flip
*/

// 일반적인 카드 플립
public class nxGeneralFlipCardEvent : nxGoStopEvent
{
    public nxGeneralFlipCardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_GENERAL_FLIP_CARD;
    public int FlipCard = -1;
    public bool isDragCard = false;
    public bool isGoodStart = false;
}

// 뻑으로 카드 플립
public class nxBBukFlipCardEvent : nxGoStopEvent
{
    public nxBBukFlipCardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_BBUK_FLIP_CARD;
    public int FlipCard = -1;
    
    public int iBBukCount = 0;         // 뻑 카운트
}

public class nxBBukRewardEvent : nxGoStopEvent
{
    public nxBBukRewardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_BBUK_REWARD;

    public int iBBukCount = 0;         // 뻑 카운트
    public long iBBukReward = 0;         // 뻑보상치(첫뻑, 연뻑시 설정된다.)
    public bool isBankruptcy = false;
}

public class nxBBukRewardConfirmEvent : nxGoStopEvent
{
    public nxBBukRewardConfirmEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_BBUK_REWARD_CONFIRM;
}

// 뻑스윕으로 카드 플립
public class nxBBukSweepFlipCardEvent : nxGoStopEvent
{
    public nxBBukSweepFlipCardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_BBUKSWEEP_FLIP_CARD;
    public int FlipCard = -1;
    public bool bSelf = false;              // 자뻑
}

// 보너스 카드 플립
public class nxBonusFlipCardEvent : nxGoStopEvent
{
    public nxBonusFlipCardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_BONUS_FLIP_CARD;
    public int FlipCard = -1;
    public int SubIndex = -1;
}

// 따닥으로 카드 플립
public class nxDDaDDakFlipCardEvent : nxGoStopEvent
{
    public nxDDaDDakFlipCardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_DDADDK_FLIP_CARD;
    public int FlipCard = -1;
}

// 쪽으로 카드 플립
public class nxJJokFlipCardEvent : nxGoStopEvent
{
    public nxJJokFlipCardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_JJOK_FLIP_CARD;
    public int FlipCard = -1;
}

// 먹을 카드 선택
public class nxSelectCardEvent : nxGoStopEvent
{
    public nxSelectCardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_SELECT_CARD;
    public List<int> aSelectBoardSlot = null;        // 선택할 보드 카드
}

// 판쓸
public class nxBoardSweepEvent : nxGoStopEvent
{
    public nxBoardSweepEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_BOARDSWEEP;
}

/*
** Drag
*/

public class netMissionResult
{
    public int iMatchSlotIndex = -1;
    public bool isSuccess = false;
}

// 먹은 카드 갖고 오기
public class nxDragCardEvent : nxGoStopEvent
{
    public nxDragCardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_DRAG_CARD;
    public List<int> aDragInfo =  new();       // Drag한 카드 리스트
    public List<int> aStealInfo = new();      // 뺏어온 카드 리스트
    public List<CollectionType> collectionTypeList = new();
    public List<CollectionType> removeWarnList = new();
    public netMissionResult missionResult = null;
}

// 국준을 피로 쓸지 선택
public class nxSelectGukJunCardEvent : nxGoStopEvent
{
    public nxSelectGukJunCardEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_SELECT_GUKJUN_CARD;
    public int iMatchSolotIndex = -1;       // InGame Match Slot Index
}

// 국준을 피로 사용
public class nxGukJunToPeeEvent : nxGoStopEvent
{
    public nxGukJunToPeeEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_GUKJUN_TO_PEE;
    public bool iUse = false;
    public int iMatchSolotIndex = -1;       // InGame Match Slot Index
}

// 나가리
public class nxNagariEvent : nxGoStopEvent
{
    public nxNagariEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_NAGARI;
}

// 고스톱 선택
public class nxSelectGoStopEvent : nxGoStopEvent
{
    public nxSelectGoStopEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_SELECT_GOSTOP;
    public int iGoCount = 0;                   // GoCount
    public long nTotalMoney = 0;
    public bool isbankruptcy = false;
}

// Go!!
public class nxGoEvent : nxGoStopEvent
{
    public nxGoEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_GO;
    public int iGoCount = 0;               // GoCount
}

// Stop!!
public class nxStopEvent : nxGoStopEvent
{
    public nxStopEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_STOP;
}

// 보드패 4장으로 총통
public class nxBoardPresidentEvent : nxGoStopEvent
{
    public nxBoardPresidentEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_BOARD_PRESIDENT;
    public List<int> aPresidentCard = null;
}

public class nxSelectPresidentEvent : nxGoStopEvent
{
    public nxSelectPresidentEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_SELECT_PRESIDENT;
    public List<int> aPresidentCard = null;
    public int iMatchSlotIndex = -1;
}

// 유저패 4장으로 총통
public class nxPlayerPresidentEvent : nxGoStopEvent
{
    public nxPlayerPresidentEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_PLAYER_PRESIDENT;
    public List<int> aPresidentCard = null;
    public int iMatchSlotIndex = -1;
}

// 총통 동시
public class nxSamePlayerPresidentEvent : nxGoStopEvent
{
    public nxSamePlayerPresidentEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_SAME_PLAYER_PRESIDENT;

    public List<int> aPresidentCard1 = null;    //자신 플레이어
    public List<int> aPresidentCard2 = null;    //다른 플레이어
}

public class netResultInfo
{
    public int winSlotIndex = -1;
    public long pointValue = 0;
    public long reward = 0;
    public int score = 0;
    public int multiPle = 1;
    public bool isNextFire = false;
    public bool isBankruptcy = false;

    public bool isPresident = false;
    public bool is3Bbuk = false;

    public int skillMultiple = 1;
}


public class nxResultEvent : nxGoStopEvent
{
    public nxResultEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_RESULT;
    public netResultInfo netResultInfo = null;
}

public class nxBankruptcyEvent : nxGoStopEvent
{
    public nxBankruptcyEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_BANKRUPTCY;
}

public class nxRevivalEvent : nxGoStopEvent
{
    public nxRevivalEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_REVIVAL;
}

// 라운드 미션
public class netRoundMission
{
    public List<int> aMissionCard = null;        // 수집할 카드
    public int iMultiple = -1;                  // 보상 (곱하기)
    public string strMissionName;           // 미션 이름
}

// 라운드 미션 통지 이벤트
public class nxRoundMissionEvent : nxGoStopEvent
{
    public nxRoundMissionEvent() { }

    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_ROUND_MISSION;
    public netRoundMission nxRoundMission = null;                                   // 이번판 미션 
}

// 미션 이벤트 성공
public class nxRoundMissionSuccessEvent : nxGoStopEvent
{
    public nxRoundMissionSuccessEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_ROUND_MISSION_SUCCESS;
    public int iMatchSlotIndex = -1;
}

// 미션 이벤트 실패
public class nxRoundMissionFailEvent : nxGoStopEvent
{
    public nxRoundMissionFailEvent() { }
    public override EGoStopEventType GetGoStopType() { return eGoStopEventType; }

    public EGoStopEventType eGoStopEventType = EGoStopEventType.EGSEVT_ROUND_MISSION_FAIL;
}

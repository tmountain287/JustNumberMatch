using Common.Manager;
using UI.Popup;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;


public enum ItemType
{
    Gold = 0,
    Hint = 1,
    TimeAttackTicket = 2,
    Change = 3,
}


[Serializable]
public class ItemInfo
{
    public ItemType itemType;
    public int count;

    public ItemInfo(ItemType type, int count)
    {
        itemType = type;
        this.count = count;
    }
}


[Serializable]
public class TodayPlayData
{
    public string today = "";
    public int playCount = 0;
    public int maxCount = 0;

    public int RemainCount { get => maxCount - playCount; }

    public TodayPlayData(int _maxCount)
    {
        today = DateTime.Now.ToString("yyyyMMdd");
        playCount = 0;
        maxCount = _maxCount;
    }

    public bool RefreshData()
    {
        string to = DateTime.Now.ToString("yyyyMMdd");

        if (today != to)
        {
            today = to;
            playCount = 0;
            return true;
        }
        return false;
    }

    public void Play()
    {
        RefreshData();
        today = DateTime.Now.ToString("yyyyMMdd");
        playCount++;
    }
}

[Serializable]
public class DailyMissionData
{
    public string today = "";
    public int playCount = 0;
    public int maxCount = 0;
    public bool isReward = false;

    public bool IsComplete { get => maxCount <= playCount; }

    public DailyMissionData(int _maxCount)
    {
        today = DateTime.Now.ToString("yyyyMMdd");
        playCount = 0;
        maxCount = _maxCount;
    }

    public bool RefreshData()
    {
        string to = DateTime.Now.ToString("yyyyMMdd");

        if (today != to)
        {
            today = to;
            playCount = 0;
            isReward = false;
            return true;
        }
        return false;
    }

    public bool Play()
    {
        RefreshData();
        if (maxCount <= playCount)
            return false;
        today = DateTime.Now.ToString("yyyyMMdd");
        playCount++;
        return true;
    }
}

public class MainMissionData
{
    public int id;
    public bool isReward = false;

    public MainMissionData(int id)
    {
        this.id = id;
    }

    public long Count
    {
        get
        {
            return 0;
        }
    }

    public bool IsComplete 
    { 
        get 
        {
            

            return false;
        } 
    }

}

[Serializable]
public class BossStarInfo
{
    public int stageID;
    public int starCount;

    public BossStarInfo(int stageID, int starCount)
    {
        this.stageID = stageID;
        this.starCount = starCount;
    }
}

[Serializable]
public class InfiniteRecord
{
    public long bestScore;     // 랭킹에 올릴 값
    public int solvedCount;   // 몇 문제 풀었는지
    public float playTimeSec;  // 몇 초 버텼는지
    public int maxCombo;      // 최대 콤보
    public long achievedAtUtc; // 기록 달성 시간 (UTC ticks)

    public InfiniteRecord() { }

    public InfiniteRecord(long score, int solved, float time, int combo)
    {
        bestScore = score;
        solvedCount = solved;
        playTimeSec = time;
        maxCombo = combo;
        achievedAtUtc = System.DateTime.UtcNow.Ticks;
    }
}

public enum TutorialType
{
    Boss,
    TimeAttck,
    Suvival,
}

[Serializable]
public class UserData
{
    public string localShuffleId;
    public int profileIndex = 0;
    public string nickName;
    public int level = 1;
    public int xp = 0;
    public Dictionary<ItemType, int> itemInfoDic = new();  
    public Dictionary<TutorialType, bool> tutorialInfoDic = new();

    public Dictionary<MissionType, DailyMissionData> dailyMissionDataDic = new();

    public bool isAdsFree = false;

    public TodayPlayData adsRewardGoldPlay = null;
    public int dailyMissionCompleteCount = 0;
    public int redStarCount = 0;

    public List<MainMissionData> currentMissionList = new();

    public bool firstAdsOpen = false;

    /// <summary>AdsSaleRemovePopup을 처음 연 UTC 시각(ticks). 0이면 아직 기록 없음(세일 타이머 미시작).</summary>
    public long adsSaleRemovePopupFirstShownUtcTicks = 0;

    public long infiniteBestScore = 0;
    /// <summary>서바이벌 마지막 플레이 점수(최근 1회)</summary>
    public long infiniteLastScore = 0;

    public long money;
    public long otherMoney;
    public int gage;
    public bool isOtherFirstMatch;
    /// <summary>프롤로그/튜토리얼 완료 여부. 로컬 빌드 기본은 완료로 두어 인게임 진입을 막지 않음.</summary>
    public bool isPrologueComplete = true;
    public List<int> hasCollectionCharacterIndexList = new();
    public Dictionary<DifficultyType, long> timeAttackInfoDic = new();
    public Dictionary<DifficultyType, int> clearStageInfoDic = new();

    public UserData(Dictionary<ItemType, int> itemInfos)
    {
        itemInfoDic = itemInfos;     

        tutorialInfoDic.Add(TutorialType.Boss, false);
        tutorialInfoDic.Add(TutorialType.TimeAttck, false);
        tutorialInfoDic.Add(TutorialType.Suvival, false);

        for (int i = (int)MissionType.ItemUse; i < (int)MissionType.SurvivalPlay + 1; i++)
        {
            dailyMissionDataDic.Add((MissionType)i, new((int)TableDataManager.Instance.TableMissionData.GetData((MissionType)i).value));
        }    

        var groupedByTypeAndDiff = TableDataManager.Instance.TableMissionData.DataList.Where(x=>x.category == MissionCategory.Main)
            .GroupBy(m => new { m.type, m.difficultyType });

        foreach (var group in groupedByTypeAndDiff)
        {
            MissionData first = group
                .OrderBy(m => m.id)   // 또는 .First() (원본 리스트 순서가 곧 우선순위라면)
                .First();

            currentMissionList.Add(new MainMissionData(first.id));
        }

        adsRewardGoldPlay = new(ConfigData.TodayAdsRewardGoldCount);

        localShuffleId = LocalUserId.GetOrCreate();

        string result = localShuffleId[..Math.Min(5, localShuffleId.Length)];

        nickName = $"Guest{result}";

        money = 10_000L;
        otherMoney = 10_000L;
        gage = 0;
        isOtherFirstMatch = false;
        isPrologueComplete = true;
        hasCollectionCharacterIndexList = new List<int>();
        timeAttackInfoDic = new Dictionary<DifficultyType, long>();
        clearStageInfoDic = new Dictionary<DifficultyType, int>();
        foreach (DifficultyType d in Enum.GetValues(typeof(DifficultyType)))
            clearStageInfoDic[d] = 0;
    }
}

public static class UserDataManager
{
    private const string SaveKey = "mansaOK";

    public static UserData UserData { get; set; } = null;

    public static readonly Dictionary<ItemType, Action> OnValueItemChanged = new();

    public static Action<int> OnValueProfileIndexChanged = null;
    public static Action OnValueNickNameChanged = null;

    public static Action OnValueAdsFreeChanged = null;
    public static Action OnValueLevelChanged = null;
    public static Action OnValueXPChanged = null;

    public static Action OnMissionDataChanged = null;

    /// <summary>미션 조건이 충족된 순간 알람 메시지 매니저에 알림 (보상 수령 시점이 아님)</summary>
    private static void NotifyMissionComplete(MissionData missionData)
    {
        if (missionData == null || AlarmMessgeManager.Instance == null) return;
        AlarmMessgeManager.Instance.OnMessage(missionData);
    }

    private static void NotifyMainMissionsLevel(int oldLevel, int newLevel)
    {
        var table = TableDataManager.Instance.TableMissionData;
        foreach (var mainData in UserData.currentMissionList)
        {
            var data = table.GetData(mainData.id);
            if (data == null || data.type != MissionType.Level || mainData.isReward) continue;
            if (oldLevel < data.value && newLevel >= (int)data.value)
                NotifyMissionComplete(data);
        }
    }

    private static void NotifyMainMissionsStar(long oldCount, long newCount)
    {
        var table = TableDataManager.Instance.TableMissionData;
        foreach (var mainData in UserData.currentMissionList)
        {
            var data = table.GetData(mainData.id);
            if (data == null || data.type != MissionType.Star || mainData.isReward) continue;
            if (oldCount < data.value && newCount >= data.value)
                NotifyMissionComplete(data);
        }
    }

    

    private static void NotifyMainMissionsDailyComplete(int oldCount, int newCount)
    {
        var table = TableDataManager.Instance.TableMissionData;
        foreach (var mainData in UserData.currentMissionList)
        {
            var data = table.GetData(mainData.id);
            if (data == null || data.type != MissionType.DailyMissionComplete || mainData.isReward) continue;
            if (oldCount < data.value && newCount >= data.value)
                NotifyMissionComplete(data);
        }
    }

    public static Action<int> OnAdsRewardGoldChanged = null;

    public static Action OnValueFirstAdsOpenChanged = null;

    public static int openAdCount = 0;
    public static int stagePlayCount = 0;

    public static int MainMissionCompleteCount = 0;

    public static MissionData MissionRewared(int _id)
    {
        MissionData m = null;
        MissionData missionData = TableDataManager.Instance.TableMissionData.GetData(_id);
        m = missionData;
        if (missionData.category == MissionCategory.Daily)
        {
            UserData.dailyMissionDataDic[missionData.type].isReward = true;
            // 메인 미션 타입 DailyMissionComplete(일일 미션 N회 달성)용: 테이블상 DailyMissionComplete 행은 Main이며,
            // 실제 카운트는 '데일리 탭' 미션(ItemUse 등) 보상 수령 시마다 증가해야 함.
            int oldDailyComplete = UserData.dailyMissionCompleteCount;
            UserData.dailyMissionCompleteCount++;
            NotifyMainMissionsDailyComplete(oldDailyComplete, UserData.dailyMissionCompleteCount);
        }
        else
        {
            MainMissionData mainMissionData = UserData.currentMissionList.FirstOrDefault(x => x.id == _id);

            MissionData nextData = TableDataManager.Instance.TableMissionData.GetNextData(_id, missionData.difficultyType);

            if (nextData != null)
                mainMissionData.id = nextData.id;
            else
                mainMissionData.isReward = true;
            m = nextData;
        }
        AddItemCount(missionData.rewardItemType, missionData.rewardValue);
        
        OnMissionDataChanged?.Invoke();
        return m;
    }

    /// <summary>
    /// DEBUG: 강제 스테이지 클리어 시 보스 레드스타·(신규 구간) 경험치를 반영합니다.
    /// randomBossRedStars가 false면 보스는 항상 3성.
    /// clearStageInfoDic을 바꾸기 전의 previousClearedTableId를 넘겨야 재실행 시 redStarCount가 맞습니다.
    /// </summary>
   

    /// <summary>보상 광고 시청 시 호출. 전면 광고 노출까지 남은 플레이 횟수 조건을 초기화한다.</summary>
    public static void ResetInterstitialCondition()
    {
        stagePlayCount = 0;
        openAdCount = 0;
    }

    public static void PlayStage()
    {
        if (!UserData.isAdsFree && Level >= ConfigData.AdOpenLevel)
        {
            if (openAdCount == 0)
            {
                openAdCount = UnityEngine.Random.Range(ConfigData.AdOpenCountMin, ConfigData.AdOpenCountMax);
            }

            stagePlayCount++;

            if (openAdCount < stagePlayCount)
            {
                GoogleAdManager.Instance.ShowInterstitialAd();
                stagePlayCount = 0;
                openAdCount = 0;
                FirstAdsOpen = true;
            }
        }
    }
    
    public static bool FirstAdsOpen
    {
        get => UserData.firstAdsOpen;
        set
        {
            UserData.firstAdsOpen = value;
            OnValueFirstAdsOpenChanged?.Invoke();
        }
    }

    private static bool premiumSalePopupShownThisSession = false;

    /// <summary>전면 광고 최초 노출 후, 광고 제거 미구매이며 세일 기간 내면 true.</summary>
    public static bool ShouldOfferPremiumPackSale()
    {
        if (UserData == null) return false;
        if (UserData.isAdsFree) return false;
        if (!UserData.firstAdsOpen) return false;

        if (UserData.adsSaleRemovePopupFirstShownUtcTicks == 0)
            return true;

        var start = new DateTime(UserData.adsSaleRemovePopupFirstShownUtcTicks, DateTimeKind.Utc);
        return (DateTime.UtcNow - start).TotalSeconds < ConfigData.PremiumPackSaleDurationSeconds;
    }

    /// <summary>팝업을 처음 띄운 시점에 호출해 세일 종료 시각 기준을 잡는다.</summary>
    public static void MarkAdsSaleRemovePopupFirstShownIfNeeded()
    {
        if (UserData == null) return;
        if (UserData.adsSaleRemovePopupFirstShownUtcTicks != 0) return;
        UserData.adsSaleRemovePopupFirstShownUtcTicks = DateTime.UtcNow.Ticks;
        Save(false);
    }

    public static TimeSpan GetPremiumPackSaleRemainingTimeSpan()
    {
        if (UserData == null) return TimeSpan.Zero;
        if (UserData.adsSaleRemovePopupFirstShownUtcTicks == 0)
            return TimeSpan.FromSeconds(ConfigData.PremiumPackSaleDurationSeconds);

        var start = new DateTime(UserData.adsSaleRemovePopupFirstShownUtcTicks, DateTimeKind.Utc);
        var end = start.AddSeconds(ConfigData.PremiumPackSaleDurationSeconds);
        var rem = end - DateTime.UtcNow;
        return rem <= TimeSpan.Zero ? TimeSpan.Zero : rem;
    }

    /// <summary>세일 팝업을 이번 앱 실행에서 한 번만 시도한다. STAGE 진입 시 호출.</summary>
    public static void TryShowPremiumSalePopupOnFirstEligibleSession()
    {
        if (premiumSalePopupShownThisSession) return;
        if (!ShouldOfferPremiumPackSale()) return;
        if (PopupManager.Instance == null) return;

        premiumSalePopupShownThisSession = true;
        PopupManager.Instance.OpenPopup<AdSaleRemovePopup>();
    }

    public static string NickName
    {
        get => UserData.nickName;
        set
        {
            UserData.nickName = value;
            OnValueNickNameChanged?.Invoke();
        }
    }

    public static int ProfileIndex
    {
        get => UserData.profileIndex;
        set
        {
            UserData.profileIndex = value;
            OnValueProfileIndexChanged?.Invoke(value);
        }
    }

    public static int Level
    {
        get => UserData.level;
        set
        {
            UserData.level = value;
            OnValueLevelChanged?.Invoke();
        }
    }

    public static int XP
    {
        get => UserData.xp;
        set
        {
            UserData.xp = value;
            OnValueXPChanged?.Invoke();
        }
    }

    public static bool IsAdsFree
    {
        get => UserData != null && UserData.isAdsFree;
        set
        {
            UserData.isAdsFree = value;
            if (value)
                GameAnalyticsHelper.SetHasRemovedAds(true);
            OnValueAdsFreeChanged?.Invoke();
        }
    }

   

    public static void RefreshAdsRewardGold()
    {
        if (UserData.adsRewardGoldPlay.RefreshData())
        {
            if (UserData.dailyMissionDataDic.TryGetValue(MissionType.AdsFreeGold, out var adsGoldMission))
                adsGoldMission.RefreshData();
            OnAdsRewardGoldChanged?.Invoke(UserData.adsRewardGoldPlay.RemainCount);
            OnMissionDataChanged?.Invoke();
            Save();
        }
    }

    public static void PlayAdsRewardGold()
    {
        PlayDailyMission(MissionType.AdsFreeGold);
        UserData.adsRewardGoldPlay.Play();
        AddItemCount(ItemType.Gold, ConfigData.AdsRewardGold);
        GameAnalyticsHelper.LogEarnVirtualCurrency("gold", ConfigData.AdsRewardGold, "ad");
        OnAdsRewardGoldChanged?.Invoke(UserData.adsRewardGoldPlay.RemainCount);
        Save();
    }   

    public static void AddItemValueChangeEvent(ItemType type, Action action)
    {
        if (!OnValueItemChanged.ContainsKey(type))
            OnValueItemChanged.Add(type, action);
        else
            OnValueItemChanged[type] += action;
    }

    public static void RemoveItemValueChangeEvent(ItemType type, Action action)
    {
        if (OnValueItemChanged.ContainsKey(type))
            OnValueItemChanged[type] -= action;
    }

    public static void AddItemCount(Dictionary<ItemType, int> _rewardItemDic)
    {
        if (_rewardItemDic == null)
            return;
        foreach (var item in _rewardItemDic)
        {
            AddItemCount(item.Key, item.Value);
        }
    }

    public static void AddItemCount(ItemType type, int count)
    {
        UserData.itemInfoDic[type] += count;
        if (OnValueItemChanged.TryGetValue(type, out var d)) d?.Invoke();
    }

    public static void SubItemCount(ItemType type, int count)
    {
        UserData.itemInfoDic[type] -= count;
        if (OnValueItemChanged.TryGetValue(type, out var d)) d?.Invoke();
    }

    public static void SetItemCount(ItemType type, int count)
    {
        UserData.itemInfoDic[type] = count;
        if (OnValueItemChanged.TryGetValue(type, out var d)) d?.Invoke();
    }

    public static int GetItemCount(ItemType type)
    {
        return UserData.itemInfoDic.TryGetValue(type, out int c) ? c : 0;
    }

    private static void MigrateUserDataFieldsIfMissing()
    {
        if (UserData == null) return;
        UserData.hasCollectionCharacterIndexList ??= new List<int>();
        UserData.timeAttackInfoDic ??= new Dictionary<DifficultyType, long>();
        UserData.clearStageInfoDic ??= new Dictionary<DifficultyType, int>();
        foreach (DifficultyType d in Enum.GetValues(typeof(DifficultyType)))
        {
            if (!UserData.clearStageInfoDic.ContainsKey(d))
                UserData.clearStageInfoDic[d] = 0;
        }
        // 구 세이브: isPrologueComplete 필드가 없으면 역직렬화 시 false로 들어올 수 있음 → 로컬 플레이 허용
        if (!PlayerPrefs.HasKey("ud_migrate_prologue_complete_v1"))
        {
            UserData.isPrologueComplete = true;
            PlayerPrefs.SetInt("ud_migrate_prologue_complete_v1", 1);
            PlayerPrefs.Save();
        }
    }

    public static bool Load()
    {
        string json = SecurePlayerPrefs.GetString(SaveKey, null);
        if (string.IsNullOrEmpty(json))
        {
            //UserData = new("나", ConfigData.InitMoney, ConfigData.InitGold, TableDataManager.Instance.TableLevelData.GetLevelTableData(1).money, ConfigData.InitCharacterID);
            UserData = new(ConfigData.InititemInfoDic);
           
            return false;
        }

        UserData = JsonConvert.DeserializeObject<UserData>(json);
        MigrateUserDataFieldsIfMissing();
       
        SendGPGReport();
        SendGPGAchievement();
        GameAnalyticsHelper.SetUserLevel(Level);
        GameAnalyticsHelper.SetHasRemovedAds(IsAdsFree);
        return true;
    }

    public static void ClearData()
    {
        PlayerPrefs.DeleteKey(SaveKey);
    }



    public static void SetForceUserData(UserData _userData)
    {
        UserData = _userData;
        Save(false);
        Load();

        foreach (var item in OnValueItemChanged)
        {
            item.Value.Invoke();
        }
        OnValueAdsFreeChanged?.Invoke();
        OnValueLevelChanged?.Invoke();
        OnValueXPChanged?.Invoke();
        OnAdsRewardGoldChanged?.Invoke(UserData.adsRewardGoldPlay.RemainCount);
    }

    public static void Save(bool _check = true, Action _onComplete = null)
    {
        string json = JsonConvert.SerializeObject(UserData);
        SecurePlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
        SendGPGReport();
        SendGPGAchievement();
        if (FirebaseManager.Instance.IsLinking)
        {
            AsyncSaveData(_check, _onComplete);
        }
        else
        {
            _onComplete?.Invoke();
        }
    }

    private static async void AsyncSaveData(bool _check, Action _onComplete = null)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            _onComplete?.Invoke();
            return;
        }

        var result = await NetworkManager.Instance.SaveUserDataAsync(useTimeout: true);

        if (!_check)
        {
            _onComplete?.Invoke();
            return;
        }

        // 서버 응답 실패 시에도 로컬은 이미 Save()에서 저장됨 → 콜백 누락 시 결과 팝업 등이 영구 미표시됨
        if (result == null)
        {
            _onComplete?.Invoke();
            return;
        }
        else
        {
            string data = SecurePlayerPrefs.Encrypt(UserDataManager.UserData);

            if (result.Type == SaveResultType.Success)
            {
                _onComplete?.Invoke();
            }
            else if (result.Type == SaveResultType.PermissionDenied && result.ConflictRecord != null)
            {
                if (result.ConflictRecord != data)
                {
                    UserData beforeData = JsonConvert.DeserializeObject<UserData>(SecurePlayerPrefs.Decrypt(result.ConflictRecord));
                    PopupManager.Instance.OpenPopup<SelectUserPopup>().Initialize(beforeData, UserDataManager.UserData, (isCurrent) =>
                    {
                        if (!isCurrent)
                        {
                            PopupManager.Instance.AllClosePopup(PopupType.NONE);
                            //InGameManager.Instance.RestartGameReady();
                            //InGameManager.Instance.GameStart();
                        }
                        else
                        {
                            _onComplete?.Invoke();
                        }
                    });
                }
                else
                {
                    _onComplete?.Invoke();
                }
            }
            else
            {
                _onComplete?.Invoke();
            }
        }
    }

    //처음 클리어면 True 리턴
   
    public static bool PlayDailyMission(MissionType _type)
    {
        var dailyData = UserData.dailyMissionDataDic[_type];
        bool wasComplete = dailyData.IsComplete;
        bool save = dailyData.Play();
        if (!wasComplete && dailyData.IsComplete && !dailyData.isReward)
        {
            var missionData = TableDataManager.Instance.TableMissionData.GetData(_type);
            NotifyMissionComplete(missionData);
        }
        OnMissionDataChanged?.Invoke();
        return save;
    }

    /// <summary>구글 플레이 리더보드 ID (타임어택=시간 ms, 서바이벌=점수)</summary>
    private const string LeaderboardTimeAttackEasy = "CgklmY6sOPgWEAIQBQ";
    private const string LeaderboardTimeAttackNormal = "CgklmY6s0PgWEAIQBg";
    private const string LeaderboardTimeAttackHard = "CgklmY6s0PgWEAIQBW";
    private const string LeaderboardSurvival = "CgklmY6s0PgWEAIQCA";

    public static long Gold => UserData != null ? GetItemCount(ItemType.Gold) : 0L;

    public static long Money
    {
        get => UserData != null ? UserData.money : 0L;
        set
        {
            if (UserData != null)
                UserData.money = value;
        }
    }

    public static long OtherMoney
    {
        get => UserData != null ? UserData.otherMoney : 0L;
        set
        {
            if (UserData != null)
                UserData.otherMoney = value;
        }
    }

    public static int Gage
    {
        get => UserData != null ? UserData.gage : 0;
        set
        {
            if (UserData != null)
                UserData.gage = value;
        }
    }

    public static void AddGauge(int add)
    {
        if (UserData == null) return;
        UserData.gage = Math.Min(ConfigData.FireGaugeMax, UserData.gage + add);
    }

    public static void SubGold(int gold)
    {
        if (gold <= 0 || UserData == null) return;
        SubItemCount(ItemType.Gold, gold);
    }

    public static void UseStealItem(StealType stealType)
    {
        // 스틸 티켓 전용 ItemType 연동 시 SubItemCount 처리
    }

    public static void WinningStreakClear()
    {
    }

    public static void RestoreResult(long loseMoney)
    {
        if (UserData == null || InGameManager.Instance == null) return;
        UserData.money = InGameManager.Instance.MyPlayer.Money.Value;
        UserData.otherMoney = InGameManager.Instance.OtherPlayer.Money.Value;
    }

    public static void ApplyMatchResultMoney(bool isCurrentPlayerSlot0, long reward, Action onComplete = null)
    {
        if (UserData == null)
        {
            onComplete?.Invoke();
            return;
        }
        if (InGameManager.Instance != null)
        {
            UserData.money = InGameManager.Instance.MyPlayer.Money.Value;
            UserData.otherMoney = InGameManager.Instance.OtherPlayer.Money.Value;
        }
        Save(true, onComplete);
    }

    public static void SetResult(bool isPlayerWin, long reward)
    {
        SetResult(isPlayerWin, reward, 0, 0);
    }

    public static void SetResult(bool isPlayerWin, long reward, int finalScore, int goCount)
    {
        if (UserData == null) return;
        if (InGameManager.Instance != null)
        {
            UserData.money = InGameManager.Instance.MyPlayer.Money.Value;
            UserData.otherMoney = InGameManager.Instance.OtherPlayer.Money.Value;
        }
        Save(false);
    }

    public static void LevelUp()
    {
        if (UserData == null || TableDataManager.Instance == null) return;
        int prev = Level;
        Level = prev + 1;
        GameAnalyticsHelper.LogLevelUp(Level, prev);
        NotifyMainMissionsLevel(prev, Level);
    }

    public static void AddViewCharacter(int characterTableId)
    {
        if (UserData == null) return;
        if (!UserData.hasCollectionCharacterIndexList.Contains(characterTableId))
            UserData.hasCollectionCharacterIndexList.Add(characterTableId);
    }

    public static void SendGPGReport()
    {
#if !UNITY_EDITOR && (SERVICE || ALPHA)
        // 타임 어택: 난이도별 최고 기록(시간 ms) — 0이면 전송 안 함, 리더보드에서 "낮을수록 좋음" 설정
        if (UserData.timeAttackInfoDic.TryGetValue(DifficultyType.Easy, out long easyMs) && easyMs > 0)
            PlatformSocialManager.Instance.ReportScore(easyMs, LeaderboardTimeAttackEasy);
        if (UserData.timeAttackInfoDic.TryGetValue(DifficultyType.Normal, out long normalMs) && normalMs > 0)
            PlatformSocialManager.Instance.ReportScore(normalMs, LeaderboardTimeAttackNormal);
        if (UserData.timeAttackInfoDic.TryGetValue(DifficultyType.Hard, out long hardMs) && hardMs > 0)
            PlatformSocialManager.Instance.ReportScore(hardMs, LeaderboardTimeAttackHard);

        // 서바이벌: 최고 점수
        if (UserData.infiniteBestScore > 0)
            PlatformSocialManager.Instance.ReportScore(UserData.infiniteBestScore, LeaderboardSurvival);
#endif
    }

    public static void SendGPGAchievement()
    {
#if !UNITY_EDITOR && (SERVICE || ALPHA)
        var achievementList = TableDataManager.Instance.TableGPGAchievementData
            .GetAchievementsByTypeAndValue(GPGAchievementType.Level, Level);
        foreach (var data in achievementList)
        {
            PlatformSocialManager.Instance.UnlockAchievement(data.id);
        }
#endif
    }
}
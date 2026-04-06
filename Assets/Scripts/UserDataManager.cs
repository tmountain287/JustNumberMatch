using Common.Manager;
using JustOneMatch.UI;
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
            MissionData data = TableDataManager.Instance.TableMissionData.GetData(id);

            if (data.type == MissionType.DailyMissionComplete)
            {
                return UserDataManager.UserData.dailyMissionCompleteCount;
            }
            else if (data.type == MissionType.Level)
            {
                return UserDataManager.Level;
                    
            }
            else if (data.type == MissionType.Star)
            {
                return UserDataManager.UserData.redStarCount;
            }
            else if (data.type == MissionType.TimeAttack)
            {
                if (UserDataManager.UserData.timeAttackInfoDic.ContainsKey((DifficultyType)data.difficultyType))
                {
                    return UserDataManager.UserData.timeAttackInfoDic[(DifficultyType)data.difficultyType];
                }
                else
                    return -1;
            }
            else if (data.type == MissionType.Survival)
            {
                return UserDataManager.UserData.infiniteBestScore;
            }

            return 0;
        }
    }

    public bool IsComplete 
    { 
        get 
        {
            MissionData data = TableDataManager.Instance.TableMissionData.GetData(id);

            if (data.type == MissionType.DailyMissionComplete)
            {
                if (data.value <= UserDataManager.UserData.dailyMissionCompleteCount)
                    return true;
            }
            else if (data.type == MissionType.Level)
            {
                if (data.value <= UserDataManager.Level)
                    return true;
            }
            else if (data.type == MissionType.Star)
            {
                if (data.value <= UserDataManager.UserData.redStarCount)
                    return true;
            }
            else if (data.type == MissionType.TimeAttack)
            {
                if (UserDataManager.UserData.timeAttackInfoDic.ContainsKey((DifficultyType)data.difficultyType))
                {
                    if (UserDataManager.UserData.timeAttackInfoDic[(DifficultyType)data.difficultyType] < data.value)
                        return true;
                }
            }
            else if (data.type == MissionType.Survival)
            {
                if (data.value <= UserDataManager.UserData.infiniteBestScore)
                    return true;
            }

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
    public Dictionary<DifficultyType, int> clearStageInfoDic = new();
    public Dictionary<DifficultyType, List<BossStarInfo>> bossStarInfoDic = new();
    public Dictionary<DifficultyType, long> timeAttackInfoDic = new();
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

    public UserData(Dictionary<ItemType, int> itemInfos)
    {
        itemInfoDic = itemInfos;

        clearStageInfoDic.Add(DifficultyType.Easy, 0);
        clearStageInfoDic.Add(DifficultyType.Normal, 0);
        clearStageInfoDic.Add(DifficultyType.Hard, 0);

        bossStarInfoDic.Add(DifficultyType.Easy, new());
        bossStarInfoDic.Add(DifficultyType.Normal, new());
        bossStarInfoDic.Add(DifficultyType.Hard, new());

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

    private static void NotifyMainMissionsTimeAttack(DifficultyType difficulty, long oldTimeMs, long newTimeMs)
    {
        var table = TableDataManager.Instance.TableMissionData;
        foreach (var mainData in UserData.currentMissionList)
        {
            var data = table.GetData(mainData.id);
            if (data == null || data.type != MissionType.TimeAttack || data.difficultyType != (int)difficulty || mainData.isReward) continue;
            bool wasComplete = oldTimeMs >= 0 && oldTimeMs < data.value;
            bool nowComplete = newTimeMs >= 0 && newTimeMs < data.value;
            if (!wasComplete && nowComplete)
                NotifyMissionComplete(data);
        }
    }

    private static void NotifyMainMissionsSurvival(long oldScore, long newScore)
    {
        var table = TableDataManager.Instance.TableMissionData;
        foreach (var mainData in UserData.currentMissionList)
        {
            var data = table.GetData(mainData.id);
            if (data == null || data.type != MissionType.Survival || mainData.isReward) continue;
            if (oldScore < data.value && newScore >= data.value)
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
    public static void ApplyDebugForcedStageProgress(DifficultyType difficulty, int lastClearedTableId, int previousClearedTableId, bool randomBossRedStars)
    {
        if (UserData == null || TableDataManager.Instance?.TableStageData?.StageTableDataDic == null)
            return;

        if (!UserData.bossStarInfoDic.TryGetValue(difficulty, out var starList) || starList == null)
            return;

        if (!TableDataManager.Instance.TableStageData.StageTableDataDic.TryGetValue(difficulty, out var stageList) || stageList == null)
            return;

        var bossesInRange = stageList.Where(x => x.stageType == StageType.Boss && x.id <= lastClearedTableId).ToList();

        var bossStarsById = new Dictionary<int, int>();
        foreach (var b in bossesInRange)
        {
            int s = randomBossRedStars ? UnityEngine.Random.Range(1, 4) : 3;
            bossStarsById[b.id] = s;
        }

        void SubtractBossContribution(int bossTableId, bool wasClearedBefore)
        {
            BossStarInfo info = starList.FirstOrDefault(x => x.stageID == bossTableId);
            if (info != null)
            {
                UserData.redStarCount -= info.starCount;
                starList.Remove(info);
            }
            else if (wasClearedBefore)
                UserData.redStarCount -= 3;
        }

        if (lastClearedTableId < previousClearedTableId)
        {
            var lostBosses = stageList.Where(x =>
                x.stageType == StageType.Boss &&
                x.id > lastClearedTableId &&
                x.id <= previousClearedTableId).ToList();
            foreach (var b in lostBosses)
                SubtractBossContribution(b.id, true);
        }

        foreach (var b in bossesInRange)
        {
            bool wasCleared = b.id <= previousClearedTableId;
            SubtractBossContribution(b.id, wasCleared);
        }

        long oldRed = UserData.redStarCount;
        foreach (var b in bossesInRange)
        {
            int s = bossStarsById[b.id];
            UserData.redStarCount += s;
            if (s < 3)
                starList.Add(new BossStarInfo(b.id, s));
        }

        if (UserData.redStarCount < 0)
            UserData.redStarCount = 0;

        NotifyMainMissionsStar(oldRed, UserData.redStarCount);

        if (lastClearedTableId > previousClearedTableId)
        {
            var newlyCleared = stageList
                .Where(x => x.id > previousClearedTableId && x.id <= lastClearedTableId)
                .OrderBy(x => x.id)
                .ToList();

            foreach (var st in newlyCleared)
            {
                if (st.stageType == StageType.Normal)
                {
                    Dictionary<ItemType, int> levelReward = AddXP(st.starMax * 10);
                    AddItemCount(levelReward);
                }
                else
                {
                    int stars = bossStarsById[st.id];
                    Dictionary<ItemType, int> levelReward = AddXP(stars * ((int)st.difficultyType + 1) * 10);
                    AddItemCount(levelReward);
                }
            }
        }

        OnMissionDataChanged?.Invoke();
    }

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
            OnValueProfileIndexChanged.Invoke(value);
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

    public static bool UpdateInfiniteRecord(long _score)
    {
        UserData.infiniteLastScore = _score;

        if (UserData.infiniteBestScore <= _score && _score > 0)
        {
            long oldScore = UserData.infiniteBestScore;
            UserData.infiniteBestScore = _score;
            NotifyMainMissionsSurvival(oldScore, _score);
            Save();
            return true;
        }

        Save();
        return false;
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

    public static Dictionary<ItemType, int> AddXP(int _value)
    {
        if (TableDataManager.Instance.TableLevelData.LastLevel == Level)
            return null;

        LevelData data = TableDataManager.Instance.TableLevelData.GetTableData(Level);

        int xp = XP + _value;

        if (data.xp <= xp)
        {
            int prevLevel = Level;
            Level++;
            XP = xp - data.xp;
            GameAnalyticsHelper.LogLevelUp(Level, prevLevel);
            GameAnalyticsHelper.SetUserLevel(Level);
            NotifyMainMissionsLevel(prevLevel, Level);

            LevelData data2 = TableDataManager.Instance.TableLevelData.GetTableData(Level);

            return data2.rewardItemDic;
        }
        else
        {
            XP = xp;
            return null;
        }
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
        return UserData.itemInfoDic[type];
    }

    public static bool Load()
    {
        string json = SecurePlayerPrefs.GetString(SaveKey, null);
        if (string.IsNullOrEmpty(json))
        {
            //UserData = new("나", ConfigData.InitMoney, ConfigData.InitGold, TableDataManager.Instance.TableLevelData.GetLevelTableData(1).money, ConfigData.InitCharacterID);
            UserData = new(ConfigData.InititemInfoDic);
            TableDataManager.Instance.TableEquationData.MakeEquationData(UserData.localShuffleId);
            return false;
        }

        UserData = JsonConvert.DeserializeObject<UserData>(json);
        TableDataManager.Instance.TableEquationData.MakeEquationData(UserData.localShuffleId);        
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

    public static Tuple<bool, long> SetTimeAttackRecord(DifficultyType _difficultyType, long _lab)
    {
        // 총 클리어 시간(ms). 0 이하는 측정 오류/비정상 — 저장·리더보드 반영 금지
        if (_lab <= 0)
        {
            long existing = UserData.timeAttackInfoDic.TryGetValue(_difficultyType, out var ex) ? ex : -1L;
            Debug.LogWarning($"[TimeAttack] Ignored invalid total ms ({_lab}) for {_difficultyType}");
            return Tuple.Create(false, existing);
        }

        long oldLab = UserData.timeAttackInfoDic.TryGetValue(_difficultyType, out var prev) ? prev : -1;
        bool save = PlayDailyMission(MissionType.TimeAttackPlay);

        if (!UserData.timeAttackInfoDic.ContainsKey(_difficultyType))
        {
            Debug.Log(_lab);
            UserData.timeAttackInfoDic.Add(_difficultyType, _lab);
            NotifyMainMissionsTimeAttack(_difficultyType, oldLab, _lab);
            Save();
            return new(true, -1);

        }
        else if (UserData.timeAttackInfoDic[_difficultyType] >= _lab)
        {
            Debug.Log(_lab);
            long preLab = UserData.timeAttackInfoDic[_difficultyType];
            UserData.timeAttackInfoDic[_difficultyType] = _lab;
            NotifyMainMissionsTimeAttack(_difficultyType, oldLab, _lab);
            Save();
            return new(true, preLab);
        }

        if (save)
            Save();

        return new(false, UserData.timeAttackInfoDic[_difficultyType]);
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
        TableDataManager.Instance.TableEquationData.MakeEquationData(UserData.localShuffleId);
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
    public static Tuple<bool, Dictionary<ItemType, int>, Dictionary<ItemType, int>> ClearStage(StageTableData _data, int _starCount = 0)
    {
        bool save = PlayDailyMission(MissionType.StagePlay);

        if (_data.stageType == StageType.Normal)
        {
            //처음 클리어
            if (UserData.clearStageInfoDic[_data.difficultyType] < _data.id)
            {
                UserData.clearStageInfoDic[_data.difficultyType] = _data.id;

                AddItemCount(_data.rewardItemDic);

                Dictionary<ItemType, int> levelupReward = AddXP(_data.starMax * 10);
                AddItemCount(levelupReward);
                return new(true, _data.rewardItemDic, levelupReward);
            }
            else
            {
                if (save)
                    Save();
                //이미 클리어했던 스테이지
                return new(false, null, null);
            }
        }
        else
        {
            if (UserData.clearStageInfoDic[_data.difficultyType] < _data.id)
            {
                UserData.clearStageInfoDic[_data.difficultyType] = _data.id;

                Dictionary<ItemType, int> clearRewardDic = null;

                //완전 클리어
                if (_starCount == 3)
                {
                    AddItemCount(_data.rewardItemDic);
                    clearRewardDic = _data.rewardItemDic;
                }
                else
                {
                    var r = _data.rewardItemDic.ToList();

                    clearRewardDic = new();

                    for (int i = 0; i < _starCount; i++)
                    {
                        clearRewardDic.Add(r[i].Key, r[i].Value);
                    }

                    UserData.bossStarInfoDic[_data.difficultyType].Add(new(_data.id, _starCount));
                }

                long oldRedStar = UserData.redStarCount;
                UserData.redStarCount += _starCount;
                NotifyMainMissionsStar(oldRedStar, UserData.redStarCount);
                Dictionary<ItemType, int> levelupReward = AddXP(_starCount * ((int)_data.difficultyType + 1) * 10);
                AddItemCount(levelupReward);
                OnMissionDataChanged?.Invoke();
                return new(true, clearRewardDic, levelupReward);
            }
            else
            {
                Dictionary<ItemType, int> clearRewardDic = new();
                BossStarInfo bossStarInfo = UserData.bossStarInfoDic[_data.difficultyType].FirstOrDefault(x => x.stageID == _data.id);
                if (bossStarInfo.starCount < _starCount)
                {
                    var r = _data.rewardItemDic.ToList();

                    for (int i = bossStarInfo.starCount; i < _starCount; i++)
                    {
                        clearRewardDic.Add(r[i].Key, r[i].Value);
                    }

                    if (_starCount < 3)
                        bossStarInfo.starCount = _starCount;
                    else
                        UserData.bossStarInfoDic[_data.difficultyType].Remove(bossStarInfo);

                    long oldRedStar = UserData.redStarCount;
                    UserData.redStarCount += _starCount - bossStarInfo.starCount;
                    NotifyMainMissionsStar(oldRedStar, UserData.redStarCount);

                    Dictionary<ItemType, int> levelupReward = AddXP((_starCount - bossStarInfo.starCount) * ((int)_data.difficultyType + 1) * 10);
                    AddItemCount(levelupReward);
                    OnMissionDataChanged?.Invoke();
                    return new(true, clearRewardDic, levelupReward);
                }
                else
                {
                    if (save)
                        Save();
                    //남은 스타가 적거나 같다
                    return new(false, null, null);
                }
            }
        }
    }

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
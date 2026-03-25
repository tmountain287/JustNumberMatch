using Common.Manager;
using Gostop.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class UserDataRespense
{
    public string userData;
}


[Serializable]
public class SuddaData
{
    public string today = "";
    public int playCount = 0;

    public SuddaData()
    {
        today = DateTime.Now.ToString("yyyyMMdd");
        playCount = 0;
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
        today = DateTime.Now.ToString("yyyyMMdd");
        playCount++;
    }
}

[Serializable]
public class UserData
{
    public bool isPush = false;
    public bool isPrologueComplete = false;
    public string nickName = "나";
    public long money = 10000;
    public int gold = 1000;
    public int fireTicket = 0;
    public int level = 1;
    public long otherMoney = 0;
    public bool isOtherFirstMatch = true;

    public int selectIndex = 1;

    public int gage = 0;

    public int playCount = 0;
    public int winCount = 0;

    public int winningStreak = 0;

    public string today = "";
    public int todayPlayCount = 0;
    public int todayWinCount = 0;
    public long todayMoney = 0;

    public int topScore = 0;
    public long topWinMoney = 0;
    public int topWinnigStreak = 0;
    public int topGocount = 0;

    public bool isPremium = false;
    public bool isSuddaPremium = false;

    public List<int> hasCharcterID = new();
    public List<int> hasSkillID = new();

    public SuddaData suddaData = null;
    public SuddaData adPeeStealData = new();

    public int peeStealCount = 0;
    public bool isJangddaeng = false;

    public UserData(string nickName, long money, int gold, long otherMoney, int characterId)
    {
        this.nickName = nickName;
        this.money = money;
        this.gold = gold;
        this.otherMoney = otherMoney;
        selectIndex = characterId;
        hasCharcterID.Add(characterId);
        suddaData = new SuddaData();
    }

    public void ClearToday(string _today)
    {
        today = _today;
        todayPlayCount = 0;
        todayWinCount = 0;
        todayMoney = 0;
    }

    public void PlaySudda()
    {

    }
}

[Serializable]
public class IntListWrapper
{
    public List<int> list = new();
}

public class ViewCharacterData
{
    private IntListWrapper list = new IntListWrapper();
    public List<int> CharacterIDList
    {
        get => list.list;
    }

    public ViewCharacterData()
    {
        Load();
    }

    public bool AddCharacterID(int _id)
    {
        if(!CharacterIDList.Contains(_id))
        {
            CharacterIDList.Add(_id);
            Save();
            return true;
        }

        return false;
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(list);
        SecurePlayerPrefs.SetString("ViewCharacter", json);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        string json = SecurePlayerPrefs.GetString("ViewCharacter", null);
        if (!string.IsNullOrEmpty(json))
        {
            list = JsonUtility.FromJson<IntListWrapper>(json);
        }
    }

    public void Delete()
    {
        PlayerPrefs.DeleteKey("ViewCharacter");
    }
}

public static class UserDataManager
{
    private const string SaveKey = "user_data";

    public static UserData UserData { get; set; } = null;
    public static ViewCharacterData ViewCharacterData { get; set; } = new();
    public static UnityEvent OnValueNickNameChanged { get; } = new();
    public static UnityEvent OnValueResultChanged { get; } = new();
    public static UnityEvent OnValueLevelChanged { get; } = new();
    public static UnityEvent OnValueSelectIndexChanged { get; } = new();
    public static UnityEvent OnValuePushChanged { get; } = new();
    public static UnityEvent<long, bool> OnValueMoneyChanged { get; } = new();
    public static UnityEvent<int, bool> OnValueGoldChanged { get; } = new();
    public static UnityEvent<int> OnValueFireTicketChanged { get; } = new();
    public static UnityEvent OnValuePeeStealCountChanged { get; } = new();
    public static UnityEvent OnCheckNewCharacter { get; } = new();
    public static UnityEvent<int> OnValueSuddaPlayCountChanged { get; } = new();

    public static UnityEvent OnValueAdPeeStealPlayCountChanged { get; } = new();
    public static Dictionary<SkillType, int> SkillDataDic { get; set; } = new();
    public static List<SkillTableData> hasSkillTableDataList { get; set; } = new();
    public static List<SkillTableData> newSkillTableDataList { get; set; } = new();

    private static int currenteWinningStreak = 0;

    public static string NickName
    {
        get => UserData.nickName;
        set
        {
            UserData.nickName = value;
            OnValueNickNameChanged.Invoke();
        }
    }

    public static int FireTicket
    {
        get => UserData.fireTicket;
        set
        {
            UserData.fireTicket = value;
            OnValueFireTicketChanged.Invoke(value);
        }
    }

    public static int PeeStealCount
    {
        get => UserData.peeStealCount;
        set
        {
            UserData.peeStealCount = value;
            OnValuePeeStealCountChanged.Invoke();
        }
    }

    public static int Gold
    {
        get => UserData.gold;
        set
        {
            UserData.gold = value;
            OnValueGoldChanged.Invoke(value, true);
        }
    }

    public static long Money
    { 
        get => UserData.money;
        set
        {
            UserData.money = value;
            OnValueMoneyChanged.Invoke(value, true);
        }
    }


    public static long OtherMoney
    {
        get => UserData.otherMoney;        
    }

    public static int Level
    {
        get => UserData.level;
        set
        {
            UserData.level = value;
        }
    }

    public static int SelectIndex
    {
        get => UserData.selectIndex;
        set
        {
            UserData.selectIndex = value;
            OnValueSelectIndexChanged.Invoke();
        }
    }

    public static bool IsPush
    {
        get => UserData.isPush;
        set
        {
            UserData.isPush = value;
            OnValuePushChanged.Invoke();
        }
    }


    public static int Gage
    {
        get => UserData.gage;
        set
        {
            UserData.gage = value;
        }
    }

    public static void RefreshSudda()
    {
        if(UserData.suddaData.RefreshData())
            OnValueSuddaPlayCountChanged.Invoke(UserData.suddaData.playCount);
    }

    public static void PlaySudda()
    {
        UserData.suddaData.Play();
        OnValueSuddaPlayCountChanged.Invoke(UserData.suddaData.playCount);
    }

    public static void RefreshAdPeeSteal()
    {
        if (UserData.adPeeStealData.RefreshData())
            OnValueAdPeeStealPlayCountChanged.Invoke();
    }

    public static void PlayAdPeeSteal()
    {
        UserData.adPeeStealData.Play();
        OnValueAdPeeStealPlayCountChanged.Invoke();
    }

    public static void AddGauge(int _value)
    {
        UserData.gage += Mathf.FloorToInt(_value * (1 + SkillDataDic[SkillType.FIRE_UP] * 0.01f));
        UserData.gage = Math.Min(UserData.gage, ConfigData.FireGaugeMax);
    }

    public static bool SubGold(int _value)
    {
        if(Gold < _value)
            return false;
        Gold -= _value;
        return true;
    }

    public static void AddGold(int _value)
    {        
        Gold += _value;
    }

    public static bool UseFireTicket()
    {
        if (FireTicket <= 0)
            return false;
        FireTicket--;
        return true;
    }

    public static void AddFireTicket(int _value)
    {
        FireTicket += _value;
    }



    public static void AddMoney(long _value)
    {
        Money += _value;
        OnValueMoneyChanged.Invoke(Money, true);
    }

    public static bool BuyCharacter(ShopCharacterTableData _data, bool _useGold = true)
    {
        if (_useGold)
        {
            if (Gold < _data.gold)
                return false;

            SubGold(_data.gold);
            _ = NetworkManager.Instance.SendItemLog("GoldBuyCharacter", -_data.gold);
        }

        UserData.hasCharcterID.Add(_data.characterId);

        var shopCharacterIds = TableDataManager.Instance.TableShopCharacterData.ShopCharacterTableDataList
            .Where(shopData => shopData.characterId == _data.characterId)
            .Select(shopData => shopData.id)
            .ToList();

        SkillTableData skillData = TableDataManager.Instance.TableSkillData.SkillTableDataList
            .Where(skillData => skillData.shopCharacterIDList.Any(id => shopCharacterIds.Contains(id)))
            .FirstOrDefault();

        List<int> characterList = new();

        skillData.shopCharacterIDList.ForEach(x =>
        {
            characterList.Add(TableDataManager.Instance.TableShopCharacterData.GetData(x).characterId);
        });

        if (characterList.All(x => UserDataManager.UserData.hasCharcterID.Contains(x)))
        {
            newSkillTableDataList.Add(skillData);
        }
        Save(true);
        return true;
    }

    public static void RefreshSkillData()
    {
        for (int i = 0; i < newSkillTableDataList.Count(); i++)
        {
            hasSkillTableDataList.Add(newSkillTableDataList[i]);
            SkillDataDic[newSkillTableDataList[i].skillType] += newSkillTableDataList[i].value;
        }

        newSkillTableDataList.Clear();
    }

    public static void CollectSkill(int _id)
    {
        UserData.hasSkillID.Add(_id);
        SkillTableData data = TableDataManager.Instance.TableSkillData.GetSkillTableData(_id);
        if (data != null)
        {
            SkillDataDic[data.skillType] += data.value;
        }
        Save();
    }

    public static string GetWinRate()
    {
        if (UserData.playCount == 0)
            return "0.00%";

        float rate = (float)UserData.winCount / UserData.playCount;
        return (rate * 100f).ToString("F2") + "%";
    }

    public static void LoadSkillData()
    {
        SkillDataDic.Clear();
        SkillDataDic = Enum.GetValues(typeof(SkillType))
            .Cast<SkillType>()
            .ToDictionary(key => key, value => 0);


        List<SkillTableData> skillTableDatas = TableDataManager.Instance.TableSkillData.SkillTableDataList;

        skillTableDatas.ForEach(data =>
        {
            List<int> characterList = new();

            data.shopCharacterIDList.ForEach(x =>
            {
                characterList.Add(TableDataManager.Instance.TableShopCharacterData.GetData(x).characterId);
            });

            if (characterList.All(x => UserData.hasCharcterID.Contains(x)))
            {
                hasSkillTableDataList.Add(data);
                SkillDataDic[data.skillType] += data.value;
            }

        });
    }

    public static void AddViewCharacter(int _id)
    {
        if(ViewCharacterData.AddCharacterID(_id))
            OnCheckNewCharacter.Invoke();
    }

    public static void LevelUp()
    {
        Level++;
        LevelTableData data = TableDataManager.Instance.TableLevelData.GetLevelTableData(UserData.level);
        UserData.otherMoney = data.money;
        UserData.isOtherFirstMatch = true;
        OnValueLevelChanged.Invoke();
        OnCheckNewCharacter.Invoke();
        Save();
    }
    
    public static void ApplyMatchResultMoney(bool _isWin, long _money, Action _onComplete = null)
    {
        if (!_isWin) _money *= -1;

        UserData.money += _money;
        UserData.otherMoney -= _money;
        UserData.isOtherFirstMatch = false;

        string today = DateTime.Now.ToString("yyyyMMdd");

        if(today == UserData.today)
        {
            UserData.todayMoney += _money;
        }
        else
        {
            UserData.todayMoney = _money;
        }
        UserData.today = DateTime.Now.ToString("yyyyMMdd");
        OnValueMoneyChanged.Invoke(Money, true);
        Save(true, _onComplete);
    }

    public static void RestoreResult(long _money)
    {
        UserData.playCount--;
        if(UserData.todayPlayCount > 0)
            UserData.todayPlayCount--;      
        UserData.winningStreak = currenteWinningStreak;
        ApplyMatchResultMoney(true, _money);
        OnValueResultChanged.Invoke();
    }

    public static void WinningStreakClear()
    {
        currenteWinningStreak = UserData.winningStreak;
    }

    public static void SetResult(bool _isWin, long _money, int _score = 0, int _goCount = 0)
    {
        string today = DateTime.Now.ToString("yyyyMMdd");

        if (today != UserData.today)
        {
            UserData.ClearToday(today);
        }

        UserData.todayPlayCount++;
        UserData.playCount++;

        if (_isWin)
        {
            UserData.winningStreak++;
            UserData.todayWinCount++;
            UserData.winCount++;
            UserData.topWinnigStreak = Mathf.Max(UserData.topWinnigStreak, UserData.winningStreak);
            UserData.topScore = Mathf.Max(UserData.topScore, _score);
            UserData.topGocount = Mathf.Max(UserData.topGocount, _goCount);
            UserData.topWinMoney = System.Math.Max(UserData.topWinMoney, _money);
            currenteWinningStreak = UserData.winningStreak;
        }
        else
        {            
            UserData.winningStreak = 0;
        }

        ApplyMatchResultMoney(_isWin, _money);
        OnValueResultChanged.Invoke();
    }    

    public static bool Load()
    {
        string json = SecurePlayerPrefs.GetString(SaveKey, null);
        if (string.IsNullOrEmpty(json))
        {
            //UserData = new("나", ConfigData.InitMoney, ConfigData.InitGold, TableDataManager.Instance.TableLevelData.GetLevelTableData(1).money, ConfigData.InitCharacterID);
            return false;
        }

        UserData = JsonUtility.FromJson<UserData>(json);

        string today = DateTime.Now.ToString("yyyyMMdd");

        if(today != UserData.today)
        {
            UserData.ClearToday(today);
        }

        UserData.suddaData.RefreshData();
        currenteWinningStreak = UserData.winningStreak;
        LoadSkillData();
        SendGPGReport();
        SendGPGAchievement();
        return true;
    }

    public static void NewUserData(string _uId)
    {
        UserData = new(_uId, ConfigData.InitMoney, ConfigData.InitGold, TableDataManager.Instance.TableLevelData.GetLevelTableData(1).money, ConfigData.InitCharacterID);
        LoadSkillData();
        Save();
        SendGPGAchievement();
    }

    public static void ClearData()
    {
        ViewCharacterData.Delete();
        PlayerPrefs.DeleteKey(SaveKey);
    }


    public static void BuyPremium()
    {
        UserData.isPremium = true;
        UserData.isSuddaPremium = true;
        UserData.suddaData.playCount = 0;
        OnValueSuddaPlayCountChanged.Invoke(UserData.suddaData.playCount);
    }

    public static void SetForceUserData(UserData _userData)
    {
        UserData = _userData;
        Save();
        Load();

        OnValueNickNameChanged.Invoke();
        OnValueResultChanged.Invoke();
        OnValueLevelChanged.Invoke();
        OnValueSelectIndexChanged.Invoke();
        OnValueMoneyChanged.Invoke(UserData.money, false);
        OnValueGoldChanged.Invoke(UserData.gold, false);
        OnCheckNewCharacter.Invoke();
        OnValuePushChanged.Invoke();
        OnValueSuddaPlayCountChanged.Invoke(UserData.suddaData.playCount);
    }

    public static void Save(bool _check = false, Action _onComplete = null)
    {
        string json = JsonUtility.ToJson(UserData);
        SecurePlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
        SendGPGReport();
        SendGPGAchievement();
       
        AsyncSaveData(_check, _onComplete);
    }

    public static void SendGPGReport()
    {
#if SERVICE
#if UNITY_IOS
        PlatformSocialManager.Instance.ReportScore(Money, "CgkIofrI1qsSEAIQBA");
        PlatformSocialManager.Instance.ReportScore(Level, "CgkIofrI1qsSEAIQAw");
        PlatformSocialManager.Instance.ReportScore(UserData.winCount, "CgkIofrI1qsSEAIQBQ");
        PlatformSocialManager.Instance.ReportScore(UserData.topScore, "CgkIofrI1qsSEAIQBg");
        PlatformSocialManager.Instance.ReportScore(UserData.topWinnigStreak, "CgkIofrI1qsSEAIQBw");
#else
        PlatformSocialManager.Instance.ReportScore(Level, "CgkIofrI1qsSEAIQiAE");
        PlatformSocialManager.Instance.ReportScore(Money, "CgkIofrI1qsSEAIQiQE");
        PlatformSocialManager.Instance.ReportScore(UserData.winCount, "CgkIofrI1qsSEAIQigE");
        PlatformSocialManager.Instance.ReportScore(UserData.topScore, "CgkIofrI1qsSEAIQiwE");
        PlatformSocialManager.Instance.ReportScore(UserData.topWinnigStreak, "CgkIofrI1qsSEAIQjAE");
#endif

#endif
    }

    public static void SendGPGAchievement()
    {
#if SERVICE
        long GetCurrentValueByType(GPGAchievementType type)
        {
            return type switch
            {
                GPGAchievementType.Level => Level,
                GPGAchievementType.BigWin => UserData.topScore,
                GPGAchievementType.PlayCount => UserData.playCount,
                GPGAchievementType.WinCount => UserData.winCount,
                GPGAchievementType.WinStreak => UserData.topWinnigStreak,
                GPGAchievementType.Money => Money,
                _ => 0
            };
        }

        foreach (GPGAchievementType type in Enum.GetValues(typeof(GPGAchievementType)))
        {
            long currentValue = GetCurrentValueByType(type);

            List<GPGAchievementData> achievementList =
                TableDataManager.Instance.TableGPGAchievementData.GetAchievementsByTypeAndValue(type, currentValue);

            achievementList.ForEach(data =>
            {
                PlatformSocialManager.Instance.UnlockAchievement(data.id);
            });
        }

        UserData.hasCharcterID.ForEach(x =>
        {
            GPGAchievementData data = TableDataManager.Instance.TableGPGAchievementData.GetAchievementsByTypeAndIndex(GPGAchievementType.Character, x);
            PlatformSocialManager.Instance.UnlockAchievement(data.id);
        });
#endif
    }

    private static async void AsyncSaveData(bool _check, Action _onComplete = null)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            _onComplete?.Invoke();
            return;
        }
        
        var (bro, result) = await NetworkManager.Instance.SaveRecord(1);

        if (!_check)
            return;

        if (result != null)
        {
            Debug.Log(result.last_us_key);
            Debug.Log(NetworkManager.Instance.US_KEY);

            if (result.last_us_key != NetworkManager.Instance.US_KEY && result.record != null && result.record != SecurePlayerPrefs.Encrypt(JsonUtility.ToJson(UserData)))
            {
                UserData beforeData = JsonUtility.FromJson<UserData>(SecurePlayerPrefs.Decrypt(result.record));
                UIManager.Instance.HideLoading();
                PopupManager.Instance.OpenPopup<SelectUserPopup>().Initialize(beforeData, UserData, (isCurrent) =>
                {
                    if (!isCurrent)
                    {
                        PopupManager.Instance.AllClosePopup(PopupType.NONE);
                        InGameManager.Instance.RestartGameReady();
                        InGameManager.Instance.GameStart();
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

    public static void RestoreIAP(string _id)
    {

    }
}
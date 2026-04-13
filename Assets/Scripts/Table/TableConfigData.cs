using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//[System.Serializable]
//public class ConfigTableData
//{
//    public string ConfigIndexString;
//    public string Value;

//    public ConfigTableData(string[] row)
//    {
//        if (row.Length >= 2)
//        {
//            ConfigIndexString = row[0].Trim();
//            Value = row[1].Trim();
//        }
//    }
//}

//public class TableConfigData : BaseTableData
//{
//    public List<ConfigTableData> ConfigList { get; private set; }
//    private Dictionary<string, string> configMap;

//    public override void Load()
//    {
//        ConfigList = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_configtable"), row => new ConfigTableData(row));
//        configMap = ConfigList.ToDictionary(x => x.ConfigIndexString, x => x.Value);

//        ConfigData.AdOpenLevel = GetInt("AdOpenLevel");
//        ConfigData.AdOpenCountMin = GetInt("AdOpenCountMin");
//        ConfigData.AdOpenCountMax = GetInt("AdOpenCountMax");
//        ConfigData.InititemInfoDic.Add(ItemType.Gold, GetInt("InitGold"));
//        ConfigData.InititemInfoDic.Add(ItemType.Hint, GetInt("InitHintCount"));
//        ConfigData.InititemInfoDic.Add(ItemType.TimeAttackTicket, GetInt("InitTimeAttackTicketCount"));
//        ConfigData.InititemInfoDic.Add(ItemType.Change, 0);

//        ConfigData.AdsRewardGold = GetInt("AdsRewardGold");
//        ConfigData.TodayAdsRewardGoldCount = GetInt("TodayAdsRewardGoldCount");

//        ConfigData.NeedTimeAttckTicketCountDic.Add(DifficultyType.Easy, GetInt("EasyTimeAttackTicketCount"));
//        ConfigData.NeedTimeAttckTicketCountDic.Add(DifficultyType.Normal, GetInt("NormalTimeAttackTicketCount"));
//        ConfigData.NeedTimeAttckTicketCountDic.Add(DifficultyType.Hard, GetInt("HardTimeAttackTicketCount"));

//        //ConfigData.RewardHintStageDic.Add(DifficultyType.Easy, GetInt("EasyRewardHint"));
//        //ConfigData.RewardHintStageDic.Add(DifficultyType.Normal, GetInt("NormalRewardHint"));
//        //ConfigData.RewardHintStageDic.Add(DifficultyType.Hard, GetInt("HardRewardHint"));

//        //ConfigData.RewardTimeAttackTicketStageDic.Add(DifficultyType.Easy, GetInt("EasyRewardTimeAttackTicket"));
//        //ConfigData.RewardTimeAttackTicketStageDic.Add(DifficultyType.Normal, GetInt("NormalRewardTimeAttackTicket"));
//        //ConfigData.RewardTimeAttackTicketStageDic.Add(DifficultyType.Hard, GetInt("HardRewardTimeAttackTicket"));

//        //ConfigData.RewardGoldDic.Add(DifficultyType.Easy, GetInt("EasyRewardGold"));
//        //ConfigData.RewardGoldDic.Add(DifficultyType.Normal, GetInt("NormalRewardGold"));
//        //ConfigData.RewardGoldDic.Add(DifficultyType.Hard, GetInt("HardRewardGold"));

//        ConfigData.UnlockNormalStageLevel = GetInt("UnlockNormalStageLevel");
//        ConfigData.UnlockHardStageLevel = GetInt("UnlockHardStageLevel");

//        ConfigData.UnlockTimeAttackLevel = GetInt("UnlockTimeAttackLevel");
//        ConfigData.UnlockUnlimtedLevel = GetInt("UnlockUnlimtedLevel");

//        ConfigData.BossTimeDic.Add(DifficultyType.Easy, GetInt("BossTimeEasy"));
//        ConfigData.BossTimeDic.Add(DifficultyType.Normal, GetInt("BossTimeNormal"));
//        ConfigData.BossTimeDic.Add(DifficultyType.Hard, GetInt("BossTimeHard"));

//        ConfigData.UnlockModeLevelList = new List<int>{ ConfigData.UnlockNormalStageLevel, ConfigData.UnlockHardStageLevel,
//        ConfigData.UnlockTimeAttackLevel,ConfigData.UnlockUnlimtedLevel};
//    }

//    private int GetInt(string key, int defaultValue = 0)
//    {
//        return configMap.TryGetValue(key, out var value) && int.TryParse(value, out var result) ? result : defaultValue;
//    }

//    private long GetLong(string key, long defaultValue = 0)
//    {
//        return configMap.TryGetValue(key, out var value) && long.TryParse(value, out var result) ? result : defaultValue;
//    }

//    private string GetString(string key, string defaultValue = "")
//    {
//        return configMap.TryGetValue(key, out var value) ? value : defaultValue;
//    }
//}

public static class ConfigData
{
    public static int AdOpenLevel = 10;
    public static int AdOpenCountMin = 2;
    public static int AdOpenCountMax = 5;

    public static Dictionary<ItemType, int> InititemInfoDic = new Dictionary<ItemType, int>
    {
        { ItemType.Gold, 500 },
        { ItemType.Hint, 3 },
        { ItemType.TimeAttackTicket, 0 },
        { ItemType.Change, 0 }
    };

    public static int AdsRewardGold = 50;
    public static int TodayAdsRewardGoldCount = 5;


    public static int UnlockNormalStageLevel;
    public static int UnlockHardStageLevel;

    public static int UnlockTimeAttackLevel;
    public static int UnlockUnlimtedLevel;

    public static List<int> UnlockModeLevelList = new List<int>
    {
        10,20,25,100
    };

    /// <summary>프리미엄 팩(AdsSaleRemove) 세일 유효 기간(초). 기본 3일.</summary>
    public static int PremiumPackSaleDurationSeconds = 3 * 24 * 60 * 60;

    public static int MissionOpenLevel = 10;

    public static int FireGaugeMax = 100;
    public static int FireGaugeAddValue = 10;

    public static int NextMultipleGold = 100;
    public static int InvalidGold = 50;
    public static int FireMutilpleGold = 200;

    public static long AdRevivalMoney = 5000L;
    public static long RevivalMoney = 10000L;
}
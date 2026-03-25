using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ConfigTableData
{
    public string ConfigIndexString;
    public string Value;

    public ConfigTableData(string[] row)
    {
        if (row.Length >= 2)
        {
            ConfigIndexString = row[0].Trim();
            Value = row[1].Trim();
        }
    }
}

public class TableConfigData : BaseTableData
{
    public List<ConfigTableData> ConfigList { get; private set; }
    private Dictionary<string, string> configMap;

    public override void Load()
    {
        ConfigList = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_configtable"), row => new ConfigTableData(row));
        configMap = ConfigList.ToDictionary(x => x.ConfigIndexString, x => x.Value);

        ConfigData.AdOpenLevel = GetInt("AdOpenLevel");
        ConfigData.MissionOpenLevel = GetInt("MissionOpenLevel");
        ConfigData.AdOpenCount = GetInt("AdOpenCount");
        ConfigData.InitMoney = GetLong("InitMoney");
        ConfigData.InitGold = GetInt("InitGold");
        ConfigData.InitCharacterID = GetInt("InitCharacterID");
        ConfigData.AdGoldReward = GetInt("AdGoldReward");
        ConfigData.RevivalMoney = GetLong("RevivalMoney");
        ConfigData.AdRevivalMoney = GetLong("AdRevivalMoney");
        ConfigData.NextMultipleGold = GetInt("NextMultipleGold");
        ConfigData.InvalidGold = GetInt("InvalidGold");
        ConfigData.FireGaugeMax = GetInt("FireGaugeMax");
        ConfigData.FireGaugeAddValue = GetInt("FireGaugeAddValue");
        ConfigData.FireMaxMultiple = GetInt("FireMaxMultiple");
        ConfigData.FireMutilpleGold = GetInt("FireMutilpleGold");
        ConfigData.AIOpenLevel = GetInt("AIOpenLevel");
        ConfigData.AIRate = GetInt("AIRate");
        ConfigData.FireAIRate = GetInt("FireAIRate");
        ConfigData.SuddaOneDayFreeCount = GetInt("SuddaOneDayFreeCount");
        ConfigData.SuddaOneDayADFreeCount = GetInt("SuddaOneDayADFreeCount");
        ConfigData.StealPeeGold = GetInt("StealPeeGold");
        ConfigData.AdPeeStealMaxCount = GetInt("AdPeeStealMaxCount");
    }

    private int GetInt(string key, int defaultValue = 0)
    {
        return configMap.TryGetValue(key, out var value) && int.TryParse(value, out var result) ? result : defaultValue;
    }

    private long GetLong(string key, long defaultValue = 0)
    {
        return configMap.TryGetValue(key, out var value) && long.TryParse(value, out var result) ? result : defaultValue;
    }

    private string GetString(string key, string defaultValue = "")
    {
        return configMap.TryGetValue(key, out var value) ? value : defaultValue;
    }
}

public static class ConfigData
{
    public static int AdOpenLevel;
    public static int MissionOpenLevel;
    public static int AdOpenCount;
    public static long InitMoney;
    public static int InitGold;
    public static int InitCharacterID;
    public static int AdGoldReward;
    public static long RevivalMoney;
    public static long AdRevivalMoney;
    public static int NextMultipleGold;
    public static int InvalidGold;
    public static int FireGaugeMax;
    public static int FireGaugeAddValue;
    public static int FireMaxMultiple;
    public static int FireMutilpleGold;
    public static int AIOpenLevel;
    public static int AIRate;
    public static int FireAIRate;
    public static int SuddaOneDayFreeCount;
    public static int SuddaOneDayADFreeCount;
    public static int StealPeeGold;
    public static int AdPeeStealMaxCount;
}
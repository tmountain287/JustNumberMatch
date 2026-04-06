using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GPGAchievementType
{
    StageClear = 0,
    ModeClear = 1,
    ModeUnlock = 2,
    Level = 3,
}

[System.Serializable]
public class GPGAchievementData
{
    public string id;
    public string name;
    public GPGAchievementType achievementType;
    public DifficultyType difficultyType;
    public long value;

    public GPGAchievementData(string[] row)
    {
        if (row.Length >= 6)
        {
#if UNITY_IOS
            id = row[1].Trim();
#else
            id = row[0].Trim();
#endif
            name = row[2].Trim();

            if (int.TryParse(row[3], out int typeValue) && System.Enum.IsDefined(typeof(GPGAchievementType), typeValue))
            {
                achievementType = (GPGAchievementType)typeValue;
            }
            else
            {
                Debug.LogWarning($"Invalid type value: {row[3]} for CharacterTableData ID {id}");
                achievementType = GPGAchievementType.StageClear; // 기본값으로 처리
            }

            if (int.TryParse(row[4], out int difficultyValue) && System.Enum.IsDefined(typeof(DifficultyType), difficultyValue))
            {
                difficultyType = (DifficultyType)difficultyValue;
            }
            else
            {
                Debug.LogWarning($"Invalid difficulty value: {row[4]} for GPGAchievementData ID {id}");
                difficultyType = DifficultyType.Easy; // 기본값으로 처리
            }

            long.TryParse(row[5], out value);
        }
    }
}

public class TableGPGAchievementData : BaseTableData
{
    public List<GPGAchievementData> GPGAchievementTableDataList { get; private set; }

    public List<GPGAchievementData> GetAchievementsByTypeAndValue(GPGAchievementType type, long maxValue)
    {
        if (GPGAchievementTableDataList == null)
        {
            Debug.LogWarning("GPGAchievementTableDataList is not loaded.");
            return new List<GPGAchievementData>();
        }

        return GPGAchievementTableDataList
            .Where(data => data.achievementType == type && data.value <= maxValue)
            .ToList();
    }

    public GPGAchievementData GetAchievementsByTypeAndIndex(GPGAchievementType type, int index) //캐릭터
    {
        if (GPGAchievementTableDataList == null)
        {
            Debug.LogWarning("GPGAchievementTableDataList is not loaded.");
            return null;
        }

        return GPGAchievementTableDataList
            .Where(data => data.achievementType == type && data.value == index).FirstOrDefault();
            
    }


    public override void Load()
    {
        GPGAchievementTableDataList = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_gpgachievementstable"), row => new GPGAchievementData(row));
    }
}
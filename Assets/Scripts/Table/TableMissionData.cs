using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MissionCategory
{
    Daily,
    Main,
}


public enum MissionType
{
    DailyMissionComplete = 0,
    Level = 1,
    Star = 2,
    TimeAttack = 3,
    Survival = 4,    
    ItemUse = 5,
    StagePlay = 6,
    AdsFreeGold = 7,
    TimeAttackPlay = 8,
    SurvivalPlay = 9,
}

[System.Serializable]
public class MissionData
{
    public int id;
    public MissionType type;
    public MissionCategory category;
    public int difficultyType;
    public string titleLocalId;
    public string subLocalId;
    public long value;
    public int preId;
    public ItemType rewardItemType;
    public int rewardValue;
    public int openLevel;


    public MissionData(string[] row)
    {
        if (row.Length >= 10)
        {
            int.TryParse(row[0], out id);
            Enum.TryParse(row[1].Trim(), out type);
            Enum.TryParse(row[2].Trim(), out category);
            int.TryParse(row[3], out difficultyType);

            titleLocalId = row[4].Trim();
            subLocalId = row[5].Trim();

            long.TryParse(row[6], out value);
            int.TryParse(row[7], out preId);

            Enum.TryParse(row[8].Trim(), out rewardItemType);
            int.TryParse(row[9], out rewardValue);
            int.TryParse(row[10], out openLevel);
        }
    }
}

public class TableMissionData : BaseTableData
{
    public List<MissionData> DataList { get; private set; }

    public MissionData GetData(int _id)
    {
        return DataList.Where(x => x.id == _id).FirstOrDefault();
    }

    public MissionData GetData(MissionType _type)
    {
        return DataList.Where(x => x.type == _type).FirstOrDefault();
    }

    public MissionData GetData(MissionType _type, int _difficultyType)
    {
        return DataList.Where(x => x.type == _type && x.difficultyType == _difficultyType).FirstOrDefault();
    }

    public List<MissionData> GetDataList(MissionCategory _category)
    {
        return DataList.Where(x => x.category == _category).ToList();
    }

    public MissionData GetNextData(int _id, int _difficultyType)
    {
        MissionData data = DataList.FirstOrDefault(x=> x.id == _id);
        return DataList.Where(x => x.type == data.type && x.id > _id && x.difficultyType == _difficultyType).FirstOrDefault();
    }

    public override void Load()
    {
        DataList = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_missiontable"), row => new MissionData(row));
    }
}
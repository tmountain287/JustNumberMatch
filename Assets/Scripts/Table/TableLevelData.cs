using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class LevelData
{
    public int level;
    public int xp;
    public Dictionary<ItemType, int> rewardItemDic = new();


    public LevelData(string[] row)
    {
        if (row.Length >= 6)
        {
            int.TryParse(row[0], out level);
            int.TryParse(row[1], out xp);



            int.TryParse(row[3], out int rewardGold);
            rewardItemDic.Add(ItemType.Gold, rewardGold);

            int.TryParse(row[4], out int rewardHint);
            rewardItemDic.Add(ItemType.Hint, rewardHint);

            int.TryParse(row[5], out int rewardTicket);
            rewardItemDic.Add(ItemType.TimeAttackTicket, rewardTicket);

            int.TryParse(row[6], out int rewardChange);
            rewardItemDic.Add(ItemType.Change, rewardChange);
        }
    }
}

public class TableLevelData : BaseTableData
{
    public List<LevelData> LevelDataList { get; private set; }
    public int LastLevel { get; private set; }

    public LevelData GetTableData(int _level)
    {
        if (LevelDataList[^1].level < _level)
            return LevelDataList[^1];

        return LevelDataList.Where(x => x.level == _level).FirstOrDefault();
    }

    public override void Load()
    {
        LevelDataList = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_leveltable"), row => new LevelData(row));
        LastLevel = LevelDataList[^1].level;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[System.Serializable]
public class LevelTableData
{
    public int level;
    public long pointValue;
    public long money;
    public int charId;
    public int normalAIRate;
    public int fireAIRate;

    public LevelTableData(int level, long pointValue, long money, int charId, int normalAIRate, int fireAIRate)
    {
        this.level = level;
        this.pointValue = pointValue;
        this.money = money;
        this.charId = charId;
        this.normalAIRate = normalAIRate;
        this.fireAIRate = fireAIRate;
    }

    public LevelTableData(string[] row)
    {
        if (row.Length >= 6)
        {
            int.TryParse(row[0], out level);
            //name = row[1].Trim();

            if (!long.TryParse(row[1].Replace(",", ""), out pointValue))
            {
                Debug.LogError($"LevelTableData long 변환 실패: 입력값 = {row[4]} (index=4)");
            }

            if (!long.TryParse(row[2].Replace(",", ""), out money))
            {
                Debug.LogError($"LevelTableData long 변환 실패: 입력값 = {row[4]} (index=4)");
            }

            int.TryParse(row[3], out charId);

            int.TryParse(row[4], out normalAIRate);
            int.TryParse(row[5], out fireAIRate);
        }
    }
}

public class TableLevelData : BaseTableData
{
    public List<LevelTableData> LevelTableDataList { get; private set; }

    public LevelTableData GetLevelTableData(int _level)
    {
        int maxLevel = LevelTableDataList.Max(x => x.level);

        if (maxLevel < _level)
        {
            LevelTableData maxData = LevelTableDataList.Where(x => x.level == maxLevel).FirstOrDefault();
            LevelTableData maxData1 = LevelTableDataList.Where(x => x.level == maxLevel - 1).FirstOrDefault();

            int lv = _level % maxLevel == 0 ? maxLevel : _level % maxLevel;

            int charCharID = GetLevelTableData(lv).charId;

            LevelTableData levelTableData = new(_level, maxData.pointValue, maxData.money + (maxData.money - maxData1.money), charCharID, maxData.normalAIRate, maxData.fireAIRate);            
            return levelTableData;
        }
        else
        {
            return LevelTableDataList.Where(x => x.level == _level).FirstOrDefault();
        }
    }

    public override void Load()
    {
        LevelTableDataList = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_leveltable"), row => new LevelTableData(row));        
    }
}

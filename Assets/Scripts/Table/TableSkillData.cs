using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum SkillType
{
    MONEY_UP = 0,
    FIRE_UP = 1,
}

[System.Serializable]
public class SkillTableData
{
    public int id;
    public List<int> shopCharacterIDList;
    public SkillType skillType;
    public int value;

    public SkillTableData(string[] row)
    {
        if (row.Length >= 4)
        {
            int.TryParse(row[0], out id);
            
            string raw = row[1];
            shopCharacterIDList = raw.Split(';').Select(int.Parse).ToList();            
            if (int.TryParse(row[2], out int typeValue) && System.Enum.IsDefined(typeof(SkillType), typeValue))
            {
                skillType = (SkillType)typeValue;
            }
            else
            {
                Debug.LogWarning($"Invalid type value: {row[2]} for TableSkillData ID {id}");
                skillType = SkillType.MONEY_UP; // 기본값으로 처리
            }

            int.TryParse(row[3], out value);
        }
    }
}

public class TableSkillData : BaseTableData
{
    public List<SkillTableData> SkillTableDataList { get; private set; }

    public SkillTableData GetSkillTableData(int _id)
    {
        return SkillTableDataList.Where(x => x.id == _id).FirstOrDefault();
    }

    public override void Load()
    {
        SkillTableDataList = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_skilltable"), row => new SkillTableData(row));        
    }
}
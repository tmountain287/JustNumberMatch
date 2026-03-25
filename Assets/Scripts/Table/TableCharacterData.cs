using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GenderType
{
    Male = 0,
    Female = 1,
}

public enum MainSkillType
{
    None = 0,
    KwangSteal = 1,
    MungSteal = 2,
    DDiSteal = 3,
    PeeSteal = 4,
}

[System.Serializable]
public class CharacterTableData
{
    public int id;
    public string name;
    public string resource;
    public GenderType gengerType;
    public MainSkillType mainSkillType;
    public string dialogue;

    public CharacterTableData(string[] row)
    {
        if (row.Length >= 6)
        {
            int.TryParse(row[0], out id);
            name = row[1].Trim();
            resource = row[2].Trim();

            if (int.TryParse(row[3], out int typeValue) && System.Enum.IsDefined(typeof(GenderType), typeValue))
            {
                gengerType = (GenderType)typeValue;
            }
            else
            {
                Debug.LogWarning($"Invalid type value: {row[3]} for CharacterTableData ID {id}");
                gengerType = GenderType.Male; // 기본값으로 처리
            }

            if (int.TryParse(row[4], out int mainSkillTypeValue) && System.Enum.IsDefined(typeof(MainSkillType), mainSkillTypeValue))
            {
                mainSkillType = (MainSkillType)mainSkillTypeValue;                
            }
            else
            {
                Debug.LogWarning($"Invalid type value: {row[4]} for CharacterTableData ID {id}");
                mainSkillType = MainSkillType.None; // 기본값으로 처리
            }

            dialogue = row[5].Trim();
        }
    }
}

public class TableCharacterData : BaseTableData
{
    public List<CharacterTableData> CharacterTableDataList { get; private set; }

    public CharacterTableData GetCharacterTableData(int _id)
    {
        return CharacterTableDataList.Where(x => x.id == _id).FirstOrDefault();
    }

    public CharacterTableData GetLevelTableDataByLevel(int _level)
    {
        int charId = TableDataManager.Instance.TableLevelData.GetLevelTableData(_level).charId;

        return GetCharacterTableData(charId);
    }

    public override void Load()
    {        
        CharacterTableDataList = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_charactertable"), row => new CharacterTableData(row));
    }
}
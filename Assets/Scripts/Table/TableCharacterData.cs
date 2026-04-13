using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GenderType
{
    Male = 0,
    Female = 1,
}


[System.Serializable]
public class CharacterData
{
    public int id;
    public string name;
    public string resource;
    public GenderType gengerType;
    public string dialogue;

    public CharacterData(string[] row)
    {
        if (row.Length >= 5)
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

            //if (int.TryParse(row[4], out int mainSkillTypeValue) && System.Enum.IsDefined(typeof(MainSkillType), mainSkillTypeValue))
            //{
            //    mainSkillType = (MainSkillType)mainSkillTypeValue;                
            //}
            //else
            //{
            //    Debug.LogWarning($"Invalid type value: {row[4]} for CharacterTableData ID {id}");
            //    mainSkillType = MainSkillType.None; // 기본값으로 처리
            //}

            dialogue = row[4].Trim();
        }
    }
}

public class TableCharacterData : BaseTableData
{
    public List<CharacterData> CharacterDataList { get; private set; }

    public CharacterData GetCharacterTableData(int _id)
    {
        return CharacterDataList.Where(x => x.id == _id).FirstOrDefault();
    }

    public CharacterData GetLevelTableDataByLevel(int _level)
    {
        //int charId = TableDataManager.Instance.TableLevelData.GetLevelTableData(_level).charId;

        //return GetCharacterTableData(charId);
        return null;
    }

    public override void Load()
    {        
        CharacterDataList = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_charactertable"), row => new CharacterData(row));
    }
}
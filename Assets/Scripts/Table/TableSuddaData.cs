using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum SuddaJokboType
{
    SAMPAL_KWANGDDANG = 0,
    KWANGDDANG = 1,
    DDANG = 2,
    ALI = 3,
    DOKSA = 4,
    GUBBING = 5,
    JANGBBING = 6,
    JANGSA = 7,
    SERYUK = 8,
    GABO = 9,
    GGS = 10,
    MANGTONG = 11,
}

[System.Serializable]
public class SuddaTableData
{
    public int id;
    public string name;
    public int reward;
    public SuddaJokboType type;
    public int value;
    public string groupName;
    public int rate;

    public SuddaTableData(int id, string name, int reward)
    {
        this.id = id;
        this.name = name;
        this.reward = reward;
    }

    public SuddaTableData(string[] row)
    {
        if (row.Length >= 7)
        {
            int.TryParse(row[0], out id);
            name = row[1].Trim();
            int.TryParse(row[2], out reward);
            if (int.TryParse(row[3], out int typeValue) && System.Enum.IsDefined(typeof(SuddaJokboType), typeValue))
            {
                type = (SuddaJokboType)typeValue;
            }
            else
            {
                Debug.LogWarning($"Invalid type value: {row[3]} for SuddaTableData ID {id}");
                type = SuddaJokboType.GGS; // 기본값으로 처리
            }

            int.TryParse(row[4], out value);
            groupName = row[5].Trim();
            int.TryParse(row[6], out rate);
        }
    }
}

public class TableSuddaData : BaseTableData
{
    public List<SuddaTableData> SuddaTableDataList { get; private set; }

    public SuddaTableData GetSuddaTableData(int _id)
    {        
        return SuddaTableDataList.Where(x => x.id == _id).FirstOrDefault();
    }

    public SuddaTableData GetSuddaTableData(SuddaJokboType _type, int _value = -1)
    {
        return SuddaTableDataList.Where(x => x.type == _type && x.value == _value).FirstOrDefault();
    }

    public List<SuddaTableData> GetSuddaTableDataList(SuddaJokboType _type)
    {
        return SuddaTableDataList.Where(x => x.type == _type).ToList();
    }


    public override void Load()
    {
        SuddaTableDataList = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_suddatable"), row => new SuddaTableData(row));        
    }

    public SuddaTableData GetRandomSuddaTableDataByRate()
    {
        if (SuddaTableDataList == null || SuddaTableDataList.Count == 0)
            return null;

        int totalRate = SuddaTableDataList.Sum(x => x.rate);
        if (totalRate <= 0)
            return null;

        int randomValue = UnityEngine.Random.Range(0, totalRate);
        int cumulative = 0;

        foreach (var data in SuddaTableDataList)
        {
            cumulative += data.rate;
            if (randomValue < cumulative)
            {
                return data;
            }
        }

        return null; // 이론상 도달하지 않음
    }
}

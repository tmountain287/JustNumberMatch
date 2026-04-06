using Common.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class LocalizationData
{
    public string id;
    public List<string> stringList = new List<string>();
    
    public LocalizationData(string[] row)
    {
        if (row.Length >= 16)
        {            
            id = row[0].Trim();

            for(int i = 1; i < row.Length; i++)
            {
                stringList.Add(SetString(row[i].Trim()));
            }
        }
    }

    private string SetString(string str)
    {
        return str.Replace("\"", "").Replace("\\n", "\n");
    }
}

public class TableLocalizationData : BaseTableData
{
    public Dictionary<LocalUIType, List<LocalizationData>> LocalizationDataDic { get; private set; }

    public string GetLocalString(LocalUIType _type, string _key, LocalType _localType)
    {
        if (string.IsNullOrEmpty(_key)) return string.Empty;

        if (LocalizationDataDic == null)
            return string.Empty;

        if (!LocalizationDataDic.TryGetValue(_type, out var rowList) || rowList == null)
            return string.Empty;

        LocalizationData data = rowList.Where(x => x.id == _key).FirstOrDefault();

        if (data == null)
        {
            Debug.LogWarning($"{_key} 값이 없습니다.");
            return string.Empty;
        }

        int langIndex = (int)_localType;
        if (data.stringList == null || langIndex < 0 || langIndex >= data.stringList.Count)
            return string.Empty;

        return data.stringList[langIndex];
    }

    public override void Load()
    {
        LocalizationDataDic = new()
        {
            [LocalUIType.Normal] = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_localizationtable"), row => new LocalizationData(row)),
            [LocalUIType.Mission] = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_localizationmissiontable"), row => new LocalizationData(row)),
            [LocalUIType.Push] = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_localizationpushtable"), row => new LocalizationData(row)),
        };
    }
}

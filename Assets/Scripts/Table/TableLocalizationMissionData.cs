using Common.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

public class TableLocalizationMissionData : BaseTableData
{
    public List<LocalizationData> LocalizationDataList { get; private set; }

    public string GetString(string _id, LocalType _localType)
    {
        LocalizationData data = LocalizationDataList.Where(x => x.id == _id).FirstOrDefault();

        if (data == null)
        {
            Debug.LogWarning($"{_id} 값이 없습니다.");
            return null;
        }      

        return data.stringList[(int)_localType];
    }

    public override void Load()
    {
        LocalizationDataList = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_localizationmissiontable"), row => new LocalizationData(row));        
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;



[System.Serializable]
public class SlangTableData
{
    public string word;

    public SlangTableData(string[] row)
    {
        if (row.Length > 0)
        {
            word = row[0].Trim().Trim('"').ToLower(); // "fuck" → fuck
        }
    }
}

public class TableSlangData : BaseTableData
{
    public List<SlangTableData> SlangTableDataList { get; private set; }

    
    public override void Load()
    {
        SlangTableDataList = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_slangtable"), row => new SlangTableData(row));        
    }

    public bool ContainsSlang(string input)
    {
        string lowerInput = input.ToLower();
        foreach (var slang in SlangTableDataList)
        {
            if (lowerInput.Contains(slang.word))
                return true;
        }
        return false;
    }
}
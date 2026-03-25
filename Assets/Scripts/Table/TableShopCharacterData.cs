using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ShopCharacterTableData
{
    public int id;
    public int characterId;
    public int openLevel;
    public int gold;

    public ShopCharacterTableData(string[] row)
    {
        if (row.Length >= 4)
        {
            int.TryParse(row[0], out id);
            int.TryParse(row[1], out characterId);
            int.TryParse(row[2], out openLevel);
            int.TryParse(row[3], out gold);
        }
    }
}

public class TableShopCharacterData : BaseTableData
{
    public List<ShopCharacterTableData> ShopCharacterTableDataList { get; private set; }

    public ShopCharacterTableData GetData(int _id)
    {
        return ShopCharacterTableDataList.Where(x => x.id == _id).FirstOrDefault();
    }

    public ShopCharacterTableData GetDataByCharacterId(int _characterId)
    {
        return ShopCharacterTableDataList.Where(x => x.characterId == _characterId).FirstOrDefault();
    }

    public override void Load()
    {
        ShopCharacterTableDataList = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_shopcharactertable"), row => new ShopCharacterTableData(row));
    }
}
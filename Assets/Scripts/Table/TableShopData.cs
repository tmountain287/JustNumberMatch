using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ShopCategoryType
{
    GoldPack,
    Item,
}


[System.Serializable]
public class ShopData
{
    public string id;
    public ShopCategoryType shopCategoryType;
    public ItemType itemType; //골드 값
    public int value;
    public ItemType needItemType;
    public int needValue;
    public int saleValue;
   
    public ShopData(string[] row)
    {
        if (row.Length >= 7)
        {
            id = row[0].Trim();

            Enum.TryParse(row[1].Trim(), out shopCategoryType);
            Enum.TryParse(row[2].Trim(), out itemType);
            int.TryParse(row[3], out value);

            Enum.TryParse(row[4].Trim(), out needItemType);
            int.TryParse(row[5], out needValue);
            int.TryParse(row[6], out saleValue);
        }
    }
}

public class TableShopData : BaseTableData
{
    public List<ShopData> ShopDataList { get; private set; }

    public ShopData GetData(string _id)
    {
        return ShopDataList.Where(x => x.id == _id).FirstOrDefault();
    }

    public override void Load()
    {
        ShopDataList = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_shoptable"), row => new ShopData(row));
    }

}

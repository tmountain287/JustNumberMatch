using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;


[System.Serializable]
public class ProductCatalogData
{
    public string id;
    public ProductType productType;
    
    public string price;
    public int bonus;
    public bool isPremium;
    public ItemType itemType;
    public int value;
    public int saleValue;

    public ProductCatalogData(string[] row)
    {
        if (row.Length >= 9)
        {
#if UNITY_ANDROID
            id = row[0].Trim();
#elif UNITY_IOS
            id = row[1].Trim();
#else
            id = row[0].Trim();
#endif

            Enum.TryParse(row[2].Trim(), out productType);

            price = row[3].Trim();
            int.TryParse(row[4], out bonus);
            int.TryParse(row[5], out int premium);

            Enum.TryParse(row[6].Trim(), out itemType);
            int.TryParse(row[7], out value);
            int.TryParse(row[8], out saleValue);

            isPremium = premium == 1;
        }
    }
}

public class TableProductCatalogData : BaseTableData
{
    public List<ProductCatalogData> ProductCatalogDataList { get; private set; }

    public ProductCatalogData GetProductCatalogData(string _id)
    {
        return ProductCatalogDataList.Where(x => x.id == _id).FirstOrDefault();
    }

    public override void Load()
    {
        ProductCatalogDataList = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_productcatalogtable"), row => new ProductCatalogData(row));
    }

}

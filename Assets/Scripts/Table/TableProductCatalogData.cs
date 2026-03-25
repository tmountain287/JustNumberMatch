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
    public int gold; //골드 값
    public int price;
    public int sale;
    public bool isPremium;
    public int fireTicket;
    public int peeSteal;
    
    public ProductCatalogData(string[] row)
    {
        if (row.Length >= 8)
        {            
            id = row[0].Trim();

            Enum.TryParse(row[1].Trim(), out productType);

            int.TryParse(row[2], out gold);
            int.TryParse(row[3], out price);
            int.TryParse(row[4], out sale);
            int.TryParse(row[5], out int premium);
            int.TryParse(row[6], out fireTicket);
            int.TryParse(row[7], out peeSteal);

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

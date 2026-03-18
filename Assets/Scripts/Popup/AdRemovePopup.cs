using Common.Manager;
using Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace JustOneMatch.UI
{
    public class AdRemovePopup : BasePopup
    {
        [SerializeField] private Button puchaceButton = null;
        [SerializeField] private Text priceText = null;

        protected override void Start()
        {
            base.Start();

            ProductCatalogData productCatalogData = TableDataManager.Instance.TableProductCatalogData.ProductCatalogDataList.Where(x => x.isPremium == true).FirstOrDefault();

            string price = productCatalogData.price; // 기본값 (항상 안전)

            if (IAPManager.Instance.InitComplete &&
                IAPManager.Instance.TryGetLocalizedPriceString(productCatalogData.id, out var localizedPrice))
            {
                price = localizedPrice; // 있으면 덮어씀
            }

            priceText.text = price;

            puchaceButton.onClick.AddListener(() =>
            {
                UIManager.Instance.ShowLoading();
                IAPManager.Instance.BuyProduct(productCatalogData.id, (flag, error) =>
                {
                    if (flag)
                    {
                        UserDataManager.IsAdsFree = true;
                        GameAnalyticsHelper.LogAdRemovedPurchase(productCatalogData.id);
                        PopupManager.Instance.ClosePopup();
                        PopupManager.Instance.OpenPopup<AdRemoveCompletePopup>();
                        UserDataManager.Save();
                    }
                    else
                    {
                        PopupManager.Instance.OpenMessageBoxPopup("알림", error);
                    }
                    UIManager.Instance.HideLoading();
                });
            });


            
        }
    }
}
using Common.Manager;
using Common.UI;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class AdSaleRemovePopup : BasePopup
    {
        [SerializeField] private Button puchaceButton = null;
        [SerializeField] private Text priceText = null;

        [SerializeField] private Text remainingTimeValueText = null;

        private ProductCatalogData productCatalogData;

        protected override void OnEnable()
        {
            base.OnEnable();
            UserDataManager.MarkAdsSaleRemovePopupFirstShownIfNeeded();
        }

        protected override void Start()
        {
            base.Start();

            productCatalogData = TableDataManager.Instance.TableProductCatalogData.ProductCatalogDataList
                .Where(x => x.isPremium == true && x.value > 0)
                .FirstOrDefault();

            if (productCatalogData == null)
            {
                Debug.LogWarning("[AdSaleRemovePopup] No premium pack product (isPremium && value>0).");
                if (puchaceButton != null) puchaceButton.interactable = false;
                return;
            }

            string price = productCatalogData.price;

            if (IAPManager.Instance.InitComplete &&
                IAPManager.Instance.TryGetLocalizedPriceString(productCatalogData.id, out var localizedPrice))
            {
                price = localizedPrice;
            }

            if (priceText != null)
                priceText.text = price;

            puchaceButton.onClick.AddListener(() =>
            {
                UIManager.Instance.ShowLoading();
                IAPManager.Instance.BuyProduct(productCatalogData.id, (flag, error) =>
                {
                    if (flag)
                    {
                        UserDataManager.IsAdsFree = true;
                        UserDataManager.AddItemCount(productCatalogData.itemType, productCatalogData.value);
                        if (productCatalogData.value > 0)
                            UserDataManager.AddItemCount(productCatalogData.itemType, productCatalogData.value);
                        GameAnalyticsHelper.LogAdRemovedPurchase(productCatalogData.id);
                        PopupManager.Instance.ClosePopup();
                        PopupManager.Instance.OpenPopup<AdSaleRemoveCompletePopup>();
                        UserDataManager.Save();
                    }
                    else
                    {
                        if (error != "UserCancelled")
                            PopupManager.Instance.OpenMessageBoxPopup("", error);
                    }
                    UIManager.Instance.HideLoading();
                });
            });
        }

        private void Update()
        {
            if (remainingTimeValueText == null)
                return;

            if (!UserDataManager.ShouldOfferPremiumPackSale())
            {
                remainingTimeValueText.text = "00d 00h 00m 00s";
                //if (puchaceButton != null)
                //    puchaceButton.interactable = false;
                return;
            }

            var r = UserDataManager.GetPremiumPackSaleRemainingTimeSpan();
            int totalSeconds = Mathf.Max(0, (int)r.TotalSeconds);
            int d = totalSeconds / 86400;
            int h = (totalSeconds % 86400) / 3600;
            int m = (totalSeconds % 3600) / 60;
            int s = totalSeconds % 60;
            remainingTimeValueText.text = $"{d}d {h:00}h {m:00}m {s:00}s";
        }
    }
}
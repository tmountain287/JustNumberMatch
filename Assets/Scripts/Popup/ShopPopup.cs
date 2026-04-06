using Common.Manager;
using Common.UI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace JustOneMatch.UI
{
    public class ShopPopup : BasePopup
    {
        [SerializeField] private List<Toggle> toggleList = null;
        [SerializeField] private List<GameObject> panelList = null;

        [SerializeField] private List<ShopItem> goldShopItemList = null;

        [SerializeField] private List<ShopItem> hintShopItemList = null;
        [SerializeField] private List<ShopItem> attackTimeTicketShopItemList = null;
        [SerializeField] private List<ShopItem> changeShopItemList = null;

        protected override void OnEnable()
        {
            base.OnEnable();
            GameAnalyticsHelper.LogShopOpen();
        }

        protected override void Start()
        {
            base.Start();
          
            for (int i = 0; i < toggleList.Count; i++)
            {
                int index = i;

                toggleList[i].onValueChanged.AddListener((isOn) =>
                {
                    panelList[index].gameObject.SetActive(isOn);
                });
            }
            {
                List<ProductCatalogData> productCatalogDataList = TableDataManager.Instance.TableProductCatalogData.ProductCatalogDataList.Where(x => x.isPremium == false && x.itemType == ItemType.Gold).ToList();

                for (int i = 0; i < productCatalogDataList.Count; i++)
                {
                    int index = i;

                    ProductCatalogData productCatalogData = productCatalogDataList[i];

                    string price = productCatalogData.price; // 기본값 (항상 안전)

                    if (IAPManager.Instance.InitComplete &&
                        IAPManager.Instance.TryGetLocalizedPriceString(productCatalogData.id, out var localizedPrice))
                    {
                        price = localizedPrice; // 있으면 덮어씀
                    }

                    goldShopItemList[i].SetItem(price, productCatalogData.value.ToString(), productCatalogData.saleValue, () =>
                    {
                        UIManager.Instance.ShowLoading();
                        IAPManager.Instance.BuyProduct(productCatalogData.id, (flag, error) =>
                        {
                            if (flag)
                            {
                                UserDataManager.AddItemCount(ItemType.Gold, productCatalogData.value);
                                PopupManager.Instance.OpenPopup<PurchaseCompletePopup>().Initialize(index, productCatalogData.value.ToString());
                                UserDataManager.Save();
                            }
                            else
                            {
                                PopupManager.Instance.OpenMessageBoxPopup("", error);
                            }
                            UIManager.Instance.HideLoading();
                        });
                    });
                }
            }
            {
                List<ShopData> shopHintDataList = TableDataManager.Instance.TableShopData.ShopDataList.Where(x => x.itemType == ItemType.Hint).ToList();

                for (int i = 0; i < shopHintDataList.Count; i++)
                {
                    string needStr = shopHintDataList[i].needValue.ToString();
                    string valueStr = $"x {shopHintDataList[i].value}";
                    int index = i;
                    hintShopItemList[i].SetItem(needStr, valueStr, shopHintDataList[i].saleValue, () =>
                    {
                        if (UserDataManager.GetItemCount(ItemType.Gold) < shopHintDataList[index].needValue)
                        {
                            PopupManager.Instance.OpenMessageBoxPopup("", LocalizationManager.Instance.GetText("NotEnoughGold"));
                        }
                        else
                        {
                            PopupManager.Instance.OpenPopup<PurchasePopup>().Initialize(ItemType.Hint, needStr, valueStr, () =>
                            {
                                UserDataManager.AddItemCount(ItemType.Hint, shopHintDataList[index].value);
                                UserDataManager.SubItemCount(ItemType.Gold, shopHintDataList[index].needValue);
                                UserDataManager.Save();
                            });
                        }
                    });
                }
            }
            {
                List<ShopData> shopAttackTimeTicetDataList = TableDataManager.Instance.TableShopData.ShopDataList.Where(x => x.itemType == ItemType.TimeAttackTicket).ToList();

                for (int i = 0; i < changeShopItemList.Count; i++)
                {
                    string needStr = shopAttackTimeTicetDataList[i].needValue.ToString();
                    string valueStr = $"x {shopAttackTimeTicetDataList[i].value}";
                    int index = i;
                    attackTimeTicketShopItemList[i].SetItem(needStr, valueStr, shopAttackTimeTicetDataList[i].saleValue, () =>
                    {
                        if (UserDataManager.GetItemCount(ItemType.Gold) < shopAttackTimeTicetDataList[index].needValue)
                        {
                            PopupManager.Instance.OpenMessageBoxPopup("", LocalizationManager.Instance.GetText("NotEnoughGold"));
                        }
                        else
                        {
                            PopupManager.Instance.OpenPopup<PurchasePopup>().Initialize(ItemType.TimeAttackTicket, needStr, valueStr, () =>
                            {
                                UserDataManager.AddItemCount(ItemType.TimeAttackTicket, shopAttackTimeTicetDataList[index].value);
                                UserDataManager.SubItemCount(ItemType.Gold, shopAttackTimeTicetDataList[index].needValue);
                                UserDataManager.Save();
                            });
                        }
                    });
                }
            }
            {
                List<ShopData> dataList = TableDataManager.Instance.TableShopData.ShopDataList.Where(x => x.itemType == ItemType.Change).ToList();

                for (int i = 0; i < dataList.Count; i++)
                {
                    string needStr = dataList[i].needValue.ToString();
                    string valueStr = $"x {dataList[i].value}";
                    int index = i;
                    changeShopItemList[i].SetItem(needStr, valueStr, dataList[i].saleValue, () =>
                    {
                        if (UserDataManager.GetItemCount(ItemType.Gold) < dataList[index].needValue)
                        {
                            PopupManager.Instance.OpenMessageBoxPopup("", LocalizationManager.Instance.GetText("NotEnoughGold"));
                        }
                        else
                        {
                            PopupManager.Instance.OpenPopup<PurchasePopup>().Initialize(ItemType.Change, needStr, valueStr, () =>
                            {
                                UserDataManager.AddItemCount(ItemType.Change, dataList[index].value);
                                UserDataManager.SubItemCount(ItemType.Gold, dataList[index].needValue);
                                UserDataManager.Save();
                            });
                        }
                    });
                }
            }

        }

        public void Initialize(ShopCategoryType shopCategoryType = ShopCategoryType.GoldPack)
        {
            toggleList[(int)shopCategoryType].isOn = true;

            for (int i = 0; i < panelList.Count; i++)
            {   
                panelList[i].gameObject.SetActive(i == (int)shopCategoryType);
            }
        }
    }
}
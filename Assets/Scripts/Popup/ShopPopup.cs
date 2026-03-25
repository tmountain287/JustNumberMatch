using Common.Manager;
using Common.UI;
using GoogleMobileAds.Api;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class ShopPopup : BasePopup
    {        
        [SerializeField] private List<ShopItemGoldPanel> shopItemGoldPanelList = null;
        [SerializeField] private List<ShopItemFireTicketPanel> shopItemFireTicketPanelList = null;
        [SerializeField] private List<ShopItemPeeSteaPanel> shopItemPeeStealPanelList = null;

        private bool init = false;

        public void Initialize()
        {
            if (!init)
            {
                init = true;
                List<string> productGoldList = new List<string>() { "gold_1000", "gold_5000", "gold_10000" };
                for (int i = 0; i < shopItemGoldPanelList.Count; i++)
                {
                    ProductCatalogData data = TableDataManager.Instance.TableProductCatalogData.GetProductCatalogData(productGoldList[i]);
                    shopItemGoldPanelList[i].SetPanel(data, () =>
                    {
                        UIManager.Instance.ShowLoading();
                        NetworkManager.Instance.BuyProductAsync(data.id, (success, result) =>
                        {
                            if (success)
                            {
                                int reward = data.gold;
                                PopupManager.Instance.OpenMessageBoxPopup("구매성공", $"<color=#FF6600>{reward.FormatComma()}골드</color> 상품을\n구매완료하였습니다.");
                                UserDataManager.AddGold(reward);
                                _ = NetworkManager.Instance.SendItemLog("GoldInApp", reward);
                                UserDataManager.Save(true);
                            }
                            else
                            {
                                if (result != "UserCancelled")
                                    PopupManager.Instance.OpenMessageBoxPopup("구매실패", result);
                            }
                            UIManager.Instance.HideLoading();

                        });
                    });
                }


                List<string> productFireTicketList = new List<string>() { "fireticket_20" };
                for (int i = 0; i < shopItemFireTicketPanelList.Count; i++)
                {
                    ProductCatalogData data = TableDataManager.Instance.TableProductCatalogData.GetProductCatalogData(productFireTicketList[i]);
                    shopItemFireTicketPanelList[i].SetPanel(data, () =>
                    {
                        UIManager.Instance.ShowLoading();
                        NetworkManager.Instance.BuyProductAsync(data.id, (success, result) =>
                        {
                            if (success)
                            {
                                PopupManager.Instance.OpenMessageBoxPopup("구매성공", $"<color=#FF6600>불꽃승부 {data.fireTicket}개</color> 상품을\n구매완료하였습니다.");
                                UserDataManager.AddFireTicket(data.fireTicket);
                                _ = NetworkManager.Instance.SendItemLog("FireTicketInApp", data.fireTicket);
                                UserDataManager.AddGold(data.gold);
                                if(data.gold > 0)
                                    _ = NetworkManager.Instance.SendItemLog("GoldInApp", data.gold);
                                UserDataManager.Save(true);
                            }
                            else
                            {
                                if (result != "UserCancelled")
                                    PopupManager.Instance.OpenMessageBoxPopup("구매실패", result);
                            }
                            UIManager.Instance.HideLoading();

                        });
                    });
                }

                List<string> productPeeStealList = new List<string>() { "pee_steal" };
                for (int i = 0; i < shopItemPeeStealPanelList.Count; i++)
                {
                    ProductCatalogData data = TableDataManager.Instance.TableProductCatalogData.GetProductCatalogData(productPeeStealList[i]);
                    shopItemPeeStealPanelList[i].SetPanel(data, () =>
                    {
                        UIManager.Instance.ShowLoading();
                        NetworkManager.Instance.BuyProductAsync(data.id, (success, result) =>
                        {
                            if (success)
                            {
                                PopupManager.Instance.OpenMessageBoxPopup("구매성공", $"<color=#FF6600>피 뺏기 {data.peeSteal}개</color> 상품을\n구매완료하였습니다.");
                                UserDataManager.PeeStealCount = UserDataManager.PeeStealCount + data.peeSteal;
                                UserDataManager.Save(true);
                            }
                            else
                            {
                                if (result != "UserCancelled")
                                    PopupManager.Instance.OpenMessageBoxPopup("구매실패", result);
                            }
                            UIManager.Instance.HideLoading();

                        });
                    });
                }
            }
        }
    }
}
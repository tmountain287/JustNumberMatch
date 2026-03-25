using Common.Manager;
using UnityEngine;
using UnityEngine.UI;
using Common.UI;

namespace Gostop.UI
{
    public class AdShopPopup : BasePopup
    {
        [SerializeField] private Button premiumButton = null;

        protected override void Start()
        {
            base.Start();

            premiumButton.onClick.AddListener(() =>
            {
                UIManager.Instance.ShowLoading();

                NetworkManager.Instance.BuyProductAsync("ads_remove", (success, result) =>
                {
                    if (success)
                    {
                        int reward = TableDataManager.Instance.TableProductCatalogData.GetProductCatalogData("ads_remove").gold;

                        UserDataManager.BuyPremium();                        
                        PopupManager.Instance.OpenMessageBoxPopup("구매성공", $"<color=#FF6600>광고제거</color> 상품을\n구매완료하였습니다.");
                        UserDataManager.AddGold(reward);

                        _ = NetworkManager.Instance.SendItemLog("GoldInApp", reward);

                        UserDataManager.Save(true);
                        PopupManager.Instance.ClosePopup<AdShopPopup>();
                    }
                    else
                    {
                        if(result != "UserCancelled")
                            PopupManager.Instance.OpenMessageBoxPopup("구매실패", result);
                    }
                    UIManager.Instance.HideLoading();
                });
            });
        }       
    }
}
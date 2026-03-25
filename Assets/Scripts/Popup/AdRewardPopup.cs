using Common.Manager;
using Common.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class AdRewardPopup : BasePopup
    {
        [SerializeField] private Text message = null;
        [SerializeField] private Button adButton = null;

        protected override void Start()
        {
            base.Start();
            adButton.onClick.AddListener(() =>
            {
                UIManager.Instance.ShowRewardedAd((adapter) =>
                {
                    _ = NetworkManager.Instance.AdReward(adapter, "AddGold", ConfigData.AdGoldReward);
                    _ = NetworkManager.Instance.SendItemLog("GoldAdReward", ConfigData.AdGoldReward);
                    UserDataManager.AddGold(ConfigData.AdGoldReward);
                    ClosePopup();
                    PopupManager.Instance.OpenMessageBoxPopup("광고시청완료!!", $"<color=#FF6600>{ConfigData.AdGoldReward}골드</color>가\n지급되었습니다!");
                    UserDataManager.Save(true);
                    
                });
            });
        }

        public void Initialize()
        {
            message.text = $"광고를 보고 <color=#FF6600>{ConfigData.AdGoldReward} 골드</color>를\n획득하시겠습니까?";
        }
    }
}
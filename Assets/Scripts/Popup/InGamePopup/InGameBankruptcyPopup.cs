using Common.Manager;
using Common.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class InGameBankruptcyPopup : BasePopup
    {        
        [SerializeField] private Text nomalMoneyText = null;
        [SerializeField] private Text adMoneyText = null;
        
        [SerializeField] private Button normalRevivalButton = null;
        [SerializeField] private Button adRevivalButton = null;

        protected override void Start()
        {
            base.Start();
            normalRevivalButton.onClick.AddListener(() =>
            {
                //NetworkManager.Instance.LobbySession.SendReqContinueGame();
                InGameManager.Instance.SendReqRevival(false);
                ClosePopup();
            });

            adRevivalButton.onClick.AddListener(() =>
            {
                UIManager.Instance.ShowRewardedAd((adapter) =>
                {
                    InGameManager.Instance.SendReqRevival(true);
                    ClosePopup();
                }, null);
            });
        }

        public void Initialize(long _nomalMoney, long _adMoney)
        {
            nomalMoneyText.text = _nomalMoney.FormatKoreanUnits();
            adMoneyText.text = _adMoney.FormatKoreanUnits();
        }
    }
}
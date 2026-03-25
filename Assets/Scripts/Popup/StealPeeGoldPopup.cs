using Common.Manager;
using Common.UI;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class StealPeeGoldPopup : BasePopup
    {
        [SerializeField] private Text message = null;
        [SerializeField] private Button okButton = null;

        private Action okAction = null;

        protected override void Start()
        {
            base.Start();
            okButton.onClick.AddListener(() =>
            {
                if(UserDataManager.Gold < ConfigData.StealPeeGold)
                {
                    PopupManager.Instance.OpenMessageBoxPopup("알림", "<color=#FF6600>골드</color>가 부족합니다.");
                }
                else
                {
                    ClosePopup(okAction);
                }                    
            });
        }

        public void Initialize(Action _okAction)
        {
            message.text = $"<color=#FF6600>{ConfigData.StealPeeGold} 골드</color>로 구매하여\n사용하시겠습니까?";
            okAction = _okAction;
        }
    }
}
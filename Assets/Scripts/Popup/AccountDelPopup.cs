using Common.Manager;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class AccountDelPopup : BasePopup
    {
        [SerializeField] private Button okButton = null;

        protected override void Start()
        {
            base.Start();
            okButton.onClick.AddListener(() =>
            {
                ClosePopup();
                PopupManager.Instance.OpenPopup<AccountDelConfirmPopup>().Initialize();
            });
        }
    }
}
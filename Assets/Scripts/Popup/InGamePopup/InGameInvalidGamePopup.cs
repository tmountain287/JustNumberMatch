using UnityEngine;
using UnityEngine.UI;
using Common.UI;

namespace UI.Popup
{
    public class InGameInvalidGamePopup : BasePopup
    {
        [SerializeField] private Button confirmButton = null;

        protected override void Start()
        {
            base.Start();
            confirmButton.onClick.AddListener(() =>
            {
                ClosePopup();
            });
        }

        private void OnDisable()
        {
            InGameManager.Instance.SendReqInvalidGameConfirm();
        }
    }
}
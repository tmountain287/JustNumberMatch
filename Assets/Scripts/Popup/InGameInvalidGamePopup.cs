using UnityEngine;
using UnityEngine.UI;
using Common.UI;

namespace Gostop.UI
{
    public class InGameInvalidGamePopup : BasePopup
    {
        [SerializeField] private Button confirmButton = null;

        protected override void Start()
        {
            base.Start();
            confirmButton.onClick.AddListener(() =>
            {
                ClosePopup(()=>
                {
                    InGameManager.Instance.SendReqInvalidGameConfirm();
                });
            });
        }
    }
}
using Common.Manager;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class RewardsPopup : BasePopup
    {
        [SerializeField] private Button okButton = null;

        protected override void Start()
        {
            base.Start();
            okButton.onClick.AddListener(() =>
            {
                Application.Quit();
            });
        }
    }
}
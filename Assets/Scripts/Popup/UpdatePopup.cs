using UnityEngine;
using UnityEngine.UI;
using Common.UI;

namespace JustOneMatch.UI
{
    public class UpdatePopup : BasePopup
    {
        [SerializeField] private Button okButton = null;

        protected override void Start()
        {
            base.Start();
            okButton.onClick.AddListener(() =>
            {
                Application.OpenURL(AppDefine.STORE_APP_URL);
            });
        }
    }
}
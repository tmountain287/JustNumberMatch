using Common.Manager;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class ExitPopup : BasePopup
    {
        [SerializeField] private Button exitButton = null;

        protected override void Start()
        {
            base.Start();
            exitButton.onClick.AddListener(() =>
            {
                Application.Quit();
            });
        }
    }
}
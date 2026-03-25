using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Common.UI;

namespace Gostop.UI
{
    public class InGameRevivalPopup : BasePopup
    {        
        [SerializeField] private Button startButton = null;
        [SerializeField] private CanvasGroup message = null;

        protected override void Start()
        {
            base.Start();

            startButton.onClick.AddListener(() =>
            {                
                InGameManager.Instance.SendReqNextGame(false, false);
                ClosePopup();
            });
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            message.alpha = 0;
            startButton.enabled = false;
            message.DOFade(1, 0.2f).SetDelay(0.2f).OnComplete(() =>
            {
                startButton.enabled = true;
            });
        }
    }
}
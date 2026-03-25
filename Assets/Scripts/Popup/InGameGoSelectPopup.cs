using UnityEngine;
using UnityEngine.UI;
using Common.UI;

namespace Gostop.UI
{
    public class InGameGoSelectPopup : BasePopup
    {
        [SerializeField] private Text goldText = null;
        //[SerializeField] private Text scoreText = null;
        [SerializeField] private Button stopButton = null;
        [SerializeField] private Button goButton = null;
        [SerializeField] private Text goButtonText = null;

        [SerializeField] private GameObject bankruptcy = null;

        protected override void Start()
        {
            base.Start();
            goButton.onClick.AddListener(() =>
            {
                //NetworkManager.Instance.LobbySession.SendReqSelectGoStop(true);
                InGameManager.Instance.SendReqSelectGoStop(true);
                ClosePopup();
            });

            stopButton.onClick.AddListener(() =>
            {
                //NetworkManager.Instance.LobbySession.SendReqSelectGoStop(false);
                InGameManager.Instance.SendReqSelectGoStop(false);
                ClosePopup();
            });
        }

        public void Initialize(bool _isbankruptcy, long _gold, int _goCount)
        {
            bankruptcy.SetActive(_isbankruptcy);
            goldText.text = $"<color=#ADADAD>스톱시,</color> <color=#00EAFF><b>+{_gold.FormatKoreanUnits()}</b></color>";
            //scoreText.text = _score.FormatComma();

            goButtonText.text = _goCount.ToString();
        }
    }
}
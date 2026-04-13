using UnityEngine;
using UnityEngine.UI;
using Common.UI;

namespace UI.Popup
{
    public class InGameGuSelectPopup : BasePopup
    {
        [SerializeField] private Button noButton = null;
        [SerializeField] private Button okButton = null;

        [SerializeField] private GameObject mungObject = null;

        private int slotIndex = -1;
        protected override void Start()
        {
            base.Start();
            okButton.onClick.AddListener(() =>
            {
                //NetworkManager.Instance.LobbySession.SendReqSelectGukjunToPee(true);
                InGameManager.Instance.SendReqSelectGukjunToPee(slotIndex, true);
                ClosePopup();
            });

            noButton.onClick.AddListener(() =>
            {
                //NetworkManager.Instance.LobbySession.SendReqSelectGukjunToPee(false);
                InGameManager.Instance.SendReqSelectGukjunToPee(slotIndex, false);
                ClosePopup();
            });
        }

        public void Initialize(int _slotIndex, bool _isMung)
        {
            slotIndex = _slotIndex;
            mungObject.SetActive(_isMung);
        }
    }
}
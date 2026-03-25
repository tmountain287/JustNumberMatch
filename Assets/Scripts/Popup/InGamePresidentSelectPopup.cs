using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common.UI;

namespace Gostop.UI
{
    public class InGamePresidentSelectPopup : BasePopup
    {
        [SerializeField] private List<Card> cardList = null;
        [SerializeField] private Button stopButton = null;
        [SerializeField] private Button continueButton = null;

        protected override void Start()
        {
            base.Start();
            continueButton.onClick.AddListener(() =>
            {
                //NetworkManager.Instance.LobbySession.SendReqSelectPresident(true);
                InGameManager.Instance.SendReqSelectPresident(true);
                ClosePopup();
            });

            stopButton.onClick.AddListener(() =>
            {
                //NetworkManager.Instance.LobbySession.SendReqSelectPresident(false);
                InGameManager.Instance.SendReqSelectPresident(false);
                ClosePopup();
            });
        }

        public void Initialize(List<int> _cardList)
        {
            _cardList.Sort();
            for (int i = 0; i < _cardList.Count; i++)
            {
                cardList[i].SetCard(_cardList[i]);
            }
        }
    }
}
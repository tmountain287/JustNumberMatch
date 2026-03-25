using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common.UI;

namespace Gostop.UI
{
    public class InGameShakeSelectPopup : BasePopup
    {
        [SerializeField] private List<Card> cardList = null;
        [SerializeField] private Button noButton = null;
        [SerializeField] private Button okButton = null;

        protected override void Start()
        {
            base.Start();
            okButton.onClick.AddListener(() =>
            {
                //NetworkManager.Instance.LobbySession.SendReqShakeResult(true);
                InGameManager.Instance.SendReqShakeResult(true);
                ClosePopup();
            });

            noButton.onClick.AddListener(() =>
            {
                InGameManager.Instance.SendReqShakeResult(false);
                //NetworkManager.Instance.LobbySession.SendReqShakeResult(false);
                ClosePopup();
            });
        }

        public void Initialize(List<int> _cardList)
        {
            _cardList.Sort();
            for (int i = 0; i < cardList.Count; i++)
            {
                cardList[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < _cardList.Count; i++)
            {
                cardList[i].SetCard(_cardList[i]);
                cardList[i].gameObject.SetActive(true);
            }
        }
    }
}
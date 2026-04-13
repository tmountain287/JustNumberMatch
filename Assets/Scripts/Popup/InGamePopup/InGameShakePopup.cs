using Common.Manager;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common.UI;

namespace UI.Popup
{
    public class InGameShakePopup : BasePopup
    {
        [SerializeField] private Transform target = null;
        [SerializeField] private List<Card> cardList = null;

        private Action onClose = null;

        private void OnDisable()
        {
            onClose = null;
            target.DOKill();
        }        

        protected override void OnEnable()
        {
            base.OnEnable();
            target.DOShakePosition(0.3f, 10.0f, 30, 90.0f, fadeOut: false)
                .OnComplete(() =>
                {
                    DOVirtual.DelayedCall(1.0f, () =>
                    {
                        PopupManager.Instance.ClosePopup<InGameShakePopup>(() =>
                        {
                            onClose?.Invoke();
                        });
                    });
                });
        }

        public void Initialize(List<int> _cardList, Action _onClose = null)
        {
            _cardList.Sort();
            onClose = _onClose;
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
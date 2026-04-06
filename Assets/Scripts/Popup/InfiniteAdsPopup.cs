using Common.Manager;
using Common.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JustOneMatch.UI
{
    public class InfiniteAdsPopup : BasePopup
    {
        [SerializeField] private Button okButton = null;
        [SerializeField] private CountdownTimer countdownTimer = null;

        private Action closeAction = null;
        private Action okAction = null;

        protected override void Start()
        {
            closeButton.onClick.AddListener(() =>
            {
                ClosePopup(closeAction);
            });

            okButton.onClick.AddListener(() =>
            {
                ClosePopup(okAction);
            });
        }

        public void Initialize(Action _okAction, Action _closeAction)
        {
            closeAction = _closeAction;
            okAction = _okAction;

            countdownTimer.SetTimer(5, () =>
            {
                ClosePopup(_closeAction);
            });

            DOVirtual.DelayedCall(0.2f, () =>
            {
                countdownTimer.StartTimer();
            });
        }
    }
}
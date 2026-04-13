using Common.Manager;
using Common.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class SelectUserPopup : BasePopup
    {
        [SerializeField] private SelectUserPanel beforePanel = null;
        [SerializeField] private SelectUserPanel currentPanel = null;

        private UserData beforeData;
        private UserData currentData;
        private Action<bool> onComplete = null;

        public void Initialize(UserData _beforeData, UserData _currentData, Action<bool> _onComplete = null)
        {
            beforeData = _beforeData;
            currentData = _currentData;

            onComplete = _onComplete;

            Initialize();
        }

        public void Initialize()
        {
            beforePanel.SetPanel(false, beforeData, () =>
            {
                ClosePopup(() =>
                    PopupManager.Instance.OpenPopup<SelectUserConfirmPopup>().Initialize(false, beforeData, onComplete));
            });

            currentPanel.SetPanel(true, currentData, () =>
            {
            ClosePopup(() =>
                PopupManager.Instance.OpenPopup<SelectUserConfirmPopup>().Initialize(true, currentData, onComplete));
            });
        }
    }
}
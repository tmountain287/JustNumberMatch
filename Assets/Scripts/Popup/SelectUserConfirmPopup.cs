using Common.Manager;
using Common.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class SelectUserConfirmPopup : BasePopup
    {
        [SerializeField] private SelectUserPanel beforePanel = null;
        [SerializeField] private SelectUserPanel currentPanel = null;

        [SerializeField] private Button cancleButton = null;
        [SerializeField] private Button confirmButton = null;

        private Action onConfirmClick = null;

        protected override void Start()
        {
            base.Start();
            cancleButton.onClick.AddListener(() =>
            {
                ClosePopup(()=>
                PopupManager.Instance.OpenPopup<SelectUserPopup>().Initialize());
                
            });

            confirmButton.onClick.AddListener(() =>
            {
                onConfirmClick?.Invoke();
            });
        }

        public void Initialize(bool _isCurrent, UserData _userData, Action<bool> _onComplete = null)
        {
            beforePanel.gameObject.SetActive(!_isCurrent);
            currentPanel.gameObject.SetActive(_isCurrent);

            if (!_isCurrent)
            {
                beforePanel.SetPanel(false, _userData);
            }
            else
            {
                currentPanel.SetPanel(true, _userData);
            }

            onConfirmClick = () =>
            {
                AsyncSaveDataMerge(_isCurrent, _userData, _onComplete);
            };
        }

        private async void AsyncSaveDataMerge(bool _isCurrent, UserData _userData, Action<bool> _onComplete)
        {
            UIManager.Instance.ShowLoading();
            string data = SecurePlayerPrefs.Encrypt(_userData);
            await NetworkManager.Instance.ApplyOverwriteAsync(data);
            UIManager.Instance.HideLoading();
            ClosePopup(()=>
            {
                PopupManager.Instance.OpenPopup<SelectUserSavePopup>().Initialize(_isCurrent, _userData, _onComplete);
            });           
        }
    }
}
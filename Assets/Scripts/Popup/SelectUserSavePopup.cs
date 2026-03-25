using Common.Manager;
using Common.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class SelectUserSavePopup : BasePopup
    {
        [SerializeField] private SelectUserPanel beforePanel = null;
        [SerializeField] private SelectUserPanel currentPanel = null;

        [SerializeField] private Button confirmButton = null;

        private Action onConfirmClick = null;

        protected override void Start()
        {
            base.Start();

            confirmButton.onClick.AddListener(() =>
            {
                onConfirmClick?.Invoke();
            });
        }
        public void Initialize(bool _isCurrent, UserData _userData, Action<bool> _onComplete)
        {
            beforePanel.gameObject.SetActive(!_isCurrent);
            currentPanel.gameObject.SetActive(_isCurrent);

            if (!_isCurrent)
            {
                beforePanel.SetPanel(_userData);
            }
            else
            {
                currentPanel.SetPanel(_userData);
            }

            onConfirmClick = () =>
            {
                ClosePopup();
                _onComplete?.Invoke(_isCurrent);
            };
        }
    }
}
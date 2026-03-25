using System;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UI
{
    public class MessageBoxPopup : BasePopup
    {
        //[Flags]
        //public enum ButtonType
        //{
        //    NONE = 0,
        //    NO = 1 << 0,
        //    YES = 1 << 1,
        //    CANCEL = 1 << 2,
        //    OK = 1 << 3,
        //    REJECT = 1 << 4,
        //    ACCEPT = 1 << 5,
        //}
        #region Inspector Fields
        [SerializeField] private Text titleText = null;
        [SerializeField] private Text messageText = null;
        [SerializeField] private Button okButton = null;
        [SerializeField] private Button cancelButton = null;
        #endregion

        private Action onOKClick = null;
        private Action onCancelClick = null;

        private void OnOKClick()
        {
            onOKClick?.Invoke();
        }

        private void OnCancelClick()
        {
            onCancelClick?.Invoke();
        }

        public void Initialize(string _title = "", string _message = "", Action _onOkClick = null, Action _onCancelClick = null)
        {
            titleText.gameObject.SetActive(!string.IsNullOrEmpty(_title));
            titleText.text = _title;
            messageText.text = _message;

            okButton.onClick.RemoveListener(OnOKClick);
            cancelButton.onClick.RemoveListener(OnCancelClick);

            onOKClick = _onOkClick;
            onCancelClick = _onCancelClick;

            if (_onOkClick != null)
            {
                okButton.onClick.AddListener(OnOKClick);
                okButton.gameObject.SetActive(true);
            }
            else
            {
                okButton.gameObject.SetActive(false);
            }

            if (_onCancelClick != null)
            {
                cancelButton.onClick.AddListener(OnCancelClick);
                cancelButton.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                cancelButton.transform.parent.gameObject.SetActive(false);
            }
        }
    }
}

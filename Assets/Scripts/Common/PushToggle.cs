using Common.Manager;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Common.UI
{
    public class PushToggle : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private Button onButton = null;
        [SerializeField] private Button offButton = null;
        #endregion

        public Action<bool> OnToggle { get; set; } = null;

        void Start()
        {
            onButton.onClick.AddListener(() =>
            {
                SetToggle(false);
            });

            offButton.onClick.AddListener(() =>
            {
                SetToggle(true);
            });
        }

        private void OnEnable()
        {
            //UserDataManager.OnValuePushChanged.AddListener(SetToggle);
            SetToggle();
        }

        private void OnDisable()
        {
            //UserDataManager.OnValuePushChanged.RemoveListener(SetToggle);
        }

        public void SetToggle()
        {
            //onButton.gameObject.SetActive(UserDataManager.IsPush);
            //offButton.gameObject.SetActive(!UserDataManager.IsPush);
        }
        

        public void SetToggle(bool _flag)
        {
            OnToggle?.Invoke(_flag);
        }
    }
}
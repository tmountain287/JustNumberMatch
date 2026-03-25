using Common.Manager;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Common.UI
{
    public class SettingToggle : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private PrefsKey prefsKey;
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

        protected virtual void OnEnable()
        {
            bool flag = GetFlag();
            onButton.gameObject.SetActive(flag);
            offButton.gameObject.SetActive(!flag);
        }

        public bool GetFlag()
        {
            return PlayerPrefsManager.Instance.GetPlayerPrefsInfo(prefsKey, 1).Value == 1;
        }

        public void SetToggle(bool _flag)
        {
            onButton.gameObject.SetActive(_flag);
            offButton.gameObject.SetActive(!_flag);

            PlayerPrefsManager.Instance.SetPlayerPrefsInfo(prefsKey, _flag ? 1 : 0);

            OnToggle?.Invoke(_flag);
        }
    }
}
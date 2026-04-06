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
        [SerializeField] private float defaultValue = 1f; // 저장값 없을 때 사용 (0 = off, 1 = on). 진동은 0 권장.
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
            float defaultVal = (prefsKey == PrefsKey.VIBRATION) ? 0f : defaultValue;
            return PlayerPrefsManager.Instance.GetPlayerPrefsInfo(prefsKey, defaultVal).Value > 0;
        }

        public void SetToggle(bool _flag)
        {
            onButton.gameObject.SetActive(_flag);
            offButton.gameObject.SetActive(!_flag);

            PlayerPrefsManager.Instance.SetPlayerPrefsInfo(prefsKey, _flag ? 1 : 0);


            OnToggle?.Invoke(_flag);
        }

        public void SetToggleActive(bool _flag)
        {
            onButton.gameObject.SetActive(_flag);
            offButton.gameObject.SetActive(!_flag);
        }
    }
}
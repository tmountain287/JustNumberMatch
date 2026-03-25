using Common.Manager;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Common.UI
{
    public class SettingToggleList : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private PrefsKey prefsKey;
        [SerializeField] private int defaltValue = 0;
        [SerializeField] private List<Toggle> toggleList = null;

        [SerializeField] private float scale = 1.2f;
        [SerializeField] private float duration = 0.1f;
        #endregion
        public Action<int> OnToggle { get; set; } = null;

        private bool isInitializing = false;

        void Start()
        {
            for (int i = 0; i < toggleList.Count; i++)
            {
                int index = i;
                toggleList[i].onValueChanged.AddListener((isOn) =>
                {
                    if (isOn && !isInitializing)
                    {
                        SetToggle(index);
                    }
                });
            }
        }

        private void OnEnable()
        {
            isInitializing = true;
            int index = PlayerPrefsManager.Instance.GetPlayerPrefsValue(prefsKey, defaltValue);            
            toggleList[index].isOn = true;
            isInitializing = false;
        }

        public virtual void SetToggle(int _index)
        {
            for (int i = 0; i < toggleList.Count; i++)
            {
                toggleList[i].interactable = i != _index;
            }

            PlayerPrefsManager.Instance.SetPlayerPrefsInfo(prefsKey, _index);

            toggleList[_index].graphic.transform.DOScale(scale, duration)
                           .SetEase(Ease.OutQuad)
                           .OnComplete(() =>
                           {
                               toggleList[_index].graphic.transform.DOScale(1f, duration)
                                   .SetEase(Ease.InQuad);
                           });

            OnToggle?.Invoke(_index);
        }
    }
}
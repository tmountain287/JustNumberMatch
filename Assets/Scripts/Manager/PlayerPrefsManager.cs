using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Manager
{
    public enum PrefsKey
    {
        Language,
        //ChipType,
        //AutoScreen,
        //Screen,
        BGM,
        FX_Sound,
        PUSH,
        VIBRATION,
    }

    public class PlayerPrefsInfo
    {
        public float Value { get; set; }
        public UnityEvent OnChangeDataEvent { get; set; } = new();

        public PlayerPrefsInfo(float _value)
        {
            Value = _value;
        }
    }

    public class PlayerPrefsManager : MonoSingletonDont<PlayerPrefsManager>
    {        
        private Dictionary<PrefsKey, PlayerPrefsInfo> playerPrefsInfoDict = new();

        public PlayerPrefsInfo GetPlayerPrefsInfo(PrefsKey _key, float _value = 0)
        {
            if(!playerPrefsInfoDict.ContainsKey(_key))
            {
                float value = PlayerPrefs.GetFloat(_key.ToString(), _value);
                playerPrefsInfoDict.Add(_key, new(value));
            }

            return playerPrefsInfoDict[_key];
        }
       

        public float GetPlayerPrefsValue(PrefsKey _key, float _value = 0)
        {
            return GetPlayerPrefsInfo(_key, _value).Value;
        }

        public void SetPlayerPrefsInfo(PrefsKey _key, float _value = 0)
        {
            if (!playerPrefsInfoDict.ContainsKey(_key))
            {
                float value = PlayerPrefs.GetFloat(_key.ToString(), _value);
                playerPrefsInfoDict.Add(_key, new(value));
            }
            else
            {
                playerPrefsInfoDict[_key].Value = _value;
            }

            PlayerPrefs.SetFloat(_key.ToString(), _value);
            PlayerPrefs.Save();

            playerPrefsInfoDict[_key].OnChangeDataEvent?.Invoke();
        }

        public void AddChangeEvent(PrefsKey _key, UnityAction _unityAction)
        {
            PlayerPrefsInfo playerPrefsInfo = GetPlayerPrefsInfo(_key);
            if(playerPrefsInfo != null)
            {
                playerPrefsInfo.OnChangeDataEvent.AddListener(_unityAction);
            }
        }

        public void RemoveChangeEvent(PrefsKey _key, UnityAction _unityAction)
        {
            PlayerPrefsInfo playerPrefsInfo = GetPlayerPrefsInfo(_key);
            if (playerPrefsInfo != null)
            {
                playerPrefsInfo.OnChangeDataEvent.RemoveListener(_unityAction);
            }
        }
    }
}
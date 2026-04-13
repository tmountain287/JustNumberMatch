using Common.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Popup
{
    

    //[Serializable]
    //public class VoiceSound
    //{
    //    public List<AudioClip> audioClipList;

    //    public string GetVoiceTypeFromEnumName(Enum sourceEnum)
    //    {
    //        string name = sourceEnum.ToString();

    //        if (Enum.TryParse(name, out VoiceType voiceType))
    //            return audioClipList[(int)voiceType];

    //        return null;
    //    }
    //}

    public class GamblerVoice : MonoBehaviour
    {
        [SerializeField] private bool isMine = false;
        
//        [SerializeField] private List<VoiceSound> voiceSoundList = null;

        public void PlayVoice(int _gender, Enum sourceEnum)
        {
            if (Enum.TryParse(sourceEnum.ToString(), out VoiceType voiceType))
            {
                string path = $"Voice/{(isMine ? "Mine" : "Other")}/{(_gender == 0 ? "Man" : "Woman")}/{voiceType}";

                AudioClip audioClip = Resources.Load<AudioClip>(path);
                if (audioClip != null)
                {
                    SoundManager.Instance.PlayFX(audioClip);
                }
            }
        }
    }
}

using Common.Manager;
using UnityEngine;

namespace Common.UI
{
    public class BgmPlayer : MonoBehaviour
    {
        [SerializeField] private bool autoPlay = false;
        [SerializeField] private AudioClip inBgm = null;

        public AudioClip InBgm { get => inBgm; set => inBgm = value; }

        // [SerializeField] private AudioClip outBgm = null;

        private void OnEnable()
        {
            if(autoPlay)
                PlayBgm();
        }

        //private void OnDisable()
        //{
        //    if(SoundManager.Instance != null)
        //        SoundManager.Instance.PlayBgm(outBgm);
        //}

        public void PlayBgm()
        {
            SoundManager.Instance.PlayBgm(InBgm);
        }
    }
}
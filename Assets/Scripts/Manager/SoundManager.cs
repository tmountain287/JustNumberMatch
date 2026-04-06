using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

namespace Common.Manager
{
    public enum FX_Type
    {
        Normal = 0,
        Game = 1,
    }

    public class SoundManager : MonoSingleton<SoundManager>
    {
        #region Inspector Fields
        [SerializeField] private AudioSource bgmAudioSource = null;
        [SerializeField] private ObjectPool fxPool = null;
        #endregion

        private List<AudioSource> audioSourceList = new ();
        private List<AudioClip> overlapList = new();

        private float bgmVolume = 1f;
        private float fxVolume = 1f;

        private Tween currentBgmTween;

        public float BgmVolume
        {
            get => bgmVolume;
            set
            {
                bgmVolume = Mathf.Clamp(value, 0f, 1f);
                bgmAudioSource.volume = bgmVolume;
            }
        }
        public float FxVolume
        {
            get => fxVolume;
            set
            {
                fxVolume = Mathf.Clamp(value, 0f, 1f);
               
                    audioSourceList.ForEach(audioSource =>
                    {
                        audioSource.volume = fxVolume;
                        Debug.Log(audioSource.clip.name);
                    });
               
            }
        }

        public void Awake()
        {
            FxVolume = PlayerPrefsManager.Instance.GetPlayerPrefsValue(PrefsKey.FX_Sound, 1);
            BgmVolume = PlayerPrefsManager.Instance.GetPlayerPrefsValue(PrefsKey.BGM, 1);
            fxPool.CreateObjectPool();
        }

        public void PlayBgm(AudioClip _audioClip)
        {
            if (bgmAudioSource.clip == _audioClip)
            {
                return;
            }

            void PlayNewBgm()
            {
                if (_audioClip != null)
                {
                    bgmAudioSource.volume = 0f;
                    bgmAudioSource.clip = _audioClip;
                    bgmAudioSource.Play();
                    currentBgmTween = DOTween.To(() => bgmAudioSource.volume, x => bgmAudioSource.volume = x, BgmVolume, 0.5f);
                }
                else
                {
                    bgmAudioSource.clip = null;
                }
            }

            if (bgmAudioSource.isPlaying)
            {
                // 현재 재생 중이면 볼륨 감소 후 교체
                currentBgmTween = DOTween.To(() => bgmAudioSource.volume, x => bgmAudioSource.volume = x, 0f, 0.5f)
                    .OnComplete(() =>
                    {
                        bgmAudioSource.Stop();
                        PlayNewBgm();
                    });
            }
            else
            {
                // 재생 중이 아니면 바로 재생
                PlayNewBgm();
            }
        }

        public void StopBgm()
        {
            if (bgmAudioSource.isPlaying)
            {
                PlayBgm(null);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (currentBgmTween != null && currentBgmTween.IsActive())
            {
                currentBgmTween.Kill();
                currentBgmTween = null;
            }
        }

        public void PlayFX(AudioClip _audioClip, float _delay)
        {
            StartCoroutine(PlayFXRoutine(_audioClip, _delay));
        }

        private IEnumerator PlayFXRoutine(AudioClip _audioClip, float _delay)
        {
            yield return new WaitForSeconds(_delay);
            PlayFX(_audioClip);
        }

        public void PlayFX(AudioClip _audioClip, float _pitch = 1.0f, bool _overlap = true)
        {
            if (_audioClip == null)
                return;

            if (!_overlap && overlapList.Contains(_audioClip))
            {
                return;
            }

            AudioSource audioSource = fxPool.GetObjectFromPool<AudioSource>();

            audioSource.volume = fxVolume;
            audioSource.clip = _audioClip;
            audioSource.pitch = _pitch;
            audioSource.Play();

            if (!_overlap)
            {
                overlapList.Add(_audioClip);
            }

            audioSourceList.Add(audioSource);

            StartCoroutine(ReturnToPoolAfterPlay(audioSource, _audioClip, _overlap));
        }

        //public void AllStopFX()
        //{
        //    foreach (var item in fxDict)
        //    {
        //        item.Value.ForEach(audio => audio.Stop());
        //    }
        //}

        //public void StopFX(int _inGameIndex)
        //{
        //    if (fxDict.ContainsKey(_inGameIndex))
        //    {
        //        fxDict[_inGameIndex].ForEach(audio => audio.Stop());
        //    }
        //}

        private IEnumerator ReturnToPoolAfterPlay(AudioSource _audioSource, AudioClip _audioClip, bool _overlap)
        {
            yield return new WaitWhile(() => _audioSource.isPlaying && _audioSource.clip == _audioClip);
            
            audioSourceList.Remove(_audioSource);             

            if (!_overlap)
            {
                overlapList.Remove(_audioClip);
            }

            fxPool.ReturnObjectToPool(_audioSource.gameObject);
        }

        public void MuteSound()
        {
            FxVolume = 0;
            BgmVolume = 0;
        }

        public void RestoreSound()
        {
            FxVolume = PlayerPrefsManager.Instance.GetPlayerPrefsValue(PrefsKey.FX_Sound, 1);
            BgmVolume = PlayerPrefsManager.Instance.GetPlayerPrefsValue(PrefsKey.BGM, 1);
        }

        public void MuteBGMSound()
        {
            BgmVolume = 0;
        }

        public void RestoreBGMSound()
        {            
            BgmVolume = PlayerPrefsManager.Instance.GetPlayerPrefsValue(PrefsKey.BGM, 1);
        }
    }
}
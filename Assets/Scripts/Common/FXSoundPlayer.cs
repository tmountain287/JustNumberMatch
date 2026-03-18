using Common.Manager;
using UnityEngine;

namespace Common.UI
{
    public class FXSoundPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip clip = null;

        private void OnEnable()
        {
            SoundManager.Instance.PlayFX(clip);
        }
    }
}
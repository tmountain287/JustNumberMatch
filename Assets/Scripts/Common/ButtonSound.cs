using Common.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Common.UI
{
    public class ButtonSound : MonoBehaviour, IPointerClickHandler
    {
        #region Inspector Fields
        [SerializeField] private AudioClip audioClip = null;
        [SerializeField] private Selectable button = null;
        #endregion

        private void OnValidate()
        {
            if (button == null)
            {
                button = GetComponent<Selectable>();
            }         
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (button != null && (!button.enabled || !button.interactable))
                return;

            if (audioClip != null)
            {
                SoundManager.Instance.PlayFX(audioClip);
            }
        }
    }
}

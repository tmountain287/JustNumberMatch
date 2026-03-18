using UnityEngine;
using UnityEngine.UI;

namespace Common.UI
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollControl : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect = null;

        private void OnValidate()
        {
            if (scrollRect == null)
            {
                scrollRect = GetComponent<ScrollRect>();
            }
        }

        void Update()
        {
            if (scrollRect.content.rect.height <= scrollRect.viewport.rect.height)
            {
                scrollRect.vertical = false;
            }
            else
            {
                scrollRect.vertical = true;
            }

            if (scrollRect.content.rect.width <= scrollRect.viewport.rect.width)
            {
                scrollRect.horizontal = false;
            }
            else
            {
                scrollRect.horizontal = true;
            }
        }
    }
}

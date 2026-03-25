using UnityEngine;
using UnityEngine.UI;
using Common.UI;
 
namespace Gostop.UI
{
    public class FullShotPopup : BasePopup
    {
        [SerializeField] private CustomScrollRect scrollRect = null;        
        [SerializeField] private Slider slider = null;
        [SerializeField] private Image character = null;

        private Vector2 oriSize = Vector2.zero;

        protected override void Start()
        {
            base.Start();

            slider.onValueChanged.AddListener((value) =>
            {
                character.rectTransform.sizeDelta = oriSize * value;
            });
        }

        public void Initialize(CharacterTableData _data)
        {
            CharacterResManager.Instance.SetImage(character, _data.resource, CharacterImage.Type.FullShot);
            
            oriSize = character.rectTransform.sizeDelta;
            
            slider.value = 1;

            scrollRect.normalizedPosition = Vector2.one * 0.5f;
        }

        private float initialDistance = 0f;
        private float initialZoom = 1f;      

        void Update()
        {
            if (Input.touchCount == 2)
            {
                Touch t1 = Input.GetTouch(0);
                Touch t2 = Input.GetTouch(1);

                if (t1.phase == TouchPhase.Began || t2.phase == TouchPhase.Began)
                {
                    initialDistance = Vector2.Distance(t1.position, t2.position);
                    initialZoom = slider.value;
                    scrollRect.blockDrag = true;
                }

                float currentDistance = Vector2.Distance(t1.position, t2.position);
                float pinchRatio = currentDistance / initialDistance;
                
                slider.value = Mathf.Clamp(initialZoom * pinchRatio, slider.minValue, slider.maxValue);
            }
            else
            {
                scrollRect.blockDrag = false;
            }
        }
    }
}
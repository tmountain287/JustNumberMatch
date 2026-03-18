using UnityEngine;

namespace Common.UI
{
    public class WorldCameraScaler : MonoBehaviour
    {
        public Camera worldCamera;
        public float referenceResolutionWidth = 1280f;  // 기준 해상도 너비 (1280)
        public float referenceResolutionHeight = 720f;  // 기준 해상도 높이 (720)

        private float lastScreenWidth;
        private float lastScreenHeight;

        void Start()
        {
            AdjustCameraAndCanvas();
        }

        void Update()
        {
            // 화면 크기 변화를 감지
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
                AdjustCameraAndCanvas();
            }
        }

        void AdjustCameraAndCanvas()
        {
            // 현재 화면 비율 계산
            float screenAspect = (float)Screen.width / Screen.height;
            float referenceAspect = referenceResolutionWidth / referenceResolutionHeight;

            if (screenAspect > referenceAspect)
            {
                float heightRate = Screen.height / referenceResolutionHeight;

                float space = (Screen.width - (referenceResolutionWidth * heightRate)) / Screen.width; //이값이 잘리는 크기계산

                worldCamera.rect = new Rect(space * 0.5f, 0, 1 - space, 1);
            }
            else if (screenAspect < referenceAspect)
            {
                //가로가 더 짧네 가로를 꽉 채우자
                float widthRate = Screen.width / referenceResolutionWidth;

                float space = (Screen.height - (referenceResolutionHeight * widthRate)) / Screen.height; //이값이 잘리는 크기계산

                worldCamera.rect = new Rect(0, space * 0.5f, 1, 1 - space);
            }
            else
            {
                worldCamera.rect = new Rect(0, 0, 1, 1);
            }
        }
    }
}

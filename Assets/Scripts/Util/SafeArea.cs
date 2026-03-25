using UnityEngine;

namespace Crystal
{
    /// <summary>
    /// Safe area implementation for notched mobile devices. Usage:
    ///  (1) Add this component to the top level of any GUI panel. 
    ///  (2) If the panel uses a full screen background image, then create an immediate child and put the component on that instead, with all other elements childed below it.
    ///      This will allow the background image to stretch to the full extents of the screen behind the notch, which looks nicer.
    ///  (3) For other cases that use a mixture of full horizontal and vertical background stripes, use the Conform X control and useTop/useBottom for Y-axis logic.
    /// </summary>
    public class SafeArea : MonoBehaviour
    {
        #region Simulations

        public enum SimDevice
        {
            None,
            iPhoneX,
            iPhoneXsMax,
            Pixel3XL_LSL,
            Pixel3XL_LSR
        }

        public static SimDevice Sim = SimDevice.None;

        // Normalised safe areas for iPhone X
        Rect[] NSA_iPhoneX = new Rect[]
        {
            new Rect (0f, 102f / 2436f, 1f, 2202f / 2436f),
            new Rect (132f / 2436f, 63f / 1125f, 2172f / 2436f, 1062f / 1125f)
        };

        // Normalised safe areas for iPhone Xs Max
        Rect[] NSA_iPhoneXsMax = new Rect[]
        {
            new Rect (0f, 102f / 2688f, 1f, 2454f / 2688f),
            new Rect (132f / 2688f, 63f / 1242f, 2424f / 2688f, 1179f / 1242f)
        };

        // Pixel 3 XL landscape left
        Rect[] NSA_Pixel3XL_LSL = new Rect[]
        {
            new Rect (0f, 0f, 1f, 2789f / 2960f),
            new Rect (0f, 0f, 2789f / 2960f, 1f)
        };

        // Pixel 3 XL landscape right
        Rect[] NSA_Pixel3XL_LSR = new Rect[]
        {
            new Rect (0f, 0f, 1f, 2789f / 2960f),
            new Rect (171f / 2960f, 0f, 2789f / 2960f, 1f)
        };

        #endregion

        RectTransform Panel;
        Rect LastSafeArea = new Rect(0, 0, 0, 0);
        Vector2Int LastScreenSize = new Vector2Int(0, 0);
        ScreenOrientation LastOrientation = ScreenOrientation.AutoRotation;

        [SerializeField] bool ConformX = true;  // Conform to screen safe area on X-axis
        [SerializeField] bool useTop = true;     // Apply top safe area
        [SerializeField] bool useBottom = true;  // Apply bottom safe area
        [SerializeField] bool Logging = false;   // Enable debug logging

        void Awake()
        {
            Panel = GetComponent<RectTransform>();

            if (Panel == null)
            {
                Debug.LogError("Cannot apply safe area - no RectTransform found on " + name);
                Destroy(gameObject);
            }

            Refresh();
        }

        void Update()
        {
            Refresh();
        }

        /// <summary>
        /// Re-applies the safe area if screen size or orientation has changed.
        /// </summary>
        void Refresh()
        {
            Rect safeArea = GetSafeArea();

            if (safeArea != LastSafeArea
                || Screen.width != LastScreenSize.x
                || Screen.height != LastScreenSize.y
                || Screen.orientation != LastOrientation)
            {
                LastScreenSize.x = Screen.width;
                LastScreenSize.y = Screen.height;
                LastOrientation = Screen.orientation;

                ApplySafeArea(safeArea);
            }
        }

        /// <summary>
        /// Gets the safe area, applying simulation data in Editor if needed.
        /// </summary>
        Rect GetSafeArea()
        {
            Rect safeArea = Screen.safeArea;

            if (Application.isEditor && Sim != SimDevice.None)
            {
                Rect nsa = new Rect(0, 0, Screen.width, Screen.height);

                switch (Sim)
                {
                    case SimDevice.iPhoneX:
                        nsa = (Screen.height > Screen.width) ? NSA_iPhoneX[0] : NSA_iPhoneX[1];
                        break;
                    case SimDevice.iPhoneXsMax:
                        nsa = (Screen.height > Screen.width) ? NSA_iPhoneXsMax[0] : NSA_iPhoneXsMax[1];
                        break;
                    case SimDevice.Pixel3XL_LSL:
                        nsa = (Screen.height > Screen.width) ? NSA_Pixel3XL_LSL[0] : NSA_Pixel3XL_LSL[1];
                        break;
                    case SimDevice.Pixel3XL_LSR:
                        nsa = (Screen.height > Screen.width) ? NSA_Pixel3XL_LSR[0] : NSA_Pixel3XL_LSR[1];
                        break;
                }

                safeArea = new Rect(
                    Screen.width * nsa.x,
                    Screen.height * nsa.y,
                    Screen.width * nsa.width,
                    Screen.height * nsa.height
                );
            }

            return safeArea;
        }

        public Vector4 GetSafeAreaInsets()
        {
            Rect safe = GetSafeArea();

            // 🔽 여기서 픽셀 좌표를 캔버스 로컬로 변환
            Canvas canvas = GetComponentInParent<Canvas>();
            Camera cam = canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            RectTransform canvasRt = canvas.GetComponent<RectTransform>();

            // 전체 화면, SafeArea 사각형을 캔버스 로컬로 변환
            Vector2 canvasBL, canvasTR, safeBL, safeTR;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, new Vector2(0, 0), cam, out canvasBL);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, new Vector2(Screen.width, Screen.height), cam, out canvasTR);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, safe.min, cam, out safeBL);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, safe.max, cam, out safeTR);

            float left = safeBL.x - canvasBL.x;
            float right = canvasTR.x - safeTR.x;
            float bottom = safeBL.y - canvasBL.y;
            float top = canvasTR.y - safeTR.y;

            return new Vector4(left, top, right, bottom);
        }

        /// <summary>
        /// Applies the safe area to the panel's RectTransform anchors.
        /// </summary>
        void ApplySafeArea(Rect r)
        {
            LastSafeArea = r;

            // Ignore X-axis?
            if (!ConformX)
            {
                r.x = 0;
                r.width = Screen.width;
            }

            // Apply top and bottom safe area selectively
            float minY = useBottom ? r.y : 0f;
            float maxY = useTop ? (r.y + r.height) : Screen.height;

            r.y = minY;
            r.height = Mathf.Max(0f, maxY - minY);

            // Apply anchor values if screen state is valid
            if (Screen.width > 0 && Screen.height > 0)
            {
                Vector2 anchorMin = r.position;
                Vector2 anchorMax = r.position + r.size;
                anchorMin.x /= Screen.width;
                anchorMin.y /= Screen.height;
                anchorMax.x /= Screen.width;
                anchorMax.y /= Screen.height;

                if (anchorMin.x >= 0 && anchorMin.y >= 0 && anchorMax.x >= 0 && anchorMax.y >= 0)
                {
                    Panel.anchorMin = anchorMin;
                    Panel.anchorMax = anchorMax;
                }
            }

            if (Logging)
            {
                Debug.LogFormat("SafeArea applied to {0}: x={1}, y={2}, w={3}, h={4} | Screen: {5}x{6}",
                    name, r.x, r.y, r.width, r.height, Screen.width, Screen.height);
            }
        }
    }
}

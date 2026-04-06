using Crystal;
using UnityEngine;
using UnityEngine.UI;

// 하단 패딩(스크린 픽셀) — 예: 전체 세로 1000, Safe bottom=50px, 배너=100px
// · 광고 없음 → Safe bottom 만 (50)
// · 광고 있음 → Safe bottom + 배너 높이 (50+100=150). 앵커 적응형 배너는 보통 홈 인디케이터 등 Safe 위에 그려져 UGUI는 둘 다 비워야 함.
// 배너 너비·SDK 쪽 Safe 반영: BannerAdWrapper 의 GetDeviceSafeWidth + 적응형 AdSize (Google 문서).

/// <summary>
/// Safe Area bottom(픽셀)과 배너 높이(픽셀)를 합쳐 이 RectTransform 의 offsetMin.y 를 맞춥니다.
/// 상위에 <see cref="SafeArea"/> 가 있으면 Safe 는 이미 적용된 영역이므로 배너 높이만 반영합니다.
/// 다른 Canvas에 동일 인셋을 코드로 적용하려면 <see cref="BannerUiLayout"/> 을 사용하세요.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[DefaultExecutionOrder(32000)]
public class BannerCanvasBottomInset : MonoBehaviour
{
    [Tooltip("에디터에서만: 0 이상이면 실제 배너 로드 없이 하단 인셋(px)을 이 값으로 시뮬레이션합니다.")]
    [SerializeField] private int editorSimulatedBannerInsetPx = -1;

    private RectTransform Rect => (RectTransform)transform;
    private Canvas _rootCanvas;
    private float _baseOffsetMinY;
    private Rect _lastSafeArea;

    private void Awake()
    {
        var r = Rect;
        _baseOffsetMinY = r.offsetMin.y;
        _rootCanvas = r.GetComponentInParent<Canvas>()?.rootCanvas;
        _lastSafeArea = Screen.safeArea;

#if UNITY_EDITOR
        var parent = transform.parent;
        if (parent != null && parent.GetComponent<LayoutGroup>() != null)
            Debug.LogWarning(
                "[BannerCanvasBottomInset] 부모에 LayoutGroup이 있으면 자식 RectTransform의 offsetMin이 레이아웃 단계에서 덮어씌워질 수 있습니다. 레이아웃 밖에 두거나 ignoreLayout 등을 검토하세요.",
                this);
#endif
    }

    private void OnEnable()
    {
        GoogleAdManager.BannerBottomInsetPixelsChanged += OnBannerInset;
        RefreshLayout();
        StartCoroutine(RefreshLayoutEndOfFrameOnce());
    }

    private void OnDisable()
    {
        GoogleAdManager.BannerBottomInsetPixelsChanged -= OnBannerInset;
        RefreshLayout();
    }

    private System.Collections.IEnumerator RefreshLayoutEndOfFrameOnce()
    {
        yield return new WaitForEndOfFrame();
        RefreshLayout();
    }

    private void LateUpdate()
    {
        if (Screen.safeArea == _lastSafeArea)
            return;
        _lastSafeArea = Screen.safeArea;
        RefreshLayout();
    }

    private void OnBannerInset(float _) => RefreshLayout();

    private float ComputeBottomPaddingScreenPixels()
    {
        if (_rootCanvas == null)
            return 0f;

        if (enabled)
        {
            float px = BannerUiLayout.GetBottomContentInsetScreenPixels(_rootCanvas, Rect);
#if UNITY_EDITOR
            if (editorSimulatedBannerInsetPx >= 0)
            {
                float banner = GoogleAdManager.Instance != null ? GoogleAdManager.Instance.GetBannerInsetScreenPixelsForLayout() : 0f;
                float simBanner = Mathf.Max(banner, editorSimulatedBannerInsetPx);
                bool skipSafe = BannerUiLayout.IsUnderCrystalSafeAreaPanel(Rect);
                float safeBottom = skipSafe ? 0f : SafeAreaUtil.GetInsets(_rootCanvas, SafeAreaUtil.Unit.Pixels).w;
                px = safeBottom + simBanner;
            }
#endif
            return px;
        }

        if (BannerUiLayout.IsUnderCrystalSafeAreaPanel(Rect))
            return 0f;
        return SafeAreaUtil.GetInsets(_rootCanvas, SafeAreaUtil.Unit.Pixels).w;
    }

    private void RefreshLayout()
    {
        var r = Rect;
        float totalScreenPx = ComputeBottomPaddingScreenPixels();
        float scale = _rootCanvas != null && _rootCanvas.scaleFactor > 0f ? _rootCanvas.scaleFactor : 1f;
        float canvasUnits = totalScreenPx / scale;
        r.offsetMin = new Vector2(r.offsetMin.x, _baseOffsetMinY + canvasUnits);
        LayoutRebuilder.MarkLayoutForRebuild(r);
    }
}

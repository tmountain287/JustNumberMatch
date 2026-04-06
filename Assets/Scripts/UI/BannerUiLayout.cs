using Crystal;
using UnityEngine;

/// <summary>
/// Safe Area 하단 + (표시 중인) 배너 높이를 합친 하단 인셋을 조회합니다.
/// 임의 Canvas의 레이아웃(anchor, offsetMin 등)에 동일한 규칙을 적용할 때 사용합니다.
/// </summary>
public static class BannerUiLayout
{
    /// <summary>
    /// 상위에 <see cref="SafeArea"/> 패널이 있으면 이미 화면 Safe로 줄어든 영역 안이므로,
    /// 스크린 기준 Safe bottom 을 다시 더하지 않습니다(이중 패딩 방지). 배너 높이만 추가합니다.
    /// </summary>
    public static bool IsUnderCrystalSafeAreaPanel(RectTransform rt) =>
        rt != null && rt.GetComponentInParent<SafeArea>() != null;

    /// <summary>
    /// 하단에서 콘텐츠가 비워야 할 총 스크린 픽셀.
    /// 배너가 안 보이면 Safe bottom 만, 보이면 Safe bottom + 배너 높이.
    /// </summary>
    public static float GetBottomContentInsetScreenPixels(Canvas canvas) =>
        GetBottomContentInsetScreenPixels(canvas, null);

    /// <inheritdoc cref="GetBottomContentInsetScreenPixels(Canvas)"/>
    public static float GetBottomContentInsetScreenPixels(Canvas canvas, RectTransform forLayoutTarget)
    {
        bool skipScreenSafeBottom = IsUnderCrystalSafeAreaPanel(forLayoutTarget);
        float safeBottom = 0f;
        if (!skipScreenSafeBottom && canvas != null)
            safeBottom = SafeAreaUtil.GetInsets(canvas, SafeAreaUtil.Unit.Pixels).w;

        float bannerPx = 0f;
        if (GoogleAdManager.Instance != null)
            bannerPx = GoogleAdManager.Instance.GetBannerInsetScreenPixelsForLayout();

        return safeBottom + bannerPx;
    }

    /// <summary>Canvas 로컬 유닛 (RectTransform 레이아웃용).</summary>
    public static float GetBottomContentInsetCanvasUnits(Canvas canvas) =>
        GetBottomContentInsetCanvasUnits(canvas, null);

    /// <inheritdoc cref="GetBottomContentInsetCanvasUnits(Canvas)"/>
    public static float GetBottomContentInsetCanvasUnits(Canvas canvas, RectTransform forLayoutTarget)
    {
        if (canvas == null) return 0f;
        float px = GetBottomContentInsetScreenPixels(canvas, forLayoutTarget);
        float sf = canvas.scaleFactor > 0f ? canvas.scaleFactor : 1f;
        return px / sf;
    }
}

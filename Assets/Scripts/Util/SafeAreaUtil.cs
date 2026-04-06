using UnityEngine;

/// <summary>
/// 캔버스 기준 Safe Area 여백(left, top, right, bottom)을 픽셀/로컬/정규화 단위로 구하는 유틸.
/// </summary>
public static class SafeAreaUtil
{
    public enum Unit { Pixels, Local, Normalized }

    /// <summary>
    /// 캔버스 기준 safe area 인셋을 반환합니다.
    /// return: Vector4(left, top, right, bottom)
    /// </summary>
    public static Vector4 GetInsets(Canvas canvas, Unit unit = Unit.Pixels, Rect? overrideSafeAreaPx = null)
    {
        if (canvas == null) return Vector4.zero;

        var canvasRt = canvas.GetComponent<RectTransform>();
        if (!canvasRt) return Vector4.zero;

        // 1) 안전영역(픽셀 좌표)
        Rect safePx = overrideSafeAreaPx ?? Screen.safeArea;

        // 2) 캔버스 전체(픽셀 좌표) — Overlay면 Screen 크기, 그 외는 PixelAdjustRect로 얻기
        Rect canvasPx = GetCanvasPixelRect(canvas, canvasRt);

        // 3) 픽셀 직사각형 → 캔버스 로컬 좌표로 변환
        Rect localCanvas = GetLocalRect(canvasRt);
        Rect localSafe = PixelRectToLocalRect(safePx, canvas, canvasRt);

        // 4) 로컬 단위 인셋 계산
        float leftLocal = localSafe.xMin - localCanvas.xMin;
        float rightLocal = localCanvas.xMax - localSafe.xMax;
        float bottomLocal = localSafe.yMin - localCanvas.yMin;
        float topLocal = localCanvas.yMax - localSafe.yMax;

        // 요청 단위로 반환
        switch (unit)
        {
            case Unit.Local:
                return new Vector4(leftLocal, topLocal, rightLocal, bottomLocal);

            case Unit.Normalized:
                float w = localCanvas.width;
                float h = localCanvas.height;
                return new Vector4(
                    w > 0 ? leftLocal / w : 0f,
                    h > 0 ? topLocal / h : 0f,
                    w > 0 ? rightLocal / w : 0f,
                    h > 0 ? bottomLocal / h : 0f
                );

            case Unit.Pixels:
            default:
                // 로컬→픽셀 변환: Canvas.scaleFactor 사용
                float sf = canvas.scaleFactor == 0 ? 1f : canvas.scaleFactor;
                return new Vector4(leftLocal * sf, topLocal * sf, rightLocal * sf, bottomLocal * sf);
        }
    }

    // ---------- helpers ----------

    // 캔버스의 픽셀 직사각형을 얻는다.
    private static Rect GetCanvasPixelRect(Canvas canvas, RectTransform canvasRt)
    {
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return new Rect(0, 0, Screen.width, Screen.height);
        }
        else
        {
            // Camera/World Space: PixelAdjustRect로 안정적으로 픽셀 경계 구함
            return RectTransformUtility.PixelAdjustRect(canvasRt, canvas);
        }
    }

    // RectTransform의 로컬 좌표계 Rect (pivot 고려)
    private static Rect GetLocalRect(RectTransform rt)
    {
        var r = rt.rect;
        // Unity의 rect는 이미 로컬 좌표(피벗 중심) 기준이므로 그대로 사용
        return r;
    }

    // 픽셀 직사각형을 캔버스 로컬 좌표의 직사각형으로 변환
    private static Rect PixelRectToLocalRect(Rect px, Canvas canvas, RectTransform canvasRt)
    {
        Camera cam = null;
        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvas.worldCamera;

        Vector2 bl, tl, tr, br;
        Rect pxc = px;

        ScreenPointToLocal(canvasRt, new Vector2(pxc.xMin, pxc.yMin), cam, out bl); // bottom-left
        ScreenPointToLocal(canvasRt, new Vector2(pxc.xMin, pxc.yMax), cam, out tl); // top-left
        ScreenPointToLocal(canvasRt, new Vector2(pxc.xMax, pxc.yMax), cam, out tr); // top-right
        ScreenPointToLocal(canvasRt, new Vector2(pxc.xMax, pxc.yMin), cam, out br); // bottom-right

        float xMin = Mathf.Min(bl.x, tl.x, tr.x, br.x);
        float xMax = Mathf.Max(bl.x, tl.x, tr.x, br.x);
        float yMin = Mathf.Min(bl.y, tl.y, tr.y, br.y);
        float yMax = Mathf.Max(bl.y, tl.y, tr.y, br.y);

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    private static void ScreenPointToLocal(RectTransform rt, Vector2 screen, Camera cam, out Vector2 local)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screen, cam, out local);
    }
}

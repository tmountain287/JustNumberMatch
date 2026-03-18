using UnityEngine;
using UnityEngine.UI;

public static class RectTransformHelper
{
    /// <summary>
    /// Match UI Element (position / size / optional anchor+pivot / optional worldPositionDirectApply)
    /// </summary>
    public static void MatchUIElementAndSizeAdvanced(RectTransform source, RectTransform target, bool syncAnchorAndPivot = true, bool useWorldPositionDirectly = true)
    {
        Canvas canvas = source.GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("Source must be under a Canvas.");
            return;
        }

        // 1️⃣ Anchor/Pivot 동기화 (옵션)
        if (syncAnchorAndPivot)
        {
            target.pivot = source.pivot;
            target.anchorMin = source.anchorMin;
            target.anchorMax = source.anchorMax;
        }

        // 2️⃣ Source Pivot 기준 정확한 WorldPos 구하기
        Vector3[] sourceCorners = new Vector3[4];
        source.GetWorldCorners(sourceCorners);

        Vector2 pivot = source.pivot;

        Vector3 bottomLeft = sourceCorners[0];
        Vector3 topRight = sourceCorners[2];

        Vector3 sourcePivotWorldPos = new Vector3(
            Mathf.Lerp(bottomLeft.x, topRight.x, pivot.x),
            Mathf.Lerp(bottomLeft.y, topRight.y, pivot.y),
            bottomLeft.z
        );

        // 3️⃣ 적용 방법 선택 (Position)
        if (useWorldPositionDirectly)
        {
            // WorldPosition 직접 적용 → 부모 anchor 영향 무시
            target.position = sourcePivotWorldPos;
        }
        else
        {
            // 기존 방식 (LocalPosition 으로 변환 → Anchor 영향 받을 수 있음)
            RectTransform targetParentRect = target.parent as RectTransform;

            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                sourcePivotWorldPos
            );

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetParentRect,
                screenPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out localPoint
            );

            target.localPosition = localPoint;
        }

        // 4️⃣ SizeDelta 복사 (width / height 동일하게 맞추기)
        Vector2 sourceSize = source.rect.size;
        target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sourceSize.x);
        target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sourceSize.y);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionUtil
{
    public static Rect GetScreenRect(Camera uiCamera, RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        Vector2 s0 = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[0]);
        Vector2 s2 = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[2]);

        float xMin = Mathf.Min(s0.x, s2.x);
        float yMin = Mathf.Min(s0.y, s2.y);
        float width = Mathf.Abs(s2.x - s0.x);
        float height = Mathf.Abs(s2.y - s0.y);
        return new Rect(xMin, yMin, width, height);
    }

    // 스크린 Rect 교집합 면적
    public static float GetScreenOverlapArea(Rect a, Rect b)
    {
        float xMin = Mathf.Max(a.xMin, b.xMin);
        float xMax = Mathf.Min(a.xMax, b.xMax);
        float yMin = Mathf.Max(a.yMin, b.yMin);
        float yMax = Mathf.Min(a.yMax, b.yMax);

        float w = xMax - xMin;
        float h = yMax - yMin;
        if (w <= 0f || h <= 0f) return 0f;
        return w * h;
    }
}

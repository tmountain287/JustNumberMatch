using UnityEngine;

public static class ColorUtil
{
    /// <summary>
    /// "#RRGGBB" 또는 "#RRGGBBAA" 형식의 hex 문자열을 Color로 변환합니다.
    /// 실패 시 fallbackColor 반환
    /// </summary>
    public static Color FromHex(string hex, Color fallbackColor = default)
    {
        if (ColorUtility.TryParseHtmlString(hex, out var color))
        {
            return color;
        }

        Debug.LogWarning($"[ColorUtil] 유효하지 않은 HEX 문자열: {hex}");
        return fallbackColor == default ? Color.white : fallbackColor;
    }
}

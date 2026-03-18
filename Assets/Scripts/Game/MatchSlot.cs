using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchSlot : MonoBehaviour
{
    [SerializeField] private Image img = null;

    // ▼ 하이라이트 관련
    [Header("Slot Highlight")]
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0f, 0.35f);
    [SerializeField] private Color normalColor = Color.white;

    public void SetHighlight()
    {
        img.color = highlightColor;
    }

    public void SetNormal()
    {
        img.color = normalColor;
    }

    private void SetSlotTint(GameObject slot, Color color)
    {        
        if (img != null) img.color = color;
        // 필요하면 Outline/Shadow 등 다른 효과도 여기서 토글 가능
    }
}

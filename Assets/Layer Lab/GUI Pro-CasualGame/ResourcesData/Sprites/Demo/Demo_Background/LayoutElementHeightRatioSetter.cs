using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutElement))]
public class LayoutElementHeightRatioSetter : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float heightRatio = 0.5f; // 부모 기준 비율 (0 ~ 1)

    [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private RectTransform parentRectTransform;

    void Awake()
    {
        layoutElement = GetComponent<LayoutElement>();
        parentRectTransform = transform.parent as RectTransform;

        UpdateHeight();
    }

    void OnEnable()
    {
        UpdateHeight();
    }

    protected void OnRectTransformDimensionsChange()
    {
        UpdateHeight();
    }

    private void UpdateHeight()
    {
        if (parentRectTransform == null)
            parentRectTransform = transform.parent as RectTransform;

        if (parentRectTransform == null)
            return;

        float parentHeight = parentRectTransform.rect.height;
        layoutElement.preferredHeight = parentHeight * heightRatio;
    }

#if UNITY_EDITOR
    private float lastHeightRatio = -1f; // OnValidate에서 이전값과 비교용
#endif

    void OnValidate()
    {
        if (layoutElement == null)
            layoutElement = GetComponent<LayoutElement>();

        if (parentRectTransform == null)
            parentRectTransform = transform.parent.GetComponent<RectTransform>();

#if UNITY_EDITOR
        // 값이 바뀌었을 때만 갱신 (디버그시 깜빡임 방지)
        if (!Mathf.Approximately(lastHeightRatio, heightRatio))
        {
            lastHeightRatio = heightRatio;
            UpdateHeight();
        }
#endif
    }
}

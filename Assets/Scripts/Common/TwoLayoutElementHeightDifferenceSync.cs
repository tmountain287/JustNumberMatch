using Common.Manager;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 부모 높이를 기준으로 자식 A, B 의 높이를 자동 계산
/// A + B == 부모 높이 유지
/// A 와 B 의 높이 차이 == heightDifference 유지 (양수/음수 모두 지원)
/// </summary>
public class TwoLayoutElementHeightDifferenceSync : MonoBehaviour
{
    [Header("부모 RectTransform")]
    [SerializeField] private RectTransform parentRectTransform;

    [Header("자식 A")]
    [SerializeField] private RectTransform childA;
    [SerializeField] private LayoutElement layoutElementA;

    [Header("자식 B")]
    [SerializeField] private RectTransform childB;
    [SerializeField] private LayoutElement layoutElementB;

    [Header("높이 차이 값 (양수/음수 모두 가능)")]
    [SerializeField] private float heightDifference = 100f;
    
    void Awake()
    {
        UpdateHeights();
    }

    private void OnEnable()
    {
        ResolutionManager.Instance.OnChangeResolution.AddListener(UpdateHeights);
    }

    private void OnDisable()
    {
        if (ResolutionManager.Instance != null)
            ResolutionManager.Instance.OnChangeResolution.RemoveListener(UpdateHeights);
    }

    private void UpdateHeights()
    {
        if (parentRectTransform == null)
            return;

        layoutElementA.preferredHeight = (parentRectTransform.rect.height + heightDifference) * 0.5f;
        layoutElementB.preferredHeight = (parentRectTransform.rect.height - heightDifference) * 0.5f;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        UpdateHeights();
    }
#endif
}

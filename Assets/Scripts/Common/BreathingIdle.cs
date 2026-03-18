using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class BreathingIdle : MonoBehaviour
{
    [SerializeField] private float scaleAmount = 0.005f; // 숨쉬기 크기 (2% 정도)
    [SerializeField] private float duration = 1.5f;      // 들숨+날숨 시간
    [SerializeField] private float delay = 0f;
    private Tween breathingTween;

    void OnEnable()
    {
        Vector3 originalScale = transform.localScale;

        Vector3 scaleDirection = new Vector3(
            Mathf.Sign(originalScale.x),
            Mathf.Sign(originalScale.y),
            Mathf.Sign(originalScale.z)
        );

        Vector3 scaleMagnitude = new Vector3(
            Mathf.Abs(originalScale.x),
            Mathf.Abs(originalScale.y),
            Mathf.Abs(originalScale.z)
        );

        Vector3 targetMagnitude = scaleMagnitude * (1f + scaleAmount);
        Vector3 targetScale = new Vector3(
            targetMagnitude.x * scaleDirection.x,
            targetMagnitude.y * scaleDirection.y,
            targetMagnitude.z * scaleDirection.z
        );

        breathingTween = transform
            .DOScale(targetScale, duration / 2f)
            .SetDelay(delay)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDisable()
    {
        breathingTween?.Kill();
    }
}

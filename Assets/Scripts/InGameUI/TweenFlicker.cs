using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TweenFlicker : MonoBehaviour
{
    [SerializeField] private Image image = null;
    [SerializeField] private float duration = 0.5f;

    private Tween flickerTween;

    private void OnValidate()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }
    }

    void OnEnable()
    {
        // 0 ↔ 1 무한 반복 깜빡임
        Color color = image.color;
        color.a = 0f;
        image.color = color;

        flickerTween = image.DOFade(1f, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .From(0f); // 알파 1 → 0 으로 시작, Yoyo에 맞게 처음부터 부드럽게
    }

    void OnDisable()
    {
        if (flickerTween != null && flickerTween.IsActive())
        {
            flickerTween.Kill();
        }
        Color color = image.color;
        color.a = 0f;
        image.color = color;
    }
}

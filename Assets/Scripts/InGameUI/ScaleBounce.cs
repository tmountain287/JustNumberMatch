using DG.Tweening;
using UnityEngine;

public class ScaleBounce : MonoBehaviour
{
    [SerializeField] private float scale = 1.5f;
    [SerializeField] private float duration = 0.01f;
    private Tween scaleTween;

    private void OnDisable()
    {
        transform.localScale = Vector3.one;
    }

    private void OnEnable()
    {
        transform.DOKill();

        scaleTween = transform.DOScale(scale, duration)
       .SetEase(Ease.OutQuad)
       .OnComplete(() =>
       {
           scaleTween = transform.DOScale(1f, duration)
               .SetEase(Ease.InQuad);
       });
    }
}
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ImageSizeAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform parentRect;
    [SerializeField] private RectTransform targetRect;
    [SerializeField] private Mask mask = null;
    [SerializeField] private RectTransform childRect;

    [SerializeField] private float duration = 0.5f;

    private void Update()
    {
        childRect.sizeDelta = new Vector2(parentRect.rect.width, parentRect.rect.height);
    }

    private void OnEnable()
    {
        StartCoroutine(GrowImage());
    }

    private void OnDisable()
    {
        mask.enabled = true;
    }

    IEnumerator GrowImage()
    {
        mask.enabled = true;
        targetRect.sizeDelta = Vector2.zero;
        float time = 0f;
        Vector2 startSize = Vector2.zero;
        while (time < duration)
        {
            Vector2 endSize = new Vector2(parentRect.rect.width, parentRect.rect.height);
            float t = time / duration;
            t = 1f - Mathf.Pow(1f - t, 3);
            targetRect.sizeDelta = Vector2.Lerp(startSize, endSize, t);
            time += Time.deltaTime;
            yield return null;
        }
        mask.enabled = false;
        targetRect.sizeDelta = new Vector2(parentRect.rect.width, parentRect.rect.height);
        
    }
}

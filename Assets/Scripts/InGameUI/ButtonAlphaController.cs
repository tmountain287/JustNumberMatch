using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonAlphaController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private CanvasGroup backgroundGroup;  // 배경용 CanvasGroup
    [SerializeField] private CanvasGroup buttonGroup;      // 버튼 자체의 CanvasGroup
    [SerializeField] private float btnTarget = 0.1f;

    private float fadeDuration = 0.1f;    

    private Coroutine fadeCoroutine;

    private void OnDisable()
    {
        backgroundGroup.alpha = 1;
        buttonGroup.alpha = 1;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCanvasGroups(backgroundGroup, 0f, buttonGroup));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        // 즉시 복원
        backgroundGroup.alpha = 1f;
        buttonGroup.alpha = 1f;
    }

    private IEnumerator FadeCanvasGroups(CanvasGroup bg, float bgTarget, CanvasGroup btn)
    {
        float bgStart = bg.alpha;
        float btnStart = btn.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            bg.alpha = Mathf.Lerp(bgStart, bgTarget, t);
            btn.alpha = Mathf.Lerp(btnStart, btnTarget, t);

            yield return null;
        }

        bg.alpha = bgTarget;
        btn.alpha = btnTarget;
    }
}
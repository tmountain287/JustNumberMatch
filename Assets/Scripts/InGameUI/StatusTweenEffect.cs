using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StatusTweenEffect : MonoBehaviour
{
    public RectTransform a;
    public RectTransform b;
    public CanvasGroup bCanvasGroup;

    private Sequence sequence;

    public void PlayEffect()
    {
        // 기존 시퀀스가 살아있다면 먼저 Kill
        if (sequence != null && sequence.IsActive()) sequence.Kill(true);

        // 초기 상태 설정
        a.localScale = Vector3.one * 3f;
        b.localScale = Vector3.one;
        bCanvasGroup.alpha = 1f;

        // 새 시퀀스 생성
        sequence = DOTween.Sequence();

        // a: 팟하고 스케일 줄이기
        sequence.Append(a.DOScale(1f, 0.2f).SetEase(Ease.OutBack));

        // b 활성화 + 효과 시작 (a 스케일 축소 완료 시점에 맞춰)
        sequence.AppendCallback(() =>
        {
            b.gameObject.SetActive(true);
        });


        // b: 커지면서 페이드 아웃
        sequence.Join(b.DOScale(1.5f, 0.2f).SetEase(Ease.OutCubic));
        sequence.Join(bCanvasGroup.DOFade(0f, 0.2f));

        // 완료 후 시퀀스 해제
        sequence.OnComplete(() =>
        {
            sequence.Kill(true); // 모든 트윈과 콜백 정리
            sequence = null;
            b.gameObject.SetActive(false);
        });

        sequence.Play();
    }

    private void OnEnable()
    {
        PlayEffect();
    }

    private void OnDisable()
    {
        // 컴포넌트 제거 시 안전하게 정리
        if (sequence != null && sequence.IsActive()) sequence.Kill(true);

        a.localScale = Vector3.one * 3f;
        b.localScale = Vector3.one;
        bCanvasGroup.alpha = 1f;

    }
}

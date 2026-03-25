using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class TweenEffect : MonoBehaviour
    {
        [SerializeField] private Image image = null;
        [SerializeField] private Image lightImage = null;

        private void OnEnable()
        {
            PlayEffect();
        }

        public void PlayEffect()
        {
            Color lightColor = lightImage.color;
            lightColor.a = 0;
            lightImage.color = lightColor;

            Color imageColor = image.color;
            imageColor.a = 1;
            image.color = imageColor;
            
            // DOTween 시퀀스 생성
            Sequence sequence = DOTween.Sequence();

            // Step 1: image 스케일 3 -> 1 (0.5초), lightImage는 보이지 않음
            sequence.Append(image.rectTransform.DOScale(Vector3.one, 0.2f)
                .From(Vector3.one * 3) // 시작 스케일 3
                .SetEase(Ease.InOutQuad));
            sequence.AppendCallback(() =>
            {
                lightImage.color = new Color(lightImage.color.r, lightImage.color.g, lightImage.color.b, 1);
            });
            sequence.Append(lightImage.DOFade(0, 0.2f).SetEase(Ease.InOutQuad));
            sequence.Join(lightImage.rectTransform.DOScale(Vector3.one * 1.5f, 0.2f).From(Vector3.one).SetEase(Ease.InOutQuad));
             // 알파 감소

            // Step 3: image와 lightImage의 알파를 동시에 0으로 (0.5초)
            sequence.Append(image.DOFade(0, 0.5f).SetEase(Ease.InOutQuad));
            sequence.Join(lightImage.DOFade(0, 0.5f).SetEase(Ease.InOutQuad));

            // 완료 후 게임 오브젝트 비활성화
            sequence.OnComplete(() =>
            {
                Debug.Log("Animation Complete");
                gameObject.SetActive(false);
            });

            // 시퀀스 재생
            sequence.SetAutoKill(true);
            sequence.Play();
        }
    }
}
using Common.Manager;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageTextUI : MonoBehaviour
{
    [SerializeField] private RectTransform rect = null;
    [SerializeField] private CanvasGroup canvasGroup = null;

    [SerializeField] private CanvasGroup leftCanvasGroup = null;
    [SerializeField] private CanvasGroup rightCanvasGroup = null;

    [SerializeField] private RectTransform leftRect = null;
    [SerializeField] private RectTransform rightRect = null;

    [SerializeField] private List<Text> textList = null;

    [SerializeField] private AudioClip alramAudioClip = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ShowStage(int _stageNumber, StageType _stageType, Action _onComplete = null)
    {
        textList.ForEach(t => t.text = string.Format(LocalizationManager.Instance.GetText(_stageType == StageType.Normal ? "Stage" : "GateStage"), _stageNumber));

        gameObject.SetActive(true);

        canvasGroup.alpha = 1;
        // 초기 상태
        leftCanvasGroup.alpha = 0;
        rightCanvasGroup.alpha = 0;

        leftRect.anchoredPosition = new Vector2(-950, 0);
        rightRect.anchoredPosition = new Vector2(950, 0);

        Sequence seq = DOTween.Sequence();

        // 등장
        seq.Append(leftCanvasGroup.DOFade(1, 0.4f));
        seq.Join(leftRect.DOAnchorPosX(0, 0.4f).SetEase(Ease.OutCubic));

        seq.Join(rightCanvasGroup.DOFade(1, 0.4f));
        seq.Join(rightRect.DOAnchorPosX(0, 0.4f).SetEase(Ease.OutCubic).OnComplete(()=>
        {
            SoundManager.Instance.PlayFX(alramAudioClip);
            _onComplete?.Invoke();
        }));

        // 튕김 효과
        seq.Append(canvasGroup.transform.DOScale(1.1f, 0.15f).SetEase(Ease.OutBack));
        seq.Append(canvasGroup.transform.DOScale(1.0f, 0.1f));

        // 1초 대기
        seq.AppendInterval(1.0f);

        // 사라짐
        seq.Append(canvasGroup.DOFade(0, 0.2f));
        seq.Join(rect.DOAnchorPosY(40, 0.2f).SetEase(Ease.InCubic));

        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
            //_onComplete?.Invoke();
        });
    }
}

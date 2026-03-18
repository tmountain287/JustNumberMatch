using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlarmMessageBox : MonoBehaviour
{
    [SerializeField] private Text message = null;

    private Sequence sequence = null;
    private Action onComplete = null;

    public void OnAlarm(string _message, Action _onComplete)
    {
        if (sequence != null)
        {
            sequence.Kill();
            sequence = null;
        }

        onComplete = _onComplete;

        message.text = _message;

        sequence = DOTween.Sequence();
        sequence.Join(transform.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutQuad));
        sequence.AppendInterval(2.0f);
        sequence.OnComplete(() => onComplete?.Invoke());
    }

    public void OnNextAlarm()
    {
        if (sequence != null)
        {
            sequence.Kill();
            sequence = null;
        }

        sequence = DOTween.Sequence();
        sequence.Join(transform.DOLocalMoveY(-150, 0.5f).SetEase(Ease.OutQuad));
        sequence.AppendInterval(1f);
        sequence.OnComplete(() => onComplete?.Invoke());
    }

    public void OnComplete()
    {
        onComplete?.Invoke();
    }
}
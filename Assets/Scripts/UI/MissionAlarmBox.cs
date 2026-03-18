using DG.Tweening;
using System;
using UnityEngine;

public class MissionAlarmBox : MonoBehaviour
{
    [SerializeField] private Transform iconTran = null;
    [SerializeField] private LocalChangeTextEvent title = null;

    private Sequence sequence = null;
    private Action onComplete = null;

    public void OnAlarm(MissionData _data, Action _onComplete)
    {
        if (sequence != null)
        {
            sequence.Kill();
            sequence = null;
        }

        onComplete = _onComplete;

        Transform tran = null;

        for (int i = 0; i < iconTran.childCount; i++)
        {
            if ((int)_data.type == i)
            {
                tran = iconTran.GetChild(i);
                tran.gameObject.SetActive(true);
            }
            else
            {
                iconTran.GetChild(i).gameObject.SetActive(false);
            }
        }

        if (_data.difficultyType > -1)
        {
            for (int j = 0; j < tran.childCount; j++)
            {
                tran.GetChild(j).gameObject.SetActive(j == _data.difficultyType);
            }
        }

        title.EntryKey = _data.titleLocalId;

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

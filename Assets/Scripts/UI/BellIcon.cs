using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BellIcon : MonoBehaviour
{
    [SerializeField] private Transform pivot = null;
    [SerializeField] private float duration = 0.2f;

    private Sequence sequence = null;

    private void OnDisable()
    {
        pivot.localRotation = Quaternion.identity;
        // 기존 sequence 있으면 Kill
        if (sequence != null)
        {
            sequence.Kill();
            sequence = null;
        }
    }

    private void OnEnable()
    {
        pivot.localRotation = Quaternion.identity;

        if (sequence != null)
        {
            sequence.Kill();
            sequence = null;
        }

        sequence = DOTween.Sequence();

        // 첫 시작: -30도 까지 이동
        sequence.Append(
            pivot.DOLocalRotate(new Vector3(0f, 0f, -20f), duration)
                .SetEase(Ease.InOutSine)
        );

        // Yoyo 반복: -30 ↔ 30 왕복 3번 (총 6회 Yoyo)
        sequence.Append(
            pivot.DOLocalRotate(new Vector3(0f, 0f, 20f), duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(5, LoopType.Yoyo) // 5번만 → 첫 Append 때문에 총 6단계 완성됨
        );

        // 마지막에 0도 복귀
        sequence.Append(
            pivot.DOLocalRotate(Vector3.zero, duration)
                .SetEase(Ease.InOutSine)
        );

        sequence.SetAutoKill(true)
            .OnKill(() => sequence = null);

        sequence.Play();

    }
}

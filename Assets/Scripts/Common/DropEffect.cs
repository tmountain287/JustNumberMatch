using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class DropEffect : MonoBehaviour
{
    [Header("필요한 설정")]
    [SerializeField] private ObjectPool objectPool;
    [SerializeField] private Transform spawnCenter;
    [SerializeField] private Transform spawnParent;

    [Header("퍼짐 설정")]
    [SerializeField] private float spreadRadius = 0.5f;
    [SerializeField] private float jumpDuration = 0.2f;

    [Header("이동 설정")]
    [SerializeField] private float baseMoveDuration = 0.30f;
    [SerializeField] private float moveDurationRandomRange = 0.02f;

    public void Start()
    {
        objectPool.CreateObjectPool();
    }

    public void SpawnGold(int _amount, Transform _targetPoint,
        Action _firsArrivedAction = null, Action _arrivedAction = null)
    {
        Vector3 centerPos = spawnCenter.position;
        bool firstArrivedCalled = false;

        for (int i = 0; i < _amount; i++)
        {
            int index = i;
            GameObject gold = objectPool.GetObjectFromPool();
            Transform goldTransform = gold.transform;
            goldTransform.DOKill();
            goldTransform.SetParent(spawnParent, true);
            goldTransform.position = centerPos;

            // === 1. 퍼지는 위치 계산 ===
            Vector3 spreadPos;
            if (_amount == 2)
            {
                float x = spreadRadius;
                float y = UnityEngine.Random.Range(-spreadRadius * 0.3f, spreadRadius * 0.3f);
                float side = (index == 0) ? -1f : 1f;
                spreadPos = centerPos + new Vector3(side * x, y, 0f);
            }
            else
            {
                Vector2 r = UnityEngine.Random.insideUnitCircle * spreadRadius;
                spreadPos = centerPos + new Vector3(r.x, r.y, 0f);
            }

            // === 2. 팟 퍼짐 ===
            goldTransform.DOMove(spreadPos, jumpDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    Vector3 startPos = goldTransform.position;
                    Vector3 targetPos = _targetPoint.position;

                    Vector3 dir = (targetPos - centerPos).normalized;
                    if (dir.sqrMagnitude < 0.001f) dir = Vector3.right;

                    Vector3 perp = Vector3.Cross(dir, Vector3.forward).normalized;

                    float side = Mathf.Sign(startPos.x - centerPos.x);
                    if (Mathf.Approximately(side, 0f))
                        side = UnityEngine.Random.value < 0.5f ? -1 : 1;

                    Vector3 curveDir = (side < 0) ? -perp : perp;

                    // === 곡선 강도 살짝 증가 버전 ===
                    float t = 0.5f + UnityEngine.Random.Range(-0.05f, 0.05f);

                    float curveStrength = UnityEngine.Random.Range(0.35f, 0.55f);  // ★ 곡선 더 강하게
                    float upAmount = UnityEngine.Random.Range(0.30f, 0.50f);  // ★ 위로 더 강조

                    Vector3 midPos =
                        Vector3.Lerp(startPos, targetPos, t) +
                        curveDir * curveStrength +
                        new Vector3(0, upAmount, 0);

                    Vector3[] path = new[] { startPos, midPos, targetPos };

                    // === 출발 딜레이(멈춤 시간) 조금 증가 ===
                    float startDelay = 0.015f * index;  // ★ 기존 0.01 → 0.015로 증가

                    float durationRandom = UnityEngine.Random.Range(-moveDurationRandomRange, moveDurationRandomRange);

                    float moveDuration = Mathf.Clamp(
                        baseMoveDuration + durationRandom,
                        baseMoveDuration - moveDurationRandomRange * 2f,
                        baseMoveDuration + moveDurationRandomRange * 2f
                    );

                    Sequence seq = DOTween.Sequence();
                    seq.AppendInterval(startDelay);
                    seq.Append(
                        goldTransform.DOPath(path, moveDuration, PathType.CatmullRom)
                        .SetEase(Ease.OutQuad)
                    );

                    seq.OnComplete(() =>
                    {
                        if (!firstArrivedCalled)
                        {
                            firstArrivedCalled = true;
                            _firsArrivedAction?.Invoke();
                        }

                        _arrivedAction?.Invoke();

                        _targetPoint.DOScale(new Vector3(1.2f, 1.2f, 1f), 0.18f)
                            .SetEase(Ease.OutBack)
                            .From();

                        objectPool.ReturnObjectToPool(gold);
                    });
                });
        }
    }
}

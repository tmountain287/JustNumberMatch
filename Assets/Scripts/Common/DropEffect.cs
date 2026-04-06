using Common.Manager;
using DG.Tweening;
using System;
using UnityEngine;
using Util;

public class DropEffect : MonoBehaviour
{
    [Header("필요한 설정")]
    [SerializeField] private ObjectPool objectPool;

    [Header("퍼짐 설정 (UI 기준 픽셀 정도로 생각)")]
    [SerializeField] private float spreadRadius = 120f;  // UI면 80~150 정도 추천
    [SerializeField] private float jumpDuration = 0.2f;

    [Header("이동 설정")]
    [SerializeField] private float baseMoveDuration = 0.30f;
    [SerializeField] private float moveDurationRandomRange = 0.05f;

    [Header("곡선 세기 설정")]
    [SerializeField] private float curveStrengthFactor = 0.7f;   // 좌우로 휘는 정도 (X거리 비례)
    [SerializeField] private float verticalArcFactor = 0.5f;   // 위로 튀는 정도 (X거리 비례 기반)

    [SerializeField] private AudioClip audioClip = null;

    private void Start()
    {
        if (objectPool != null)
            objectPool.CreateObjectPool();
    }

    /// <summary>
    /// 아이템 드랍 연출 (코인/골드/힌트 등)
    /// </summary>
    public void SpawnItem(
        ItemType _itemType,
        int amount,
        Transform parent,
        Vector3 startWorldPos,
        Transform targetPoint,
        Action firstArrivedAction = null,
        Action arrivedAction = null,
        Action lastArrivedAction = null)
    {
        if (amount <= 0 || parent == null || targetPoint == null)
            return;

        bool firstArrivedCalled = false;
        int arrivedCount = 0;

        // 부모 기준 local 좌표로 변환 (부모가 누구든 상관없게)
        Vector3 startLocalPos = parent.InverseTransformPoint(startWorldPos);
        Vector3 targetLocalPos = parent.InverseTransformPoint(targetPoint.position);

        for (int i = 0; i < amount; i++)
        {
            int index = i;

            GameObject gold = objectPool.GetObjectFromPool();

            // 자식 중 _itemType에 해당하는 것만 활성화
            for (int j = 0; j < gold.transform.childCount; j++)
            {
                gold.transform.GetChild(j).gameObject.SetActive((int)_itemType == j);
            }

            Transform goldTr = gold.transform;
            goldTr.DOKill();

            goldTr.SetParent(parent, false);
            goldTr.localPosition = startLocalPos;
            goldTr.localScale = Vector3.one;

            // === 1. 퍼지는 위치 계산 (부모 local 기준) ===
            Vector3 centerPos = startLocalPos;
            float uiSpread = spreadRadius;

            Vector3 spreadPos;
            if (amount == 2)
            {
                float x = uiSpread;
                float y = UnityEngine.Random.Range(-uiSpread * 0.3f, uiSpread * 0.3f);
                float side = (index == 0) ? -1f : 1f;
                spreadPos = centerPos + new Vector3(side * x, y, 0f);
            }
            else
            {
                Vector2 r = UnityEngine.Random.insideUnitCircle * uiSpread;
                spreadPos = centerPos + new Vector3(r.x, r.y, 0f);
            }

            // === 2. 처음 팟 퍼지는 이동 ===
            goldTr.DOLocalMove(spreadPos, jumpDuration)
                  .SetEase(Ease.OutQuad)
                  .OnComplete(() =>
                  {
                      Vector3 startPos = goldTr.localPosition;
                      Vector3 targetPos = targetLocalPos;

                      // === 진행 방향 (start → target) ===
                      Vector3 dir = targetPos - startPos;
                      if (dir.sqrMagnitude < 0.001f)
                          dir = Vector3.right;
                      dir.Normalize();

                      // dir 기준 왼쪽(perp) 방향 (시계반대)
                      Vector3 perp = Vector3.Cross(dir, Vector3.forward).normalized;

                      // X 기준 오른쪽 / 왼쪽 판별
                      float horizontal = Mathf.Sign(targetPos.x - startPos.x);
                      if (Mathf.Approximately(horizontal, 0f))
                          horizontal = 1f; // 거의 같은 X면 그냥 오른쪽 취급

                      // 기본 규칙 (타겟이 "위쪽"일 때 기준):
                      // 오른쪽 → 시계반대(perp), 왼쪽 → 시계(-perp)
                      float side = (horizontal > 0f) ? 1f : -1f;

                      // 타겟이 "아래"에 있으면 방향 반전
                      bool isTargetBelow = targetPos.y < startPos.y;
                      if (isTargetBelow)
                      {
                          side *= -1f;
                      }

                      Vector3 curveDir = perp * side;

                      // === 3. 곡선 세기 / 아치 세기 (🔥 가로 거리(X) 비례) ===
                      float horizontalDistance = Mathf.Abs(targetPos.x - startPos.x);

                      // 너무 가까우면 곡선이 안 느껴질 수 있으니 최소값 보정
                      float effectiveDistance = Mathf.Max(horizontalDistance, 50f);

                      // 중간 지점 비율 (0.5에서 살짝만 랜덤)
                      float t = 0.5f + UnityEngine.Random.Range(-0.05f, 0.05f);

                      // 좌우로 휘는 정도 (X 거리 비례)
                      float curveStrength =
                          effectiveDistance * curveStrengthFactor * UnityEngine.Random.Range(0.9f, 1.1f);

                      // 위로 튀는 정도도 X 거리 기준으로 비례
                      float upAmount =
                          effectiveDistance * verticalArcFactor * UnityEngine.Random.Range(0.9f, 1.1f);

                      Vector3 midPos =
                          Vector3.Lerp(startPos, targetPos, t) +
                          curveDir * curveStrength +
                          new Vector3(0, upAmount, 0);

                      Vector3[] path = new[] { startPos, midPos, targetPos };

                      // 출발 딜레이 (뒤 코인일수록 살짝 늦게)
                      float startDelay = 0.04f * index;

                      // 이동 시간 랜덤
                      float durationRandom = UnityEngine.Random.Range(-moveDurationRandomRange, moveDurationRandomRange);
                      float moveDuration = Mathf.Clamp(
                          baseMoveDuration + durationRandom,
                          baseMoveDuration - moveDurationRandomRange * 2f,
                          baseMoveDuration + moveDurationRandomRange * 2f
                      );

                      moveDuration = 0.5f; // 필요 없으면 이 줄 제거해도 됨

                      Sequence seq = DOTween.Sequence();
                      seq.AppendInterval(startDelay);
                      seq.Append(
                          goldTr.DOLocalPath(path, moveDuration, PathType.CatmullRom)
                                .SetEase(Ease.OutQuad)
                      );

                      seq.OnComplete(() =>
                      {
                          SoundManager.Instance.PlayFX(audioClip);

                          if (!firstArrivedCalled)
                          {
                              firstArrivedCalled = true;
                              firstArrivedAction?.Invoke();
                          }

                          arrivedAction?.Invoke();

                          arrivedCount++;
                          if (arrivedCount == amount)
                          {
                              lastArrivedAction?.Invoke();
                          }

                          targetPoint.DOKill();
                          targetPoint.localScale = Vector3.one;

                          targetPoint.DOPunchScale(
                              punch: Vector3.one * 0.25f,
                              duration: 0.18f,
                              vibrato: 1,
                              elasticity: 0.5f
                          );

                          objectPool.ReturnObjectToPool(gold);
                      });
                  });
        }
    }
}

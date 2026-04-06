using Common.Manager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MatchStick : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
{
    [SerializeField] private GameObject pivot = null;
    [SerializeField] private AudioClip returnAudioClip = null;
    [SerializeField] private AudioClip errorAudioClip = null;

    [SerializeField] private AudioClip putAudioClip = null;

    [SerializeField] private Image img = null;

    [SerializeField] private RectTransform rectTransform;
    private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;

    private Camera UICamera => canvas != null ? canvas.worldCamera : null;

    private Transform originalParent;
    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;

    private BaseRecognizer previousRecognizer;
    private Transform previousSlot;

    public RectTransform RectTransform { get => rectTransform; set => rectTransform = value; }

    [SerializeField] private TweenFlicker flicker;
    [Header("Slot Highlight")]
    [SerializeField] private Color moveColor = new Color(1f, 1f, 0f, 0.35f);
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0f, 0.35f);
    [SerializeField] private Color normalColor = Color.white;

    private Transform firstSlot;
    private GameObject currentBestSlot;
    private GameObject lastHighlightedSlot;

    // --- 프리뷰 오퍼레이터 대상 ---
    private BaseRecognizer currentBestRecognizer; // 현재 선택된(미리보기) 유닛
    private BaseRecognizer lastOverlappedUnit;    // 마지막으로 활성 프리뷰가 붙어있던 유닛

    // --- 드래그/홀드 상태 ---
    private bool isDragging;
    private bool isHolding; // 성냥을 들고 있을 때만 프리뷰 전환 허용

    // --- 포인터 좌표 (우선순위/근접 판정용) ---
    private Vector2 lastPointerScreenPos;
    private bool hasPointerPos;

    // --- 히스테리시스/쿨다운/이탈 지연 ---
    private const float EnterOverlapRatio = 0.12f; // (면적 기반 fallback용) 들어올 때
    private const float ExitOverlapRatio = 0.06f; // (면적 기반 fallback용) 나갈 때
    private const float SwitchCooldownSec = 0.08f; // 입·이탈 최소 간격 (전환에는 적용 X)
    private const float ClearDelaySec = 0.06f; // 완전 이탈 지연
    private float nextSwitchAllowedTime = 0f;
    private float pendingClearAt = -1f;

    // --- 슬롯 선택 로직 상수 ---
    private const float AreaEps = 0.5f;            // 겹침 면적 동률 허용오차 (px^2)
    private const float DistEps = 0.5f;            // 거리 동률 허용오차 (px)
    private const float PointerNearRadiusPx = 56f; // 포인터 근처로 판단할 반경 (px)

    private BaseRecognizer lastClearedUnit;
private float lastClearedAt = -1f;
    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void SetGameObject(bool _flag)
    {
        pivot.SetActive(_flag);
    }

    public void SetParent(Transform _transform)
    {
        transform.SetParent(_transform);
        //RectTransform.localEulerAngles = Vector3.zero;
        //RectTransform.localPosition = Vector3.zero;
        firstSlot = _transform;
    }

    public void OnDisable()
    {
        Clear();
        SetBlockRaycasts(true);
    }

    public void Clear()
    {
        flicker.enabled = false;
        SetNormal();
        ClearHighlight();
    }

    public void SetHighlight() => img.color = highlightColor;
    public void SetNormal() => img.color = normalColor;
    public void SetMoveColor() => img.color = moveColor;
    public void OnFlicker() => flicker.enabled = true;

    public void SetBlockRaycasts(bool _flag)
    {
        if (canvasGroup != null) canvasGroup.blocksRaycasts = _flag;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        lastPointerScreenPos = eventData.position;
        hasPointerPos = true;

        if (RectTransform.parent != null && RectTransform.parent.CompareTag("MatchSlot"))
        {
            isHolding = true;

            var recognizer = RectTransform.parent.GetComponentInParent<BaseRecognizer>();

            SetMoveColor();
            GameMgr.Instance.AllMatchUnlock(false);

            originalParent = RectTransform.parent;
            originalLocalPos = RectTransform.localPosition;
            originalLocalRot = RectTransform.localRotation;

            if (originalParent != null && originalParent.CompareTag("MatchSlot"))
            {
                previousSlot = originalParent;
                previousRecognizer = originalParent.GetComponentInParent<BaseRecognizer>();
            }

            RectTransform.SetParent(canvas.transform);

            recognizer?.OnDigitUpdated();
        }

        RectTransform.localScale = Vector3.one * 3f;

        UpdateBestSlotAndHighlight();
        GameMgr.Instance.OnAddOperation();
        //if (isHolding) UpdateBestUnitOverlapAndNotify();

        GameMgr.Instance.ShowSlots();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        lastPointerScreenPos = eventData.position;
        hasPointerPos = true;       

        RectTransform.localScale = Vector3.one;

        if (!isDragging)
        {            
            CheckEnd();
            GameMgr.Instance.ClearAddOpearation();
        }        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        lastPointerScreenPos = eventData.position;
        hasPointerPos = true;

        //if (isHolding) UpdateBestUnitOverlapAndNotify();

    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        lastPointerScreenPos = eventData.position;
        hasPointerPos = true;

        UpdateBestSlotAndHighlight();

        //if (isHolding) UpdateBestUnitOverlapAndNotify();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDragging)
        {           
            CheckEnd();
            GameMgr.Instance.ClearAddOpearation();
        }
        
        GameMgr.Instance.ValidateEquation();
        isDragging = false;

        // 상태 초기화
        isHolding = false;
        pendingClearAt = -1f;
        nextSwitchAllowedTime = 0f;
        hasPointerPos = false;
    }

    public void CheckEnd()
    {       
        if (currentBestSlot != null)
        {
            SoundManager.Instance.PlayFX(putAudioClip);

            RectTransform.SetParent(currentBestSlot.transform);
            RectTransform.localPosition = Vector3.zero;
            RectTransform.localRotation = Quaternion.identity;

            var recognizer = currentBestSlot.GetComponentInParent<BaseRecognizer>();
            recognizer?.OnDigitUpdated();

            if (previousRecognizer != null && previousRecognizer != recognizer)
                previousRecognizer.OnDigitUpdated();

            if (currentBestSlot.transform == firstSlot)
            {
                SetNormal();
                GameMgr.Instance.AllMatchUnlock(true);
                GameMgr.Instance.HideSlots(null);
            }
            else
            {
                SetHighlight();
                GameMgr.Instance.SetMatchLock(this);
                GameMgr.Instance.HideSlots(firstSlot);
            }

            RectTransform.localScale = Vector3.one;
        }
        else
        {
            //SoundManager.Instance.PlayFX(returnAudioClip);

            //// 겹치는 슬롯 없음 → 원위치 복귀
            //RectTransform.SetParent(originalParent);
            //RectTransform.localPosition = originalLocalPos;
            //RectTransform.localRotation = originalLocalRot;
            //if (originalParent == firstSlot)
            //{
            //    SetNormal();
            //    GameMgr.Instance.AllMatchUnlock(true);
            //    GameMgr.Instance.HideSlots(null);
            //}
            //else
            //{
            //    SetHighlight();
            //    GameMgr.Instance.SetMatchLock(this);
            //    GameMgr.Instance.HideSlots(firstSlot);
            //}
            //previousRecognizer?.OnDigitUpdated();

            ForceCancelAndReturn();
        }

        
        //ClearHighlight();
        
        // GameMgr.Instance.DestroyOperator();  // 드래그 종료 시 프리뷰 정리

        // 내부 상태 초기화
        lastOverlappedUnit = null;
        currentBestSlot = null;
        currentBestRecognizer = null;
        isDragging = false;
    }

    public void ForceCancelAndReturn()
    {
        // 이미 안 들고 있으면 패스
        if (!isDragging && !isHolding) return;

        // 사운드는 선택사항
        SoundManager.Instance.PlayFX(returnAudioClip);

        // 원래 자리로 복귀
        RectTransform.SetParent(originalParent);
        RectTransform.localPosition = originalLocalPos;
        RectTransform.localRotation = originalLocalRot;
        RectTransform.localScale = Vector3.one;

        if (originalParent == firstSlot)
        {
            SetNormal();
            GameMgr.Instance.AllMatchUnlock(true);
            GameMgr.Instance.HideSlots(null);
        }
        else
        {
            SetHighlight();
            GameMgr.Instance.SetMatchLock(this);
            GameMgr.Instance.HideSlots(firstSlot);
        }

        previousRecognizer?.OnDigitUpdated();
        GameMgr.Instance.ClearAddOpearation();

        // 하이라이트 & 상태 초기화
        ClearHighlight();
        currentBestSlot = null;
        lastHighlightedSlot = null;
        lastOverlappedUnit = null;
        currentBestRecognizer = null;

        isDragging = false;
        isHolding = false;
        pendingClearAt = -1f;
        nextSwitchAllowedTime = 0f;
        hasPointerPos = false;
    }

    // ─────────────────────────────────────────────────────────────
    // 슬롯 베스트 선택(near-first → 면적 → 거리 타이브레이커)
    private void UpdateBestSlotAndHighlight()
    {
        GameObject best = ComputeBestSlot();
        if (best == lastHighlightedSlot) return;

        if (lastHighlightedSlot != null)
            lastHighlightedSlot.GetComponent<MatchSlot>().SetNormal();

        if (best != null)
            best.GetComponent<MatchSlot>().SetHighlight();

        lastHighlightedSlot = best;
        currentBestSlot = best;
    }

    // ─────────────────────────────────────────────────────────────
    // 유닛 프리뷰 토글: 포인터 기준 진입/이탈 + 유효 유닛 간 전환은 즉시
    //private void UpdateBestUnitOverlapAndNotify()
    //{
    //    if (!isHolding) return;

    //    // ⬇️ HasEqual이 false면 프리뷰를 확실히 정리하고 나감(상태 얼어버림 방지)
    //    if (!GameMgr.Instance.Equation.HasEqaul)
    //    {
    //        if (lastOverlappedUnit != null)
    //        {
    //            lastClearedUnit = lastOverlappedUnit;
    //            lastClearedAt = Time.unscaledTime;

    //            GameMgr.Instance.DestroyOperator();
    //            lastOverlappedUnit = null;
    //            currentBestRecognizer = null;

    //            pendingClearAt = -1f;
    //            nextSwitchAllowedTime = 0f;
    //        }
    //        return;
    //    }

    //    var best = ComputeBestUnitByPointer(UICamera, out float proximityRatio);
    //    bool hasBest = best != null;

    //    // --- 이하 기존 로직 동일 ---
    //    // 현재 아무 유닛에 붙어있지 않은 상태 → 진입 판단
    //    if (lastOverlappedUnit == null)
    //    {
    //        if (!hasBest) return;

    //        // 같은 유닛 재진입은 쿨다운 무시
    //        const float ReenterWindowSec = 0.35f;
    //        bool reenterSame =
    //            (best == lastClearedUnit) &&
    //            (lastClearedAt > 0f) &&
    //            (Time.unscaledTime - lastClearedAt <= ReenterWindowSec);

    //        if (reenterSame || Time.unscaledTime >= nextSwitchAllowedTime)
    //        {
    //            GameMgr.Instance.SpawnOperator(best);
    //            lastOverlappedUnit = best;
    //            currentBestRecognizer = best;

    //            if (!reenterSame)
    //                nextSwitchAllowedTime = Time.unscaledTime + SwitchCooldownSec;

    //            pendingClearAt = -1f;
    //        }
    //        return;
    //    }

    //    // 유닛 밖 → 지연 후 파괴
    //    if (!hasBest)
    //    {
    //        if (pendingClearAt < 0f) pendingClearAt = Time.unscaledTime + ClearDelaySec;

    //        if (Time.unscaledTime >= pendingClearAt && Time.unscaledTime >= nextSwitchAllowedTime)
    //        {
    //            lastClearedUnit = lastOverlappedUnit;
    //            lastClearedAt = Time.unscaledTime;

    //            GameMgr.Instance.DestroyOperator();
    //            lastOverlappedUnit = null;
    //            currentBestRecognizer = null;

    //            nextSwitchAllowedTime = Time.unscaledTime + SwitchCooldownSec;
    //        }
    //        return;
    //    }

    //    // 같은 유닛 유지 → 파괴 예약 취소
    //    if (best == lastOverlappedUnit)
    //    {
    //        pendingClearAt = -1f;
    //        return;
    //    }

    //    // 다른 유닛으로 전환 → 즉시 교체
    //    GameMgr.Instance.DestroyOperator();
    //    GameMgr.Instance.SpawnOperator(best);
    //    lastOverlappedUnit = best;
    //    currentBestRecognizer = best;
    //    pendingClearAt = -1f;
    //}



    //// 포인터(손가락) 위치 기준으로 가장 가까운 유닛을 찾음
    //private BaseRecognizer ComputeBestUnitByPointer(Camera cam, out float proximityRatio)
    //{
    //    proximityRatio = 0f;
    //    if (!hasPointerPos) return null;

    //    BaseRecognizer best = null;
    //    float bestDist = float.MaxValue;
    //    Rect bestRect = default;

    //    // 모든 BaseRecognizer를 후보로
    //    foreach (var unit in GameMgr.Instance.DigitRecognizerList)
    //    {
    //        if (unit == null || !unit.gameObject.activeInHierarchy) continue;

    //        // 현재 프리뷰 오퍼레이터는 후보 제외
    //        var preview = GameMgr.Instance.CurrentPreviewOperator;
    //        if (preview != null && unit == preview) continue;

    //        // 유닛의 슬롯들을 합쳐 바운딩 박스 구성
    //        if (unit.MatchSlotRectList == null || unit.MatchSlotRectList.Count == 0) continue;

    //        bool any = false;
    //        Rect unitRect = default;

    //        foreach (var slot in unit.MatchSlotRectList)
    //        {
    //            if (slot == null || !slot.gameObject.activeInHierarchy) continue;
    //            Rect r = CollisionUtil.GetScreenRect(cam, slot);
    //            if (!any) { unitRect = r; any = true; }
    //            else
    //            {
    //                float xMin = Mathf.Min(unitRect.xMin, r.xMin);
    //                float yMin = Mathf.Min(unitRect.yMin, r.yMin);
    //                float xMax = Mathf.Max(unitRect.xMax, r.xMax);
    //                float yMax = Mathf.Max(unitRect.yMax, r.yMax);
    //                unitRect = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    //            }
    //        }
    //        if (!any) continue;

    //        // 포인터가 유닛 안/근처인지
    //        float dist = DistancePointToRect(lastPointerScreenPos, unitRect);
    //        bool contains = unitRect.Contains(lastPointerScreenPos);
    //        bool near = dist <= PointerNearRadiusPx;

    //        if (!(contains || near)) continue;

    //        // 더 가까운 유닛 선호, 거리가 거의 같으면 형제 인덱스 높은(앞쪽) 유닛 선호
    //        if (dist + DistEps < bestDist ||
    //            (Mathf.Abs(dist - bestDist) <= DistEps &&
    //             unit.transform.GetSiblingIndex() > (best != null ? best.transform.GetSiblingIndex() : -1)))
    //        {
    //            best = unit;
    //            bestDist = dist;
    //            bestRect = unitRect;
    //        }
    //    }

    //    if (best != null)
    //    {
    //        // 근접도 (안이면 1, 반경까지 선형 감소)
    //        bool inside = bestRect.Contains(lastPointerScreenPos);
    //        proximityRatio = inside ? 1f : Mathf.Clamp01(1f - (bestDist / PointerNearRadiusPx));
    //    }

    //    // fallback: 포인터가 없거나 근접 실패 시 겹침 면적 방식(안정성용)
    //    if (best == null)
    //        best = ComputeBestUnitByOverlap(cam, out _);

    //    return best;
    //}

    //// (fallback) 겹침 면적 기반
    //private BaseRecognizer ComputeBestUnitByOverlap(Camera cam, out float bestRatio)
    //{
    //    bestRatio = 0f;
    //    Rect me = CollisionUtil.GetScreenRect(cam, RectTransform);

    //    BaseRecognizer best = null;
    //    float bestArea = 0f;

    //    foreach (var unit in GameMgr.Instance.DigitRecognizerList)
    //    {
    //        if (unit == null || !unit.gameObject.activeInHierarchy) continue;

    //        var preview = GameMgr.Instance.CurrentPreviewOperator;
    //        if (preview != null && unit == preview) continue;

    //        if (unit.MatchSlotRectList == null || unit.MatchSlotRectList.Count == 0) continue;

    //        bool any = false;
    //        Rect unitRect = default;

    //        foreach (var slot in unit.MatchSlotRectList)
    //        {
    //            if (slot == null || !slot.gameObject.activeInHierarchy) continue;
    //            Rect r = CollisionUtil.GetScreenRect(cam, slot);
    //            if (!any) { unitRect = r; any = true; }
    //            else
    //            {
    //                float xMin = Mathf.Min(unitRect.xMin, r.xMin);
    //                float yMin = Mathf.Min(unitRect.yMin, r.yMin);
    //                float xMax = Mathf.Max(unitRect.xMax, r.xMax);
    //                float yMax = Mathf.Max(unitRect.yMax, r.yMax);
    //                unitRect = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    //            }
    //        }
    //        if (!any) continue;

    //        float overlap = CollisionUtil.GetScreenOverlapArea(me, unitRect);
    //        if (overlap <= 0f) continue;

    //        if (overlap > bestArea)
    //        {
    //            bestArea = overlap;
    //            best = unit;

    //            float denom = Mathf.Min(me.width * me.height, unitRect.width * unitRect.height);
    //            bestRatio = denom > 0f ? overlap / denom : 0f;
    //        }
    //    }

    //    return best;
    //}

    // --- Helpers: 슬롯 선택(near-first → 면적 → 거리) -----------------------

    private GameObject ComputeBestSlot()
    {
        Rect myScreenRect = CollisionUtil.GetScreenRect(UICamera, RectTransform);

        GameObject bestNear = null;
        float bestNearDist = float.MaxValue;
        float bestNearAreaRatio = 0f;

        GameObject best = null;
        float bestArea = 0f;
        float bestDist = float.MaxValue;

        foreach (RectTransform slotRect in GameMgr.Instance.Equation.MatchSlotRectList)
        {
            if (slotRect == null) continue;

            // 슬롯 점유 시 제외 (실제 성냥 존재 여부로 판단)
            if (slotRect.GetComponentInChildren<MatchStick>() != null) continue;

            Rect slotScreenRect = CollisionUtil.GetScreenRect(UICamera, slotRect);
            float area = CollisionUtil.GetScreenOverlapArea(myScreenRect, slotScreenRect);
            if (area <= 0f) continue;

            // 포인터 기준 최소거리
            float distToRect = float.MaxValue;
            if (hasPointerPos)
                distToRect = DistancePointToRect(lastPointerScreenPos, slotScreenRect);

            // 면적 정규화(비교 안정화)
            float denom = Mathf.Min(myScreenRect.width * myScreenRect.height,
                                    slotScreenRect.width * slotScreenRect.height);
            float areaRatio = (denom > 0f) ? (area / denom) : 0f;

            bool isNear = hasPointerPos && (distToRect <= PointerNearRadiusPx);

            if (isNear)
            {
                bool betterDist = distToRect + DistEps < bestNearDist;
                bool tieDist = Mathf.Abs(distToRect - bestNearDist) <= DistEps;

                if (bestNear == null || betterDist || (tieDist && areaRatio > bestNearAreaRatio + 1e-5f))
                {
                    bestNear = slotRect.gameObject;
                    bestNearDist = distToRect;
                    bestNearAreaRatio = areaRatio;
                }
                continue;
            }

            // 기본 후보: 면적 우선, 면적 동률이면 거리
            float distSqr;
            if (hasPointerPos)
            {
                Vector2 slotCenter = slotScreenRect.center;
                distSqr = (slotCenter - lastPointerScreenPos).sqrMagnitude;
            }
            else
            {
                Vector3 slotCenterW = slotRect.TransformPoint(slotRect.rect.center);
                Vector3 myCenterW = RectTransform.TransformPoint(RectTransform.rect.center);
                distSqr = (slotCenterW - myCenterW).sqrMagnitude;
            }

            bool betterArea = area > bestArea + AreaEps;
            bool tieArea = Mathf.Abs(area - bestArea) <= AreaEps;

            if (best == null || betterArea || (tieArea && distSqr < bestDist))
            {
                best = slotRect.gameObject;
                bestArea = area;
                bestDist = distSqr;
            }
        }

        return bestNear != null ? bestNear : best;
    }

    private static float DistancePointToRect(Vector2 p, Rect r)
    {
        float dx = (p.x < r.xMin) ? (r.xMin - p.x) : (p.x > r.xMax ? (p.x - r.xMax) : 0f);
        float dy = (p.y < r.yMin) ? (r.yMin - p.y) : (p.y > r.yMax ? (p.y - r.yMax) : 0f);
        return Mathf.Sqrt(dx * dx + dy * dy); // 내부면 0
    }

    private void ClearHighlight()
    {
        if (lastHighlightedSlot != null)
        {
            lastHighlightedSlot.GetComponent<MatchSlot>().SetNormal();
            lastHighlightedSlot = null;
        }
    }
}

using Common.Manager;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum RecognizerType
{
    None = 0,
    Digit = 1,
    Operator = 2,
    AddOperator = 3,
}

public enum OperatorType
{
    None = 0,
    Minus = 1,
    Plus = 2,
    Equals = 3,
    None_Plus = 4,
    Minus2 = 5,
    Plus2 = 6,

}

public enum SpawnType
{
    None = 0,
    Spawn = 1,
}

public class RecognizerInfo
{
    public RecognizerType RecognizerType { get; set; }
    public object Value { get; set; }
 
    public bool[] Flags { get; set; }

    public RecognizerInfo(RecognizerType recognizerType, object value, bool[] flags = null)
    {
        RecognizerType = recognizerType;
        Value = value;      
        Flags = flags;
    }
}

public class SegmentTable
{

    public static Dictionary<RecognizerType, List<RecognizerInfo>> NumberSegmentTableList = new Dictionary<RecognizerType, List<RecognizerInfo>>
    {
        {
            RecognizerType.Digit, new List<RecognizerInfo>
            {
                new(RecognizerType.Digit, 0,  new bool[]{ true,  true,  true,  true,  true,  true,  false }),
                new(RecognizerType.Digit, 1,  new bool[]{ false, true,  true,  false, false, false, false }),
                new(RecognizerType.Digit, 1,  new bool[]{ false, false, false, false, true,  true,  false }),
                new(RecognizerType.Digit, 2,  new bool[]{ true,  true,  false, true,  true,  false, true  }),
                new(RecognizerType.Digit, 3,  new bool[]{ true,  true,  true,  true,  false, false, true  }),
                new(RecognizerType.Digit, 4,  new bool[]{ false, true,  true,  false, false, true,  true  }),
                new(RecognizerType.Digit, 5,  new bool[]{ true,  false, true,  true,  false, true,  true  }),
                new(RecognizerType.Digit, 6,  new bool[]{ true,  false, true,  true,  true,  true,  true  }),
                new(RecognizerType.Digit, 7,  new bool[]{ true,  true,  true,  false, false, false, false }),
                new(RecognizerType.Digit, 8,  new bool[]{ true,  true,  true,  true,  true,  true,  true  }),
                new(RecognizerType.Digit, 9,  new bool[]{ true,  true,  true,  true,  false, true,  true  }),
                new(RecognizerType.Digit, 11, new bool[]{ false, true,  true,  false, true,  true,  false }),
            }
        },

        {
            RecognizerType.Operator, new List<RecognizerInfo>
            {
                new(RecognizerType.Operator, OperatorType.None, new bool[] { false, false, false }),
                new(RecognizerType.Operator, OperatorType.None_Plus, new bool[] { false, true, false }),
                new(RecognizerType.Operator, OperatorType.Minus, new bool[] { true, false, false }),
                new(RecognizerType.Operator, OperatorType.Plus, new bool[] { true, true, false }),
                new(RecognizerType.Operator, OperatorType.Equals, new bool[] { true, false, true }),
                new(RecognizerType.Operator, OperatorType.Minus2, new bool[] { false, false, true }),

            }
        },

        {
            RecognizerType.AddOperator, new List<RecognizerInfo>
            {
                new(RecognizerType.Operator, OperatorType.None, new bool[] { false }),
                new(RecognizerType.Operator, OperatorType.Minus, new bool[] { true }),
            }
        }
    };

    public static Dictionary<RecognizerType, List<RecognizerInfo>> SlotSegmentTableList = new Dictionary<RecognizerType, List<RecognizerInfo>>
    {
        {
            RecognizerType.Digit, new List<RecognizerInfo>
            {
                new(RecognizerType.Digit, 0,  new bool[]{ true,  true,  true,  true,  true,  true,  true }),
                new(RecognizerType.Digit, 1,  new bool[]{ true,  true,  true,  true,  true,  true,  true }),
                new(RecognizerType.Digit, 1,  new bool[]{ true,  true,  true,  true,  true,  true,  true }),
                new(RecognizerType.Digit, 2,  new bool[]{ true,  true,  true,  true,  true,  true,  true }),
                new(RecognizerType.Digit, 3,  new bool[]{ true,  true,  true,  true,  true,  true,  true }),
                new(RecognizerType.Digit, 4,  new bool[]{ true,  true,  true,  true,  true,  true,  true }),
                new(RecognizerType.Digit, 5,  new bool[]{ true,  true,  true,  true,  true,  true,  true }),
                new(RecognizerType.Digit, 6,  new bool[]{ true,  true,  true,  true,  true,  true,  true }),
                new(RecognizerType.Digit, 7,  new bool[]{ true,  true,  true,  true,  true,  true,  true }),
                new(RecognizerType.Digit, 8,  new bool[]{ true,  true,  true,  true,  true,  true,  true }),
                new(RecognizerType.Digit, 9,  new bool[]{ true,  true,  true,  true,  true,  true,  true }),
                new(RecognizerType.Digit, 11, new bool[]{ true,  true,  true,  true,  true,  true,  true }),
                new(RecognizerType.None, 0, new bool[]{ true,  true,  true,  true,  true,  true,  true })
            }
        },

        {
            RecognizerType.Operator, new List<RecognizerInfo>
            {
                new(RecognizerType.Operator, OperatorType.Minus, new bool[] { true, true, true }),
                new(RecognizerType.Operator, OperatorType.Plus, new bool[] { true, true, false }),
                new(RecognizerType.Operator, OperatorType.Equals, new bool[] { true, false, true }),
                new(RecognizerType.Operator, OperatorType.Minus, new bool[] { true, true, true }),
                new(RecognizerType.None, OperatorType.None, new bool[]{ true,  false,  false }),
                new(RecognizerType.Operator, OperatorType.None_Plus, new bool[] { true, true, false }),
                new(RecognizerType.Operator, OperatorType.Minus2, new bool[] { false, true, false }),
                new(RecognizerType.Operator, OperatorType.Plus2, new bool[] { false, true, true }),

            }
        },
        {
            RecognizerType.AddOperator, new List<RecognizerInfo>
            {
                new(RecognizerType.Operator, OperatorType.None, new bool[] { true }),
                new(RecognizerType.Operator, OperatorType.Minus, new bool[] { true }),
            }
        }
    };
}

public abstract class BaseRecognizer : MonoBehaviour
{
    [SerializeField] private RecognizerType recognizerType = RecognizerType.None;
    [SerializeField] private SpawnType spawnType = SpawnType.None;
    
    [SerializeField] private OperatorRecognizer operatorRecognizer = null;
    [SerializeField] protected Transform[] segmentSlots;

    protected List<RecognizerInfo> segmentTableList;
    protected List<RecognizerInfo> slotTableList;
     
    public UnityEvent OnChanged = new();
    public RecognizerInfo CurrentRecognizerInfo { get; set; }
    public EquationPosType EquationPosType { get; set; }
    public SpawnType SpawnType { get => spawnType; set => spawnType = value; }
    public OperatorRecognizer OperatorRecognizer { get => operatorRecognizer; }
    public RecognizerType RecognizerType { get => recognizerType; }

    public virtual List<RectTransform> MatchSlotRectList
    { 
        get
        {
            List<RectTransform> list = new List<RectTransform>();

            foreach (var segment in segmentSlots) 
            {
                if(segment.gameObject.activeSelf)
                    list.Add(segment.GetComponent<RectTransform>()); 
            }
            return list;
        }
    }
         

    public List<RecognizerInfo> SegmentTableList 
    {
        get => SegmentTable.NumberSegmentTableList[RecognizerType];
    }

    public List<RecognizerInfo> SlotSegmentTableList
    {
        get => SegmentTable.SlotSegmentTableList[RecognizerType];
    }


    public virtual void ShowSlots()
    {

    }

    public virtual void HideSlots(Transform _firstSlot)
    {
        foreach (var item in segmentSlots)
        {
            item.gameObject.SetActive(item.GetComponentInChildren<MatchStick>() != null || _firstSlot == item);
        }
    }

    public virtual void OnDigitUpdated()
    {
        // 1) 현재 점등 상태 수집
        bool[] active = new bool[segmentSlots.Length];
        for (int i = 0; i < segmentSlots.Length; i++)
        {
            // 슬롯에 성냥이 하나라도 있으면 true
            active[i] = segmentSlots[i].GetComponentInChildren<MatchStick>() != null;
        }

        // 2) 테이블에서 동일 패턴 찾기 (길이 다르면 패스)
        RecognizerInfo matched = null;
        foreach (var info in SegmentTableList)
        {
            if (info.Flags == null || info.Flags.Length != active.Length) continue;
            if (IsSamePattern(active, info.Flags))
            {
                matched = info;
                break; // 첫 번째 매칭 채택 (원래 정책 유지)
            }
        }

        // 3) 매칭 성공: 값이 바뀐 경우에만 갱신 이벤트 발행
        if (matched != null)
        {
            if (!SameId(CurrentRecognizerInfo, matched))
            {
                CurrentRecognizerInfo = matched;
                OnChanged.Invoke();

                // ★ 연산자일 때만 None 제거 콜백 호출
                if (SpawnType == SpawnType.Spawn &&
                    matched.RecognizerType == RecognizerType.Operator &&
                    matched.Value is OperatorType op &&
                    op == OperatorType.None)
                {
                    onRemoveNone?.Invoke();
                }
            }
            return;
        }

        // 4) 매칭 실패 → 인식 해제(한 번만 이벤트)
        if (CurrentRecognizerInfo != null)
        {
            CurrentRecognizerInfo = null;
            OnChanged.Invoke();
        }
    }

    // 값/타입 기준으로 동일성 판정 (참조 비교 아님)
    private static bool SameId(RecognizerInfo a, RecognizerInfo b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        if (a.RecognizerType != b.RecognizerType) return false;
        return Equals(a.Value, b.Value); // int면 int 값 비교, enum이면 enum 값 비교
    }

    // (예시) 패턴 비교: 길이까지 이미 맞춘 상태라 가볍게 비교
    private static bool IsSamePattern(bool[] a, bool[] b)
    {
        if (a == null || b == null || a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i]) return false;
        return true;
    }

    public Action onRemoveNone = null;
      


    public virtual void SetValue(object _value, bool _isAni = true)
    {
        if (_value is int intValue)
        {            
            if(intValue < 0)
            {
                operatorRecognizer.SetValue(OperatorType.Minus);
            }
            SetValue(Mathf.Abs(intValue), _isAni);
        }
        else if (_value is OperatorType enumValue)
        {
            SetValue(enumValue, _isAni);
        }        
    }

    public void SetValue(int _value, bool _isAni = true)
    {        
        var result = SegmentTableList.FirstOrDefault(kv => (int)kv.Value == _value);        
        SetValue2(result, _isAni);
    }

    public void SetValue(OperatorType _value, bool _isAni = true)
    {
        var result = SegmentTableList.FirstOrDefault(kv => (OperatorType)kv.Value == _value);
        SetValue2(result, _isAni);
    }


    public void SetValue2(RecognizerInfo result, bool _isAni = true)
    {
        for (int i = 0; i < result.Flags.Length; i++)
        {
            if (result.Flags[i])
            {
                MatchStick matchStick = ObjectPoolManager.Instance.GetMatchStick();

                matchStick.SetGameObject(false);
                matchStick.SetParent(segmentSlots[i]);

                // 초기 랜덤 위치 & 회전
                Vector3 randomPos = new Vector3(
                    UnityEngine.Random.Range(-400f, 400f),
                    UnityEngine.Random.Range(-300f, 300f),
                    0f
                );

                Quaternion randomRot = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(-180f, 180f));

                matchStick.transform.localPosition = randomPos;
                matchStick.transform.localRotation = randomRot;

                // 애니메이션 OFF → 바로 위치/스케일/회전 결정
                if (!_isAni)
                {
                    matchStick.SetGameObject(true);
                    matchStick.transform.localScale = Vector3.one;
                    matchStick.transform.localPosition = Vector3.zero;
                    matchStick.transform.localRotation = Quaternion.identity;
                    continue;
                }
                
                // --------------------------
                // 애니메이션 ON
                // --------------------------
                Sequence seq = DOTween.Sequence();
                seq.AppendInterval(0.5f); // 0.5초 기다림
                seq.AppendCallback(() => matchStick.SetGameObject(true));

                // 1) 이동 + 회전 + 스케일 연출
                seq.Append(
                    matchStick.transform.DOLocalMove(Vector3.zero, 0.9f)
                    .SetEase(Ease.OutBack)
                );

                seq.Join(
                    matchStick.transform.DOLocalRotate(Vector3.forward * 360f, 0.9f, RotateMode.FastBeyond360)
                    .SetEase(Ease.OutBack)
                );

                seq.Join(
                    matchStick.transform.DOScale(1.1f, 0.45f).SetLoops(2, LoopType.Yoyo)
                );

                // 2) 착지 느낌
                seq.Append(
                    matchStick.transform.DOPunchPosition(Vector3.up * 10f, 0.25f, 8, 0.5f)
                );

                seq.Play();
                matchStick.transform.localScale = Vector3.one;
            }
        }

        CurrentRecognizerInfo = result;
        gameObject.SetActive(true);
    }



    public MatchStick GetHintMatchStick(EquationData start, EquationData target)
    { 
        if (target.Value == null)
        {
            target.Value = OperatorType.None;
        }
        var startValue = start.Value;
        var targetValue = target.Value;

        bool[] from;
        bool[] to;

        if (startValue is int intValue)
        {
            from = SegmentTableList.FirstOrDefault(kv => (int)kv.Value == (int)startValue).Flags;
            to = SegmentTableList.FirstOrDefault(kv => (int)kv.Value == (int)targetValue).Flags;
        }
        else
        {           
            from = SegmentTableList.FirstOrDefault(kv => (OperatorType)kv.Value == (OperatorType)startValue).Flags;
            to = SegmentTableList.FirstOrDefault(kv => (OperatorType)kv.Value == (OperatorType)targetValue).Flags;
        }

        // true -> false 로 변한 인덱스들 찾기
        var changedIndices = from
            .Select((value, index) => new { value, index })
            .Where(x => x.value == true && to[x.index] == false)
            .Select(x => x.index)
            .ToList();

        //if (changedIndices.Count > 1 && changedIndices.Count == 0)
        //{
        //    Debug.LogError("문제 풀이가 잘못되었습니다.");
        //    return null;
        //}
        if (changedIndices.Count > 0)
        {
            MatchStick matchStick = segmentSlots[changedIndices[0]].GetComponentInChildren<MatchStick>();

            matchStick.OnFlicker();
            return matchStick;
        }

        return null;
    }
}
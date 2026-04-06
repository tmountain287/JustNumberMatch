using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OperatorRecognizer : BaseRecognizer
{
    [SerializeField] private Transform pivot = null;

    public override void ShowSlots()
    {
        RecognizerInfo result = null;

        pivot.localPosition = Vector3.zero;

        if (CurrentRecognizerInfo == null)
        {
            result = SlotSegmentTableList.FirstOrDefault(kv => kv.RecognizerType == RecognizerType.None);
        }
        else if((OperatorType)CurrentRecognizerInfo.Value == OperatorType.Minus)
        { 
            if(GameMgr.Instance.Equation.HasEqaul)
            {
                result = SlotSegmentTableList.FirstOrDefault(kv => (OperatorType)kv.Value == OperatorType.Plus);
            }
            else
            {
                result = SlotSegmentTableList.FirstOrDefault(kv => (OperatorType)kv.Value == OperatorType.Equals);
                pivot.localPosition = new Vector3(0, 40, 0);
            }
        }
        else if ((OperatorType)CurrentRecognizerInfo.Value == OperatorType.Minus2)
        {
            if (GameMgr.Instance.Equation.HasEqaul)
            {
                result = SlotSegmentTableList.FirstOrDefault(kv => (OperatorType)kv.Value == OperatorType.Plus2);
            }
            else
            {
                result = SlotSegmentTableList.FirstOrDefault(kv => (OperatorType)kv.Value == OperatorType.Equals);
                pivot.localPosition = new Vector3(0, 40, 0);
            }
        }
        else
        {
            result = SlotSegmentTableList.FirstOrDefault(kv => (OperatorType)kv.Value == (OperatorType)CurrentRecognizerInfo.Value);
            RepositionPivot();
        }

        if (result != null)
        {
            for (int i = 0; i < result.Flags.Length; i++)
            {
                if(segmentSlots[i].GetComponentInChildren<MatchStick>() != null) 
                {
                    segmentSlots[i].gameObject.SetActive(true);
                }
                else
                {
                    segmentSlots[i].gameObject.SetActive(result.Flags[i]);
                }                
            }
        }
    }

    public override void HideSlots(Transform _firstSlot)
    {
        base.HideSlots(_firstSlot);
        RepositionPivot();
    }

    private void RepositionPivot()
    {
        if (CurrentRecognizerInfo != null && (OperatorType)CurrentRecognizerInfo.Value == OperatorType.Equals)
        {
            pivot.localPosition = new Vector3(0, 40, 0);
        }
        else
        {
            pivot.localPosition = Vector3.zero;
        }
    }

    public override void OnDigitUpdated()
    {
        base.OnDigitUpdated();
    }

    public override void SetValue(object _value, bool _isAni = true)
    {
        base.SetValue(_value, _isAni);

        RepositionPivot();
    }
}
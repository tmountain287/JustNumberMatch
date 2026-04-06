using Common.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Util;

public enum EquationType
{

}

public class Equation : MonoBehaviour
{
    [SerializeField] private HorizontalLayoutGroup horizontalLayoutGroup = null;
    [SerializeField] private List<BaseRecognizer> unitRecognizerList = new();

    [SerializeField] private AudioClip setAudioClip = null;

    private List<RecognizerInfo> tokens = new List<RecognizerInfo>();
    private EquationTableData data = null;

    public float Spacing { get => horizontalLayoutGroup.spacing; }

    public List<RectTransform> MatchSlotRectList
    {
        get => unitRecognizerList.Where(x => x.MatchSlotRectList != null).SelectMany(x => x.MatchSlotRectList).ToList();
    }

    public List<BaseRecognizer> UnitRecognizerList { get => unitRecognizerList; }

    public bool HasEqaul
    {
        get
        {
            List<BaseRecognizer> _list = unitRecognizerList.Where(x => x.RecognizerType == RecognizerType.Operator).ToList();

            for (int i = 0; i < _list.Count; i++)
            {
                if (_list[i].CurrentRecognizerInfo == null)
                {
                    continue;
                }

                if ((OperatorType)_list[i].CurrentRecognizerInfo.Value == OperatorType.Equals)
                    return true;
            }

            return false;
        }
    }

    public void Start()
    {
        Initialized();
    }

    public void Initialized()
    {

        //unitRecognizerList.ForEach(t => t.OnChanged.AddListener(ValidateEquation));
    }

    public void ShowSlots()
    {
        unitRecognizerList.ForEach(t => t.ShowSlots());
    }

    public void HideSlots(Transform _firstSlot)
    {
        unitRecognizerList.ForEach(t => t.HideSlots(_firstSlot));
    }

    float GetValue(int count, float start, float end)
    {
        float x1 = 5, y1 = start;  // 시작점
        float x2 = 8, y2 = end;  // 끝점

        // 선형 보간 (Linear Interpolation)
        float t = (count - x1) / (x2 - x1);  // 0~1 비율
        return y1 + (y2 - y1) * t;
    }

    public void SetStage(EquationTableData _data)
    {
        data = _data;
        unitRecognizerList.Clear();

        int count = 0;
        _data.equationDataList.ForEach(x =>
        {
            if (x.Value != null)
            {
                BaseRecognizer baseRecognizer = x.RecognizerType == RecognizerType.Digit ?
                    ObjectPoolManager.Instance.GetDigitRecognizer() :
                    ObjectPoolManager.Instance.GetOerationRecognizer();

                //baseRecognizer.EquationPosType = x.RecognizerType;
                baseRecognizer.SetValue(x.Value);
                baseRecognizer.transform.SetParent(transform);
                baseRecognizer.transform.localScale = Vector3.one;
                unitRecognizerList.Add(baseRecognizer);
                count++;
            }
        });

        SoundManager.Instance.PlayFX(setAudioClip, 1.3f);

        transform.localScale = Vector3.one * GetValue(count, 1, 0.8f);
        horizontalLayoutGroup.spacing = GetValue(count, 60, 40);

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }


    public void OnHint()
    {
        //var list = data.GetDifferentValues();

        unitRecognizerList[data.changedIndex].GetHintMatchStick(data.equationDataList[data.changedIndex], data.changedTo);

        //list.ForEach(kv =>
        //{
        //    BaseRecognizer unitRecognizer = unitRecognizerList.Where(x => x.EquationPosType == kv.pos).FirstOrDefault();
        //    if (unitRecognizer != null)
        //    {
        //        unitRecognizer.GetHintMatchStick(kv.start, kv.target);
        //    }            
        //});        

        //if (data.moveFrom.equationPosType == EquationPosType.Operation)
        //{
        //    unitRecognizer.GetHintMatchStick((OperatorType)data.numberDataDic[data.moveFrom.equationPosType], data.moveFrom.OperatorType, data.moveFrom.index);
        //}
        //else
        //{
        //    if (data.moveFrom.equationPosType == data.moveTo.equationPosType)
        //        unitRecognizer.GetHintMatchStick(data.numberDataDic[data.moveFrom.equationPosType], data.moveTo.value, data.moveFrom.index);
        //    else
        //        unitRecognizer.GetHintMatchStick(data.numberDataDic[data.moveFrom.equationPosType], data.moveFrom.value, data.moveFrom.index);
        //}
    }

    public void ChangeAddOperation()
    {
        var add = UnitRecognizerList.FirstOrDefault(x => x is AddOperatorRecognizer);

        if (add != null)
        {
            int index = UnitRecognizerList.FindIndex(x => x is AddOperatorRecognizer);
            MatchStick matchStick = add.GetComponentInChildren<MatchStick>();

            ObjectPoolManager.Instance.MatchStickPoolList.Remove(matchStick);
            ObjectPoolManager.Instance.MatchStickPool.ReturnObjectToPool(matchStick.gameObject);

            BaseRecognizer baseRecognizer = ObjectPoolManager.Instance.GetOerationRecognizer();

            baseRecognizer.SetValue(OperatorType.Minus, false);
            baseRecognizer.transform.SetParent(transform);
            baseRecognizer.transform.SetSiblingIndex(index);
            baseRecognizer.transform.localScale = Vector3.one;

            unitRecognizerList.Insert(index, baseRecognizer);
            //SoundManager.Instance.PlayFX(setAudioClip, 1.3f);
            transform.localScale = Vector3.one * GetValue(unitRecognizerList.Count, 1, 0.8f);
            horizontalLayoutGroup.spacing = GetValue(unitRecognizerList.Count, 60, 40);

            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

            ObjectPoolManager.Instance.ClearAddOpearation(true);
        }
    }


    public Tuple<string, ValidationResultType> ValidateEquation()
    {
        tokens.Clear();

        var recogList = GetComponentsInChildren<BaseRecognizer>().ToList();

        recogList.ForEach(t =>
        {
            if (t.gameObject.activeSelf)
            {
                if (t.CurrentRecognizerInfo != null)
                {
                    if (t.CurrentRecognizerInfo.Value is OperatorType ot)
                    {
                        if (ot != OperatorType.None)
                        {
                            tokens.Add(t.CurrentRecognizerInfo);
                        }
                    }
                    else
                    {
                        tokens.Add(t.CurrentRecognizerInfo);
                    }
                }
                else
                {
                    tokens.Add(t.CurrentRecognizerInfo);
                }

            }
        });

        if (tokens.Any(t => t == null))
        {
            Debug.Log("토큰 중 null 값이 있습니다. 수식 인식이 아직 완료되지 않았습니다.");
            return new(null, ValidationResultType.NULL);
        }

        var ok = EquationValidator.TryValidateTokens(tokens, out var res1);


        if (res1.Success && res1.AreAllEqual)
        {
            Debug.Log(res1.ToString());
            return new(string.Join(" = ", res1.SegmentNormalized), ValidationResultType.OK);
        }
        else if (res1.Success && !res1.AreAllEqual)
        {
            Debug.Log(res1.ToString());

            return new(string.Join(" = ", res1.SegmentNormalized), ValidationResultType.FALSE);
        }
        else
        {
            return new(null, res1.ValidationResultType);
        }
    }
}
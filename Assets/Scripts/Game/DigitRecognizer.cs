using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;

public class DigitRecognizer : BaseRecognizer
{
    //public override List<RectTransform> MatchSlotRectList
    //{
    //    get
    //    {
    //        List<RectTransform> list = new List<RectTransform>();

    //        RecognizerInfo result;

    //        if (CurrentRecognizerInfo == null)
    //        {
    //            result = SlotSegmentTableList.FirstOrDefault(kv => kv.RecognizerType == RecognizerType.None);
    //        }
    //        else
    //        {
    //            result = SlotSegmentTableList.FirstOrDefault(kv => (int)kv.Value == (int)CurrentRecognizerInfo.Value);
    //        }

    //        for (int i = 0; i < result.Flags.Length; i++)
    //        {
    //            if (result.Flags[i])
    //                list.Add(segmentSlots[i].GetComponent<RectTransform>());
    //        }
    //        return list;
    //    }
    //}

    public override void ShowSlots()
    {
        RecognizerInfo result;
        if (CurrentRecognizerInfo == null)
        {
            result = SlotSegmentTableList.FirstOrDefault(kv => kv.RecognizerType == RecognizerType.None);
        }
        else
        {
            result = SlotSegmentTableList.FirstOrDefault(kv => (int)kv.Value == (int)CurrentRecognizerInfo.Value);
        }

        for (int i = 0; i < result.Flags.Length; i++)
        {           
            segmentSlots[i].gameObject.SetActive(result.Flags[i]);
        }
    }
}
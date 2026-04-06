using System.Linq;

public class AddOperatorRecognizer : BaseRecognizer
{
    public override void ShowSlots()
    {
        RecognizerInfo result;
        if (CurrentRecognizerInfo == null)
        {
            result = SlotSegmentTableList.FirstOrDefault(kv => kv.RecognizerType == RecognizerType.None);
        }
        else
        {
            result = SlotSegmentTableList.FirstOrDefault(kv => (OperatorType)kv.Value == (OperatorType)CurrentRecognizerInfo.Value);
        }

        for (int i = 0; i < result.Flags.Length; i++)
        {
            segmentSlots[i].gameObject.SetActive(result.Flags[i]);
        }
    }
}
using UnityEngine;

// ConditionalFieldAttribute 클래스 정의
public class ConditionalFieldAttribute : PropertyAttribute
{
    public string ConditionFieldName { get; private set; }

    public ConditionalFieldAttribute(string conditionFieldName)
    {
        ConditionFieldName = conditionFieldName;
    }
}

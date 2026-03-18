using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

public class GameObjectChangeEditorUtility : Editor
{
    // 상태를 저장하는 static 변수 (초기 상태는 false로 Off)
    public static bool IsOn { get; set; } = false;

    [MenuItem("Tools/TestObject On | Off &f")]  // Alt + f 단축키 설정
    public static void ToggleGameObjectOnOffState()
    {        
        SetGameObjectOnOff(!IsOn);
        IsOn = !IsOn;
    }

    // GameObject On 함수
    public static void SetGameObjectOnOff(bool _isOn)
    {
        ToolUtilGameObject[] objects = Resources.FindObjectsOfTypeAll<ToolUtilGameObject>()
                                                .Where(obj => !EditorUtility.IsPersistent(obj) && !(obj.hideFlags.HasFlag(HideFlags.NotEditable | HideFlags.HideAndDontSave)))
                                                .ToArray();

        foreach (var obj in objects)
        {
            obj.SetGameObject(_isOn);  // GameObject On으로 설정
            Debug.Log($"Set GameObject On for: {obj.name}");
        }
    }
}
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

[InitializeOnLoad]
public static class EditorPlayEndHandler
{
    static EditorPlayEndHandler()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            //Debug.Log("에디터 플레이 모드 종료 시 실행됨");
            //GameObjectChangeEditorUtility.SetGameObjectOnOff(GameObjectChangeEditorUtility.IsOn);
        }
    }
}
#endif

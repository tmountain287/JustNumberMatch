using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(TweenMonitor))]
public class TweenMonitorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TweenMonitor monitor = (TweenMonitor)target;

        if (GUILayout.Button("ScroreBoardRefresh Tween List"))
        {
            monitor.Refresh();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Active Tweens:", EditorStyles.boldLabel);

        foreach (var info in monitor.playingTweenInfos)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"ID: {info.id}", GUILayout.Width(200));
            EditorGUILayout.ObjectField(info.target, typeof(Object), true);
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif

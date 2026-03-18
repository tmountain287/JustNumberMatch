using UnityEngine;
using UnityEditor;

public class SceneResourceFinder : EditorWindow
{
    private Object target;

    [MenuItem("Tools/Find Resource Usage (Accurate)")]
    static void ShowWindow()
    {
        GetWindow<SceneResourceFinder>("Find Resource Usage");
    }

    private void OnGUI()
    {
        target = EditorGUILayout.ObjectField("Target Resource", target, typeof(Object), false);

        if (GUILayout.Button("Find In Scene"))
        {
            if (target == null)
            {
                Debug.LogWarning("Please assign a target resource.");
                return;
            }

            int hitCount = 0;
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (var go in allObjects)
            {
                if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(go))) continue; // Skip Project Assets (prefabs etc.)
                if (EditorUtility.IsPersistent(go)) continue; // Skip non-scene objects
                if (!go.scene.IsValid()) continue; // Not in an active scene

                Component[] components = go.GetComponentsInChildren<Component>(true);
                foreach (var comp in components)
                {
                    if (comp == null) continue;

                    SerializedObject so = new SerializedObject(comp);
                    SerializedProperty prop = so.GetIterator();

                    while (prop.NextVisible(true))
                    {
                        if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                            prop.objectReferenceValue == target)
                        {
                            Debug.Log($"✅ Found on GameObject: {go.name}, Component: {comp.GetType().Name}", go);
                            hitCount++;
                            break; // break component loop, move to next GameObject
                        }
                    }
                }
            }

            Debug.Log($"🔍 Done. Total matching objects: {hitCount}");
        }
    }
}

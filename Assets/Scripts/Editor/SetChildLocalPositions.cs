using UnityEditor;
using UnityEngine;

public class SetChildLocalPositions : EditorWindow
{
    private GameObject selectedObject; // 선택된 오브젝트
    private float offsetX = 1.0f;       // 오프셋 값
    private float offsetY = 1.0f;       // 오프셋 값

    [MenuItem("Tools/Set Child Local Positions")]
    public static void ShowWindow()
    {
        GetWindow<SetChildLocalPositions>("Set Child Local Positions");
    }

    private void OnGUI()
    {
        // 오브젝트 선택 필드
        selectedObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", selectedObject, typeof(GameObject), true);

        // 오프셋 값 입력 필드
        offsetX = EditorGUILayout.FloatField("OffsetX", offsetX);
        offsetY = EditorGUILayout.FloatField("OffsetY", offsetY);

        // 적용 버튼
        if (GUILayout.Button("Set Local Positions"))
        {
            if (selectedObject != null)
            {
                SetChildPositions();
            }
            else
            {
                Debug.LogWarning("No parent object selected!");
            }
        }
    }

    private void SetChildPositions()
    {
        if (selectedObject == null) return;

        // 자식 노드 순회 및 로컬 위치 설정
        Transform parentTransform = selectedObject.transform;
        int childCount = parentTransform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = parentTransform.GetChild(i);
            Vector3 newPosition = new Vector3(offsetX * i, offsetY * i, 0);
            Undo.RecordObject(child, "Set Child Local Position"); // Undo 지원
            child.localPosition = newPosition;
        }

        Debug.Log("Child local positions updated!");
    }
}

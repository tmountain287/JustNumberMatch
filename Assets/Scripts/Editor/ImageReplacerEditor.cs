using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterImage))]
public class ImageReplacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CharacterImage replacer = (CharacterImage)target;

        if (GUILayout.Button("비주얼 교체"))
        {
            replacer.ReplaceVisuals();

            replacer.MatchTransform(); // 여기서 위치/스케일 복사 수행

            RectTransform rectTransform = replacer.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(1080f, 2240f);
                Debug.Log($"[{replacer.name}] RectTransform 사이즈를 1080x2240으로 변경했습니다.");
            }
            else
            {
                Debug.LogWarning($"[{replacer.name}] RectTransform이 존재하지 않습니다.");
            }

            EditorUtility.SetDirty(replacer);
            PrefabUtility.RecordPrefabInstancePropertyModifications(replacer);
            Debug.Log("✅ 비주얼 교체 완료");
        }
    }
}

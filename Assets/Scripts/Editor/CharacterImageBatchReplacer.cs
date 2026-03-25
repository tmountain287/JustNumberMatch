using UnityEditor;
using UnityEngine;

public class CharacterImageBatchReplacer
{
    [MenuItem("Tools/Character/모든 프리팹 비주얼 교체")]
    public static void ReplaceVisualsInAllCharacterPrefabs()
    {
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources/Character" });
        int count = 0;

        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (instance == null) continue;

            CharacterImage characterImage = instance.GetComponentInChildren<CharacterImage>();
            if (characterImage != null)
            {
                characterImage.ReplaceVisuals();
                characterImage.MatchTransform();

                RectTransform rectTransform = characterImage.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = new Vector2(1080f, 2240f);
                    Debug.Log($"[{characterImage.name}] RectTransform 사이즈를 1080x2240으로 변경했습니다.");
                }

                PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.UserAction);
                EditorUtility.SetDirty(characterImage);
                PrefabUtility.RecordPrefabInstancePropertyModifications(characterImage);
                count++;
                Debug.Log($"적용 완료: {path}");
            }
            else
            {
                Debug.LogWarning($"CharacterImage 없음: {path}");
            }

            GameObject.DestroyImmediate(instance);
        }

        Debug.Log($"모든 프리팹 처리 완료. 적용된 프리팹 수: {count}");
    }
}

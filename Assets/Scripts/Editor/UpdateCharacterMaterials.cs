using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class CreateOrUpdateCharacterMaterials : EditorWindow
{
    private static readonly string textureRootPath = "Assets/Resources/Character/Image";
    private static readonly string materialRootPath = "Assets/Resources/Character/Materials/";

    private static readonly Dictionary<string, string> materialTypes = new()
    {
        { "NormalMaterial", "Assets/rawRes/Templates/NormalMaterialTemplate.mat" },
        { "GrayMaterial", "Assets/rawRes/Templates/GrayMaterialTemplate.mat" },
        { "MoreGrayMaterial", "Assets/rawRes/Templates/MoreGrayMaterialTemplate.mat" },
    };

    [MenuItem("Tools/Character/Create Or Update Materials")]
    public static void CreateOrUpdateMaterials()
    {
        string[] textureGUIDs = AssetDatabase.FindAssets("t:Texture2D", new[] { textureRootPath });
        int created = 0, updated = 0;

        foreach (string guid in textureGUIDs)
        {
            string texPath = AssetDatabase.GUIDToAssetPath(guid);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (texture == null) continue;

            string fileName = Path.GetFileNameWithoutExtension(texPath);

            foreach (var pair in materialTypes)
            {
                string typeFolder = pair.Key;
                string templatePath = pair.Value;

                string matFolder = Path.Combine(materialRootPath, typeFolder);
                string matPath = Path.Combine(matFolder, fileName + ".mat");

                Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat == null)
                {
                    // 템플릿 복사
                    Material template = AssetDatabase.LoadAssetAtPath<Material>(templatePath);
                    if (template == null)
                    {
                        Debug.LogError($"❌ 템플릿 없음: {templatePath}");
                        continue;
                    }

                    mat = Object.Instantiate(template);
                    if (!Directory.Exists(matFolder)) Directory.CreateDirectory(matFolder);
                    AssetDatabase.CreateAsset(mat, matPath);
                    created++;
                    Debug.Log($"[생성] {matPath}");
                }

                if (mat.HasProperty("_MainTex"))
                {
                    mat.SetTexture("_MainTex", texture);
                    EditorUtility.SetDirty(mat);
                    updated++;
                }
                else
                {
                    Debug.LogWarning($"[스킵] '{mat.name}'에는 _MainTex가 없습니다.");
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ 생성된 머티리얼: {created}, 텍스처 적용된 머티리얼: {updated}");
    }
}

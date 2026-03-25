// CharacterResManager.cs
using Common.Manager;
using Common.UI;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


[System.Serializable]
public class CharacterVisualInfo
{
    public Vector3 position;
    public Vector3 scale;
    
}

[System.Serializable]
public class CharacterVisualData
{
    public Sprite sprite;
    public List<Material> materialList;
    public Dictionary<CharacterImage.Type, CharacterVisualInfo> typeData = new();
}

public class CharacterResManager : MonoSingletonDont<CharacterResManager>
{
    private Dictionary<string, CharacterVisualData> characterDataDict = new();

    public UnityEvent<float> OnProgressChanged = new(); // UI에서 연결 가능

    public IEnumerator LoadAllCharacterVisualsAsync(Action onComplete = null)
    {
        characterDataDict.Clear();

        List<CharacterTableData> characterList = TableDataManager.Instance.TableCharacterData.CharacterTableDataList;
        int total = characterList.Count;

        for (int i = 0; i < total; i++)
        {
            string prefabName = characterList[i].resource;

            if (characterDataDict.TryGetValue(prefabName, out var cached))
                continue;

            ResourceRequest request = Resources.LoadAsync<GameObject>($"Character/{prefabName}");
            yield return request;

            GameObject prefab = request.asset as GameObject;
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab not found: {prefabName}");
                continue;
            }

            CharacterImage ci = prefab.GetComponentInChildren<CharacterImage>();
            if (ci == null) continue;

            var visualData = new CharacterVisualData
            {
                sprite = ci.sprite,
                materialList = ci.materialList
            };

            foreach (CharacterImage.Type type in Enum.GetValues(typeof(CharacterImage.Type)))
            {
                Graphic graphic = ci.GetGraphicByType(type);
                if (graphic == null) continue;

                var info = new CharacterVisualInfo
                {
                    position = graphic.rectTransform.localPosition,
                    scale = graphic.rectTransform.localScale
                };

                visualData.typeData[type] = info;
            }

            characterDataDict[prefab.name] = visualData;

            float progress = (i + 1f) / total;
            OnProgressChanged?.Invoke(progress);

            yield return null; // 프레임 분산
        }

        onComplete?.Invoke();
        Debug.Log($"Loaded {characterDataDict.Count} character visuals.");
    }


    public CharacterVisualData GetCharacterData(string prefabName)
    {
        if (characterDataDict.TryGetValue(prefabName, out var cached))
            return cached;

        GameObject prefab = Resources.Load<GameObject>($"Character/{prefabName}");
        if (prefab == null)
        {
            Debug.LogError($"Character prefab not found: {prefabName}");
            return null;
        }

        CharacterImage ci = prefab.GetComponentInChildren<CharacterImage>();
        if (ci == null)
        {
            Debug.LogError($"CharacterImage component not found on: {prefabName}");
            return null;
        }

        var visualData = new CharacterVisualData();
        visualData.sprite = ci.sprite;
        visualData.materialList = ci.materialList;

        foreach (CharacterImage.Type type in Enum.GetValues(typeof(CharacterImage.Type)))
        {
            Graphic graphic = ci.GetGraphicByType(type);
            if (graphic == null) continue;

            var info = new CharacterVisualInfo
            {
                position = graphic.rectTransform.localPosition,
                scale = graphic.rectTransform.localScale
            };

            visualData.typeData[type] = info;
        }

        characterDataDict[prefabName] = visualData;
        return visualData;
    }

    public CharacterVisualInfo GetCharacterVisualInfo(string prefabName, CharacterImage.Type type)
    {
        CharacterVisualData visualData = GetCharacterData(prefabName);

        if (visualData == null)
        {
            Debug.LogError($"{prefabName}이 없습니다");
            return null;
        }

        visualData.typeData.TryGetValue(type, out var data);
        return data;
    }

    public void SetImage(Graphic graphic, string prefabName, CharacterImage.Type type, CharacterImage.GrayType  grayType = CharacterImage.GrayType.None)
    {
        graphic.gameObject.SetActive(false);
        CharacterVisualData visualData = GetCharacterData(prefabName);

        if (visualData == null) 
        {
            Debug.LogError($"{prefabName}이 없습니다");
            return;
        }

        visualData.typeData.TryGetValue(type, out var info);

        if (info != null)
        {
            if (graphic is Image image && visualData.sprite != null)
            {
                image.sprite = visualData.sprite;
                image.SetNativeSize();
            }
            else if (graphic is RawImage raw && visualData.materialList[(int)grayType])
            {
                raw.material = visualData.materialList[(int)grayType];               
            }

            graphic.rectTransform.localScale = info.scale;
            graphic.rectTransform.localPosition = info.position;
        }
        else
        {
            Debug.LogError($"{prefabName}이 없습니다");
        }
        graphic.gameObject.SetActive(true);
    }
}
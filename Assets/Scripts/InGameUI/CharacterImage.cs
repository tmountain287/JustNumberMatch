using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterImage : MonoBehaviour
{
    public enum Type
    {
        Profile = 0,
        LevelUp = 1,
        Fire_Left = 2,
        Fire_Right = 3,
        NewCharacter = 4,
        Effect = 5,
        Result = 6,
        FullShot = 7,
        LobbyUI = 8,
        MyCharacter = 9,
        Collection = 10,
        CollectionFullshot = 11,
    }

    public enum GrayType
    {
        None = 0,
        Gray = 1,
        MoreGray = 2,
    }

    [SerializeField] private List<Graphic> graphicList = null;
    [SerializeField] private string fileName = "";

    public Sprite sprite = null;
    public List<Material> materialList = new();

    public Transform referenceTransform; // A 트랜스폼
    public Transform targetTransform;    // B 트랜스폼


    public string ResourcePathSprite => $"Character/Image/{fileName}";
    public List<string> ResourcePathMaterial => new List<string>
    {
        $"Character/Materials/NormalMaterial/{fileName}",
        $"Character/Materials/GrayMaterial/{fileName}",
        $"Character/Materials/MoreGrayMaterial/{fileName}"
    };

    private void OnValidate()
    {
        if(transform.parent == null)
        {
            return;
        }

        string a = transform.parent.name;

        string digits = System.Text.RegularExpressions.Regex.Match(a, @"\d+").Value;

        // 앞의 0 제거
        string result = int.Parse(digits).ToString(); // "1"

        fileName = result;
    }

    public void ReplaceVisuals()
    {
        if (fileName == "-1")
            return;

        materialList.Clear();

        for (int i=0;i< ResourcePathMaterial.Count;i++)
        {
            Material mat = Resources.Load<Material>(ResourcePathMaterial[i]);
            if (mat == null)
            {
                Debug.LogWarning($"Material 리소스를 찾을 수 없습니다: {ResourcePathMaterial[i]}");
                continue;
            }
            materialList.Add(mat);
        }

        sprite = Resources.Load<Sprite>(ResourcePathSprite);

        foreach (var g in graphicList)
        {
            if (g is Image image)
            {
                if (sprite != null)
                    image.sprite = sprite;
                else
                    Debug.LogWarning($"Sprite 리소스를 찾을 수 없습니다: {ResourcePathSprite}");
            }
            else if (g is RawImage raw)
            {
                if (materialList.Count > 0)
                    raw.material = materialList[0];
            }
        }
    }

    public void MatchTransform()
    {
        if (referenceTransform != null && targetTransform != null)
        {
            targetTransform.localPosition = referenceTransform.localPosition;
            targetTransform.localScale = referenceTransform.localScale;
            Debug.Log($"[{name}] targetTransform을 referenceTransform과 일치시켰습니다.");
        }
        else
        {
            Debug.LogWarning($"[{name}] referenceTransform 또는 targetTransform이 null입니다.");
        }
    }

    public Graphic On(Type _type)
    {
        Graphic target = null;
        for (int i = 0; i < graphicList.Count; i++)
        {
            bool active = i == (int)_type;
            graphicList[i].gameObject.SetActive(active);
            if (active)
                target = graphicList[i];
        }
        return target;
    }

    public Graphic GetGraphicByType(Type type)
    {
        int index = (int)type;
        if (index >= 0 && index < graphicList.Count)
            return graphicList[index];
        return null;
    }
}
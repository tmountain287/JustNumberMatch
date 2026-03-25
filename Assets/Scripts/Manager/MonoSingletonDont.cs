using UnityEngine;

public class MonoSingletonDont<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();

                if (instance == null)
                {
                    // Resources 폴더에서 prefab 로드
                    GameObject prefab = Resources.Load<GameObject>(typeof(T).Name);
                    if (prefab != null)
                    {
                        GameObject obj = Instantiate(prefab);
                        instance = obj.GetComponent<T>();
                        obj.name = typeof(T).Name;
                        DontDestroyOnLoad(obj);
                    }
                    else
                    {
                        // fallback: 빈 오브젝트라도 생성
                        GameObject obj = new GameObject(typeof(T).Name);
                        instance = obj.AddComponent<T>();
                        DontDestroyOnLoad(obj);
                    }
                }
            }
            return instance;
        }
    }

    public static T GetInstance()
    {
        return instance;
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

#if UNITY_EDITOR
    private void OnApplicationQuit()
    {
        DestroyImmediate(gameObject);
    }
#endif
}

using UnityEngine;

public class MonoSingletonDont<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static bool isQuitting;

    public static T Instance
    {
        get
        {
            if (isQuitting) return null;

            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();

                if (instance == null)
                {
                    GameObject prefab = Resources.Load<GameObject>(typeof(T).Name);
                    GameObject obj;

                    if (prefab != null)
                    {
                        obj = Object.Instantiate(prefab);
                        obj.name = typeof(T).Name;
                        instance = obj.GetComponent<T>();
                    }
                    else
                    {
                        obj = new GameObject(typeof(T).Name);
                        instance = obj.AddComponent<T>();
                    }

                    Object.DontDestroyOnLoad(obj);
                }
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (isQuitting)
        {
            Destroy(gameObject);
            return;
        }

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

    protected virtual void OnApplicationQuit()
    {
        isQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    // ✅ attribute 제거. 대신 외부에서 호출 가능하게만 해둠.
    internal static void ResetStaticsForDomainReloadDisabled()
    {
        instance = null;
        isQuitting = false;
    }
}

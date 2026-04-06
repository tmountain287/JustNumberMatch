using Common.Manager;
using UnityEngine;

public abstract class PlayerPrefsChangeEvent : MonoBehaviour
{
    [SerializeField] private PrefsKey prefsKey;

    private void OnEnable()
    {
        SetChangeEvent();
        if (PlayerPrefsManager.Instance != null)
            PlayerPrefsManager.Instance.AddChangeEvent(prefsKey, SetChangeEvent);
    }

    protected virtual void OnDisable()
    {
        if (PlayerPrefsManager.Instance != null)
            PlayerPrefsManager.Instance.RemoveChangeEvent(prefsKey, SetChangeEvent);
    }
    
    protected abstract void SetChangeEvent();
}

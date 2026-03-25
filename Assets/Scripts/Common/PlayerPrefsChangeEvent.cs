using Common.Manager;
using UnityEngine;

public abstract class PlayerPrefsChangeEvent : MonoBehaviour
{
    [SerializeField] private PrefsKey prefsKey;

    private void OnEnable()
    {
        SetChangeEvent();
        PlayerPrefsManager.Instance.AddChangeEvent(prefsKey, SetChangeEvent);
    }

    protected virtual void OnDisable()
    {
        if (PlayerPrefsManager.GetInstance() != null)
            PlayerPrefsManager.Instance.RemoveChangeEvent(prefsKey, SetChangeEvent);
    }
    
    protected abstract void SetChangeEvent();
}

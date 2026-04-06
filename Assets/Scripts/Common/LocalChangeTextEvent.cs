using Common.Manager;
using UnityEngine;
using UnityEngine.UI;

public class LocalChangeTextEvent : PlayerPrefsChangeEvent
{
    [SerializeField] private LocalUIType localUIType = LocalUIType.Normal;
    [SerializeField] public string entryKey = "";
    [SerializeField] private Text text = null;

    private object[] args = null;

    public string EntryKey
    {
        get => entryKey;
        set
        {
            entryKey = value;
            SetChangeEvent();
        }
    }

    private void OnValidate()
    {
        if (text == null)
        {
            text = GetComponent<Text>();
        }
    }

    protected override void SetChangeEvent()
    {
        if (text == null)
            text = GetComponent<Text>();
        if (text == null || LocalizationManager.Instance == null)
            return;

        text.text = LocalizationManager.Instance.GetText(entryKey, localUIType);
    }

    public void SetText(string key, params object[] args)
    {
        if (text == null)
            text = GetComponent<Text>();
        if (text == null || LocalizationManager.Instance == null)
            return;

        string formatted = LocalizationManager.Instance.GetText(key, localUIType);

        if (args != null && args.Length > 0)
            text.text = string.Format(formatted, args);
        else
            text.text = formatted;
    }
}
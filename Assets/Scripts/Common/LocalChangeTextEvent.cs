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
        text.text = LocalizationManager.Instance.GetText(entryKey, localUIType);
    }

    public void SetText(string key, params object[] args)
    {
        // key로 바로 가져오기
        string formatted = LocalizationManager.Instance.GetText(key, localUIType);

        if (args != null && args.Length > 0)
            text.text = string.Format(formatted, args);
        else
            text.text = formatted;
    }
}
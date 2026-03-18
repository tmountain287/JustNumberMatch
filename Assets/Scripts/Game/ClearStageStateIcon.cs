using UnityEngine;
using UnityEngine.UI;

public class ClearStageStateIcon : MonoBehaviour
{
    [SerializeField] private Image img = null;
    [SerializeField] private Sprite on = null;
    [SerializeField] private Sprite off = null;

    [SerializeField] private Text text = null;

    private void OnDisable()
    {
        gameObject.SetActive(false);
    }

    public void SetIcon(int _index)
    {
        text.text = (_index + 1).ToString();
        SetOn(false);
        gameObject.SetActive(true);
    }

    public void SetOn(bool _flag)
    {
        img.sprite = _flag ? on : off;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class DebugUIButton : MonoBehaviour
{
    [SerializeField] private Button button = null;
    
    void Start()
    {
        button.enabled = false;
#if TEST && !SERVICE
        button.enabled = true;
        button.onClick.AddListener(() =>
        {
            UIManager.Instance.OnUI(Common.UI.BaseUI.Type.DEBUG, true);
        });
#endif
    }
}

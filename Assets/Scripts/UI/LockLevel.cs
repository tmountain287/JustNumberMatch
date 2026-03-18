using UnityEngine;
using UnityEngine.UI;

public class LockLevel : MonoBehaviour
{
    [SerializeField] private int index = 0;
    [SerializeField] private Text levelText = null;
    [SerializeField] private GameObject lockObj = null;
    [SerializeField] private Button button = null;
 
    private void OnEnable()
    {
        UserDataManager.OnValueLevelChanged += Set;
        Set();
    }

    private void OnDisable()
    {
        UserDataManager.OnValueLevelChanged -= Set;
    }

    private void Set()
    {
        int needLevel = ConfigData.UnlockModeLevelList[index];
        levelText.text = needLevel.ToString();
        lockObj.SetActive(UserDataManager.Level < needLevel);

        if(button != null) button.enabled = UserDataManager.Level >= needLevel;
    }
}

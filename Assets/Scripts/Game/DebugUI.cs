using Common.UI;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : BaseUI
{
    [SerializeField] private Button closeButton = null;
    [SerializeField] private Button levelUpButton = null;
    [SerializeField] private Button easyStageButton = null;

    void Start()
    {
        closeButton.onClick.AddListener(() =>
        {
            UIManager.Instance.OnUI(Common.UI.BaseUI.Type.DEBUG, false);
        });

        levelUpButton.onClick.AddListener(() =>
        {
            UserDataManager.Level += 100;
            UserDataManager.Save();
        });

        easyStageButton.onClick.AddListener(() =>
        {
            UserDataManager.UserData.clearStageInfoDic[DifficultyType.Easy] = 20;
            UserDataManager.Save();
        });
    }   
}

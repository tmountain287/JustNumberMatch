using Common.Manager;
using Common.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : BaseUI
{
    [SerializeField] private Button closeButton = null;
    [SerializeField] private Button levelUpButton = null;

    [Header("스테이지 클리어 강제 (표시 스테이지 번호 stage 기준)")]
    [SerializeField] private Dropdown difficultyDropdown = null;
    [SerializeField] private InputField stageNumberInput = null;
    [SerializeField] private Toggle bossRedStarRandomToggle = null;
    [SerializeField] private Button applyClearStageButton = null;

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

        if (difficultyDropdown != null)
        {
            difficultyDropdown.ClearOptions();
            difficultyDropdown.AddOptions(new List<string> { "Easy", "Normal", "Hard" });
        }

        if (bossRedStarRandomToggle != null)
            bossRedStarRandomToggle.isOn = true;       
    }

    /// <summary>
    /// 선택한 난이도에서, 입력한 표시 스테이지 번호(stage)까지 클리어한 것처럼 clearStageInfoDic을 덮어씁니다.
    /// 0 이하이면 해당 난이도 진행도만 초기화합니다.
    /// </summary>
  
}

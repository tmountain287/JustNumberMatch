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

        if (applyClearStageButton != null)
            applyClearStageButton.onClick.AddListener(ApplyForcedStageClear);
    }

    /// <summary>
    /// 선택한 난이도에서, 입력한 표시 스테이지 번호(stage)까지 클리어한 것처럼 clearStageInfoDic을 덮어씁니다.
    /// 0 이하이면 해당 난이도 진행도만 초기화합니다.
    /// </summary>
    void ApplyForcedStageClear()
    {
        if (TableDataManager.Instance?.TableStageData?.StageTableDataDic == null || UserDataManager.UserData == null)
        {
            Debug.LogWarning("[DebugUI] TableDataManager 또는 UserData가 없습니다.");
            return;
        }

        if (difficultyDropdown == null || stageNumberInput == null)
        {
            Debug.LogWarning("[DebugUI] difficultyDropdown 또는 stageNumberInput이 연결되지 않았습니다.");
            return;
        }

        var diff = (DifficultyType)difficultyDropdown.value;
        if (!UserDataManager.UserData.clearStageInfoDic.ContainsKey(diff))
            return;

        if (!int.TryParse(stageNumberInput.text.Trim(), out int stageNum))
        {
            Debug.LogWarning("[DebugUI] 스테이지 숫자를 정수로 입력하세요.");
            return;
        }

        var list = TableDataManager.Instance.TableStageData.StageTableDataDic[diff];
        if (list == null || list.Count == 0)
            return;

        if (stageNum <= 0)
        {
            UserDataManager.UserData.clearStageInfoDic[diff] = 0;
            ClearBossStarInfosForDifficulty(diff);
            UserDataManager.Save();
            Debug.Log($"[DebugUI] {diff} 클리어 진행 초기화 (0)");
            return;
        }

        StageTableData last = list.Where(x => x.stage <= stageNum).OrderByDescending(x => x.id).FirstOrDefault();
        if (last == null)
        {
            UserDataManager.UserData.clearStageInfoDic[diff] = 0;
            ClearBossStarInfosForDifficulty(diff);
            UserDataManager.Save();
            Debug.LogWarning($"[DebugUI] stage<={stageNum} 에 해당하는 스테이지가 없어 진행도를 0으로 맞췄습니다.");
            return;
        }

        int previousClearId = UserDataManager.UserData.clearStageInfoDic[diff];
        UserDataManager.UserData.clearStageInfoDic[diff] = last.id;

        bool randomBossStars = bossRedStarRandomToggle == null || bossRedStarRandomToggle.isOn;
        UserDataManager.ApplyDebugForcedStageProgress(diff, last.id, previousClearId, randomBossStars);

        UserDataManager.Save();
        Debug.Log($"[DebugUI] {diff} 강제 클리어: id={last.id}, 보스별 {(randomBossStars ? "랜덤 1~3" : "3 고정")} 레드스타, 신규 구간 XP·레벨업 보상 반영");
    }

    static void ClearBossStarInfosForDifficulty(DifficultyType diff)
    {
        if (UserDataManager.UserData.bossStarInfoDic != null &&
            UserDataManager.UserData.bossStarInfoDic.TryGetValue(diff, out var bossList) && bossList != null)
            bossList.Clear();
    }
}

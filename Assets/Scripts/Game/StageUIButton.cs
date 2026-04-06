using Common.Manager;
using JustOneMatch.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StageUIButton : MonoBehaviour
{
    [SerializeField] private DifficultyType difficultyType;
    [SerializeField] private Button button = null;

    [SerializeField] private Slider slider = null;
    [SerializeField] private Text sliderValueText = null;

    [SerializeField] private Text lockText = null;
    [SerializeField] private GameObject lockObj = null;

    void Start()
    {
        button.onClick.AddListener(() =>
        {
            PopupManager.Instance.OpenPopup<StagePopup>().Initialize(difficultyType);
        });
    }

    private void OnEnable()
    {
        RefreshRedStarSlider();
        UserDataManager.OnMissionDataChanged += RefreshRedStarSlider;
    }

    private void OnDisable()
    {
        UserDataManager.OnMissionDataChanged -= RefreshRedStarSlider;
    }

    /// <summary>
    /// 이 난이도 스테이지 테이블의 보스전에서 얻을 수 있는 레드스타 총합(각 보스 starMax 합) 대비,
    /// 실제로 해당 보스들에서 획득한 레드스타 합계.
    /// </summary>
    void RefreshRedStarSlider()
    {
        if (slider == null || sliderValueText == null)
            return;
        if (TableDataManager.Instance?.TableStageData?.StageTableDataDic == null || UserDataManager.UserData == null)
            return;

        if (!TableDataManager.Instance.TableStageData.StageTableDataDic.TryGetValue(difficultyType, out List<StageTableData> stageList) || stageList == null)
        {
            slider.maxValue = 1f;
            slider.value = 0f;
            sliderValueText.text = "-";
            return;
        }

        int maxStars = GetMaxRedStarsFromBossStages(stageList);
        int earned = GetEarnedRedStarsFromBossStages(stageList);

        if (maxStars <= 0)
        {
            slider.maxValue = 1f;
            slider.value = 0f;
            sliderValueText.text = "-";
            return;
        }

        slider.maxValue = maxStars;
        slider.value = Mathf.Clamp(earned, 0, maxStars);
        sliderValueText.text = $"{earned}/{maxStars}";
    }

    static int GetMaxRedStarsFromBossStages(List<StageTableData> stageList)
    {
        int sum = 0;
        for (int i = 0; i < stageList.Count; i++)
        {
            StageTableData s = stageList[i];
            if (s.stageType == StageType.Boss)
                sum += s.starMax;
        }
        return sum;
    }

    int GetEarnedRedStarsFromBossStages(List<StageTableData> stageList)
    {
        var ud = UserDataManager.UserData;
        var bossInfos = ud.bossStarInfoDic[difficultyType];
        int clearedId = ud.clearStageInfoDic[difficultyType];

        int sum = 0;
        for (int i = 0; i < stageList.Count; i++)
        {
            StageTableData s = stageList[i];
            if (s.stageType != StageType.Boss)
                continue;

            BossStarInfo info = bossInfos.FirstOrDefault(x => x.stageID == s.id);
            if (info != null)
                sum += info.starCount;
            else if (clearedId >= s.id)
                sum += s.starMax;
        }
        return sum;
    }
}

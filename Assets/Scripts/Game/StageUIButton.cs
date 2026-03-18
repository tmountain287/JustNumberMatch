using Common.Manager;
using JustOneMatch.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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

 
    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(() =>
        {
            PopupManager.Instance.OpenPopup<StagePopup>().Initialize(difficultyType);
        });
    }

    private void OnEnable()
    {
        int maxStage = TableDataManager.Instance.TableStageData.StageTableDataDic[difficultyType].Count;

        int clearStage = UserDataManager.UserData.clearStageInfoDic[difficultyType];
        
        sliderValueText.text = $"{clearStage}/{maxStage}";
        slider.value = (float)clearStage / maxStage;
    }
}

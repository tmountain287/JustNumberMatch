using UI.Popup;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MissionPopupItem : MonoBehaviour
{
    [SerializeField] private List<Color> colors = null;
    [SerializeField] private Image backImage = null;

    [SerializeField] private Transform iconTran = null;
    [SerializeField] private LocalChangeTextEvent title = null;
    [SerializeField] private LocalChangeTextEvent subTitle = null;

    [SerializeField] private Slider slider = null;
    [SerializeField] private Text progressText = null;
    [SerializeField] private RewardItem rewardItem = null;

    [SerializeField] private Button carlmButton = null;

    [SerializeField] private GameObject clearReward = null;

    private MissionData missionData = null;    

    private void Start()
    {
        carlmButton.onClick.AddListener(() =>
        {
          //  UIManager.Instance.TopUI.SetBlockAutoUpdate(true);

            int count = Mathf.Min(missionData.rewardValue, 15);

            //UIManager.Instance.OnEffect(missionData.rewardItemType, count, rewardItem.transform.position, UIManager.Instance.TopUI.GetIcon(missionData.rewardItemType), ()=>
            //{
            //    UIManager.Instance.TopUI.SetBlockAutoUpdate(false);
            //});

            MissionData m = UserDataManager.MissionRewared(missionData.id);
            GameAnalyticsHelper.LogMissionComplete(missionData.id, missionData.rewardItemType.ToString());
            UserDataManager.Save();

            if (m != null) missionData = m;
            SetItem(missionData);            
        });
    }

    public void SetItem(MissionData _missionData)
    {
        missionData = _missionData;

        clearReward.gameObject.SetActive(false);

        bool isComplete = false;
        bool isReward = false;
        long count = 0;

        if (missionData.category == MissionCategory.Daily)
        {
            DailyMissionData data = UserDataManager.UserData.dailyMissionDataDic[missionData.type];

            count = data.playCount;
            isComplete = data.IsComplete;
            isReward = data.isReward;

            clearReward.SetActive(isComplete &&  isReward);
        }
        else
        {
            // 보상 수령 직후 슬롯 id가 다음 미션으로 바뀌므로, id가 아니라 (type, difficultyType) 그룹으로 슬롯을 찾음
            MainMissionData mainMissionData = UserDataManager.UserData.currentMissionList.FirstOrDefault(x =>
            {
                MissionData slotMission = TableDataManager.Instance.TableMissionData.GetData(x.id);
                return slotMission != null && slotMission.type == missionData.type && slotMission.difficultyType == missionData.difficultyType;
            });
            if (mainMissionData != null)
            {
                count = mainMissionData.Count;
                isComplete = mainMissionData.IsComplete;
                isReward = mainMissionData.isReward;
            }
        }

        carlmButton.gameObject.SetActive(isComplete && !isReward);

        backImage.color = isComplete ? colors[1] : colors[0];

        slider.gameObject.SetActive(!isComplete);

        if(missionData.type == MissionType.TimeAttack)
        {
            slider.gameObject.SetActive(false);

            subTitle.SetText(missionData.subLocalId, missionData.value.FormatFromMs_HMS());
        }
        else 
        {
            if (!isComplete)
            {
                slider.maxValue = missionData.value;
                slider.value = count;
                progressText.text = $"{count}/{missionData.value}";
                slider.gameObject.SetActive(true);
            }

            subTitle.SetText(missionData.subLocalId, missionData.value);
        }

        Transform tran = null;

        for (int i = 0; i < iconTran.childCount; i++)
        {
            if ((int)missionData.type == i)
            {
                tran = iconTran.GetChild(i);
                tran.gameObject.SetActive(true);
            }
            else
            {
                iconTran.GetChild(i).gameObject.SetActive(false);
            }
        }

        if (missionData.difficultyType > -1)
        {
            for (int j = 0; j < tran.childCount; j++)
            {
                tran.GetChild(j).gameObject.SetActive(j == missionData.difficultyType);
            }
        }

        title.EntryKey = missionData.titleLocalId;

        rewardItem.SetRewardItem(missionData.rewardItemType, missionData.rewardValue, isComplete);
    }
}
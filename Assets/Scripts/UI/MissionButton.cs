using Common.Manager;
using JustOneMatch.UI;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MissionButton : MonoBehaviour
{
    [SerializeField] private Button button = null;
    [SerializeField] private GameObject countObj = null;
    [SerializeField] private Text countText = null;
 
    private void Start()
    {      
        button.onClick.AddListener(() =>
        {
            int mainCount = UserDataManager.UserData.currentMissionList.Count(x => x.IsComplete && !x.isReward);

            int dailyCount = 0;
            foreach (var item in UserDataManager.UserData.dailyMissionDataDic)
            {
                if (item.Value.IsComplete && !item.Value.isReward)
                    dailyCount++;
            }

            PopupManager.Instance.OpenPopup<MissionPopup>().Initialize(dailyCount == 0 && mainCount > 0 ? MissionCategory.Main : MissionCategory.Daily);
        });
    }

    private void OnEnable()
    {
        SetCount();
        UserDataManager.OnMissionDataChanged += SetCount;
    }

    private void OnDisable()
    {
        UserDataManager.OnMissionDataChanged -= SetCount;
    }

    private void SetCount()
    {
        int count = UserDataManager.UserData.currentMissionList.Count(x => x.IsComplete && !x.isReward);
        foreach (var item in UserDataManager.UserData.dailyMissionDataDic)
        {
            if (item.Value.IsComplete && !item.Value.isReward)
                count++;
        }

        if (count > 0)
        {
            countText.text = count.ToString();
            countObj.SetActive(true);
        }
        else
        {
            countObj.SetActive(false);
        }
    }
}

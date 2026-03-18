using Common.Manager;
using JustOneMatch.UI;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MissionDot : MonoBehaviour
{
    [SerializeField] private MissionCategory category = MissionCategory.Daily;
    [SerializeField] private GameObject countObj = null;
    [SerializeField] private Text countText = null;
 
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
        // 테이블에서 이 dot의 category와 같은 미션만 가져와서 완료+미보상만 카운팅
        var dataList = TableDataManager.Instance.TableMissionData.GetDataList(category);
        int count = 0;

        foreach (var missionData in dataList)
        {
            if (category == MissionCategory.Daily)
            {
                if (UserDataManager.UserData.dailyMissionDataDic.TryGetValue(missionData.type, out var dailyData)
                    && dailyData.IsComplete && !dailyData.isReward)
                    count++;
            }
            else // Main
            {
                var mainData = UserDataManager.UserData.currentMissionList.FirstOrDefault(x => x.id == missionData.id);
                if (mainData != null && mainData.IsComplete && !mainData.isReward)
                    count++;
            }
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

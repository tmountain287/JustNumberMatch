using Common.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardItemList : MonoBehaviour
{    
    [SerializeField] private RewardItem rewardItem = null;
    [SerializeField] private GameObject noRewarObj = null;
    [SerializeField] private AudioClip popAudioClip = null;

    private List<RewardItem> rewardItemList = new();
        
    public void SetRewardItemList(Dictionary<ItemType, int> _rewardItemDic)
    {
        if(_rewardItemDic != null && _rewardItemDic.Count > 0)
        {
            int index = 0;

            foreach (var kvp in _rewardItemDic)
            {
                // 필요한 만큼 rewardItem 생성
                if (rewardItemList.Count <= index)
                {
                    RewardItem newItem = Instantiate(rewardItem, rewardItem.transform.parent);
                    newItem.transform.SetParent(transform);
                    rewardItemList.Add(newItem);
                }

                // 데이터 세팅
                rewardItemList[index].gameObject.SetActive(true);
                rewardItemList[index].SetRewardItem(kvp.Key, kvp.Value);

                index++;
            }

            // 남는 RewardItem은 비활성화 처리
            for (int i = index; i < rewardItemList.Count; i++)
            {
                rewardItemList[i].gameObject.SetActive(false);
            }

            noRewarObj.SetActive(false);
        }
        else
        {
            for (int i = 0; i < rewardItemList.Count; i++)
            {
                rewardItemList[i].gameObject.SetActive(false);
            }

            noRewarObj.SetActive(true);
        }

        //gameObject.SetActive(true);
    }

    /// <param name="_multiplier">배수 (예: 2 = 2배, 5 = 5배)</param>
    public void PlayMultiple(int _multiplier = 2)
    {
        SoundManager.Instance.PlayFX(popAudioClip, 0.1f);
        for (int i = 0; i < rewardItemList.Count; i++)
        {
            if (rewardItemList[i].gameObject.activeSelf)
            {
                rewardItemList[i].Play(_multiplier);
            }
        }
    }
}
using Common.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class StageButtonItemSlot
{
    public GameObject root;    
    public GameObject line;
    public GameObject check;
}


public class StageButton : MonoBehaviour
{
    [SerializeField] private List<GameObject> bgList = null;
    [SerializeField] private List<GameObject> focusList = null;

    [SerializeField] private Button button = null;

    [SerializeField] private Text text = null;
    [SerializeField] private Text disableText = null;
    [SerializeField] private GameObject lockObj = null;

    [SerializeField] private List<StageButtonItemSlot> itemSlotList = null;

    [SerializeField] private GameObject leftLine = null;
    [SerializeField] private GameObject rightLine = null;

    [SerializeField] private GameObject bossObj = null;
    [SerializeField] private GameObject bossDisableObj = null;
    [SerializeField] private GameObject disable = null;

    [SerializeField] private GameObject starObj = null;
    [SerializeField] private List<GameObject> starList = null;
    [SerializeField] private List<GameObject> starObjList = null;

    [SerializeField] private GameObject bossStarObj = null;
    [SerializeField] private List<GameObject> boosStarList = null;

    [SerializeField] private List<GameObject> checkObj = null;

    private Action onClick = null;

    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(() =>
        {
            onClick?.Invoke();
        });
    }

    public void SetItemData(DifficultyType _difficultyType, StageTableData _data, Action _onClick)
    {
        int stageID = UserDataManager.UserData.clearStageInfoDic[_difficultyType];

        for (int i = 0; i < bgList.Count; i++)
        {
            bgList[i].gameObject.SetActive(i == (int)_difficultyType);
        }

        for (int i = 0; i < focusList.Count; i++)
        {
            focusList[i].gameObject.SetActive(i == (int)_difficultyType && stageID + 1 == _data.id);
        }

        if (_data.isLock)
        {
            starObj.SetActive(false);
            bossStarObj.SetActive(false);
            disable.SetActive(true);
            lockObj.SetActive(true);

            bossDisableObj.SetActive(false);
            bossObj.SetActive(false);
            text.gameObject.SetActive(false);
            disableText.gameObject.SetActive(false);

            leftLine.SetActive(true);
            rightLine.SetActive(false);

            itemSlotList.ForEach(x =>
            {
                x.root.SetActive(false);
                x.line.SetActive(false);
                x.check.SetActive(false);
            });
            return;
        }

        lockObj.SetActive(false);

        starObj.SetActive(_data.stageType == StageType.Normal);
        bossStarObj.SetActive(_data.stageType == StageType.Boss);

        if (_data.stageType == StageType.Normal)
        {
            starObjList.ForEach(x => x.SetActive(false));

            for (int i = 0; i < _data.starMax; i++)
            {
                starObjList[i].SetActive(true);
                starList[i].SetActive(_data.id <= stageID);
            }

            button.enabled = _data.id <= stageID + 1;
        }
        else
        {
            boosStarList.ForEach(x => x.SetActive(false));

            if (_data.id <= stageID)
            {
                BossStarInfo bossStarInfo = UserDataManager.UserData.bossStarInfoDic[_data.difficultyType].FirstOrDefault(x => x.stageID == _data.id);

                int sm = bossStarInfo == null ? _data.starMax : bossStarInfo.starCount;

                for (int i = 0; i < sm; i++)
                {
                    boosStarList[i].SetActive(true);
                }

                button.enabled = bossStarInfo != null;
            }
            else
            {
                button.enabled = _data.id == stageID + 1;
            }
        }

        disable.SetActive(_data.id > stageID + 1);

        onClick = _onClick;
        text.text = _data.stage.ToString();
        disableText.text = _data.stage.ToString();

        bossDisableObj.SetActive(_data.stageType == StageType.Boss);
        bossObj.SetActive(_data.stageType == StageType.Boss);
        text.gameObject.SetActive(_data.stageType != StageType.Boss);
        disableText.gameObject.SetActive(_data.stageType != StageType.Boss);

        leftLine.SetActive(_data.stage != 1);
        rightLine.SetActive(!_data.isMax);

        itemSlotList.ForEach(x =>
        {
            x.root.SetActive(false);
            x.line.SetActive(false);
            x.check.SetActive(false);
        });

        if (_data.stageType == StageType.Normal)
        {
            foreach (var item in _data.rewardItemDic)
            {
                itemSlotList[(int)item.Key].root.SetActive(true);
                itemSlotList[(int)item.Key].line.SetActive(true);

                itemSlotList[(int)item.Key].check.SetActive(_data.id <= stageID);
            }
        }
        else
        {
            List<StageButtonItemSlot> siList = new();

            foreach (var item in _data.rewardItemDic)
            {
                itemSlotList[(int)item.Key].root.SetActive(true);
                itemSlotList[(int)item.Key].line.SetActive(true);

                siList.Add(itemSlotList[(int)item.Key]);
            }

            if (_data.id <= stageID)
            {
                BossStarInfo bossStarInfo = UserDataManager.UserData.bossStarInfoDic[_data.difficultyType].FirstOrDefault(x => x.stageID == _data.id);

                int sm = bossStarInfo == null ? _data.starMax : bossStarInfo.starCount;

                for (int i = 0; i < sm; i++)
                {
                    siList[i].check.SetActive(true);
                }
            }
        }
    }
}
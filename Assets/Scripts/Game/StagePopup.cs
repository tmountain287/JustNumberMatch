using Common.Manager;
using Common.UI;
using Crystal;
using SuperScrollView;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Analytics.IAnalytic;

namespace JustOneMatch.UI
{
    public class StagePopup : BasePopup
    {
        //[SerializeField] private List<DifficutyPanel> difficutyPanelList = null;
        //[SerializeField] private List<Toggle> toggleList = null;

        [SerializeField] private SafeArea safeArea;
        [SerializeField] private LoopListView2 loopListView2;      

        private List<StageTableData> dataList = null;

        private int totalItemCount = 0;
        private float leftPadding = 0f;
        private float rightPadding = 0f;

        //protected override void Start()
        //{
        //    base.Start();

        //    for(int i = 0; i < toggleList.Count; i++)
        //    {
        //        int index = i;
        //        toggleList[i].onValueChanged.AddListener((isOn) =>
        //        {
        //            difficutyPanelList[index].gameObject.SetActive(isOn);
        //        });
        //    }
        //}

        private DifficultyType currentDifficultyType;

        public void Initialize(DifficultyType _difficultyType, int _stage = -1)
        {
            int id = UserDataManager.UserData.clearStageInfoDic[_difficultyType];


            int bossIndex = TableDataManager.Instance.TableStageData.StageTableDataDic[_difficultyType].FindIndex(x => x.id > id && x.stageType == StageType.Boss);        

            dataList = TableDataManager.Instance.TableStageData.StageTableDataDic[_difficultyType].GetRange(0, bossIndex + 1);

            leftPadding = safeArea.GetSafeAreaInsets().x;
            rightPadding = safeArea.GetSafeAreaInsets().z;

            //Resize();
            currentDifficultyType = _difficultyType;
            //dataList = TableDataManager.Instance.TableStageData.StageTableDataDic[_difficultyType];
            
            if (!dataList[^1].isMax)
            {
                StageTableData d = new StageTableData();
                d.isLock = true;
                dataList.Add(d);
            }

            totalItemCount = dataList.Count
                     + (leftPadding > 0 ? 1 : 0)
                     + (rightPadding > 0 ? 1 : 0);


            if (loopListView2.IsListViewInited)
            {
                loopListView2.SetListItemCount(totalItemCount);
            }
            else
            {
                loopListView2.InitListView(totalItemCount, OnGetItemByIndex);
            }            

            id = id == 0 ? 1 : id;           

            int index = dataList.FindIndex(x => x.id == id);

            if(index == -1)
            {
                index = dataList.Count - 1;
            }
            loopListView2.MovePanelToItemIndex(index + (leftPadding > 0 ? 1 : 0), 300);
            loopListView2.RefreshAllShownItem();
        }

        LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
        {
            if (leftPadding > 0 && index == 0)
            {
                var paddingTopItem = listView.NewListViewItem("PaddingItem");
                paddingTopItem.CachedRectTransform.sizeDelta = new Vector2(leftPadding, 0);
                return paddingTopItem;
            }

            // Bottom Padding
            if (rightPadding > 0 && index == totalItemCount - 1)
            {
                var paddingBottomItem = listView.NewListViewItem("PaddingItem");
                paddingBottomItem.CachedRectTransform.sizeDelta = new Vector2(rightPadding, 0);
                return paddingBottomItem;
            }

            int dataIndex = index - (leftPadding > 0 ? 1 : 0);

            if (dataIndex < 0 || dataIndex >= dataList.Count)
                return null;


            StageTableData itemData = dataList[dataIndex];
            //SimpleItemData itemData = mDataSourceMgr.GetItemDataByIndex(index);
            if (itemData == null)
            {
                return null;
            }
            //get a new item. Every item can use a different prefab, the parameter of the NewListViewItem is the prefab’name. 
            //And all the prefabs should be listed in ItemPrefabList in LoopListView2 Inspector Setting
            LoopListViewItem2 item = listView.NewListViewItem("StageButton");
            StageButton itemScript = item.GetComponent<StageButton>();
            //if (item.IsInitHandlerCalled == false)
            //{
            //    item.IsInitHandlerCalled = true;
            //    itemScript.Init(OnItemClicked);
            //}
            itemScript.SetItemData(currentDifficultyType, itemData, () =>
            {
                PopupManager.Instance.ClosePopup();
                GameMgr.Instance.StartStageMode(itemData);
            });
            //  itemScript.SetItemSelected(mCurrentSelectItemId == itemData.mId);
            return item;
        }

        private Vector2 lastSize;

        void Update()
        {
            Vector2 currentSize = rectTransform.rect.size;
            if (currentSize != lastSize)
            {
                lastSize = currentSize;

                //loopListView2.RecycleAllItem(); // 아이템 전부 반환
                //loopListView2.SetListItemCount(dataList.Count, false); // 아이템 개수 재설정
                //loopListView2.RefreshAllShownItem(); // 현재 보여지는 아이템 갱신
                //loopListView2.MovePanelToItemIndex(0, 0); // 리스트 처음부터 재시작 (선택사항)

                loopListView2.ResetListView(false);
            }
        }

    }
      
}
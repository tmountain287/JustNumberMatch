using Common.Manager;
using Common.UI;
using Crystal;
using SuperScrollView;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class MissionPopup : BasePopup
    {
        [SerializeField] private SafeArea safeArea = null;
        [SerializeField] private LoopListView2 loopListView2 = null;

        [SerializeField] private List<Toggle> toggleList = null;

        [SerializeField] private float topPadding = 100f;
        [SerializeField] private float bottomPadding = 100f;

        private int totalItemCount = 0;
        private Vector4 safeAreaVector = Vector4.zero;
        private List<MissionData> dataList = null;

        private MissionCategory missionCategory = MissionCategory.Daily;

        protected override void Start()
        {
            base.Start();

            for (int i = 0; i < toggleList.Count; i++)
            {
                int index = i;

                toggleList[i].onValueChanged.AddListener((isOn) =>
                {
                    if (missionCategory != (MissionCategory)index)
                        Initialize((MissionCategory)index);
                });
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            GameAnalyticsHelper.LogMissionOpen();
            ResolutionManager.Instance.OnChangeResolution.AddListener(Refresh);
        }

        protected override void OnDisable()
        {
            if (ResolutionManager.Instance != null)
                ResolutionManager.Instance.OnChangeResolution.RemoveListener(Refresh);
        }

        /// <summary>
        /// 보상 받을 미션이 있는 탭을 골라서 연다. 데일리 없고 메인만 있으면 메인 탭으로 연다.
        /// </summary>
        public void Initialize(bool _isReset = false)
        {
            Initialize(GetCategoryWithClaimable(), _isReset);
        }

        /// <summary>
        /// 보상 받을 미션이 있는 카테고리 반환. 데일리 없고 메인만 있으면 Main, 아니면 Daily.
        /// </summary>
        public static MissionCategory GetCategoryWithClaimable()
        {
            int dailyClaimable = 0;
            foreach (var item in UserDataManager.UserData.dailyMissionDataDic)
            {
                if (item.Value.IsComplete && !item.Value.isReward) dailyClaimable++;
            }
            int mainClaimable = UserDataManager.UserData.currentMissionList.Count(x => x.IsComplete && !x.isReward);

            if (mainClaimable > 0 && dailyClaimable == 0)
                return MissionCategory.Main;
            return MissionCategory.Daily;
        }

        public void Initialize(MissionCategory _category, bool _isReset = false)
        {
            toggleList[(int)_category].isOn = true;
            missionCategory = _category;

            if (missionCategory == MissionCategory.Daily)
            {
                dataList = TableDataManager.Instance.TableMissionData.GetDataList(_category);

                var dailyDic = UserDataManager.UserData.dailyMissionDataDic;

                dataList = dataList
                    .OrderBy(x =>
                    {
                        DailyMissionData dm;

                        // 딕셔너리에 없으면 기본값
                        if (!dailyDic.TryGetValue(x.type, out dm))
                            return 0;  // isReward 아님

                        return dm.isReward ? 1 : 0;   // 보상 받은 미션 → 뒤로
                    })
                    .ThenBy(x =>
                    {
                        DailyMissionData dm;

                        if (!dailyDic.TryGetValue(x.type, out dm))
                            return 1;  // 완료 아님

                        return dm.IsComplete ? 0 : 1; // 완료된 미션 → 앞으로
                    })
                    .ThenBy(x => x.id)
                    .ToList();
            }
            else
            {
                // 메인 미션 그룹을 모두 클리어(마지막 미션 보상 수령)한 항목은 목록에서 제외
                var tableMission = TableDataManager.Instance.TableMissionData;
                dataList = UserDataManager.UserData.currentMissionList
                    .Where(mainData =>
                    {
                        var data = tableMission.GetData(mainData.id);
                        if (data == null) return false;
                        // 보상 수령했고 다음 미션이 없으면 해당 그룹 전체 클리어 → 제외
                        if (mainData.isReward)
                        {
                            var nextData = tableMission.GetNextData(mainData.id, data.difficultyType);
                            if (nextData == null) return false;
                        }
                        return true;
                    })
                    .Select(mainData => tableMission.GetData(mainData.id))
                    .Where(m => m != null)
                    .ToList();

                dataList = dataList.OrderByDescending(m =>
                            {
                                var mainData = UserDataManager.UserData.currentMissionList
                                    .FirstOrDefault(x => x.id == m.id);

                                return mainData?.IsComplete ?? false;
                            }).ToList();
            }

            safeAreaVector = safeArea.GetSafeAreaInsets();

            totalItemCount = dataList.Count
                  + (topPadding > 0 ? 1 : 0)
                  + (bottomPadding > 0 ? 1 : 0);

            if (!loopListView2.IsListViewInited)
            {
                // dataList = TableDataManager.Instance.TableSkillData.SkillTableDataList;


                loopListView2.InitListView(totalItemCount, OnGetItemByIndex);
            }
            else
            {
                loopListView2.SetListItemCount(totalItemCount, _isReset);
                loopListView2.RefreshAllShownItem();
            }

            loopListView2.ResetListView();
            //loopListView2.MovePanelToItemIndex(index + (topPadding > 0 ? 1 : 0), 30);
        }

        LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
        {
            if (topPadding > 0 && index == 0)
            {
                var paddingTopItem = listView.NewListViewItem("PaddingItem");
                paddingTopItem.CachedRectTransform.sizeDelta = new Vector2(0, topPadding);
                return paddingTopItem;
            }

            // Bottom Padding
            if (bottomPadding > 0 && index == totalItemCount - 1)
            {
                var paddingBottomItem = listView.NewListViewItem("PaddingItem");
                paddingBottomItem.CachedRectTransform.sizeDelta = new Vector2(0, bottomPadding + safeAreaVector.w);
                return paddingBottomItem;
            }

            int dataIndex = index - (topPadding > 0 ? 1 : 0);

            if (dataIndex < 0 || dataIndex >= dataList.Count)
                return null;

            MissionData itemData = dataList[dataIndex];
            if (itemData == null)
            {
                return null;
            }
            /*get a new item. Every item can use a different prefab, 
            the parameter of the NewListViewItem is the prefab’name. 
            And all the prefabs should be listed in ItemPrefabList in LoopGridView Inspector Setting  */
            LoopListViewItem2 item = loopListView2.NewListViewItem("MissionPopupItem");
            //get your own component
            MissionPopupItem itemScript = item.GetComponent<MissionPopupItem>();
            // IsInitHandlerCalled is false means this item is new created but not fetched from pool.
            if (item.IsInitHandlerCalled == false)
            {
                item.IsInitHandlerCalled = true;
                //itemScript.Init();// here to init the item, such as add button click event listener.
            }
            //update the item’s content for showing, such as image,text.
            itemScript.SetItem(itemData);
            return item;
        }

        private void Refresh()
        {
            safeAreaVector = safeArea.GetSafeAreaInsets();
            loopListView2.ResetListView(false);
        }
    }
}
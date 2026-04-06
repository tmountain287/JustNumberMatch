using Common.Manager;
using Common.UI;
using Crystal;
using SuperScrollView;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
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
        [SerializeField] private Button scrollToCurrentStageButton;
        [SerializeField] private Button redStarButton;

        [Tooltip("스크롤 가운데 정렬 시 셀 너비를 자동으로 못 읽을 때만 사용하는 보조값(StageButton 너비). 보통 0으로 두면 됩니다.")]
        [FormerlySerializedAs("centerScrollCellWidth")]
        [SerializeField] private float centerScrollCellWidthFallback = 0f;

        private List<StageTableData> dataList = null;

        /// <summary>표시 중인 StageButton에서 측정한 셀 너비. 0이면 보조값/추정값 사용.</summary>
        private float cachedStageCellWidth;

        /// <summary>LoopListView2 ItemPosMgr용. 첫 StageButton 생성 시 item.Padding으로 설정.</summary>
        private float measuredBetweenPadding;

        /// <summary>자동 측정 실패 시 사용하는 기본 추정 너비.</summary>
        const float StageCellWidthFallbackGuess = 100f;

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

        private void Awake()
        {
            if (scrollToCurrentStageButton != null)
                scrollToCurrentStageButton.onClick.AddListener(ScrollToCurrentProgressStage);

            redStarButton.onClick.AddListener(() =>
            {
                PopupManager.Instance.OpenPopup<RedStarListPopup>().Initialize(currentDifficultyType);
            });
        }

        private DifficultyType currentDifficultyType;

        public void Initialize(DifficultyType _difficultyType, int _stage = -1)
        {
            int id = UserDataManager.UserData.clearStageInfoDic[_difficultyType];

            var stageTableList = TableDataManager.Instance.TableStageData.StageTableDataDic[_difficultyType];
            int bossIndex = stageTableList.FindIndex(x => x.id > id && x.stageType == StageType.Boss);

            // 다음 보스가 없으면(해당 난이도 전부 클리어 등) FindIndex == -1 → GetRange(0,0)이 되어 리스트가 비고 [^1]에서 예외 남
            if (bossIndex < 0)
                dataList = new List<StageTableData>(stageTableList);
            else
                dataList = stageTableList.GetRange(0, bossIndex + 1);

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

            measuredBetweenPadding = 0f;

            if (loopListView2.IsListViewInited)
            {
                loopListView2.SetListItemCount(totalItemCount);
            }
            else
            {
                loopListView2.InitListView(totalItemCount, OnGetItemByIndex, null, GetLoopListItemSizeByIndex);
            }

            int targetStageTableId = GetTargetStageTableIdForScroll();
            int dataIndex = dataList.FindIndex(x => !x.isLock && x.id == targetStageTableId);
            if (dataIndex < 0)
                dataIndex = GetLastRealStageDataIndex(dataList);

            int listIndex = dataIndex + (leftPadding > 0 ? 1 : 0);
            MoveListToIndexCentered(listIndex);

            // 보이지 않는 셀은 기본(mItemDefaultWithPaddingSize)으로만 잡혀 스크롤바 핸들이 어긋남 → 전체 인덱스 크기 일괄 반영
            if (totalItemCount > 0)
            {
                loopListView2.UpdateItemSizeAtOnce(0, totalItemCount - 1);
                RefreshLoopListScrollLayout();
                MoveListToIndexCentered(listIndex);
            }
        }

        /// <summary>SuperScrollView: 콘텐츠 총 길이·스크롤바가 실제 셀 크기와 맞도록 (itemSize, padding) 제공.</summary>
        (float, float) GetLoopListItemSizeByIndex(int index)
        {
            if (leftPadding > 0 && index == 0)
                return (leftPadding, 0f);
            if (rightPadding > 0 && index == totalItemCount - 1)
                return (rightPadding, 0f);

            float cell = cachedStageCellWidth > 0f
                ? cachedStageCellWidth
                : (centerScrollCellWidthFallback > 0f ? centerScrollCellWidthFallback : StageCellWidthFallbackGuess);
            return (cell, measuredBetweenPadding);
        }

        void RefreshLoopListScrollLayout()
        {
            if (loopListView2 == null)
                return;
            Canvas.ForceUpdateCanvases();
            ScrollRect sr = loopListView2.ScrollRect;
            if (sr != null && sr.content != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(sr.content);
            if (sr == null)
                return;
            if (sr.horizontal)
                sr.horizontalNormalizedPosition = sr.horizontalNormalizedPosition;
            if (sr.vertical)
                sr.verticalNormalizedPosition = sr.verticalNormalizedPosition;
        }

        void TryCacheStageCellWidthFromShownItems(bool forceRebuildLayout = false)
        {
            if (loopListView2 == null || !loopListView2.IsListViewInited)
                return;

            int n = loopListView2.ShownItemCount;
            for (int i = 0; i < n; i++)
            {
                LoopListViewItem2 lvItem = loopListView2.GetShownItemByIndex(i);
                if (lvItem == null || lvItem.GetComponent<StageButton>() == null)
                    continue;
                RectTransform rt = lvItem.CachedRectTransform;
                if (forceRebuildLayout)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                float w = rt.rect.width;
                if (w > 0f)
                {
                    cachedStageCellWidth = w;
                    return;
                }
            }
        }

        float GetCenterScrollOffset()
        {
            if (loopListView2 == null)
                return 0f;
            float viewport = loopListView2.ViewPortSize;
            if (viewport <= 0f)
                return 0f;
            float cellW = cachedStageCellWidth > 0f
                ? cachedStageCellWidth
                : (centerScrollCellWidthFallback > 0f ? centerScrollCellWidthFallback : StageCellWidthFallbackGuess);
            return Mathf.Clamp((viewport - cellW) * 0.5f, -viewport, viewport);
        }

        void MoveListToIndexCentered(int listIndex)
        {
            if (loopListView2 == null || !loopListView2.IsListViewInited)
                return;

            float hadReliableWidth = cachedStageCellWidth;

            TryCacheStageCellWidthFromShownItems();
            loopListView2.MovePanelToItemIndex(listIndex, GetCenterScrollOffset());
            Canvas.ForceUpdateCanvases();
            TryCacheStageCellWidthFromShownItems(forceRebuildLayout: true);

            // 최초 오픈: Move 전에는 셀이 없어 추정 offset으로 스크롤됨 → 실제 레이아웃 후 너비로 한 번 더 맞춤
            if (hadReliableWidth <= 0f && cachedStageCellWidth > 0f)
                loopListView2.MovePanelToItemIndex(listIndex, GetCenterScrollOffset());
        }

        /// <summary>
        /// 현재 플레이 중인 스테이지(또는 다음 진행 스테이지)로 리스트를 스크롤합니다.
        /// 해당 스테이지가 없으면(난이도 전체 클리어 등) 마지막 스테이지로 이동합니다.
        /// </summary>
        public void ScrollToCurrentProgressStage()
        {
            if (dataList == null || dataList.Count == 0 || loopListView2 == null || !loopListView2.IsListViewInited)
                return;

            int targetStageTableId = GetTargetStageTableIdForScroll();
            int dataIndex = dataList.FindIndex(x => !x.isLock && x.id == targetStageTableId);
            if (dataIndex < 0)
                dataIndex = GetLastRealStageDataIndex(dataList);

            int listIndex = dataIndex + (leftPadding > 0 ? 1 : 0);
            MoveListToIndexCentered(listIndex);
        }

        int GetTargetStageTableIdForScroll()
        {
            StageTableData playing = GameMgr.Instance != null ? GameMgr.Instance.CurrentStageTableData : null;
            if (playing != null && playing.difficultyType == currentDifficultyType)
                return playing.id;

            int cleared = UserDataManager.UserData.clearStageInfoDic[currentDifficultyType];
            int next = cleared + 1;
            return next < 1 ? 1 : next;
        }

        static int GetLastRealStageDataIndex(List<StageTableData> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (!list[i].isLock && list[i].id > 0)
                    return i;
            }
            return 0;
        }

        /// <summary>
        /// 스테이지 테이블 id에 해당하는 셀로 리스트 스크롤을 맞춥니다. (Initialize 시점과 동일한 패딩 보정)
        /// </summary>
        public void ScrollToStageByTableId(int stageTableId)
        {
            if (dataList == null || dataList.Count == 0 || loopListView2 == null || !loopListView2.IsListViewInited)
                return;

            int dataIndex = dataList.FindIndex(x => x.id == stageTableId);
            if (dataIndex < 0)
                return;

            int listIndex = dataIndex + (leftPadding > 0 ? 1 : 0);
            MoveListToIndexCentered(listIndex);
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
            RectTransform itemRt = item.CachedRectTransform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemRt);
            float cellW = itemRt.rect.width;
            if (cellW > 0f)
                cachedStageCellWidth = cellW;
            if (measuredBetweenPadding <= 0f && item.Padding > 0f)
                measuredBetweenPadding = item.Padding;
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

                cachedStageCellWidth = 0f;
                measuredBetweenPadding = 0f;
                loopListView2.ResetListView(false);
                TryCacheStageCellWidthFromShownItems(forceRebuildLayout: true);
                if (totalItemCount > 0)
                {
                    loopListView2.UpdateItemSizeAtOnce(0, totalItemCount - 1);
                    RefreshLoopListScrollLayout();
                }
            }
        }
    }      
}
using Common.Manager;
using Common.UI;
using SuperScrollView;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JustOneMatch.UI
{
    /// <summary>
    /// 모드(난이도)별로, 보스전 클리어 후 레드스타를 아직 3개 채우지 못한 보스 목록을 LoopListView2로 표시합니다.
    /// </summary>
    public class RedStarListPopup : BasePopup
    {
        [SerializeField] private LoopListView2 loopListView2;
        [SerializeField] private GameObject emptyStateRoot;
        [SerializeField] private GameObject emptyMessage;
        [SerializeField] private GameObject allClearMessage;

        private readonly List<BossStarInfo> rows = new();
        private DifficultyType difficultyType;

        public void Initialize(DifficultyType _difficultyType)
        {
            difficultyType = _difficultyType;
            RefreshList();
        }

        void RefreshList()
        {
            RebuildRows();

            bool listEmpty = rows.Count <= 0;
            bool allClear = listEmpty && IsAllBossRedStarsMaxed();

            if (emptyStateRoot != null)
                emptyStateRoot.SetActive(listEmpty);
            if (emptyMessage != null)
                emptyMessage.SetActive(listEmpty && !allClear);
            if (allClearMessage != null)
                allClearMessage.SetActive(listEmpty && allClear);

            if (loopListView2 == null)
                return;

            int count = rows.Count;
            if (loopListView2.IsListViewInited)
            {
                loopListView2.SetListItemCount(count, false);
                loopListView2.RefreshAllShownItem();
            }
            else
            {
                loopListView2.InitListView(Mathf.Max(0, count), OnGetItemByIndex);
            }
        }

        /// <summary>
        /// 클리어 진행상 도달한 모든 보스에서 레드 스타가 최대인 경우.
        /// (3성 달성 시 BossStarInfo가 리스트에서 제거되므로, bossStarInfoDic만 보면 올클을 판별할 수 없음)
        /// </summary>
        bool IsAllBossRedStarsMaxed()
        {
            if (TableDataManager.Instance?.TableStageData == null || UserDataManager.UserData == null)
                return false;

            if (!TableDataManager.Instance.TableStageData.StageTableDataDic.TryGetValue(difficultyType, out List<StageTableData> stageList) ||
                stageList == null || stageList.Count == 0)
                return false;

            int clearedId = UserDataManager.UserData.clearStageInfoDic[difficultyType];
            List<StageTableData> bossesReached = stageList
                .Where(x => x.stageType == StageType.Boss && x.id <= clearedId)
                .ToList();

            if (bossesReached.Count == 0)
                return false;

            UserDataManager.UserData.bossStarInfoDic.TryGetValue(difficultyType, out List<BossStarInfo> infos);

            foreach (StageTableData boss in bossesReached)
            {
                BossStarInfo info = infos?.FirstOrDefault(x => x.stageID == boss.id);
                if (info != null && info.starCount < boss.starMax)
                    return false;
            }

            return true;
        }

        void RebuildRows()
        {
            rows.Clear();

            if (TableDataManager.Instance?.TableStageData == null || UserDataManager.UserData == null)
                return;

            List<BossStarInfo> infos = UserDataManager.UserData.bossStarInfoDic[difficultyType];
            if (infos == null || infos.Count == 0)
                return;

            foreach (BossStarInfo info in infos.OrderBy(x => x.stageID))
            {
                StageTableData stage = TableDataManager.Instance.TableStageData.GetTableData(difficultyType, info.stageID);
                if (stage == null || stage.stageType != StageType.Boss)
                    continue;
                if (info.starCount >= stage.starMax)
                    continue;
                rows.Add(info);
            }
        }

        LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
        {
            if (index < 0 || index >= rows.Count)
                return null;

            LoopListViewItem2 bossItem = listView.NewListViewItem("RedStarListPopupItem");
            RedStarListPopupItem bossScript = bossItem.GetComponent<RedStarListPopupItem>();
            if (bossScript != null)
                bossScript.SetData(difficultyType, rows[index], OnBossRowGoClicked);
            return bossItem;
        }

        void OnBossRowGoClicked(int stageTableId)
        {
            PopupManager.Instance.ClosePopup<RedStarListPopup>(() =>
            {
                StagePopup stagePopup = PopupManager.Instance.FindOpenPopup<StagePopup>();
                stagePopup?.ScrollToStageByTableId(stageTableId);
            });
        }
    }
}
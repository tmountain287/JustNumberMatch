using DG.Tweening;
using SuperScrollView;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common.UI;

namespace UI.Popup
{
    public class InGameCollectionPopup : BasePopup
    {
        [SerializeField] private LoopListView2 mLoopListView;

        [SerializeField] private List<Button> buttonList = null;
        [SerializeField] private List<Text> buttonTextList = null;

        private List<PlayerData> playerDataList = new();

        private int currentClickIndex = -1;

        protected override void Start()
        {
            base.Start();
            
            for (int i = 0; i < buttonList.Count; i++)
            {
                int index = i;

                buttonList[i].onClick.AddListener(() =>
                {
                    if (currentClickIndex != index)
                    {
                        buttonList[index].transform.DOScale(1.2f, 0.1f)
                               .SetEase(Ease.OutQuad)
                               .OnComplete(() =>
                               {
                                   buttonList[index].transform.DOScale(1f, 0.1f)
                                       .SetEase(Ease.InQuad);
                               });
                        currentClickIndex = index;
                        mLoopListView.SetSnapTargetItemIndex(index);
                    }
                });

            }
        }

        public void Initialize(List<PlayerData> _playerDataList, int _slotIndex)
        {
            playerDataList = _playerDataList;
            if (!mLoopListView.IsListViewInited)
            {
                LoopListViewInitParam initParam = LoopListViewInitParam.CopyDefaultInitParam();
                initParam.mSmoothDumpRate = 0.1f;
                initParam.mSnapVecThreshold = 99999;
                initParam.mSnapFinishThreshold = 0.05f;       // 스냅 완료 조건 완화
                //initParam.mSnapVecThreshold = 200f;           // 속도 줄어들면 스냅 빨리 시작
                initParam.mDistanceForNew0 = 150f;
                initParam.mDistanceForRecycle0 = 250f;
                mLoopListView.mOnEndDragAction = OnEndDrag;
                mLoopListView.mOnSnapNearestChanged = OnSnapNearestChanged;
                mLoopListView.InitListView(playerDataList.Count, OnGetItemByIndex, initParam);
            }
            else
            {
                mLoopListView.SetListItemCount(playerDataList.Count);
                mLoopListView.RefreshAllShownItem();
            }

            OnScaleUpdate = () =>
            {
                mLoopListView.MovePanelToItemIndex(_slotIndex, 0);
                mLoopListView.FinishSnapImmediately();
            };
            
            UpdateButtons();
            //mLoopListView.SetSnapTargetItemIndex(_slotIndex);
        }

        void OnSnapNearestChanged(LoopListView2 listView, LoopListViewItem2 item)
        {
            UpdateButtons();
        }

        void UpdateButtons()
        {
            int curNearestItemIndex = mLoopListView.CurSnapNearestItemIndex;
            currentClickIndex = mLoopListView.CurSnapNearestItemIndex;
            for (int i = 0; i < buttonList.Count; i++)
            {
                buttonList[i].enabled = curNearestItemIndex != i;
            }

            for (int i = 0; i < buttonTextList.Count; i++)
            {
                buttonTextList[i].color = curNearestItemIndex == i ? Color.white : new Color(165 / 255f, 206 / 255f, 145 / 255f);
            }

            //int curNearestItemIndex = mLoopListView.CurSnapNearestItemIndex;
            //if (curNearestItemIndex < 0 || curNearestItemIndex >= playerDataList.Count)
            //{
            //    return;
            //}
            //int count = mDotElemList.Count;
            //if (curNearestItemIndex >= count)
            //{
            //    return;
            //}
            //RefreshAllDots(curNearestItemIndex);
        }

        void RefreshAllDots(int selectedIndex)
        {
            //for (int i = 0; i < mDotElemList.Count; ++i)
            //{
            //    DotElem elem = mDotElemList[i];
            //    if (i != selectedIndex)
            //    {
            //        elem.mDotNormal.SetActive(true);
            //        elem.mDotSelect.SetActive(false);
            //    }
            //    else
            //    {
            //        elem.mDotNormal.SetActive(false);
            //        elem.mDotSelect.SetActive(true);
            //    }
            //}
        }


        void OnEndDrag()
        {
            float vec = mLoopListView.ScrollRect.velocity.x;
            int curNearestItemIndex = mLoopListView.CurSnapNearestItemIndex;
            LoopListViewItem2 item = mLoopListView.GetShownItemByItemIndex(curNearestItemIndex);
            if (item == null)
            {
                mLoopListView.ClearSnapData();
                return;
            }
            if (Mathf.Abs(vec) < 50f)
            {
                mLoopListView.SetSnapTargetItemIndex(curNearestItemIndex);
                return;
            }
            Vector3 pos = mLoopListView.GetItemCornerPosInViewPort(item, ItemCornerEnum.LeftTop);
            if (pos.x > 0)
            {
                if (vec > 0)
                {
                    mLoopListView.SetSnapTargetItemIndex(curNearestItemIndex - 1);
                }
                else
                {
                    mLoopListView.SetSnapTargetItemIndex(curNearestItemIndex);
                }
            }
            else if (pos.x < 0)
            {
                if (vec > 0)
                {
                    mLoopListView.SetSnapTargetItemIndex(curNearestItemIndex);
                }
                else
                {
                    mLoopListView.SetSnapTargetItemIndex(curNearestItemIndex + 1);
                }
            }
            else
            {
                if (vec > 0)
                {
                    mLoopListView.SetSnapTargetItemIndex(curNearestItemIndex - 1);
                }
                else
                {
                    mLoopListView.SetSnapTargetItemIndex(curNearestItemIndex + 1);
                }
            }
        }

        LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= playerDataList.Count)
            {
                return null;
            }

            PlayerData itemData = playerDataList[pageIndex];
            if (itemData == null)
            {
                return null;
            }

            LoopListViewItem2 item = listView.NewListViewItem("CollectionCardItem");
            CollectionCardItem itemScript = item.GetComponent<CollectionCardItem>();
            if (item.IsInitHandlerCalled == false)
            {
                item.IsInitHandlerCalled = true;
                //itemScript.Init();
            }
            itemScript.SetItemData(itemData);
            return item;
        }



    }
}
using Crystal;
using SuperScrollView;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gostop.UI
{
    public class ShopSkillPanel : MonoBehaviour
    {
        [SerializeField] private SafeArea safeArea = null;
        [SerializeField] private RectTransform rectTransform = null;
        [SerializeField] private LoopListView2 loopListView2 = null;

        [SerializeField] private float topPadding = 100f;
        [SerializeField] private float bottomPadding = 100f;

        [SerializeField] private GameObject block = null;        
        [SerializeField] private Transform[] skillEffects = null;

        private List<SkillTableData> dataList = null;
        private Action<ShopCharacterTableData> onSelect = null;

        public int SelectedIndex = -1;
        private int totalItemCount = 0;

        private Vector4 safeAreaVector = Vector4.zero;

        public void OnSkillEffect(SkillType _skillType, Transform _sourceTransform)
        {
            block.SetActive(true);
            Transform fx = skillEffects[(int)_skillType];

            fx.SetParent(_sourceTransform);
            fx.localPosition = Vector3.zero;

            //DOVirtual.DelayedCall(0.1f, () =>
            //{
            //    fx.SetParent(skillEffect);
            //});

            //DOVirtual.DelayedCall(0.4f, () =>
            //{
            //    fx.SetParent(_sourceTransform);
            //});
            fx.gameObject.SetActive(true);
        }

        public void Initialize(Action<ShopCharacterTableData> _onSelect)
        {
            safeAreaVector = safeArea.GetSafeAreaInsets();
            lastSize = rectTransform.rect.size;
            SelectedIndex = UserDataManager.SelectIndex;
            onSelect = _onSelect;

            if (!loopListView2.IsListViewInited)
            {
                dataList = TableDataManager.Instance.TableSkillData.SkillTableDataList;

                totalItemCount = dataList.Count
                      + (topPadding > 0 ? 1 : 0)
                      + (bottomPadding > 0 ? 1 : 0);

                loopListView2.InitListView(totalItemCount, OnGetItemByIndex);
            }
            else
            {
                loopListView2.SetListItemCount(totalItemCount, true);
                loopListView2.RefreshAllShownItem();
            }

            int index = 0;

            for (int i = 0; i < dataList.Count; i++)
            {
                for (int j = 0; j < dataList[i].shopCharacterIDList.Count; j++)
                {
                    if (TableDataManager.Instance.TableShopCharacterData.GetData(dataList[i].shopCharacterIDList[j]).characterId == UserDataManager.SelectIndex)
                    {
                        index = i;
                        break;
                    }
                }
            }

            loopListView2.MovePanelToItemIndex(index + (topPadding > 0 ? 1 : 0), 30);
        }

        public void RefreshPanel()
        {
            loopListView2.SetListItemCount(totalItemCount, false);
            loopListView2.RefreshAllShownItem();
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

            SkillTableData itemData = dataList[dataIndex];
            if (itemData == null)
            {
                return null;
            }
            /*get a new item. Every item can use a different prefab, 
            the parameter of the NewListViewItem is the prefab’name. 
            And all the prefabs should be listed in ItemPrefabList in LoopGridView Inspector Setting  */
            LoopListViewItem2 item = loopListView2.NewListViewItem("StoreSkillItem");
            //get your own component
            StoreSkillItem itemScript = item.GetComponent<StoreSkillItem>();
            // IsInitHandlerCalled is false means this item is new created but not fetched from pool.
            if (item.IsInitHandlerCalled == false)
            {
                item.IsInitHandlerCalled = true;
                //itemScript.Init();// here to init the item, such as add button click event listener.
            }
            //update the item’s content for showing, such as image,text.
            itemScript.SetItemData(this, SelectedIndex, itemData, (data) =>
            {
                onSelect?.Invoke(data);
                SelectedIndex = data.characterId;
                RefreshPanel();
            }, ()=>RefreshPanel());
            return item;
        }

        private Vector2 lastSize;

        void Update()
        {
            Vector2 currentSize = rectTransform.rect.size;
            if (currentSize != lastSize)
            {
                lastSize = currentSize;
                safeAreaVector = safeArea.GetSafeAreaInsets();

                Debug.Log("다시 설정");

                //loopListView2.RecycleAllItem(); // 아이템 전부 반환
                //loopListView2.SetListItemCount(dataList.Count, false); // 아이템 개수 재설정
                //loopListView2.RefreshAllShownItem(); // 현재 보여지는 아이템 갱신
                //loopListView2.MovePanelToItemIndex(0, 0); // 리스트 처음부터 재시작 (선택사항)

                loopListView2.ResetListView(false);
            }
        }
    }
}
 
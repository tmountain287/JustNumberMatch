using SuperScrollView;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class ShopCharacterPanel : MonoBehaviour
    {
        [SerializeField] private LoopGridView loopGridView = null;
        
        private List<ShopCharacterTableData> dataList = null;

        public void Initialize()
        {
            if(!loopGridView.IsListViewInited)
            {
                dataList = TableDataManager.Instance.TableShopCharacterData.ShopCharacterTableDataList;
                loopGridView.InitGridView(dataList.Count, OnGetItemByRowColumn);
            }
            else
            {
                loopGridView.SetListItemCount(dataList.Count, true);
                loopGridView.RefreshAllShownItem();
            }
        }       

        public void RefreshPanel()
        {
            loopGridView.SetListItemCount(dataList.Count, false);
            loopGridView.RefreshAllShownItem();
        }

        LoopGridViewItem OnGetItemByRowColumn(LoopGridView gridView, int index, int row, int column)
        {
            if (index < 0 || index >= dataList.Count)
            {
                return null;
            }
            //get the data to showing
            ShopCharacterTableData itemData = dataList[index];
            if (itemData == null)
            {
                return null;
            }
            /*get a new item. Every item can use a different prefab, 
            the parameter of the NewListViewItem is the prefab’name. 
            And all the prefabs should be listed in ItemPrefabList in LoopGridView Inspector Setting  */
            LoopGridViewItem item = gridView.NewListViewItem("StoreCharacterItem");
            //get your own component
            StoreCharacterItem itemScript = item.GetComponent<StoreCharacterItem>();
            // IsInitHandlerCalled is false means this item is new created but not fetched from pool.
            if (item.IsInitHandlerCalled == false)
            {
                item.IsInitHandlerCalled = true;
                //itemScript.Init();// here to init the item, such as add button click event listener.
            }
            //update the item’s content for showing, such as image,text.
            //itemScript.SetItemData(itemData, index, RefreshPanel);
            return item;
        }
    }
}

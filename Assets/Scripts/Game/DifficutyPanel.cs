using Common.Manager;
using SuperScrollView;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DifficutyPanel : MonoBehaviour
{
    [SerializeField] private LoopGridView mLoopGridView = null;
    [SerializeField] private DifficultyType difficultyType = DifficultyType.Easy;


    private List<EquationTableData> dataList = null;
    private int totalItemCount = 0;

    void Start()
    {
        dataList = TableDataManager.Instance.TableEquationData.EquationTableDataDic[difficultyType];

        totalItemCount = dataList.Count;
        //+ (topPadding > 0 ? 1 : 0)
        //+ (bottomPadding > 0 ? 1 : 0);

        //LoopGridViewSettingParam settingParam = new LoopGridViewSettingParam();
        //settingParam.mItemSize = new Vector2(150, 150);
        //settingParam.mItemPadding = new Vector2(20, 20);
        //settingParam.mPadding = new RectOffset(20, 20, 20, 20);
        //settingParam.mGridFixedType = GridFixedType.RowCountFixed;
        //settingParam.mFixedRowOrColumnCount = 10;

        mLoopGridView.InitGridView(totalItemCount, OnGetItemByRowColumn);//, settingParam);
    }

    public void Initialize()
    {
        //if (!mLoopGridView.IsListViewInited)
        //{
        //    dataList = TableDataManager.Instance.TableEquationData.EquationTableDataDic[difficultyType];

        //    totalItemCount = dataList.Count;
        //    //+ (topPadding > 0 ? 1 : 0)
        //    //+ (bottomPadding > 0 ? 1 : 0);

        //    //LoopGridViewSettingParam settingParam = new LoopGridViewSettingParam();
        //    //settingParam.mItemSize = new Vector2(150, 150);
        //    //settingParam.mItemPadding = new Vector2(20, 20);
        //    //settingParam.mPadding = new RectOffset(20, 20, 20, 20);
        //    //settingParam.mGridFixedType = GridFixedType.RowCountFixed;
        //    //settingParam.mFixedRowOrColumnCount = 10;

        //    mLoopGridView.InitGridView(totalItemCount, OnGetItemByRowColumn);//, settingParam);
        //}
        //else
        //{
        //    mLoopGridView.SetListItemCount(totalItemCount, true);
        //    mLoopGridView.RefreshAllShownItem();
        //}

        //mLoopGridView.MovePanelToItemByIndex(0);//.MovePanelToItemIndex(index + (topPadding > 0 ? 1 : 0), 30);
    }

    LoopGridViewItem OnGetItemByRowColumn(LoopGridView gridView, int index, int row, int column)
    {
        if (index < 0 || index >= totalItemCount)
        {
            return null;
        }
        //get the data to showing
        EquationTableData itemData = TableDataManager.Instance.TableEquationData.GetTableData(difficultyType, index + 1);

        if (itemData == null)
        {
            return null;
        }
        /*get a new item. Every item can use a different prefab, 
        the parameter of the NewListViewItem is the prefab¡¯name. 
        And all the prefabs should be listed in ItemPrefabList in LoopGridView Inspector Setting  */
        LoopGridViewItem item = gridView.NewListViewItem("StageButton");
        //get your own component
        StageButton itemScript = item.GetComponent<StageButton>();
        //// IsInitHandlerCalled is false means this item is new created but not fetched from pool.
        //if (item.IsInitHandlerCalled == false)
        //{
        //    item.IsInitHandlerCalled = true;
        //    itemScript.Init();// here to init the item, such as add button click event listener.
        //}
        ////update the item¡¯s content for showing, such as image,text.
        //itemScript.SetItemData(difficultyType, itemData, index, ()=>
        //{
        //    PopupManager.Instance.ClosePopup();
        //    GameMgr.Instance.SetStage(difficultyType, itemData);
        //});
        return item;
    }
}

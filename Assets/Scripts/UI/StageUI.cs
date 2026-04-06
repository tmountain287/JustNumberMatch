using Common.Manager;
using Common.UI;
using JustOneMatch.UI;
using SuperScrollView;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

public class StageInfo
{
    public DifficultyType difficultyType;

    public StageInfo(DifficultyType difficulty)
        { this.difficultyType = difficulty; }
}

public class StageUI : BaseUI
{
    [SerializeField] private Button timeAttackButton = null;
    [SerializeField] private Button profileButton = null;

    private void Start()
    {
        profileButton.onClick.AddListener(() =>
        {
            PopupManager.Instance.OpenPopup<UserInfoPopup>().Initialize();
        });
    }

    //[SerializeField] private LoopListView2 loopListView2 = null;
    //[SerializeField] private RectTransform viewport;
    //[SerializeField] private RectTransform rectTransform = null;

    //private List<StageInfo> stageInfoList = null;

    //private float itemSizeWidth = 512;
    //private float itemPadding = 3;

    //private int emptyCount = 0;
    //private float emptyWidth = 0;

    //private void Resize()
    //{
    //    emptyCount = 2;
    //    emptyWidth = (viewport.rect.width * 0.5f - (itemSizeWidth * 0.5f)) - itemPadding;
    //    emptyCount = emptyWidth > 0 ? 2 : 0;
    //}

    //private void OnEnable()
    //{
    //    Resize();

    //    stageInfoList = new List<StageInfo>()
    //    {
    //        {new (DifficultyType.Easy) },
    //        {new (DifficultyType.Normal) },
    //        {new (DifficultyType.Hard) },
    //        {new (DifficultyType.CommingSoon) },
    //        {new (DifficultyType.CommingSoon) },
    //    };

    //    if (loopListView2.IsListViewInited)
    //    {
    //        loopListView2.SetListItemCount(stageInfoList.Count + emptyCount);
    //    }
    //    else
    //    {
    //        loopListView2.InitListView(stageInfoList.Count + emptyCount, OnGetItemByIndex);
    //    }

    //    loopListView2.RefreshAllShownItem();
    //    StartCoroutine(OnLasy());
    //    loopListView2.mOnSnapNearestChanged = OnSnapNearestChanged;
    //    OnCenter(2);
    //}

    //IEnumerator OnLasy()
    //{
    //    yield return new WaitForEndOfFrame();
    //    loopListView2.MovePanelToItemIndex(2, 0);
    //    loopListView2.FinishSnapImmediately();
    //}

    //void OnCenter(int _index)
    //{
    //    int count = loopListView2.ShownItemCount;

    //    for (int i = 0; i < count; ++i)
    //    {
    //        LoopListViewItem2 t = loopListView2.GetShownItemByIndex(i);
    //        StageUIButton itemScript = t.GetComponent<StageUIButton>();

    //        if (itemScript != null)
    //        {
    //            itemScript.OnCenter(itemScript.ItemDataIndex == _index);
    //        }
    //        else
    //        {
    //            if (itemScript != null)
    //                itemScript.OnCenter(false);
    //        }
    //    }
    //}

    //void OnSnapNearestChanged(LoopListView2 listView, LoopListViewItem2 item)
    //{
    //    StageUIButton myCharacterItem = item.GetComponent<StageUIButton>();

    //    if (myCharacterItem != null)
    //    {
    //        OnCenter(myCharacterItem.ItemDataIndex);
    //    }
    //}

    //LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
    //{
    //    if (index < 0)
    //    {
    //        return null;
    //    }

    //    int ec = emptyCount / 2;

    //    //the first 2 items are the taking place empty items.
    //    if (index < ec)
    //    {
    //        LoopListViewItem2 itemEmpty = listView.NewListViewItem("ItemPrefab1");
    //        itemEmpty.GetComponent<RectTransform>().sizeDelta = new(emptyWidth, 300);
    //        return itemEmpty;
    //    }
    //    index = index - ec;

    //    //the last 2 items are the taking place empty items.
    //    if (index >= stageInfoList.Count)
    //    {
    //        LoopListViewItem2 itemEmpty = listView.NewListViewItem("ItemPrefab1");
    //        itemEmpty.GetComponent<RectTransform>().sizeDelta = new(emptyWidth, 300);
    //        return itemEmpty;
    //    }

    //    StageInfo itemData = stageInfoList[index];
    //    if (itemData == null)
    //    {
    //        return null;
    //    }
    //    //get a new item. Every item can use a different prefab, the parameter of the NewListViewItem is the prefab’name. 
    //    //And all the prefabs should be listed in ItemPrefabList in LoopListView2 Inspector Setting
    //    LoopListViewItem2 item = listView.NewListViewItem("StageUIButton");
    //    StageUIButton itemScript = item.GetComponent<StageUIButton>();
    //    if (item.IsInitHandlerCalled == false)
    //    {
    //        item.IsInitHandlerCalled = true;
    //        //itemScript.Init();
    //    }

    //    itemScript.SetItemData(itemData, index);
    //    return item;
    //}

    //void LateUpdate()
    //{
    //    loopListView2.UpdateAllShownItemSnapData();
    //    int count = loopListView2.ShownItemCount;
    //    for (int i = 0; i < count; ++i)
    //    {
    //        LoopListViewItem2 item = loopListView2.GetShownItemByIndex(i);
    //        StageUIButton itemScript = item.GetComponent<StageUIButton>();
    //        if (itemScript != null)
    //        {
    //            float scale = 1 - Mathf.Abs(item.DistanceWithViewPortSnapCenter) / 800f;
    //            scale = Mathf.Clamp(scale, 0.9f, 1);
    //            float alpha = 1 - Mathf.Abs(item.DistanceWithViewPortSnapCenter) / 800f;
    //            alpha = Mathf.Clamp(alpha, 0.5f, 1);
    //            itemScript.mContentRootObj.GetComponent<CanvasGroup>().alpha = alpha;
    //            itemScript.mContentRootObj.transform.localScale = new Vector3(scale, scale, 1);
    //        }
    //    }
    //}

    //private Vector2 lastSize;

    //void Update()
    //{
    //    Vector2 currentSize = rectTransform.rect.size;
    //    if (currentSize != lastSize)
    //    {
    //        lastSize = currentSize;

    //        //loopListView2.RecycleAllItem(); // 아이템 전부 반환
    //        //loopListView2.SetListItemCount(dataList.Count, false); // 아이템 개수 재설정
    //        //loopListView2.RefreshAllShownItem(); // 현재 보여지는 아이템 갱신
    //        //loopListView2.MovePanelToItemIndex(0, 0); // 리스트 처음부터 재시작 (선택사항)
    //        //if (loopListView2.IsListViewInited)
    //        //{
    //        //    Resize();
    //        //    loopListView2.SetListItemCount(UserDataManager.MyCharacterDatasList.Count + emptyCount);
    //        //    loopListView2.RefreshAllShownItem();
    //        //}
    //        Resize();
    //        loopListView2.ResetListView(false);
    //        loopListView2.RefreshAllShownItem();

    //        //int c = Mathf.FloorToInt((viewport.rect.width * 0.5f - (itemSizeWidth * 0.5f)) / (itemPadding + itemSizeWidth)) * 2;

    //        //if(c != emptyCount)
    //        //{
    //        //    emptyCount = c;
    //        //    loopListView2.SetListItemCount(UserDataManager.MyCharacterDatasList.Count + emptyCount);
    //        //    loopListView2.RefreshAllShownItem();
    //        //}                    
    //    }

    //    if (Input.GetKey(KeyCode.Space))
    //    {
    //        loopListView2.MovePanelToItemIndex(2, 0);
    //        loopListView2.FinishSnapImmediately();
    //    }
    //}

}
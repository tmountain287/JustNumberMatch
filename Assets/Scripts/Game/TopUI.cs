using Common.Manager;
using JustOneMatch.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopUI : MonoBehaviour
{
    [SerializeField] private Button debugButton = null;

    [SerializeField] private Button settingButton = null;
    [SerializeField] private Button listButton = null;
    [SerializeField] private Button hintButton = null;
    [SerializeField] private Button resumeButton = null;

    [SerializeField] private Button exitButton = null;
    [SerializeField] private List<ItemStateBox> itemStateBoxList = null;

    // Start is called before the first frame update
    void Start()
    {
        //#if UNITY_EDITOR || TEST
        //        debugButton.onClick.AddListener(() =>
        //        {
        //            UIManager.Instance.OnUI(BaseUI.Type.DEBUG, true);
        //        });
        //        debugButton.enabled = true;
        //#else
        //            debugButton.enabled = false;
        //#endif

        settingButton.onClick.AddListener(() =>
        {
            PopupManager.Instance.OpenPopup<SettingPopup>().Initialize();
        });

        //listButton.onClick.AddListener(() =>
        //{
        //    PopupManager.Instance.OpenPopup<StagePopup>().Initialize(GameMgr.Instance.CurrentTableData.difficultyType);
        //});
    }

    public Transform GetIcon(ItemType _itemType)
    {
        return itemStateBoxList[(int)_itemType].IconTransform;
    }

    public void SetBlockAutoUpdate(bool autoUpdate)
    {
        itemStateBoxList.ForEach(item=>item.SetBlockAutoUpdate(autoUpdate));
    }
}

using Common.Manager;
using Common.UI;
using JustOneMatch.UI;
using UnityEngine;
using UnityEngine.UI;

public class ItemStateBox : MonoBehaviour
{
    [SerializeField] private RectTransform rect = null;
    [SerializeField] private ShopCategoryType shopCategoryType = ShopCategoryType.GoldPack;
    [SerializeField] private ItemType itemType = ItemType.Gold;
    [SerializeField] private NumberIncrement valueText = null;
    [SerializeField] private Button addButton = null;
    [SerializeField] private Button itemButton = null;
    [SerializeField] private Transform iconTransform = null;

    // --- 추가 부분 ---
    // 자동 업데이트를 잠시 막을지 여부
    private bool _blockAutoUpdate = false;
    // 막혀있는 동안 들어온 마지막 값
    private bool _hasPending;
    private int _pendingValue;

    public Transform IconTransform { get => iconTransform; }

    // -----------------

    private void OnValidate()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        addButton.onClick.AddListener(() =>
        {
            PopupManager.Instance.OpenPopup<ShopPopup>().Initialize(shopCategoryType);
        });
    }

    private void OnEnable()
    {
        UserDataManager.AddItemValueChangeEvent(itemType, SetValue);
        valueText.SetNumber(UserDataManager.GetItemCount(itemType), false);  // 처음엔 즉시 세팅
    }

    private void OnDisable()
    {
        UserDataManager.RemoveItemValueChangeEvent(itemType, SetValue);
    }

    private void SetValue()
    {
        int count = UserDataManager.GetItemCount(itemType);

        // 막혀 있으면 넘버링은 안 돌리고 값만 기억
        if (_blockAutoUpdate)
        {
            _pendingValue = count;
            _hasPending = true;
            return;
        }

        // 평소처럼 바로 넘버링
        valueText.SetNumber(count);
    }

    /// <summary>
    /// 넘버링 자동 업데이트를 켜고/끄는 함수
    /// </summary>
    public void SetBlockAutoUpdate(bool block)
    {
        _blockAutoUpdate = block;

        // 다시 열었는데 pending 값이 있으면 여기서 한 번만 넘버링
        if (!block && _hasPending)
        {
            valueText.SetNumber(_pendingValue);
            _hasPending = false;
        }
    }

    /// <summary>
    /// 외부에서 "지금 값으로 한 번 갱신해!" 하고 강제로 호출하고 싶을 때
    /// </summary>
    public void ForceRefreshNow()
    {
        int count = UserDataManager.GetItemCount(itemType);
        _pendingValue = count;
        _hasPending = false;
        _blockAutoUpdate = false;
        valueText.SetNumber(count);
    }
}

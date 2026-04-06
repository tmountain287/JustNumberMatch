using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SkillItemButton : MonoBehaviour
{
    [SerializeField] private ItemType itemType = ItemType.Gold;
    [SerializeField] private Button button = null;
    [SerializeField] private Text countText = null;
    [SerializeField] private GameObject countObj = null;
    [SerializeField] private GameObject adsObj = null;
    [SerializeField] private GameObject goldObj = null;
    [SerializeField] private Text goldValue = null;
    [SerializeField] private GameObject disableObj = null;

    [SerializeField] private GameObject useCountObj = null;
    [SerializeField] private Text useCountText = null;

    private Action onUseAction = null;
    private Action onGoldUseAction = null;
    private Action onAdsAction = null;

    private int goldNeedValue = -1;

    // --- 추가 부분 ---
    // 자동 업데이트를 잠시 막을지 여부
    private bool _blockAutoUpdate = false;
    // 막혀있는 동안 들어온 마지막 값
    private bool _hasPending;
    private int _pendingValue;
    // -----------------

    private void OnValidate()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
    }

    private void Start()
    {
        button.onClick.AddListener(() =>
        {
            int count = UserDataManager.GetItemCount(itemType);
            if (count > 0 || UserDataManager.GetItemCount(ItemType.Gold) >= goldNeedValue)
            {
                if (remainCount > 0)
                {
                    remainCount--;
                    SetCount();
                }
                onUseAction?.Invoke();
            }
            else
            {
                onAdsAction?.Invoke();
            }
        });
    }

    private void OnEnable()
    {
        if (goldNeedValue == -1)
        {
            goldNeedValue = TableDataManager.Instance.TableShopData.ShopDataList.Where(x => x.itemType == itemType && x.value == 1).FirstOrDefault().needValue;
            goldValue.text = goldNeedValue.ToString();
        }

        UserDataManager.AddItemValueChangeEvent(itemType, SetCount);
        UserDataManager.AddItemValueChangeEvent(ItemType.Gold, SetCount);

        SetCount();
    }

    private void OnDisable()
    {
        UserDataManager.RemoveItemValueChangeEvent(itemType, SetCount);
        UserDataManager.RemoveItemValueChangeEvent(ItemType.Gold, SetCount);
    }

    private GameModeType gameModeType;
    private int remainCount = 0;
    private int maxCount = 0;

    public void SetButton(GameModeType _gameModeType, Action _onUseAction, Action _onAdsAction, int _maxCount = 0)
    {
        gameModeType = _gameModeType;
        onUseAction = _onUseAction;
        onAdsAction = _onAdsAction;

        if (_maxCount > 0)
        {
            maxCount = _maxCount;
            remainCount = _maxCount;
        }
    }

    public void SetCount()
    {
        int count = UserDataManager.GetItemCount(itemType);

        // 막혀 있으면 넘버링은 안 돌리고 값만 기억
        if (_blockAutoUpdate)
        {
            _pendingValue = count;
            _hasPending = true;
            return;
        }

        if (gameModeType == GameModeType.STAGE || gameModeType == GameModeType.BOSS_STAGE)
        {
            useCountObj.SetActive(false);

            if (count > 0)
            {
                countText.text = count > 99 ? "99+" : count.ToString();
                countObj.SetActive(true);
                goldObj.SetActive(false);
                adsObj.SetActive(false);
            }
            else if (UserDataManager.GetItemCount(ItemType.Gold) >= goldNeedValue)
            {
                countObj.SetActive(false);
                goldObj.SetActive(true);
                adsObj.SetActive(false);
            }
            else
            {
                countObj.SetActive(false);
                goldObj.SetActive(false);
                adsObj.SetActive(true);
            }
        }
        else
        {
            useCountObj.SetActive(true);
            useCountText.text = $"{remainCount}/{maxCount}";

            if (remainCount == 0)
            {
                countObj.SetActive(false);
                goldObj.SetActive(false);
                disableObj.SetActive(true);
            }
            else
            {
                if (count > 0)
                {
                    countText.text = count > 99 ? "99+" : count.ToString();
                    countObj.SetActive(true);
                    goldObj.SetActive(false);
                    adsObj.SetActive(false);
                }
                else if (UserDataManager.GetItemCount(ItemType.Gold) >= goldNeedValue)
                {
                    countObj.SetActive(false);
                    goldObj.SetActive(true);
                    adsObj.SetActive(false);
                }
                else
                {
                    countObj.SetActive(false);
                    goldObj.SetActive(false);
                    disableObj.SetActive(true);
                }
            }
        }
    }

    public void SetBlockAutoUpdate(bool block)
    {
        _blockAutoUpdate = block;

        // 다시 열었는데 pending 값이 있으면 여기서 한 번만 넘버링
        if (!block && _hasPending)
        {
            if (_pendingValue > 0)
            {
                countText.text = _pendingValue > 99 ? "99+" : _pendingValue.ToString();
                countObj.SetActive(true);
                goldObj.SetActive(false);
                adsObj.SetActive(false);
            }
            else if (UserDataManager.GetItemCount(ItemType.Gold) >= goldNeedValue)
            {
                countObj.SetActive(false);
                goldObj.SetActive(true);
                adsObj.SetActive(false);
            }
            else
            {
                countObj.SetActive(false);
                goldObj.SetActive(false);
                adsObj.SetActive(true);
            }
            _hasPending = false;
        }
    }

    public void SetEnable(bool _enable)
    {
        button.gameObject.SetActive(_enable);
        disableObj.SetActive(!_enable);
    }
}

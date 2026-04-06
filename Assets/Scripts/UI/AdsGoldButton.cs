using Common.Manager;
using JustOneMatch.UI;
using UnityEngine;
using UnityEngine.UI;

public class AdsGoldButton : MonoBehaviour
{    
    [SerializeField] private Button button = null;
    [SerializeField] private GameObject countObj = null;
    [SerializeField] private Text countText = null;
    [SerializeField] private Text rewardValueText = null;

    [SerializeField] private GameObject disable = null;
    [SerializeField] private MidnightCountdown midnightCountdown = null;

    private void Start()
    {
        rewardValueText.text = $"x{ConfigData.AdsRewardGold}";

        button.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowRewardedAd((adapter) =>
            {
                UserDataManager.PlayAdsRewardGold();
                PopupManager.Instance.OpenPopup<AdsRewardCompletePopup>();
            }, null, "gold");
        });        
    }

    private void OnEnable()
    {
        SetCount(UserDataManager.UserData.adsRewardGoldPlay.RemainCount);
        UserDataManager.OnAdsRewardGoldChanged += SetCount;
    }

    private void OnDisable()
    {
        UserDataManager.OnAdsRewardGoldChanged -= SetCount;
    }

    private void SetCount(int _count)
    {
        if (_count > 0)
        {
            button.enabled = true;
            countText.text = _count.ToString();
            countObj.SetActive(true);
            disable.SetActive(false);
        }
        else
        {
            button.enabled = false;
            disable.SetActive(true);
            midnightCountdown.onFinished = ()=> UserDataManager.RefreshAdsRewardGold();
            countObj.SetActive(false);
        }
    }
}

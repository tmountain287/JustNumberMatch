using Common.Manager;using Common.UI;
using JustOneMatch.UI;
using UnityEngine;
using UnityEngine.UI;

public class BottomUI : MonoBehaviour
{
    [SerializeField] private Button adRemoveButton = null;
    [SerializeField] private Button shopButton = null;
    [SerializeField] private Button achievementsButton = null;
    [SerializeField] private Button leaderboardButton = null;
    [SerializeField] private Button timeAttackButton = null;
    [SerializeField] private Button timeTrialButton = null;

    void Start()
    {
        achievementsButton.onClick.AddListener(() =>
        {
            PlatformSocialManager.Instance.ShowAchievementsUI(() =>
            {
#if UNITY_IOS
                            PopupManager.Instance.OpenMessageBoxPopup(LocalizationManager.Instance.GetText("Failed to connect to Game Center"), LocalizationManager.Instance.GetText("Failed Game Center Achievements"));
#else
                PopupManager.Instance.OpenMessageBoxPopup(LocalizationManager.Instance.GetText("Failed to connect to Google Play Games"), LocalizationManager.Instance.GetText("Failed Google Play Achievements"));
#endif
            });
        });

        leaderboardButton.onClick.AddListener(() =>
        {
            PlatformSocialManager.Instance.ShowLeaderboardsUI(() =>
            {
#if UNITY_IOS
                            PopupManager.Instance.OpenMessageBoxPopup(LocalizationManager.Instance.GetText("Failed to connect to Game Center"), LocalizationManager.Instance.GetText("Failed Game Center Leaderboard"));
#else
                PopupManager.Instance.OpenMessageBoxPopup(LocalizationManager.Instance.GetText("Failed to connect to Google Play Games"), LocalizationManager.Instance.GetText("Failed Google Play Leaderboard"));
#endif

            });
        });

        shopButton.onClick.AddListener(() =>
        {
            PopupManager.Instance.OpenPopup<ShopPopup>().Initialize();
        });

        timeAttackButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowUI(BaseUI.Type.TIMEATTACT);
        });

        adRemoveButton.onClick.AddListener(() =>
        {
            PopupManager.Instance.OpenPopup<AdRemovePopup>();
        });

        timeTrialButton.onClick.AddListener(() =>
        {
            PopupManager.Instance.OpenPopup<SurvivalModePopup>().Initialize(() =>
            {
                GameMgr.Instance.StartInfiniteMode();
            });
        });
    }

    private void OnEnable()
    {
        SetAdRemoveButton();
        UserDataManager.OnValueAdsFreeChanged += SetAdRemoveButton;
        UserDataManager.OnValueFirstAdsOpenChanged += SetAdRemoveButton;
    }

    private void OnDisable()
    {
        UserDataManager.OnValueAdsFreeChanged -= SetAdRemoveButton;
        UserDataManager.OnValueFirstAdsOpenChanged -= SetAdRemoveButton;
    }

    private void SetAdRemoveButton()
    {
        adRemoveButton.gameObject.SetActive(!UserDataManager.IsAdsFree && UserDataManager.FirstAdsOpen);
    }
}
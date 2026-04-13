using Common.Manager;
using UI.Popup;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 세일 조건이 맞을 때만 표시되고, 남은 시간(00d 00h 00m 00s)이 갱신됩니다.
/// 스크립트가 멈추지 않게 하려면 <see cref="visibilityRoot"/>에 버튼·타이머가 들어 있는 자식을 지정하세요.
/// 비어 있으면 같은 오브젝트의 CanvasGroup으로 숨기거나, 없으면 이 GameObject를 SetActive 합니다.
/// </summary>
public class AdsSaleButton : MonoBehaviour
{
    [Tooltip("켜고 끌 UI 루트(버튼+타이머 묶음).")]
    [SerializeField] private GameObject visibilityRoot = null;

    [SerializeField] private Button button = null;
    [SerializeField] private Text remainingTimeValueText = null;

    private CanvasGroup cachedCanvasGroup;
    private bool lastOfferState;
    private bool offerStateInitialized;

    private void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(() =>
            {
                PopupManager.Instance.OpenPopup<AdSaleRemovePopup>();
            });
        }
    }

    private void OnEnable()
    {
        UserDataManager.OnValueAdsFreeChanged += OnSaleEligibilityChanged;
        UserDataManager.OnValueFirstAdsOpenChanged += OnSaleEligibilityChanged;
        offerStateInitialized = false;
        Refresh(forceVisibility: true);
    }

    private void OnDisable()
    {
        UserDataManager.OnValueAdsFreeChanged -= OnSaleEligibilityChanged;
        UserDataManager.OnValueFirstAdsOpenChanged -= OnSaleEligibilityChanged;
    }

    private void OnSaleEligibilityChanged()
    {
        Refresh(forceVisibility: true);
    }

    private void Update()
    {
        Refresh(forceVisibility: false);
    }

    private void Refresh(bool forceVisibility)
    {
        bool offer = UserDataManager.ShouldOfferPremiumPackSale();

        if (forceVisibility || !offerStateInitialized || offer != lastOfferState)
        {
            lastOfferState = offer;
            offerStateInitialized = true;
            SetVisible(offer);
        }

        if (!offer)
            return;

        if (remainingTimeValueText != null)
        {
            var r = UserDataManager.GetPremiumPackSaleRemainingTimeSpan();
            int totalSeconds = Mathf.Max(0, (int)r.TotalSeconds);
            int d = totalSeconds / 86400;
            int h = (totalSeconds % 86400) / 3600;
            int m = (totalSeconds % 3600) / 60;
            int s = totalSeconds % 60;
            remainingTimeValueText.text = $"{d}d {h:00}h {m:00}m {s:00}s";
        }
    }

    private void SetVisible(bool visible)
    {
        if (visibilityRoot != null)
        {
            visibilityRoot.SetActive(visible);
            return;
        }

        if (cachedCanvasGroup == null)
            cachedCanvasGroup = GetComponent<CanvasGroup>();
        if (cachedCanvasGroup != null)
        {
            cachedCanvasGroup.alpha = visible ? 1f : 0f;
            cachedCanvasGroup.interactable = visible;
            cachedCanvasGroup.blocksRaycasts = visible;
            return;
        }

        gameObject.SetActive(visible);
    }
}

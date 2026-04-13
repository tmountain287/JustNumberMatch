using Common.Manager;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class InGameTopUI : MonoBehaviour
    {
        [SerializeField] private Button debugButton = null;
        [SerializeField] private Button storeButton = null;
        [SerializeField] private Button menuButton = null;
        [SerializeField] private Text levelText = null;
        [SerializeField] private NumberIncrement goldValue = null;

        [SerializeField] private Button goldAdButton = null; 
        [SerializeField] private Button goldAdPlusButton = null;

        // Start is called before the first frame update
        void Start()
        {
//#if UNITY_EDITOR || TEST
//            debugButton.onClick.AddListener(() =>
//            {
//                UIManager.Instance.OnUI(BaseUI.Type.DEBUG, true);
//            });
//            debugButton.enabled = true;
//#else
//            debugButton.enabled = false;
//#endif

            storeButton.onClick.AddListener(() =>
            {
                PopupManager.Instance.OpenPopup<ShopPopup>().Initialize();
            });

            menuButton.onClick.AddListener(() =>
            {
                PopupManager.Instance.OpenPopup<SettingPopup>().Initialize();
            });

            goldAdButton.onClick.AddListener(() =>
            {
               // PopupManager.Instance.OpenPopup<AdRewardPopup>().Initialize();
            });

            goldAdPlusButton.onClick.AddListener(() =>
            {
              //  PopupManager.Instance.OpenPopup<AdRewardPopup>().Initialize();
            });
        }

        private void OnEnable()
        {
            SetLevel();
            goldValue.SetNumber(UserDataManager.Gold, false);
           // UserDataManager.OnValueLevelChanged.AddListener(SetLevel);
           // UserDataManager.OnValueGoldChanged.AddListener(SetGold);
        }

        private void OnDisable()        
        {
           // UserDataManager.OnValueLevelChanged.RemoveListener(SetLevel);
           // UserDataManager.OnValueGoldChanged.RemoveListener(SetGold);
        }

        public void SetLevel()
        {
            levelText.text = UserDataManager.Level.ToString();
        }

        public void SetGold(int _value, bool _isAni)
        {
            goldValue.SetNumber(_value, _isAni);
        }

        //public void IconAnimation()
        //{
        //    Icon.DOKill(true);
        //    Icon.DOScale(new Vector3(1.2f, 1.2f, 1.0f), 0.3f).SetEase(Ease.OutBack).From();
        //}
    }
}

using Common.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class MiniGameButton : MonoBehaviour
    {
        [SerializeField] private InGameUI inGameUI = null;
        [SerializeField] private Button button = null;
        [SerializeField] private GameObject countObj = null;
        [SerializeField] private Text countText = null;

        //private void Start()
        //{
        //    button.onClick.AddListener(() =>
        //    {
        //        Refrsh();
        //        SoundManager.Instance.StopBgm();
        //        PopupManager.Instance.OpenPopup<CarpGamePopup>().Initialize(()=>
        //        {
        //            inGameUI.PlayBGM();
        //        });
        //    });
        //    InvokeRepeating(nameof(Refrsh), 0f, 10f);
        //}

        //private void OnEnable()
        //{
        //    SetCount(UserDataManager.UserData.miniGameData.playCount);
        //    UserDataManager.OnValueMiniGamePlayCountChanged.AddListener(SetCount);
        //}

        //private void OnDisable()
        //{            
        //    UserDataManager.OnValueMiniGamePlayCountChanged.RemoveListener(SetCount);
        //}

        //private void SetCount(int _count)
        //{
        //    int freeTotal = ConfigData.MiniGameOneDayFreeCount + ConfigData.MiniGameOneDayADFreeCount - _count;

        //    if(freeTotal > 0)
        //    {
        //        countText.text = freeTotal.ToString();
        //        countObj.SetActive(true);
        //    }
        //    else
        //    {
        //        countObj.SetActive(false);
        //    }
        //}

        //private void Refrsh()
        //{
        //    UserDataManager.RefreshMiniGame();
        //}
    }
}
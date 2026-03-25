using Common.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class SuddaButton : MonoBehaviour
    {
        [SerializeField] private InGameUI inGameUI = null;
        [SerializeField] private Button button = null;
        [SerializeField] private GameObject countObj = null;
        [SerializeField] private Text countText = null;

        private void Start()
        {
            button.onClick.AddListener(() =>
            {
                Refrsh();
                SoundManager.Instance.StopBgm();
                PopupManager.Instance.OpenPopup<SuddaPopup>().Initialize(()=>
                {
                    inGameUI.PlayBGM();
                });
            });
            InvokeRepeating(nameof(Refrsh), 0f, 10f);
        }

        private void OnEnable()
        {
            SetCount(UserDataManager.UserData.suddaData.playCount);
            UserDataManager.OnValueSuddaPlayCountChanged.AddListener(SetCount);
        }

        private void OnDisable()
        {            
            UserDataManager.OnValueSuddaPlayCountChanged.RemoveListener(SetCount);
        }

        private void SetCount(int _count)
        {
            int freeTotal = ConfigData.SuddaOneDayFreeCount + ConfigData.SuddaOneDayADFreeCount - _count;

            if(freeTotal > 0)
            {
                countText.text = freeTotal.ToString();
                countObj.SetActive(true);
            }
            else
            {
                countObj.SetActive(false);       
            }
        }

        private void Refrsh()
        {
            UserDataManager.RefreshSudda();
        }
    }
}
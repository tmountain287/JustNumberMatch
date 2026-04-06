
using Common.Manager;
using Common.UI;
using Cysharp.Threading.Tasks;
using Holdem.UI.Popup;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JustOneMatch.UI
{
    public class SettingPopup : BasePopup
    {
        [SerializeField] private SettingToggle bgmToggle = null;
        [SerializeField] private SettingToggle fxToggle = null;

        [SerializeField] private Slider bgmSlider = null;
        [SerializeField] private Slider fxSlider = null;

        [SerializeField] private Button cheerButton = null;
        [SerializeField] private Button inviteButton = null;
        [SerializeField] private Button accountDelButton = null;

        [SerializeField] private Button restorePurchasesButton = null;

        [SerializeField] private GameObject latestObj = null;
        [SerializeField] private Button updateButton = null;

        [SerializeField] private List<Button> flagButtonList = null;

        private bool isAccountMessage = false;

        private Action closeAddAction = null;

        protected override void OnEnable()
        {
            base.OnEnable();
            GameAnalyticsHelper.LogSettingOpen();
        }

        protected override void Start()
        {
            base.Start();

            bgmToggle.OnToggle = (_flag) =>
            {
                SoundManager.Instance.BgmVolume = _flag ? 1 : 0;

                bgmSlider.onValueChanged.RemoveAllListeners();

                bgmSlider.value = _flag ? 1 : 0;

                bgmSlider.onValueChanged.AddListener((value) =>
                {
                    if (SoundManager.Instance.BgmVolume != value)
                    {
                        SoundManager.Instance.BgmVolume = value;
                        PlayerPrefsManager.Instance.SetPlayerPrefsInfo(PrefsKey.BGM, value);
                        bgmToggle.SetToggleActive(value > 0);
                    }
                });
            };

            fxToggle.OnToggle = (_flag) =>
            {
                SoundManager.Instance.FxVolume = _flag ? 1 : 0;

                fxSlider.onValueChanged.RemoveAllListeners();

                fxSlider.value = _flag ? 1 : 0;

                fxSlider.onValueChanged.AddListener((value) =>
                {
                    if (SoundManager.Instance.FxVolume != value)
                    {
                        SoundManager.Instance.FxVolume = value;
                        PlayerPrefsManager.Instance.SetPlayerPrefsInfo(PrefsKey.FX_Sound, value);
                        fxToggle.SetToggleActive(value > 0);
                    }
                });
            };

            bgmSlider.onValueChanged.AddListener((value) =>
            {
                if (SoundManager.Instance.BgmVolume != value)
                {
                    SoundManager.Instance.BgmVolume = value;
                    PlayerPrefsManager.Instance.SetPlayerPrefsInfo(PrefsKey.BGM, value);
                    bgmToggle.SetToggleActive(value > 0);
                }
            });

            fxSlider.onValueChanged.AddListener((value) =>
            {
                if (SoundManager.Instance.FxVolume != value)
                {
                    SoundManager.Instance.FxVolume = value;
                    PlayerPrefsManager.Instance.SetPlayerPrefsInfo(PrefsKey.FX_Sound, value);
                    fxToggle.SetToggleActive(value > 0);
                }
            });

            for (int i = 0; i < flagButtonList.Count; i++)
            {
                int index = i;
                flagButtonList[i].onClick.AddListener(()=>
                {
                    flagButtonList.ForEach(x =>
                    {
                        x.transform.GetChild(0).gameObject.SetActive(false);
                    });

                    flagButtonList[index].transform.GetChild(0).gameObject.SetActive(true);

                    PlayerPrefsManager.Instance.SetPlayerPrefsInfo(PrefsKey.Language, index);
                });
            }


            inviteButton.onClick.AddListener(() =>
            {
                new NativeShare()
                    .SetSubject(LocalizationManager.Instance.GetText("AppName"))
                    .SetText(string.Format(LocalizationManager.Instance.GetText("InviteMessage"), LocalizationManager.Instance.GetText("AppName"), AppDefine.STORE_APP_URL))
                    .SetTitle(LocalizationManager.Instance.GetText("Share App"))
                    .Share();
            });

            accountDelButton.onClick.AddListener(() =>
            {
                PopupManager.Instance.OpenPopup<AccountDelPopup>();
            });

           

            updateButton.onClick.AddListener(() =>
            {
                Application.OpenURL(AppDefine.STORE_APP_URL);
            });

            cheerButton.onClick.AddListener(() =>
            {
                Application.OpenURL(AppDefine.STORE_APP_URL);
            });

            restorePurchasesButton.onClick.AddListener(async () =>
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    PopupManager.Instance.OpenMessageBoxPopup("", LocalizationManager.Instance.GetText("Please check network"));
                    return;
                }

                UIManager.Instance.ShowLoading();
                try
                {
                    await NetworkManager.Instance.RecoveryPendingItem();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[SettingPopup] RecoveryPendingItem failed: {ex.Message}");
                    PopupManager.Instance.OpenMessageBoxPopup("", LocalizationManager.Instance.GetText("Please check network"));
                }
                finally
                {
                    UIManager.Instance.HideLoading();
                }
            });
        }

       
        public void Initialize()
        {
            bgmSlider.value = SoundManager.Instance.BgmVolume;
            fxSlider.value = SoundManager.Instance.FxVolume;

            for (int i = 0; i < flagButtonList.Count; i++)
            {
                flagButtonList[i].transform.GetChild(0).gameObject.SetActive(i == (int)PlayerPrefsManager.Instance.GetPlayerPrefsInfo(PrefsKey.Language).Value);
            }            

#if UNITY_ANDROID
            accountDelButton.gameObject.SetActive(false);
            restorePurchasesButton.gameObject.SetActive(false);
#elif UNITY_IOS
            accountDelButton.gameObject.SetActive(true);
            restorePurchasesButton.gameObject.SetActive(true);
#else
            accountDelButton.gameObject.SetActive(false);
            restorePurchasesButton.gameObject.SetActive(false);
#endif

            float currentVersion = float.Parse(PlayerPrefs.GetString("Version", "0"));

            updateButton.gameObject.SetActive(currentVersion > float.Parse(Application.version));
            latestObj.SetActive(currentVersion <= float.Parse(Application.version));

            closeAddAction = null;
           // CharacterResManager.Instance.SetImage(charImage, TableDataManager.Instance.TableCharacterData.GetCharacterTableData(UserDataManager.SelectIndex).resource, CharacterImage.Type.LobbyUI);

            string uid = PlayerPrefs.GetString("UserUID", "");
            bool isGoogleLogin = !string.IsNullOrEmpty(uid);

            if (!isGoogleLogin && !isAccountMessage)
            {
                //AlarmMessgeManager.Instance.OnMessage("<color=#FF8436>계정연동</color>을 진행하여\n게임정보를 안전하게 저장하세요.");
                isAccountMessage = true;
            }           
        }
    }
}
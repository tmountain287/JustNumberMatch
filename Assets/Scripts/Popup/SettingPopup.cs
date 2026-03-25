using backend_cli.BackEnd;
using Common.Manager;
using Common.UI;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class SettingPopup : BasePopup
    {
        [SerializeField] private Image charImage = null;
        [SerializeField] private Text nickName = null;
        [SerializeField] private NumberIncrement moneyValue = null;
        [SerializeField] private Text goldValue = null;

        [SerializeField] private Button nickNameChangeButton = null;

        [SerializeField] private SettingToggle bgmToggle = null;
        [SerializeField] private SettingToggle fxToggle = null;
        [SerializeField] private PushToggle alarmToggle = null;

        [SerializeField] private Button playerInfoButton = null;
        [SerializeField] private Button achievementsButton = null;
        [SerializeField] private Button rankingButton = null;

        [SerializeField] private Button cheerButton = null;
        [SerializeField] private Button inviteButton = null;
        [SerializeField] private Button supportButton = null;
        [SerializeField] private Button quitButton = null;
        [SerializeField] private Button accountDelButton = null;

        [SerializeField] private Button restorePurchasesButton = null;

        [SerializeField] private GameObject latestObj = null;
        [SerializeField] private Button updateButton = null;

        [SerializeField] private Text userIDTitle = null;
        [SerializeField] private Text userID = null;
        [SerializeField] private GameObject serverIDObj = null;
        [SerializeField] private Text serverID = null;

        [SerializeField] private Button loginButton = null;
        [SerializeField] private Button logoutButton = null;

        [SerializeField] private Button close2Button = null;

        private bool isAccountMessage = false;

        private Action closeAddAction = null;

        protected override void OnEnable()
        {
            base.OnEnable();
            UserDataManager.OnValueNickNameChanged.AddListener(SetNickName);
            UserDataManager.OnValueMoneyChanged.AddListener(SetMoney);
        }

        private void OnDisable()
        {
            UserDataManager.OnValueNickNameChanged.RemoveListener(SetNickName);
            UserDataManager.OnValueMoneyChanged.RemoveListener(SetMoney);
        }

        public void SetMoney(long _value, bool _isAni)
        {
            moneyValue.SetNumber(_value, _isAni);
        }

        public void SetNickName()
        {
            nickName.text = UserDataManager.NickName;
        }

        protected override void Start()
        {
            base.Start();

            nickNameChangeButton.onClick.AddListener(() =>
            {
                PopupManager.Instance.OpenPopup<NickNameChangePopup>().Initialize();
            });

            bgmToggle.OnToggle = (_flag) =>
            {
                SoundManager.Instance.BgmVolume = _flag ? 1 : 0;
            };

            fxToggle.OnToggle = (_flag) =>
            {
                SoundManager.Instance.FxVolume = _flag ? 1 : 0;
            };

            alarmToggle.OnToggle = (_flag) =>
            {
                string message = $"{DateTime.Now:yyyy년 M월 d일} 광고성 정보가\n<color=#FF8436>수신 {(_flag ? "동의" : "거부")}</color> 처리되었습니다.";
                AlarmMessgeManager.Instance.OnMessage(message);
                UserDataManager.IsPush = _flag;
                UserDataManager.Save(true, () =>
                {
                    _ = NetworkManager.Instance.PushAgree(FirebasePushReceiver.Instance.PushToken, _flag);
                });
            };

            playerInfoButton.onClick.AddListener(() =>
            {
                PopupManager.Instance.OpenPopup<MyRecordPopup>().Initialize();
            });

            achievementsButton.onClick.AddListener(() =>
            {
                PlatformSocialManager.Instance.ShowAchievementsUI(() =>
                {
#if UNITY_IOS
                    PopupManager.Instance.OpenMessageBoxPopup("Game Center 연결 실패", "Game Center를 연결해야\n업적 메뉴를 이용할 수 있습니다.");
#else
                    PopupManager.Instance.OpenMessageBoxPopup("Play게임 서비스 연결 실패", "구글Play게임 서비스를 연결해야\n업적 메뉴를 이용할 수 있습니다.");
#endif
                });
            });

            rankingButton.onClick.AddListener(() =>
            {
                PlatformSocialManager.Instance.ShowLeaderboardsUI(() =>
                {
#if UNITY_IOS
                    PopupManager.Instance.OpenMessageBoxPopup("Game Center 연결 실패", "Game Center를 연결해야\n랭킹 메뉴를 이용할 수 있습니다.");
#else
                    PopupManager.Instance.OpenMessageBoxPopup("Play게임 서비스 연결 실패", "구글Play게임 서비스를 연결해야\n랭킹 메뉴를 이용할 수 있습니다.");
#endif

                });
            });

            inviteButton.onClick.AddListener(() =>
            {
                new NativeShare()
                    .SetSubject("최고수맞고")
                    .SetText($"최고수맞고\n힘든일 모두 잊고 즐거운 맞고 한판!!\n지금 바로 플레이하세요\n{AppDefine.STORE_APP_URL}")
                    .SetTitle("앱 공유")
                    .Share();
            });

            quitButton.onClick.AddListener(() =>
            {
                PopupManager.Instance.OpenPopup<ExitPopup>();
            });

            accountDelButton.onClick.AddListener(() =>
            {
                PopupManager.Instance.OpenPopup<AccountDelPopup>();
            });

            loginButton.onClick.AddListener(() =>
            {
                UIManager.Instance.ShowLoading();

                PlatformLoginReceiver.Instance.StartLogin(() =>
                {
#if UNITY_IOS
                    AsyncLinkAccount(FederationType.APPLE);
#else
                    AsyncLinkAccount(FederationType.GOOGLE);
#endif

                    //RefreshAccountInfo();
                    //UIManager.Instance.HideLoading();

                },
                (error) =>
                {
                    UIManager.Instance.HideLoading();
#if UNITY_IOS
                    if (error == "1001") //사용자 취소 (12501 안드로이드) (-5 IOS)
#else
                    if (error == "12501") //사용자 취소 (12501 안드로이드) (-5 IOS)
#endif
                    {
                        //취소함
                        return;
                    }

                    if (string.IsNullOrEmpty(error))
                        error = "계정연동에 실패했습니다.\n고객센터에 문의해주세요.";
                    PopupManager.Instance.OpenMessageBoxPopup("", error);
                });
            });


            logoutButton.onClick.AddListener(() =>
            {
                Action onLogOutSuccess = () =>
                {
                    UserDataManager.ClearData();
                    NetworkManager.Instance.ClearData();

                    PopupManager.Instance.OpenMessageBoxPopup("", "사용계정이 로그아웃 되었습니다.\n게임이 다시 시작됩니다.", () =>
                    {
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    });
                };
#if UNITY_IOS
                UIManager.Instance.ShowLoading();
                PlatformLoginReceiver.Instance.LogOut(()=>
                {
                    onLogOutSuccess?.Invoke();
                    UIManager.Instance.HideLoading();
                }, (error)=>
                {
                    UIManager.Instance.HideLoading();
                    if (error == "1001")
                        return;
                    PopupManager.Instance.OpenMessageBoxPopup("", $"로그아웃이 실패했습니다.\n고객센터에 문의해주세요.\n{error}");
                });
#else
                PlatformLoginReceiver.Instance.LogOut();
                onLogOutSuccess?.Invoke();
#endif
                //RefreshAccountInfo();
            });

            updateButton.onClick.AddListener(() =>
            {
                Application.OpenURL(AppDefine.STORE_APP_URL);
            });

            cheerButton.onClick.AddListener(() =>
            {
                Application.OpenURL(AppDefine.STORE_APP_URL);
            });

            supportButton.onClick.AddListener(() =>
            {
                Application.OpenURL(AppDefine.SUPPORT_URL);
            });

            close2Button.onClick.AddListener(() =>
            {
                closeAddAction?.Invoke();
                ClosePopup();
            });

            restorePurchasesButton.onClick.AddListener(async () =>
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    PopupManager.Instance.OpenMessageBoxPopup("", "온라인 상태에서만\n가능합니다.\n네트워크 연결 후\n다시 진행해 주세요.");
                    return;
                }

                UIManager.Instance.ShowLoading();

                if (!NetworkManager.Instance.IsLogined)
                {
                    var (bro, result) = await NetworkManager.Instance.Login(SystemInfo.deviceUniqueIdentifier);
                    if (bro.IsSuccess() && result != null)
                    {

                    }
                    else
                    {
                        UIManager.Instance.HideLoading();
                        PopupManager.Instance.OpenMessageBoxPopup("", $"로그인에 실패했습니다.\n잠시 후 다시 시도해주세요.\n({bro.errorCode})");
                        return;
                    }
                }

                await NetworkManager.Instance.RecoveryPendingItem((message) =>
                {
                    PopupManager.Instance.OpenMessageBoxPopup("", message);
                });
                UIManager.Instance.HideLoading();

            });
        }

        private async void AsyncLinkAccount(string _federationType)
        {
            var (bro, result) = await NetworkManager.Instance.LinkAccount(PlatformLoginReceiver.Instance.Token, _federationType);
            if (bro.IsSuccess() && result != null)
            {
                string emali = PlayerPrefs.GetString("UserEmail");

                string strUserEmail = !string.IsNullOrEmpty(emali) ? emali : "******@****.***";

                if (result.record != null && result.record.last_us_key != NetworkManager.Instance.US_KEY && result.record.record != SecurePlayerPrefs.Encrypt(JsonUtility.ToJson(UserDataManager.UserData)))
                {
                    //데이터 충돌
                    UIManager.Instance.HideLoading();
                    UserData beforeData = JsonUtility.FromJson<UserData>(SecurePlayerPrefs.Decrypt(result.record.record));
                    PopupManager.Instance.OpenPopup<SelectUserPopup>().Initialize(beforeData, UserDataManager.UserData, (isCurrent) =>
                    {
                        RefreshAccountInfo();

                        if (!isCurrent)
                        {
                            InGameManager.Instance.RestartGameReady();

                            closeAddAction = () =>
                            {
                                InGameManager.Instance.GameStart();
                            };
                        }

                        PopupManager.Instance.OpenMessageBoxPopup("알림", $"{strUserEmail}으로\n연동 되었습니다.");
                    });
                }
                else
                {
                    PopupManager.Instance.OpenMessageBoxPopup("알림", $"{strUserEmail}으로\n연동 되었습니다.");
                    await NetworkManager.Instance.SaveRecord();
                    RefreshAccountInfo();
                    UIManager.Instance.HideLoading();
                }
            }
            else
            {
                UIManager.Instance.HideLoading();
                PopupManager.Instance.OpenMessageBoxPopup("알림", "계정연동에 실패했습니다.\n고객센터에 문의 바랍니다.");
#if !UNITY_IOS
                PlatformLoginReceiver.Instance.LogOut();
#else
                PlatformLoginReceiver.Instance.DeleteUserInfo();
#endif
            }
        }

        private void RefreshAccountInfo()
        {
            //계정연동후 비행기 모드면 토큰이 없다
            //bool isGoogleLogin = !string.IsNullOrEmpty(GoogleLoginReceiver.Instance.Token);
                        
            bool isGoogleLogin = PlatformLoginReceiver.Instance.IsLinking;

            string strUserEmail = PlayerPrefs.GetString("UserEmail");

            if (isGoogleLogin)
            {         
                if(string.IsNullOrEmpty(strUserEmail))
                {
                    userIDTitle.text = "서버 ID :";
                    userID.text = PlayerPrefs.GetString("us_id");
                }
                else
                {
                    userIDTitle.text = "계정 ID :";
                    userID.text = strUserEmail;
                }
            }
            else
            {
                userIDTitle.text = "게스트 ID :";
                userID.text = SystemInfo.deviceUniqueIdentifier;
            }
            
            loginButton.gameObject.SetActive(!isGoogleLogin);
            logoutButton.gameObject.SetActive(isGoogleLogin);
        }

        public void Initialize()
        {
#if UNITY_ANDROID
            quitButton.gameObject.SetActive(true);
            accountDelButton.gameObject.SetActive(false);
            restorePurchasesButton.gameObject.SetActive(false);
#elif UNITY_IOS
            quitButton.gameObject.SetActive(false);
            accountDelButton.gameObject.SetActive(true);
            restorePurchasesButton.gameObject.SetActive(true);
#else
            quitButton.gameObject.SetActive(true);
            accountDelButton.gameObject.SetActive(false);
            restorePurchasesButton.gameObject.SetActive(false);
#endif

            float currentVersion = float.Parse(PlayerPrefs.GetString("Version", "0"));

            updateButton.gameObject.SetActive(currentVersion > float.Parse(Application.version));
            latestObj.SetActive(currentVersion <= float.Parse(Application.version));

            closeAddAction = null;
            CharacterResManager.Instance.SetImage(charImage, TableDataManager.Instance.TableCharacterData.GetCharacterTableData(UserDataManager.SelectIndex).resource, CharacterImage.Type.LobbyUI);

            string uid = PlayerPrefs.GetString("UserUID", "");
            bool isGoogleLogin = !string.IsNullOrEmpty(uid);

            if (!isGoogleLogin && !isAccountMessage)
            {
                AlarmMessgeManager.Instance.OnMessage("<color=#FF8436>계정연동</color>을 진행하여\n게임정보를 안전하게 저장하세요.");
                isAccountMessage = true;
            }

            SetNickName();
            moneyValue.SetNumber(UserDataManager.Money, false);
            goldValue.text = UserDataManager.Gold.FormatComma();
            RefreshAccountInfo();
        }
    }
}
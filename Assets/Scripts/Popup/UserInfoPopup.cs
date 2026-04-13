using Common.Manager;
using Common.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Popup
{
    public class UserInfoPopup : BasePopup
    {       
        [SerializeField] private LocalChangeTextEvent userIDTitle = null;
        [SerializeField] private Text userID = null;
        [SerializeField] private Button editButton = null;

        [SerializeField] private Button loginButton = null;
        [SerializeField] private Button logoutButton = null;

        [SerializeField] private List<Toggle> profileToggles = null;
        [SerializeField] private Transform profliePanel = null;

        private int profileIndex;
        private string nickName;

        private void OnValidate()
        {
            profileToggles ??= profliePanel.GetComponentsInChildren<Toggle>().ToList();
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            GameAnalyticsHelper.LogUserInfoOpen();
        }

        protected override void Start()
        {
            base.Start();

            editButton.onClick.AddListener(() =>
            {
                PopupManager.Instance.OpenPopup<NickNameChangePopup>().Initialize(()=>
                {
                    if(profileIndex != UserDataManager.ProfileIndex ||  nickName != UserDataManager.NickName)
                    {
                        UserDataManager.Save();
                    }
                });
            });

            loginButton.onClick.AddListener(() =>
            {
                if (FirebaseManager.Instance.CanLink)
                {
                    UIManager.Instance.ShowLoading();

                    PlatformLoginReceiver.Instance.StartLogin(() =>
                    {
#if UNITY_IOS
                    //AsyncLinkAccount(FederationType.APPLE);
#else
                        //AsyncLinkAccount(FederationType.GOOGLE);
                        AsyncLinkAccount();
#endif
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
                            error = LocalizationManager.Instance.GetText("Failed to link");
                        PopupManager.Instance.OpenMessageBoxPopup("", error);
                    });
                }
                else
                {
                    string error = LocalizationManager.Instance.GetText("Failed to link");
                    PopupManager.Instance.OpenMessageBoxPopup("", error);
                }
            });


            logoutButton.onClick.AddListener(() =>
            {
                Action onLogOutSuccess = () =>
                {
                    UserDataManager.ClearData();
                    //NetworkManager.Instance.ClearData();

                    PopupManager.Instance.OpenMessageBoxPopup("", LocalizationManager.Instance.GetText("has been logged out"), () =>
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
                    PopupManager.Instance.OpenMessageBoxPopup("", string.Format(LocalizationManager.Instance.GetText("Faild log out"), error));
                });
#else
                PlatformLoginReceiver.Instance.LogOut();
                onLogOutSuccess?.Invoke();
#endif
                //RefreshAccountInfo();
            });
        }

        private async void AsyncLinkAccount()
        {
            Action fail = () =>
            {
                UIManager.Instance.HideLoading();
                PopupManager.Instance.OpenMessageBoxPopup("", LocalizationManager.Instance.GetText("Failed to link"));
#if !UNITY_IOS
                PlatformLoginReceiver.Instance.LogOut();
#else
                    PlatformLoginReceiver.Instance.DeleteUserInfo();
#endif
            };

#if UNITY_IOS
                    
#else
            var res1 = await NetworkManager.Instance.LinkAccountAsync();
#endif
            Debug.Log(res1.error);
            if (res1.success)
            {
                var result = await NetworkManager.Instance.SaveUserDataAsync(false);

                if (result == null)
                {
                    fail.Invoke();
                }
                else
                {
                    string emali = PlayerPrefs.GetString("UserEmail");

                    string strUserEmail = !string.IsNullOrEmpty(emali) ? emali : "******@****.***";
                    string data = SecurePlayerPrefs.Encrypt(UserDataManager.UserData);

                    if (result.Type == SaveResultType.Success)
                    {
                        PopupManager.Instance.OpenMessageBoxPopup(LocalizationManager.Instance.GetText("alram"), string.Format(LocalizationManager.Instance.GetText("Successfully linked"), strUserEmail));
                        await NetworkManager.Instance.ApplyOverwriteAsync(data);
                        RefreshAccountInfo();
                        UIManager.Instance.HideLoading();
                    }
                    else if (result.Type == SaveResultType.PermissionDenied && result.ConflictRecord != null)
                    {
                        if (result.ConflictRecord != data)
                        {
                            //데이터 충돌
                            UIManager.Instance.HideLoading();
                            UserData beforeData = JsonConvert.DeserializeObject<UserData>(SecurePlayerPrefs.Decrypt(result.ConflictRecord));
                            PopupManager.Instance.OpenPopup<SelectUserPopup>().Initialize(beforeData, UserDataManager.UserData, (isCurrent) =>
                            {
                                RefreshAccountInfo();

                                //if (!isCurrent)
                                //{
                                //    InGameManager.Instance.RestartGameReady();

                                //    closeAddAction = () =>
                                //    {
                                //        InGameManager.Instance.GameStart();
                                //    };
                                //}

                                PopupManager.Instance.OpenMessageBoxPopup(LocalizationManager.Instance.GetText("alram"), string.Format(LocalizationManager.Instance.GetText("Successfully linked"), strUserEmail));
                            });
                        }
                        else
                        {
                            PopupManager.Instance.OpenMessageBoxPopup(LocalizationManager.Instance.GetText("alram"), string.Format(LocalizationManager.Instance.GetText("Successfully linked"), strUserEmail));
                            await NetworkManager.Instance.ApplyOverwriteAsync(data);
                            RefreshAccountInfo();
                            UIManager.Instance.HideLoading();
                        }
                    }
                    else
                    {
                        fail.Invoke();
                    }
                }
            }
            else
            {
                fail.Invoke();
            }
        }

        private void RefreshAccountInfo()
        {
            //계정연동후 비행기 모드면 토큰이 없다
            //bool isGoogleLogin = !string.IsNullOrEmpty(GoogleLoginReceiver.Instance.Token);

            bool isGoogleLogin = FirebaseManager.Instance.IsLinking;

            string strUserEmail = PlayerPrefs.GetString("UserEmail");

            if (isGoogleLogin)
            {
                if (string.IsNullOrEmpty(strUserEmail))
                {
                    userIDTitle.EntryKey = "ServerID";
                    userID.text = PlayerPrefs.GetString("us_id");
                }
                else
                {
                    userIDTitle.EntryKey = "AccountID";
                    userID.text = strUserEmail;
                }
            }
            else
            {
                userIDTitle.EntryKey = "GuestID";
                userID.text = SystemInfo.deviceUniqueIdentifier;
            }

            loginButton.gameObject.SetActive(!isGoogleLogin);
            logoutButton.gameObject.SetActive(isGoogleLogin);
        }

        public void Initialize()
        {
            profileIndex = UserDataManager.ProfileIndex;
            nickName = UserDataManager.NickName;

            profileToggles[UserDataManager.ProfileIndex].isOn = true;
            RefreshAccountInfo();
        }
    }
}
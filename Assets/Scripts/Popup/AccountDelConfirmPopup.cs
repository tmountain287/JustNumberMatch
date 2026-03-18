using Common.Manager;
using Common.UI;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Holdem.UI.Popup
{
    public class AccountDelConfirmPopup : BasePopup
    {
        [SerializeField] private Button okButton = null;
        [SerializeField] private Text uid = null;
        [SerializeField] private Text accountId = null;

        protected override void Start()
        {
            base.Start();
            okButton.onClick.AddListener(async () => { await OnConfirmDeleteAsync(); }); 
        }

        private async Task OnConfirmDeleteAsync()
        {
            ClosePopup();

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                PopupManager.Instance.OpenMessageBoxPopup("", LocalizationManager.Instance.GetText("Account deletion online"));
                return;
            }

            UIManager.Instance.ShowLoading();

            string usId = FirebaseUserData.UID;
            if (string.IsNullOrEmpty(usId))
            {
                UIManager.Instance.HideLoading();
                PopupManager.Instance.OpenMessageBoxPopup("", LocalizationManager.Instance.GetText("Account deletion fail"));
                return;
            }

            try
            {
                // 1) Firestore userdata 문서 삭제 (인증된 본인만 가능)
                bool firestoreDeleted = await FirestoreDiag.Instance.DeleteUserDataAsync(usId);
                if (!firestoreDeleted)
                {
                    UIManager.Instance.HideLoading();
                    PopupManager.Instance.OpenMessageBoxPopup("", string.Format(LocalizationManager.Instance.GetText("Account deletion fail"), "Firestore"));
                    return;
                }

                // 2) Firebase Auth 계정 삭제 (선택: 계정 완전 삭제)
                if (FirebaseManager.Instance != null && FirebaseManager.Instance.Auth?.CurrentUser != null)
                {
                    try
                    {
                        await FirebaseManager.Instance.Auth.CurrentUser.DeleteAsync();
                    }
                    catch (Exception authEx)
                    {
                        Debug.LogWarning($"[AccountDel] Firebase Auth 삭제 실패(무시 가능): {authEx.Message}");
                        FirebaseManager.Instance.Auth?.SignOut();
                    }
                }

                // 3) 로컬 데이터 초기화
                UserDataManager.ClearData();
                FirebaseUserData.DeleteUserInfo();

                UIManager.Instance.HideLoading();
                PopupManager.Instance.OpenMessageBoxPopup("", LocalizationManager.Instance.GetText("Account deletion completed"), () =>
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                });
            }
            catch (Exception ex)
            {
                UIManager.Instance.HideLoading();
                Debug.LogError($"[AccountDel] {ex.Message}");
                PopupManager.Instance.OpenMessageBoxPopup("", string.Format(LocalizationManager.Instance.GetText("Account deletion fail"), ex.Message));
            }
        }

        public void Initialize()
        {
            string strUserEmail = PlayerPrefs.GetString("UserEmail");
            uid.text = $"UID : {SystemInfo.deviceUniqueIdentifier}";

            if (FirebaseUserData.IsLinked)
            {
                if (string.IsNullOrEmpty(strUserEmail))
                {
                    accountId.text = string.Format(LocalizationManager.Instance.GetText("ServerID"), PlayerPrefs.GetString("us_id"));
                }
                else
                {
                    accountId.text = string.Format(LocalizationManager.Instance.GetText("AccountID"), strUserEmail);
                }
            }
            else
                accountId.text = string.Format(LocalizationManager.Instance.GetText("AccountID"), LocalizationManager.Instance.GetText("Not linked"));
        }
    }
}
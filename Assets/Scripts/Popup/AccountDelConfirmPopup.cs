using Common.Manager;
using Common.UI;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class AccountDelConfirmPopup : BasePopup
    {
        [SerializeField] private Button okButton = null;
        [SerializeField] private Text uid = null;
        [SerializeField] private Text accountId = null;

        protected override void Start()
        {
            base.Start();
            okButton.onClick.AddListener(async () =>
            {
                ClosePopup();

                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    PopupManager.Instance.OpenMessageBoxPopup("", "온라인 상태에서만\n계정 삭제가 가능합니다.\n네트워크 연결 후\n다시 진행해 주세요.");
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
                        PopupManager.Instance.OpenMessageBoxPopup("", $"계정삭제에 실패했습니다.\n잠시 후 다시 시도해주세요.\n({bro.errorCode})");
                        UIManager.Instance.HideLoading();
                        return;
                    }
                }

                Action onLogOutSuccess = async () =>
                {
                    var (bro, result) = await NetworkManager.Instance.AccountDelete();
                    UIManager.Instance.HideLoading();
                    if (bro.IsSuccess())
                    {
                        UserDataManager.ClearData();
                        NetworkManager.Instance.ClearData();

                        PopupManager.Instance.OpenMessageBoxPopup("", "계정삭제가 완료되었습니다.\n게임이 다시 시작됩니다.", () =>
                        {
                            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                        });
                    }
                    else
                    {
                        PopupManager.Instance.OpenMessageBoxPopup("", $"계정삭제에 실패했습니다.\n잠시 후 다시 시도해주세요.\n({bro.errorCode})");
                    }
                };

                if (PlatformLoginReceiver.Instance.IsLinking)
                {
                    PlatformLoginReceiver.Instance.LogOut(() =>
                    {
                        onLogOutSuccess?.Invoke();
                    }, (error) =>
                    {
                        UIManager.Instance.HideLoading();
                        if (error == "1001")
                        {
                            return;
                        }
                        PopupManager.Instance.OpenMessageBoxPopup("", $"로그아웃이 실패하여\n계정삭제에 실패했습니다.\n고객센터에 문의해주세요.\n{error}");
                    });
                }
                else
                {
                    onLogOutSuccess?.Invoke();
                }
            }); 
        }

        public void Initialize()
        {
            string strUserEmail = PlayerPrefs.GetString("UserEmail");
            uid.text = $"UID : {SystemInfo.deviceUniqueIdentifier}";

            if (PlatformLoginReceiver.Instance.IsLinking)
            {
                if (string.IsNullOrEmpty(strUserEmail))
                {
                    accountId.text = $"서버 ID : {PlayerPrefs.GetString("us_id")}";
                }
                else
                {
                    accountId.text = $"게정 ID : {strUserEmail}";
                }
            }
            else
                accountId.text = "계정 ID : 미연동";
        }
    }
}
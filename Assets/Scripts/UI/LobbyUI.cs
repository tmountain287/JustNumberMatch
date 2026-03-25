using backend_cli.BackEnd;
using Common.Manager;
using Common.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gostop.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : BaseUI
{
    [SerializeField] private Image charImage = null;
    [SerializeField] private Image logo = null;

    [SerializeField] private Slider progressbar = null;
    [SerializeField] private Text progressText = null;

    void Start()
    {        
        StartCoroutine(LobbyStart());
    }

    private int AndroidVersion()
    {
        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            return version.GetStatic<int>("SDK_INT");
        }
    }

    IEnumerator LobbyStart()
    {
        bool hasUserData = UserDataManager.Load();

        int charIndex = hasUserData ? UserDataManager.SelectIndex : ConfigData.InitCharacterID;

        CharacterResManager.Instance.SetImage(charImage, TableDataManager.Instance.TableCharacterData.GetCharacterTableData(charIndex).resource, CharacterImage.Type.LobbyUI);
        Color c = Color.black;
        c.a = 0;
        charImage.color = c;
        charImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        charImage.DOFade(1, 0.5f);
        charImage.DOColor(Color.white, 0.3f).SetDelay(0.8f);

        yield return new WaitForSeconds(1.0f);
        logo.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        RequestUserPermission();
        progressText.text = "정보 초기화 중...";
        progressbar.gameObject.SetActive(true);
        progressbar.DOValue(0.1f, 1f);
        yield return new WaitForSeconds(0.5f);

        if (hasUserData)
        {
            TryLCheckAsync();
        }
        else
        {
            EnforceLCheckAsync();
        }
    }
 
    private async void EnforceLCheckAsync()
    {
        progressText.text = "버전 체크 중...";
        progressbar.DOValue(0.2f, 1f);

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            PopupManager.Instance.OpenMessageBoxPopup("알림", "최초 실행시에는 네트워크 연결이 필요합니다.", () =>
            {
                Application.Quit();
            });
            return;
        }

        {
            var (bro, result) = await NetworkManager.Instance.CheckAppVersion();

            if (bro.IsSuccess() && result != null)
            {
                if (float.Parse(Application.version) < float.Parse(result.minVersion))  //버전이 낮아서 강제 업데이트 해야됨
                {
                    PopupManager.Instance.OpenPopup<UpdatePopup>();
                    return;
                }
            }
            else
            {
                PopupManager.Instance.OpenMessageBoxPopup("알림", $"최초 실행시에는 \n서버와 연결이 필요합니다.\n서버 연결에 실패했습니다.\n잠시 후 다시 시도해주세요.\n({bro.statusCode})", () =>
                {
                    Application.Quit();
                });
                return;
            }
        }
        {
            progressText.text = "서버 체크 중...";
            progressbar.DOValue(0.3f, 1f);
            var (bro, result) = await NetworkManager.Instance.CheckServer();

            if (bro.IsSuccess() && result != null)
            {
                if (result.status == "check")
                {
                    PopupManager.Instance.OpenMessageBoxPopup("알림", "서버점검중입니다.\n최초 실행시에는 서버와 연결이 필요합니다.", () =>
                    {
                        Application.Quit();
                    });
                    return;
                }
            }
            else
            {
                PopupManager.Instance.OpenMessageBoxPopup("알림", $"최초 실행시에는 \n서버와 연결이 필요합니다.\n서버 연결에 실패했습니다.\n잠시 후 다시 시도해주세요.\n({bro.statusCode})", () =>
                {
                    Application.Quit();
                });
                return;
            }
        }

        progressText.text = "약관 확인 중...";
        TryNoticeAsync();
        PopupManager.Instance.OpenPopup<AgreementPopup>().Initialize((isAgree) =>
        {
            EnforceLoginAsync(isAgree);
        });        
    }

    private async void TryLCheckAsync()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            StartProgressbar();
            return;
        }
        else
        {
            progressText.text = "버전 체크 중...";
            progressbar.DOValue(0.2f, 1f);
            {
                var (bro, result) = await NetworkManager.Instance.CheckAppVersion(2);

                if (bro.IsSuccess() && result != null)
                {
                    if (float.Parse(Application.version) < float.Parse(result.minVersion))  //버전이 낮아서 강제 업데이트 해야됨
                    {
                        PopupManager.Instance.OpenPopup<UpdatePopup>();
                        return;
                    }
                }
            }
        }
        TryNoticeAsync();
        TryLoginAsync();
    }

    private async void TryNoticeAsync()
    {
        CNetDocument.LoadShowNoticeInfoList();

        var (bro, result) = await NetworkManager.Instance.CheckNotice();

        if (bro.IsSuccess() && result != null)
        {
            foreach (var info in result)
            {
                if (info.alive == "Y" && info.imageUrl.StartsWith("https://"))
                {
                    if(!CNetDocument.ShowNoticeInfoList.showNoticeInfos.Any(x=> x.id == info.popupId && x.poupVer == info.popupVer))
                    {
                        CNetDocument.NoticeInfoQueue.Enqueue(info);
                    }

                    Debug.Log($"[{info.popupId}] *제목: {info.title}, *버전 : {info.popupVer}, *이미지: {info.imageUrl}, *링크: {info.linkUrl}");
                }
            }
        }
    }

    private async void EnforceLoginAsync(bool _isAgree)
    {
        progressText.text = "로그인 중...";
        var (bro, result) = await NetworkManager.Instance.Login(SystemInfo.deviceUniqueIdentifier);
        if (bro.IsSuccess() && result != null)
        {            
            UserDataManager.NewUserData(result.us_nick.Length >= 10 ? result.us_nick.Substring(0, 10) : result.us_nick);
            UserDataManager.IsPush = _isAgree;
            UserDataManager.Save();
            await NetworkManager.Instance.SaveRecord();
            NetworkManager.Instance.SendPushToken();
            StartProgressbar();
         }
        else
        {
            PopupManager.Instance.OpenMessageBoxPopup("알림", $"최초 실행시에는 \n서버와 연결이 필요합니다.\n서버 연결에 실패했습니다.\n잠시 후 다시 시도해주세요.\n({bro.statusCode})", () =>
            {
                Application.Quit();
            });
        }
    }

    private async void TryLoginAsync()
    {
        progressText.text = "로그인 중...";
        async UniTask HandleUserChoiceAsync()
        {
            progressText.text = "구매 복구 확인 중...";
            await NetworkManager.Instance.RecoveryPendingItem();
            await NetworkManager.Instance.SaveRecord(1);
            StartProgressbar();
        }

        progressbar.DOValue(0.4f, 1f);
        var (bro, result) = await NetworkManager.Instance.Login(SystemInfo.deviceUniqueIdentifier, 3);
        if (bro.IsSuccess() && result != null)
        {
            if (result.record != null && result.record.last_us_key != NetworkManager.Instance.US_KEY && result.record.record != SecurePlayerPrefs.Encrypt(JsonUtility.ToJson(UserDataManager.UserData)))
            {
                //데이터 충돌
                Debug.Log("데이터 충돌");
                UserData beforeData = JsonUtility.FromJson<UserData>(SecurePlayerPrefs.Decrypt(result.record.record));
                PopupManager.Instance.OpenPopup<SelectUserPopup>().Initialize(beforeData, UserDataManager.UserData, async (isCurrent) =>
                {
                    await HandleUserChoiceAsync();
                });
            }
            else
            {
                await HandleUserChoiceAsync();
            }
        }
        else
        {
            StartProgressbar();
        }
    }

   

    private void RequestUserPermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Android 13(API 33)+에서만 실행
        if (AndroidVersion() >= 33)
        {
            var permission = "android.permission.POST_NOTIFICATIONS";

            // 이미 권한이 있는지 확인
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(permission))
            {
                Debug.Log("알림 권한 요청 중...");
                UnityEngine.Android.Permission.RequestUserPermission(permission);
            }
            else
            {
                Debug.Log("알림 권한 이미 허용됨.");
            }
        }
#endif
    }

    private float currentProgress = 0;

    private void StartProgressbar()
    {
        currentProgress = progressbar.value;
        progressText.text = "게임 진입 중...";
        CharacterResManager.Instance.OnProgressChanged.AddListener(SetProgressbar);

        StartCoroutine(CharacterResManager.Instance.LoadAllCharacterVisualsAsync(()=>
        {
            CharacterResManager.Instance.OnProgressChanged.RemoveListener(SetProgressbar);
            progressbar.DOValue(1f, 1f).OnComplete(() =>
            {
                InGameManager.Instance.GameInit();
                UIManager.Instance.ShowUI(Type.GAME);
            });
        }));       
    }

    private void SetProgressbar(float _value)
    {
        progressbar.DOKill();
        progressbar.DOValue(currentProgress + _value * (1 - currentProgress), 0.05f);
    }
}
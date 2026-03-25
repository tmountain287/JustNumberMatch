using backend_cli.BackEnd;
using Common.Manager;
using Common.UI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class NoticePopup : BasePopup
    {
        [SerializeField] private Button linkButton = null;
        [SerializeField] private Image targetImage = null;
        [SerializeField] private Toggle toggle = null;
        [SerializeField] private GameObject loading = null;

        [SerializeField] private Button closeButton2 = null;

        private Coroutine coroutine = null;
        private AppPopUpInfo noticeInfo = null;
        private Action closeAction = null;        

        protected override void Start()
        {
            linkButton.onClick.AddListener(() =>
            {
                if (noticeInfo != null)
                {
                    Application.OpenURL(noticeInfo.linkUrl);
                }
            });

            closeButton.onClick.AddListener(() =>
            {
                if(toggle.isOn)
                {
                    CNetDocument.AddShowNoticeInfoList(new(DateTime.Now.ToString("yyyyMMdd"), noticeInfo.popupId, noticeInfo.popupVer));
                }

                if(CNetDocument.NoticeInfoQueue.Count > 0)
                {
                    Refresh(CNetDocument.NoticeInfoQueue.Dequeue());
                }
                else
                {
                    PopupManager.Instance.ClosePopup(PopupType.NONE, closeAction);
                }
            });

            closeButton2.onClick.AddListener(() =>
            {
                if (toggle.isOn)
                {
                    CNetDocument.AddShowNoticeInfoList(new(DateTime.Now.ToString("yyyyMMdd"), noticeInfo.popupId, noticeInfo.popupVer));
                }

                if (CNetDocument.NoticeInfoQueue.Count > 0)
                {
                    Refresh(CNetDocument.NoticeInfoQueue.Dequeue());
                }
                else
                {
                    PopupManager.Instance.ClosePopup(PopupType.NONE, closeAction);
                }
            });
        }

        public void Initialize(AppPopUpInfo _noticeInfo, Action _closeAction)
        {
            closeAction = _closeAction;
            Refresh(_noticeInfo);
        }

        public void Refresh(AppPopUpInfo _noticeInfo)
        {
            noticeInfo = _noticeInfo;
            toggle.isOn = false;

            linkButton.enabled = !string.IsNullOrEmpty(noticeInfo.linkUrl);

            targetImage.sprite = null;
            targetImage.gameObject.SetActive(false);

            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
            loading.SetActive(true);
            coroutine = StartCoroutine(LoadImageFromUrl(noticeInfo.imageUrl));
        }

        IEnumerator LoadImageFromUrl(string url)
        {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
            {
                yield return uwr.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                if (uwr.result != UnityWebRequest.Result.Success)
#else
                if (uwr.isNetworkError || uwr.isHttpError)
#endif
                {
                    Debug.Log("이미지 로드 실패: " + uwr.error);
                }
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

                    // Texture2D → Sprite 변환
                    Sprite sprite = Sprite.Create(texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f));

                    targetImage.sprite = sprite;
                    targetImage.gameObject.SetActive(true);
                }

                loading.SetActive(false);
            }
        }
    }
}
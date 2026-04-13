using Common.Manager;
using Common.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

namespace UI.Popup
{
    public class InGameFireMatchPopup : BasePopup
    {
        //[SerializeField] private Text levelText = null;
        //[SerializeField] private Text nameText = null;
       // [SerializeField] private CharImage character1 = null;
        //[SerializeField] private CharImage character2 = null;
        [SerializeField] private GameObject goldOffButton = null;
        
        [SerializeField] private Button goldButton = null;
        [SerializeField] private Button adButton = null;

        [SerializeField] private Button confirmButton = null;

        [SerializeField] private Text goldValueText = null;
        [SerializeField] private Text goldOffValueText = null;


        [SerializeField] private Text goldButtonText = null;
        [SerializeField] private Text adButtonText = null;

        [SerializeField] private Transform pivot1 = null;
        [SerializeField] private Transform pivot2 = null;

        [SerializeField] private Transform text1 = null;
        [SerializeField] private Transform text2 = null;

        [SerializeField] private Transform buttons = null;
        [SerializeField] private CanvasGroup message = null;

        [SerializeField] private GameObject fireSpine = null;

        [SerializeField] private GameObject Textx2 = null;
        [SerializeField] private GameObject Textx10 = null;

        [SerializeField] private Transform startText = null;

        private Action onClick = null;

        protected override void Start()
        {
            base.Start();
            //confirmButton.onClick.AddListener(() =>
            //{
            //    onClick?.Invoke();
            //});

            //goldButton.onClick.AddListener(() =>
            //{
            //    if (ConfigData.FireMutilpleGold <= UserDataManager.Gold)
            //    {
            //        InGameManager.Instance.SendReqFireMatchConfirm(true, true);
            //        ClosePopup();
            //    }
            //    else
            //    {
            //        PopupManager.Instance.OpenMessageBoxPopup("알림", "<color=#FF6600>골드</color>가 부족합니다.");
            //    }
            //});

            //adButton.onClick.AddListener(() =>
            //{
            //    UIManager.Instance.ShowRewardedAd((adapter) =>
            //    {
            //        _ = NetworkManager.Instance.AdReward(adapter, "FireMatch");
            //        InGameManager.Instance.SendReqFireMatchConfirm(true);
            //        ClosePopup();
            //    }, null, GameAnalyticsSDK.Models.AdPlacement.PlayStage, "FireMatch", 1);
            //});

            //goldValueText.text = ConfigData.FireMutilpleGold.ToString();
            //goldOffValueText.text = ConfigData.FireMutilpleGold.ToString();

            //goldButtonText.text = $"x{ConfigData.FireMaxMultiple}배 찬스";
            //adButtonText.text = $"x{ConfigData.FireMaxMultiple}배 찬스";
        }

        //public void Initialize(CharacterData _data1, CharacterData _data2, bool _useTicket, bool _isDebug = false)
        //{
        //    startText.localPosition = new Vector3(0, _useTicket ? -223 : -325, 0);

        //    goldButton.gameObject.SetActive(UserDataManager.Gold >= ConfigData.FireMutilpleGold);
        //    goldOffButton.gameObject.SetActive(UserDataManager.Gold < ConfigData.FireMutilpleGold);
        //    //levelText.text = $"LEVEL UP {_level}";
        //    //nameText.text = _data.name;


        //    onClick = _isDebug ? () =>
        //    {
        //        ClosePopup();
        //    }
        //    : () =>
        //    {
        //        InGameManager.Instance.SendReqFireMatchConfirm();
        //        ClosePopup();
        //    };

        //    Textx2.SetActive(!_useTicket);
        //    Textx10.SetActive(_useTicket);

        //    buttons.gameObject.SetActive(!_useTicket);

        //    goldButton.enabled = _isDebug == false;
        //    adButton.enabled = _isDebug == false;
          
        //    character1.SetImage(_data1.id);
        //    character2.SetImage(_data2.id);

        //    PlayAnimation();
        //}

        private Sequence sequence;

        public void PlayAnimation()
        {
            confirmButton.enabled = false;
            goldButton.enabled = false;
            adButton.enabled = false;

            pivot1.localPosition = new Vector3(500f, pivot1.localPosition.y, pivot1.localPosition.z);
            pivot2.localPosition = new Vector3(-500f, pivot2.localPosition.y, pivot2.localPosition.z);

            fireSpine.SetActive(false);

            text1.gameObject.SetActive(false);
            text2.gameObject.SetActive(false);

            buttons.localScale = new Vector3(1f, 0f, 1f);
            
            message.alpha = 0f;
            message.transform.localScale = Vector3.one * 0.8f;

            //Color charColor = character1.Image.color;
            //charColor.a = 0f;
            //character1.Image.color = charColor;

            //Color charColor2 = character2.Image.color;
            //charColor2.a = 0f;
            //character2.Image.color = charColor2;
            
            //sequence?.Kill();

            //sequence = DOTween.Sequence();

            //// pivot1 이동
            //sequence.Append(pivot1.DOLocalMoveX(285f, 0.2f).SetEase(Ease.OutBack));
            //sequence.Join(character2.Image.DOFade(1f, 0.2f));
            //// pivot2 이동
            //sequence.Append(pivot2.DOLocalMoveX(-285f, 0.2f).SetEase(Ease.OutBack));
            //sequence.Join(character1.Image.DOFade(1f, 0.2f));

            // text1 scale
            sequence.AppendCallback(() =>
            {
                text1.gameObject.SetActive(true);
                text1.transform.localScale = Vector3.one * 3f;
            });
            sequence.Append(text1.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).OnComplete(()=>
            {
                fireSpine.SetActive(true);
            }));

            // text2 scale
            sequence.AppendCallback(() =>
            {
                text2.gameObject.SetActive(true);
                text2.transform.localScale = Vector3.one * 3f;
            });
            sequence.Append(text2.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));

            // buttons + message 동시에 실행
            sequence.AppendCallback(() =>
            {
                buttons.DOScaleY(1f, 0.3f).SetEase(Ease.OutBack);

                message.DOFade(1f, 0.3f);
                message.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            });

            sequence.OnComplete(() =>
            {
                DOVirtual.DelayedCall(0.2f, () =>
                {
                    confirmButton.enabled = true;
                    goldButton.enabled = true;
                    adButton.enabled = true;
                });
            });

            sequence.SetAutoKill(true).OnKill(() => sequence = null);
            sequence.Play();
        }
    }
}
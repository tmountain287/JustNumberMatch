using Common.Manager;
using Common.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class InGameResultPopup : BasePopup
    {
        [SerializeField] private Transform panel = null;
        [SerializeField] private Transform charPivot = null;
        [SerializeField] private Image image = null;

        [SerializeField] private Text pointValueText = null;
        [SerializeField] private Text goldText = null;
        [SerializeField] private Text scoreText = null;

        [SerializeField] private Text detailScoreText = null;

        [SerializeField] private GameObject nomalScore = null;
        [SerializeField] private GameObject detailScore = null;

        [SerializeField] private Button detailButton = null;
        [SerializeField] private Button simplyButton = null;

        [SerializeField] private GameObject winButtons = null;
        [SerializeField] private GameObject failButtons = null;

        [SerializeField] private Button next2FreeButton = null;
        [SerializeField] private Button next2AdsButton = null;

        [SerializeField] private Text freeValueText = null;
        [SerializeField] private Text invalidValueText = null;

        [SerializeField] private Text next2FreeButtonText = null;
        [SerializeField] private Text next2AdsButtonText = null;

        [SerializeField] private Button invalidFreeButton = null;
        [SerializeField] private Button invalidAdsButton = null;

        [SerializeField] private Button nextButton = null;

        [SerializeField] private Button nextLevelButton = null;

        [SerializeField] private Button exitButton = null;
        [SerializeField] private Button shareButton = null;

        [SerializeField] private Transform buttonPanel = null;

        [SerializeField] private GameObject pasanObject = null;
        [SerializeField] private GameObject nonPasanObject = null;

        [SerializeField] private GameObject win1 = null;
        [SerializeField] private GameObject win2 = null;
        [SerializeField] private GameObject win3 = null;
        [SerializeField] private GameObject lose = null;

        [SerializeField] private GameObject fire = null;

        [SerializeField] private CanvasGroup tipPivot = null;
        [SerializeField] private GameObject tip = null;
        [SerializeField] private Text tipMessage = null;

        [SerializeField] private Transform textGroupPivot = null;

        [SerializeField] private AudioClip winSound = null;
        [SerializeField] private AudioClip loseSound = null;

        [SerializeField] private Transform etcButtonPivot = null;

        [SerializeField] private List<GameObject> nextText = null;
        [SerializeField] private List<GameObject> oneMoreText = null;

        [SerializeField] private Button itemFireButton = null;
        //[SerializeField] private Text itemFireText = null;
        [SerializeField] private Text itemFireCount = null;

        private bool isNextAd = false;

        protected override void OnEnable()
        {
            base.OnEnable();

            UserDataManager.OnValueFireTicketChanged.AddListener(SetFireTicket);
        }

        private void OnDisable()
        {
            win1.SetActive(false);
            win2.SetActive(false);
            win3.SetActive(false);
            lose.SetActive(false);
            UserDataManager.OnValueFireTicketChanged.RemoveListener(SetFireTicket);
        }

        private void SetFireTicket(int _count)
        {
            itemFireCount.text = $"(아이템 {_count}개 보유)";
        }

        protected override void Start()
        {
            base.Start();
            nextButton.onClick.AddListener(() =>
            {
                //NetworkManager.Instance.LobbySession.SendReqContinueGame();
                if (isNextAd)
                    GoogleAdManager.Instance.ShowInterstitialAd();
                InGameManager.Instance.SendReqNextGame(false, false);
                ClosePopup();
            });

            next2FreeButton.onClick.AddListener(() =>
            {
                if (nextGoldValue <= UserDataManager.Gold)
                {
                    InGameManager.Instance.SendReqNextGame(true, false);
                    ClosePopup();
                }
                else
                {
                    PopupManager.Instance.OpenMessageBoxPopup("알림", "<color=#FF6600>골드</color>가 부족합니다.");
                }
            });

            next2AdsButton.onClick.AddListener(() =>
            {
                UIManager.Instance.ShowRewardedAd((adapter) =>
                {
                    //두배튕기기
                    _ = NetworkManager.Instance.AdReward(adapter,"AddWinMultiple");
                    InGameManager.Instance.SendReqNextGame(true, true);
                    ClosePopup();
                });
            });

            invalidFreeButton.onClick.AddListener(() =>
            {
                if (ConfigData.InvalidGold <= UserDataManager.Gold)
                {
                    UserDataManager.SubGold(ConfigData.InvalidGold);
                    _ = NetworkManager.Instance.SendItemLog("GoldInvalid", -ConfigData.InvalidGold);
                    UIManager.Instance.ShowLoading();
                    UserDataManager.Save(true, () =>
                    {
                        InGameManager.Instance.SendReqInvalidGame();
                        UIManager.Instance.HideLoading();
                        ClosePopup();
                    });
                }
                else
                {
                    PopupManager.Instance.OpenMessageBoxPopup("알림", "<color=#FF6600>골드</color>가 부족합니다.");
                }
            });

            invalidAdsButton.onClick.AddListener(() =>
            {
                UIManager.Instance.ShowRewardedAd((adapter) =>
                {
                    _ = NetworkManager.Instance.AdReward(adapter,"InvalidGame");
                    InGameManager.Instance.SendReqInvalidGame();
                    ClosePopup();
                });
            });

            nextLevelButton.onClick.AddListener(() =>
            {
                if (isNextAd)
                    GoogleAdManager.Instance.ShowInterstitialAd();
                InGameManager.Instance.SendReqNextLevel();
                ClosePopup();
            });

            detailButton.onClick.AddListener(() =>
            {
                detailScore.SetActive(true);
                nomalScore.SetActive(false);

                detailButton.gameObject.SetActive(false);
                simplyButton.gameObject.SetActive(true);
            });

            simplyButton.onClick.AddListener(() =>
            {
                detailScore.SetActive(false);
                nomalScore.SetActive(true);

                detailButton.gameObject.SetActive(true);
                simplyButton.gameObject.SetActive(false);
            });

            exitButton.onClick.AddListener(() =>
            {
                PopupManager.Instance.OpenPopup<ExitPopup>();
            });


            invalidValueText.text = ConfigData.InvalidGold.ToString();

            shareButton.onClick.AddListener(() =>
            {
                StartCoroutine(TakeScreenshotAndShare());                
            });

            itemFireButton.onClick.AddListener(() =>
            {
                if(UserDataManager.FireTicket > 0)
                {
                    InGameManager.Instance.SendReqUseFiretTicket();
                    _ = NetworkManager.Instance.SendItemLog("FireTicketUse", -1);
                    ClosePopup();
                }
                else
                {
                    PopupManager.Instance.OpenPopup<ShopPopup>().Initialize();
                }
            });
        }

        private float panelScaleDuration = 0.4f;
        private float panelAfterDelay = 0.3f;
        private float moveDuration = 0.5f;
        private float tipDelay = 0.7f;

        private int nextGoldValue = 0;

        public void Initialize(CharacterTableData _winPlayer)
        {
            CharacterResManager.Instance.SetImage(image, _winPlayer.resource, CharacterImage.Type.Result);

            buttonPanel.gameObject.SetActive(false);
            charPivot.gameObject.SetActive(false);

            etcButtonPivot.localScale = new Vector3(1, 0, 1);
            etcButtonPivot.DOScaleY(1f, 0.2f).SetDelay(moveDuration + panelAfterDelay).SetEase(Ease.OutBack);

            panel.localScale = Vector3.zero;
            panel.DOScale(1f, panelScaleDuration).SetEase(Ease.OutBack);

            Color charColor2 = image.color;
            charColor2.a = 0f;
            image.color = charColor2;

            charPivot.localPosition = new Vector3(-500f, charPivot.localPosition.y, charPivot.localPosition.z);
            buttonPanel.localPosition = new Vector3(buttonPanel.localPosition.x, -150, buttonPanel.localPosition.z);

            charPivot.DOLocalMoveX(0, moveDuration).SetDelay(panelAfterDelay).OnStart(() => charPivot.gameObject.SetActive(true)).SetEase(Ease.InOutSine);
            image.DOFade(1f, moveDuration).SetDelay(panelAfterDelay);

            buttonPanel.DOLocalMoveY(0, moveDuration).SetDelay(panelAfterDelay).OnStart(() => buttonPanel.gameObject.SetActive(true)).SetEase(Ease.OutBack);

            textGroupPivot.gameObject.SetActive(false);
            textGroupPivot.localPosition = new Vector3(1500f, textGroupPivot.localPosition.y, textGroupPivot.localPosition.z);
            textGroupPivot.DOLocalMoveX(0, moveDuration - 0.2f).SetDelay(panelAfterDelay).OnStart(() => textGroupPivot.gameObject.SetActive(true)).SetEase(Ease.InOutSine);


            tipPivot.gameObject.SetActive(false);
            tipPivot.transform.localPosition = new Vector3(tipPivot.transform.localPosition.x, -30, tipPivot.transform.localPosition.z);

            tipPivot.alpha = 0;

            tipPivot.transform.DOLocalMoveY(0, moveDuration).SetDelay(tipDelay).OnStart(() => tipPivot.gameObject.SetActive(true)).SetEase(Ease.OutBack);
            tipPivot.DOFade(1f, moveDuration).SetDelay(tipDelay);

            tip.SetActive(true);

            win1.gameObject.SetActive(true);
        }

        public void Initialize(bool _isNextAd, netResultInfo _result, PlayerData _winPlayer, PlayerData _losePlayer, int _winMultiple, int _fireMultiple, bool _isNagari)
        {
            //next2FreeButton.interactable = UserDataManager.Gold >= ConfigData.NextMultipleGold;
            //invalidFreeButton.interactable = UserDataManager.Gold >= ConfigData.InvalidGold;

            buttonPanel.gameObject.SetActive(false);
            charPivot.gameObject.SetActive(false);

            etcButtonPivot.localScale = new Vector3(1, 0, 1);
            etcButtonPivot.DOScaleY(1f, 0.2f).SetDelay(moveDuration + panelAfterDelay).SetEase(Ease.OutBack);

            panel.localScale = Vector3.zero;
            panel.DOScale(1f, panelScaleDuration).SetEase(Ease.OutBack);

            Color charColor2 = image.color;
            charColor2.a = 0f;
            image.color = charColor2;

            charPivot.localPosition = new Vector3(-500f, charPivot.localPosition.y, charPivot.localPosition.z);
            buttonPanel.localPosition = new Vector3(buttonPanel.localPosition.x, -150, buttonPanel.localPosition.z);

            charPivot.DOLocalMoveX(0, moveDuration).SetDelay(panelAfterDelay).OnStart(() => charPivot.gameObject.SetActive(true)).SetEase(Ease.InOutSine);
            image.DOFade(1f, moveDuration).SetDelay(panelAfterDelay);

            buttonPanel.DOLocalMoveY(0, moveDuration).SetDelay(panelAfterDelay).OnStart(() => buttonPanel.gameObject.SetActive(true)).SetEase(Ease.OutBack);

            textGroupPivot.gameObject.SetActive(false);
            textGroupPivot.localPosition = new Vector3(1500f, textGroupPivot.localPosition.y, textGroupPivot.localPosition.z);
            textGroupPivot.DOLocalMoveX(0, moveDuration - 0.2f).SetDelay(panelAfterDelay).OnStart(() => textGroupPivot.gameObject.SetActive(true)).SetEase(Ease.InOutSine);

            CharacterResManager.Instance.SetImage(image, TableDataManager.Instance.TableCharacterData.GetCharacterTableData(_winPlayer.characterID).resource, CharacterImage.Type.Result);

            isNextAd = _isNextAd;

            nomalScore.SetActive(true);
            detailScore.SetActive(false);

            detailButton.gameObject.SetActive(true);
            simplyButton.gameObject.SetActive(false);

            if (_result.winSlotIndex == 0)
            {
                pasanObject.SetActive(_result.isBankruptcy);
                nonPasanObject.SetActive(!_result.isBankruptcy);
            }
            else
            {
                pasanObject.SetActive(false);
                nonPasanObject.SetActive(true);
            }


            pointValueText.text = $"점 {_result.pointValue.FormatKoreanUnits()}";
            scoreText.text = _result.multiPle > 1 ? $"{_result.score}점 X {_result.multiPle}배 = 총 {_result.score * _result.multiPle}점" :
                    $"{_result.score}점 = 총 {_result.score}점";

            List<CollectionData> collectionDatas = new List<CollectionData>();

            StringBuilder sb = new StringBuilder();

            if (_result.is3Bbuk)
            {
                sb.Append("3뻑7점");
                if (_fireMultiple > 1)
                {
                    sb.Append("x");
                    sb.Append("불꽃승부");
                    sb.Append(_fireMultiple);
                    sb.Append("배");
                }
                if (_isNagari)
                {
                    sb.Append("x나가리2배");
                }
                if (_winMultiple > 0)
                {
                    sb.Append("x");
                    sb.Append("다음판");
                    sb.Append((int)Mathf.Pow(2, _winMultiple));
                    sb.Append("배");
                }
            }
            else if (_result.isPresident)
            {
                sb.Append("총통7점");
                if (_fireMultiple > 1)
                {
                    sb.Append("x");
                    sb.Append("불꽃승부");
                    sb.Append(_fireMultiple);
                    sb.Append("배");
                }
                if (_isNagari)
                {
                    sb.Append("x나가리2배");
                }
                if (_winMultiple > 0)
                {
                    sb.Append("x");
                    sb.Append("다음판");
                    sb.Append((int)Mathf.Pow(2, _winMultiple));
                    sb.Append("배");
                }
            }
            else
            {
                _winPlayer.CollectionTypeList.ForEach(c =>
                {
                    CollectionData data = CollectionDataInfo.CollectionDataInfoList.Where(x => x.collectionType == c).FirstOrDefault();
                    if (data != null)
                    {
                        if (data.score > 0)
                        {
                            collectionDatas.Add(data);
                        }
                    }
                });

                for (int i = 0; i < collectionDatas.Count; i++)
                {
                    sb.Append(collectionDatas[i].name);
                    sb.Append(collectionDatas[i].score);
                    sb.Append("점");
                    if (i < collectionDatas.Count - 1)
                        sb.Append("+");
                }

                if (_winPlayer.MungScore > 0)
                {
                    if (sb.Length > 0)
                        sb.Append("+");
                    sb.Append("열끗");
                    sb.Append(_winPlayer.MungScore);
                    sb.Append("점");
                }
                if (_winPlayer.DdiScore > 0)
                {
                    if (sb.Length > 0)
                        sb.Append("+");
                    sb.Append("띠");
                    sb.Append(_winPlayer.DdiScore);
                    sb.Append("점");
                }
                if (_winPlayer.PeeScore > 0)
                {
                    if (sb.Length > 0)
                        sb.Append("+");
                    sb.Append("피");
                    sb.Append(_winPlayer.PeeScore);
                    sb.Append("점");
                }
                if (_winPlayer.goCount > 0)
                {
                    if (sb.Length > 0)
                        sb.Append("+");
                    sb.Append("고");
                    sb.Append(_winPlayer.goCount);
                    sb.Append("점");
                }

                if (_result.multiPle > 1)
                {
                    sb.Append("\n");
                    if (_fireMultiple > 1)
                    {
                        sb.Append("x");
                        sb.Append("불꽃승부");
                        sb.Append(_fireMultiple);
                        sb.Append("배");
                    }
                    if (_isNagari)
                    {
                        sb.Append("x나가리2배");
                    }
                    if (_winMultiple > 0)
                    {
                        sb.Append("x");
                        sb.Append("다음판");
                        sb.Append((int)Mathf.Pow(2, _winMultiple));
                        sb.Append("배");
                    }
                    if (_winPlayer.MissionMultiple > 1)
                    {
                        sb.Append("x");
                        sb.Append("미션");
                        sb.Append(_winPlayer.MissionMultiple);
                        sb.Append("배");
                    }
                    if (_winPlayer.MungMultiple > 1)
                    {
                        sb.Append("x");
                        sb.Append("멍따");
                        sb.Append(_winPlayer.MungMultiple);
                        sb.Append("배");
                    }
                    if (_winPlayer.ShakeMultiple > 1)
                    {
                        sb.Append("x");
                        sb.Append("흔들기");
                        sb.Append(_winPlayer.ShakeMultiple);
                        sb.Append("배");
                    }
                    if (_winPlayer.GoMultiple > 1)
                    {
                        sb.Append("x");
                        sb.Append(_winPlayer.goCount);
                        sb.Append("고");
                        sb.Append(_winPlayer.GoMultiple);
                        sb.Append("배");
                    }

                    if (_losePlayer.IsKwangBak && _winPlayer.KwangCount >= 3)
                    {
                        sb.Append("x");
                        sb.Append("광박2배");
                    }
                    if (_losePlayer.IsPeeBak && _winPlayer.PeeScore > 0)
                    {
                        sb.Append("x");
                        sb.Append("피박2배");
                    }
                    if (_losePlayer.IsGoBak)
                    {
                        sb.Append("x");
                        sb.Append("고박2배");
                    }
                }
            }

            detailScoreText.text = sb.ToString();

            if (_result.winSlotIndex == 0)
            {
                if (UserDataManager.SkillDataDic[SkillType.MONEY_UP] > 0)
                {
                    goldText.text = $"<color=#FFFF00>+{_result.reward.FormatKoreanUnits()}</color> <size=40><color=#00C9FF>({UserDataManager.SkillDataDic[SkillType.MONEY_UP]}%UP)</color></size>";
                }
                else
                {
                    goldText.text = $"<color=#FFFF00>+{_result.reward.FormatKoreanUnits()}</color>";
                }
            }
            else
            {
                goldText.text = $"<color=#FF0000>-{_result.reward.FormatKoreanUnits()}</color>";
            }

            int winMul = (int)Mathf.Pow(2, _winMultiple + 1);

            winMul = Mathf.Min(winMul, 8);

            if (_result.winSlotIndex == 0 && !_result.isBankruptcy)
            {
                next2FreeButtonText.text = $"<color=#FFD800>{winMul}</color>배";
                next2AdsButtonText.text = $"<color=#FFD800>{winMul}</color>배";

                for (int i = 0; i < 2; i++)
                {
                    nextText[i].SetActive(_winMultiple != 3);
                    oneMoreText[i].SetActive(_winMultiple == 3);
                }


                if (_winMultiple == 3)
                {
                    nextGoldValue = ConfigData.NextMultipleGold * (int)Mathf.Pow(2, _winMultiple - 1) + ConfigData.NextMultipleGold;

                    freeValueText.text = nextGoldValue.ToString();
                }
                else
                {
                    nextGoldValue = ConfigData.NextMultipleGold * (int)Mathf.Pow(2, _winMultiple);
                    freeValueText.text = nextGoldValue.ToString();
                }
            }

            if(_result.winSlotIndex == 0 && !_result.isBankruptcy && _result.isNextFire)
            {
                tipMessage.text = _winMultiple == 3 ? $"<color=#EEC302><size=40>'{winMul}배 한번더' </size></color>에 도전하세요!" : $"<color=#EEC302><size=40>'다음판 {winMul}배' </size></color>에 도전하세요!";

                tipPivot.gameObject.SetActive(false);
                tipPivot.transform.localPosition = new Vector3(tipPivot.transform.localPosition.x, -30, tipPivot.transform.localPosition.z);

                tipPivot.alpha = 0;

                tipPivot.transform.DOLocalMoveY(0, moveDuration).SetDelay(tipDelay).OnStart(() => tipPivot.gameObject.SetActive(true)).SetEase(Ease.OutBack);
                tipPivot.DOFade(1f, moveDuration).SetDelay(tipDelay);

                tip.SetActive(true);
            }
            else
            {
                tip.SetActive(false);
            }

            winButtons.SetActive(_result.winSlotIndex == 0);
            failButtons.SetActive(_result.winSlotIndex != 0);

            if (_result.winSlotIndex == 0)
            {
                int baseScore = _result.score * _result.multiPle / (int)Mathf.Pow(2, _winMultiple) / _fireMultiple;

                if (baseScore >= 100)
                {
                    win2.SetActive(true);
                }
                else if (baseScore >= 200)
                {
                    win3.SetActive(true);
                }
                else
                {
                    win1.SetActive(true);
                }

                SoundManager.Instance.PlayFX(winSound);
            }
            else
            {
                lose.SetActive(true);
                SoundManager.Instance.PlayFX(loseSound);
            }

            fire.SetActive(!_result.isBankruptcy && _result.isNextFire);

            if (!_result.isBankruptcy && !_result.isNextFire)
            {
                tipPivot.gameObject.SetActive(false);
                tipPivot.transform.localPosition = new Vector3(tipPivot.transform.localPosition.x, -30, tipPivot.transform.localPosition.z);

                tipPivot.alpha = 0;

                tipPivot.transform.DOLocalMoveY(0, moveDuration).SetDelay(tipDelay).OnStart(() => tipPivot.gameObject.SetActive(true)).SetEase(Ease.OutBack);
                tipPivot.DOFade(1f, moveDuration).SetDelay(tipDelay);

                SetFireTicket(UserDataManager.FireTicket);
            }

            itemFireButton.gameObject.SetActive(!_result.isBankruptcy && !_result.isNextFire);
        }


        private string MakeSampleScreenShotImage()
        {
            Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            ss.Apply();

            string filename = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string filePath = Path.Combine(Application.temporaryCachePath, filename);
            File.WriteAllBytes(filePath, ss.EncodeToPNG());

            Destroy(ss);

            return filePath;
        }


        private IEnumerator TakeScreenshotAndShare()
        {
            yield return new WaitForEndOfFrame();


            string filePath = MakeSampleScreenShotImage();
            string filename = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            new NativeShare().AddFile(filePath)
            .SetSubject(filename).SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
                .Share();

            // Share on WhatsApp only, if installed (Android only)
            //if( NativeShare.TargetExists( "com.whatsapp" ) )
            //    new NativeShare().AddFile( filePath ).AddTarget( "com.whatsapp" ).Share();
        }
    }
}
using Common.Manager;
using Common.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class SuddaJokboInfo
    {
        public int id;
        public List<List<int>> snLists;

        public SuddaJokboInfo(int _id, List<List<int>> _snLists)
        {
            id = _id;
            snLists = _snLists;
        }

        public List<int> GetRandomShuffledPair()
        {
            if (snLists == null || snLists.Count == 0)
                return null;

            // 랜덤으로 하나 선택
            List<int> selected = new List<int>(snLists[UnityEngine.Random.Range(0, snLists.Count)]);

            // 셔플
            for (int i = selected.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (selected[i], selected[j]) = (selected[j], selected[i]);
            }

            return selected;
        }
    }

    public class SuddaPopup : BasePopup
    {
        [SerializeField] private Animator characterAnimator = null;
        [SerializeField] private Button infoButton = null;

        [SerializeField] private GameObject startButtons = null;
        [SerializeField] private Button freeStartButton = null;
        [SerializeField] private RectTransform freeStartButtonRect = null;
        [SerializeField] private Button adFreeStartButton = null;
        
        [SerializeField] private Button vipStartButton = null;

        [SerializeField] private GameObject freeStartButtonBlock = null;
        [SerializeField] private GameObject adStartButtonBlock = null;

        [SerializeField] private Text freeStartValue = null;
        [SerializeField] private Text adFreeStartValue = null;

        [SerializeField] private Button confirmButton = null;

        [SerializeField] private List<SuddaJokboItem> jokboItems = new List<SuddaJokboItem>();

        [SerializeField] private List<SuddaCard> cards = new List<SuddaCard>();
        [SerializeField] private SpinePlayerOneshot hand = null;
        [SerializeField] private DropEffect dropEffect = null;

        [SerializeField] private GameObject jokboObj = null;
        [SerializeField] private List<Text> jokboText = null;
        
        [SerializeField] private NumberIncrement goldValue = null;

        [SerializeField] private Transform moneyIcon = null;

        [SerializeField] private AudioClip flipAudioClip = null;
        [SerializeField] private AudioClip checkAudioClip = null;
        [SerializeField] private AudioClip moneyAudioClip = null;

        [SerializeField] private Transform testButtons = null;
        [SerializeField] private List<Toggle> testToggles = null;

        private List<int> snList = new() { 0,  1,  //1
                                           4,  5,  //2
                                           8,  9,  //3
                                          12, 13,  //4
                                          16, 17,  //5
                                          20, 21,  //6 
                                          24, 25,  //7
                                          28, 29,  //8
                                          32, 33,  //9
                                          36, 37 };//10

        private List<SuddaJokboInfo> suddaJokboInfoList = new()
        {
            new(0, new List<List<int>>{new List<int>{8, 28} }),    //삼팔광땡
            new(1, new List<List<int>>{new List<int>{0, 28} }),    //일팔광땡
            new(2, new List<List<int>>{new List<int>{0, 8} }),    //일삼광땡
            new(3, new List<List<int>>{new List<int>{36, 37} }),    //장땡
            new(4, new List<List<int>>{new List<int>{32, 33} }),    //9땡
            new(5, new List<List<int>>{new List<int>{28, 29} }),    //8땡
            new(6, new List<List<int>>{new List<int>{24, 25} }),    //7땡
            new(7, new List<List<int>>{new List<int>{20, 21} }),    //6땡
            new(8, new List<List<int>>{new List<int>{16, 17} }),    //5땡
            new(9, new List<List<int>>{new List<int>{12, 13} }),    //4땡
            new(10, new List<List<int>>{new List<int>{8, 9} }),    //3땡
            new(11, new List<List<int>>{new List<int>{4, 5} }),    //2땡
            new(12, new List<List<int>>{new List<int>{0, 1} }),    //1땡
            new(13, new List<List<int>>{new List<int>{0, 4}, new List<int>{0, 5}, new List<int>{1, 4}, new List<int>{1, 5} }),    //알리
            new(14, new List<List<int>>{new List<int>{0, 12}, new List<int>{0, 13}, new List<int>{1, 12}, new List<int>{1, 13} }),    //독사
            new(15, new List<List<int>>{new List<int>{0, 32}, new List<int>{0, 33}, new List<int>{1, 32}, new List<int>{1, 33} }),    //구삥
            new(16, new List<List<int>>{new List<int>{0, 36}, new List<int>{0, 37}, new List<int>{1, 36}, new List<int>{1, 37} }),    //장삥
            new(17, new List<List<int>>{new List<int>{12, 36}, new List<int>{12, 37}, new List<int>{13, 36}, new List<int>{13, 37} }),    //장사
            new(18, new List<List<int>>{new List<int>{12, 20}, new List<int>{12, 21}, new List<int>{13, 20}, new List<int>{13, 21} }),    //세륙
        };
        private List<int> result = null;
        private SuddaTableData resultSuddaTableData = null;

        bool IsInJokboInfoList(int a, int b)
        {
            foreach (var jokbo in suddaJokboInfoList)
            {
                foreach (var pair in jokbo.snLists)
                {
                    if (pair.Count == 2 &&
                        ((pair[0] == a && pair[1] == b) || (pair[0] == b && pair[1] == a)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        List<List<int>> CreateGGSList(List<int> snList, int _ggs)
        {
            List<List<int>> result = new();
            int count = snList.Count;

            for (int i = 0; i < count - 1; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    int num1 = (snList[i] / 4) + 1;
                    int num2 = (snList[j] / 4) + 1;

                    if ((num1 + num2) % 10 == _ggs)
                    {
                        if (IsInJokboInfoList(snList[i], snList[j]))
                            continue;
                        
                        result.Add(new List<int> { snList[i], snList[j] });
                    }
                }
            }

            return result;
        }

        int cheatIndexID = 0;
        int rateIndex = 0;

        protected override void OnEnable()
        {
            UserDataManager.OnValueSuddaPlayCountChanged.AddListener(RefreshButtons);
        }

        private void OnDisable()
        {            
            UserDataManager.OnValueSuddaPlayCountChanged.AddListener(RefreshButtons);
        }

        protected override void Start()
        {
            for (int i=0;i< testButtons.childCount;i++)
            {
                int index = i;
                Transform testT = testButtons.GetChild(i);
                testT.GetChild(0).GetComponent<Text>().text = TableDataManager.Instance.TableSuddaData.GetSuddaTableData(index).name;
                Button b = testT.GetComponent<Button>();
                testT.GetComponent<Button>().onClick.AddListener(() =>
                {
                    cheatIndexID = index;

                    for (int j = 0; j < testButtons.childCount; j++)
                    {
                        testButtons.GetChild(j).GetComponent<Image>().color = Color.white;
                    }

                    testT.GetComponent<Image>().color = Color.gray;

                });
            }

            for(int i=0;i< testToggles.Count;i++)
            {
                int index = i;
                testToggles[i].onValueChanged.AddListener((isOn) =>
                {
                    if(isOn)
                    {
                        rateIndex = index;
                        testButtons.gameObject.SetActive(index == 0);
                    }
                });
            }

            for (int ggs = 9, id = 19; ggs >= 0; ggs--, id++)
            {
                suddaJokboInfoList.Add(new(id, CreateGGSList(snList, ggs)));
            }
            //suddaJokboInfoList.Add(new(19, CreateGGSList(snList, 9)));
            //suddaJokboInfoList.Add(new(20, CreateGGSList(snList, 8)));
            //suddaJokboInfoList.Add(new(21, CreateGGSList(snList, 7)));
            //suddaJokboInfoList.Add(new(22, CreateGGSList(snList, 6)));
            //suddaJokboInfoList.Add(new(23, CreateGGSList(snList, 5)));
            //suddaJokboInfoList.Add(new(24, CreateGGSList(snList, 4)));
            //suddaJokboInfoList.Add(new(25, CreateGGSList(snList, 3)));
            //suddaJokboInfoList.Add(new(26, CreateGGSList(snList, 2)));
            //suddaJokboInfoList.Add(new(27, CreateGGSList(snList, 1)));
            //suddaJokboInfoList.Add(new(28, CreateGGSList(snList, 0)));
         
            closeButton.onClick.AddListener(() => ClosePopup(closeAction));
         
            infoButton.onClick.AddListener(() =>
            {
                PopupManager.Instance.OpenPopup<SuddaInfoPopup>();
            });

            freeStartButton.onClick.AddListener(() =>
            {
                SetState(GameStartState());
            });

            adFreeStartButton.onClick.AddListener(() =>
            {
                UIManager.Instance.ShowRewardedAd((adapter) =>
                {
                    _ = NetworkManager.Instance.AdReward(adapter,"SuddaPlay");
                    SetState(GameStartState());
                });
            });

            vipStartButton.onClick.AddListener(() =>
            {
                PopupManager.Instance.OpenPopup<AdShopPopup>();
                //UserDataManager.BuySuddaPremium();
                //RefreshButtons(UserDataManager.UserData.suddaData.playCount);
            });

            confirmButton.onClick.AddListener(() =>
            {
                SetState(ConfirmState());
            });            

            SetState(InitState());
            JokboItemInit();
        }

        public void JokboItemInit()
        {
            Array types = Enum.GetValues(typeof(SuddaJokboType));

            for (int i = 0; i < types.Length; i++)
            {
                SuddaJokboType type = (SuddaJokboType)types.GetValue(i);
                //Debug.Log($"타입: {type}, 값: {(int)type}");

                List<SuddaTableData> list = TableDataManager.Instance.TableSuddaData.GetSuddaTableDataList(type);

                if (list.Count == 1)
                {
                    jokboItems[i].SetItem(list[0].groupName, list[0].reward.FormatComma(), type);
                }
                else if (list.Count > 1)
                {
                    int minReward = list.Min(x => x.reward);
                    int maxReward = list.Max(x => x.reward);

                    string name = list[0].groupName;

                    jokboItems[i].SetItem(name, minReward == maxReward ? minReward.FormatComma() : $"{minReward.FormatComma()}-{maxReward.FormatComma()}", type);
                }
            }
        }

        private Action closeAction = null;

        public void Initialize(Action _closeAction)
        {
            closeAction = _closeAction;

            goldValue.SetNumber(UserDataManager.Gold, false);
            RefreshButtons(UserDataManager.UserData.suddaData.playCount);
            //cheatIndexID = 0;
            //rateIndex = 0;

            //for (int i = 0; i < testToggles.Count; i++)
            //{
            //    testToggles[i].isOn = false;
            //}

            //testToggles[0].isOn = true;
        }

        private void RefreshButtons(int _count)
        {
            if (UserDataManager.UserData.isSuddaPremium)
            {
                freeStartButtonRect.sizeDelta = new Vector2(500, freeStartButtonRect.sizeDelta.y);

                freeStartButton.gameObject.SetActive(true);
                adFreeStartButton.gameObject.SetActive(false);

                freeStartValue.text = $"({ConfigData.SuddaOneDayFreeCount + ConfigData.SuddaOneDayADFreeCount - _count}/{ConfigData.SuddaOneDayFreeCount + ConfigData.SuddaOneDayADFreeCount})";

                if (ConfigData.SuddaOneDayFreeCount + ConfigData.SuddaOneDayADFreeCount > _count)
                {
                    freeStartButton.enabled = true;
                    freeStartButtonBlock.gameObject.SetActive(false);
                }
                else
                {
                    freeStartButton.enabled = false;
                    freeStartButtonBlock.gameObject.SetActive(true);
                }

                vipStartButton.gameObject.SetActive(false);
            }
            else
            {
                if (_count < ConfigData.SuddaOneDayFreeCount)
                {
                    freeStartButtonRect.sizeDelta = new Vector2(305.3165f, freeStartButtonRect.sizeDelta.y);
                    freeStartButton.gameObject.SetActive(true);
                    adFreeStartButton.gameObject.SetActive(false);

                    freeStartButton.enabled = true;
                    freeStartButtonBlock.gameObject.SetActive(false);

                    freeStartValue.text = $"({ConfigData.SuddaOneDayFreeCount - _count}/{ConfigData.SuddaOneDayFreeCount})";
                }
                else 
                {
                    if (ConfigData.SuddaOneDayADFreeCount > 0)
                    {
                        freeStartButtonRect.sizeDelta = new Vector2(305.3165f, freeStartButtonRect.sizeDelta.y);

                        freeStartButton.gameObject.SetActive(false);
                        adFreeStartButton.gameObject.SetActive(true);

                        adFreeStartValue.text = $"({ConfigData.SuddaOneDayADFreeCount - (_count - ConfigData.SuddaOneDayFreeCount)}/{ConfigData.SuddaOneDayADFreeCount})";

                        if (ConfigData.SuddaOneDayFreeCount + ConfigData.SuddaOneDayADFreeCount > _count)
                        {
                            adFreeStartButton.enabled = true;
                            adStartButtonBlock.gameObject.SetActive(false);
                        }
                        else
                        {
                            adFreeStartButton.enabled = false;
                            adStartButtonBlock.gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        freeStartButton.gameObject.SetActive(true);
                        adFreeStartButton.gameObject.SetActive(false);

                        freeStartButton.enabled = false;
                        freeStartButtonBlock.gameObject.SetActive(true);

                        freeStartValue.text = $"({ConfigData.SuddaOneDayFreeCount - _count}/{ConfigData.SuddaOneDayFreeCount})";
                    }
                }

                vipStartButton.gameObject.SetActive(true);
            }
        }

        private Coroutine currentCoroutine = null;

        private void SetState(IEnumerator nextState)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            currentCoroutine = StartCoroutine(nextState);
        }

        private IEnumerator InitState()
        {
            startButtons.SetActive(true);

            UserDataManager.RefreshSudda();
            RefreshButtons(UserDataManager.UserData.suddaData.playCount);
            confirmButton.gameObject.SetActive(false);
            closeButton.gameObject.SetActive(true);
            CanClose = true;
            jokboItems.ForEach(x => x.SetCheck(false));            
            yield return null;
        }

        private IEnumerator GameStartState()
        {
            UserDataManager.PlaySudda();

            startButtons.gameObject.SetActive(false);

            //freeStartButton.enabled = false;
            //adFreeStartButton.enabled = false;
            //vipStartButton.enabled = false;
            yield return new WaitForSeconds(0.5f);

            //if (rateIndex == 0)
            //    result = suddaJokboInfoList[cheatIndexID].GetRandomShuffledPair();
            //else if (rateIndex == 1)
            //    result = snList.OrderBy(_ => UnityEngine.Random.value).Take(2).ToList();
            //else if (rateIndex == 2)

            if (UserDataManager.UserData.isJangddaeng)
            {
                result = suddaJokboInfoList[TableDataManager.Instance.TableSuddaData.GetRandomSuddaTableDataByRate().id].GetRandomShuffledPair();
                resultSuddaTableData = GetHand(result[0], result[1]);
            }
            else
            {
                result = new();
                result.AddRange(suddaJokboInfoList[3].snLists[0]);

                resultSuddaTableData = GetHand(result[0], result[1]);
                
                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    result.Reverse();
                }                

                UserDataManager.UserData.isJangddaeng = true;
            }
            
            UserDataManager.AddGold(resultSuddaTableData.reward);
            _ = NetworkManager.Instance.SendItemLog("GoldMiniGame", resultSuddaTableData.reward);
            UserDataManager.Save();

            cards[0].FlipCard(result[0]);
            SoundManager.Instance.PlayFX(flipAudioClip);
            
            closeButton.gameObject.SetActive(false);
            CanClose = false;
            
            yield return new WaitForSeconds(0.3f);
            confirmButton.gameObject.SetActive(true);
        }

        Transform jokboTransform = null;

        private IEnumerator ConfirmState()
        {
            confirmButton.gameObject.SetActive(false);
            hand.gameObject.SetActive(true);            
            //yield return new WaitForSeconds(0.1f);
            cards[1].SetShadow(false);
            cards[1].transform.localScale = Vector3.one * 1.0611f;
            cards[1].SetCard(result[1]);
            yield return new WaitForSeconds(3f);
            cards[1].SetShadow(true);
            hand.gameObject.SetActive(false);
            cards[1].transform.DOScale(0.81f, 0.01f).SetEase(Ease.InOutSine).OnComplete(()=>
            {
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    jokboText.ForEach(text => text.text = resultSuddaTableData.name);
                    jokboObj.SetActive(true);

                    SuddaJokboItem item = jokboItems.Where(x => x.Type == resultSuddaTableData.type).FirstOrDefault();
                    if (item != null)
                    {
                        item.SetCheck(true);
                        jokboTransform = item.transform;
                        SoundManager.Instance.PlayFX(checkAudioClip);

                        if(resultSuddaTableData.type == SuddaJokboType.GGS)
                        {
                            item.SetReward(resultSuddaTableData.reward.FormatComma());
                        }

                        string path = $"Sound/Voice/Sudda/{resultSuddaTableData.type}{(resultSuddaTableData.value == -1 ? "" : $"_{resultSuddaTableData.value}")}";

                        AudioClip audioClip = Resources.Load<AudioClip>(path);
                        if (audioClip != null)
                        {
                            SoundManager.Instance.PlayFX(audioClip);
                        }
                    }

                    characterAnimator.SetTrigger((resultSuddaTableData.type == SuddaJokboType.MANGTONG || resultSuddaTableData.type == SuddaJokboType.GGS) ? "Pose2" : "Pose3");
                });

            });
            yield return new WaitForSeconds(1.5f);
            SetState(ResultState());
        }

        private IEnumerator ResultState()
        {
            if (resultSuddaTableData.reward > 0)
            {
                dropEffect.transform.position = jokboTransform.position;

                int coinCount = Mathf.Clamp(resultSuddaTableData.reward / 30, 2, 20);

                dropEffect.SpawnGold(coinCount, moneyIcon, () =>
                {
                    goldValue.Duration = coinCount * 0.05f;
                    goldValue.SetNumber(UserDataManager.Gold);
                }, () =>
                {
                    moneyIcon.DOKill(true);
                    moneyIcon.DOScale(new Vector3(1.2f, 1.2f, 1.0f), 0.3f).SetEase(Ease.OutBack).From();
                    SoundManager.Instance.PlayFX(moneyAudioClip);
                });
                
                yield return new WaitForSeconds(1.9f + coinCount * 0.05f);
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
            SetState(FinishState());
        }

        private IEnumerator FinishState()
        {
            characterAnimator.SetTrigger("Idle");
            cards.ForEach(card => card.FlipCard());
            SoundManager.Instance.PlayFX(flipAudioClip);
            jokboObj.SetActive(false);
            jokboItems.ForEach(x => x.SetCheck(false));
            JokboItemInit();
            yield return new WaitForSeconds(0.2f);
            SetState(InitState());
            UserDataManager.Save(true);
        }

        private SuddaTableData GetHand(int a, int b)
        {
            // 삼팔광땡
            if (IsMatch(a, b, 8, 28))
                return TableDataManager.Instance.TableSuddaData.GetSuddaTableData(SuddaJokboType.SAMPAL_KWANGDDANG);

            // 일삼광땡
            if (IsMatch(a, b, 0, 8))
                return TableDataManager.Instance.TableSuddaData.GetSuddaTableData(SuddaJokboType.KWANGDDANG, 0);

            //일팔광땡
            if (IsMatch(a, b, 0, 28))
                return TableDataManager.Instance.TableSuddaData.GetSuddaTableData(SuddaJokboType.KWANGDDANG, 1);

            // 땡 판정
            int modA = (a / 4) + 1;
            int modB = (b / 4) + 1;

            if (modA == modB)
            {
                return TableDataManager.Instance.TableSuddaData.GetSuddaTableData(SuddaJokboType.DDANG, modA);
            }

            if (IsPair(modA, modB, 1, 2))
            {
                return TableDataManager.Instance.TableSuddaData.GetSuddaTableData(SuddaJokboType.ALI);
            }
            if (IsPair(modA, modB, 1, 4))
            {
                return TableDataManager.Instance.TableSuddaData.GetSuddaTableData(SuddaJokboType.DOKSA);
            }
            if (IsPair(modA, modB, 1, 9))
            {
                return TableDataManager.Instance.TableSuddaData.GetSuddaTableData(SuddaJokboType.GUBBING);
            }
            if (IsPair(modA, modB, 1, 10))
            {
                return TableDataManager.Instance.TableSuddaData.GetSuddaTableData(SuddaJokboType.JANGBBING);
            }
            if (IsPair(modA, modB, 4, 10))
            {
                return TableDataManager.Instance.TableSuddaData.GetSuddaTableData(SuddaJokboType.JANGSA);
            }
            if (IsPair(modA, modB, 3, 6))
            {
                return TableDataManager.Instance.TableSuddaData.GetSuddaTableData(SuddaJokboType.SERYUK);
            }


            //// 특별 끗 족보 체크
            //if (IsSpecialKkeut(modA, modB, out string specialName))
            //{
            //    return specialName;
            //}

            // 일반 끗 판정
            int kkeut = (modA + modB) % 10;
            if (kkeut == 9) return TableDataManager.Instance.TableSuddaData.GetSuddaTableData(SuddaJokboType.GABO);
            else if(kkeut==0) return TableDataManager.Instance.TableSuddaData.GetSuddaTableData(SuddaJokboType.MANGTONG);
            else return TableDataManager.Instance.TableSuddaData.GetSuddaTableData(SuddaJokboType.GGS, kkeut);
        }

        private bool IsMatch(int a, int b, int x, int y)
        {
            return (a == x && b == y) || (a == y && b == x);
        }

        private bool IsPair(int a, int b, int x, int y)
        {
            return (a == x && b == y) || (a == y && b == x);
        }
    }
}
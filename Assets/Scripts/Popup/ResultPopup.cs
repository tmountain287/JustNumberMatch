using Common.Manager;
using Common.UI;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JustOneMatch.UI
{
    public class ResultPopup : BasePopup
    {       
        [SerializeField] private Text stageClearText = null;
        [SerializeField] private Button restartButton = null;
        [SerializeField] private Button nextButton = null;
        [SerializeField] private Button exitButton = null;
        [SerializeField] private LevelUI levelUI = null;
        [SerializeField] private RewardItemList rewardItemList = null;

        [SerializeField] private Button claimAgainButton = null;
        [SerializeField] private List<GameObject> starList = null;

        [SerializeField] private List<GameObject> normalStarList = null;
        [SerializeField] private List<GameObject> bossStarList = null;

        [SerializeField] private List<RectTransform> rectList = null;
        [SerializeField] private AudioClip popAudioClip = null;

        private Action restartAction = null;
        private Action nextAction = null;
        private Action exitAction = null;

        private StageTableData data = null;

        protected override void Start()
        {
            base.Start();
            restartButton.onClick.AddListener(() =>
            {
                ClosePopup();
                restartAction?.Invoke();
            });

            nextButton.onClick.AddListener(() =>
            {
                ClosePopup();
                nextAction?.Invoke();
            });

            claimAgainButton.onClick.AddListener(() =>
            {
                // 보스 스테이지: 2배, 일반 스테이지: 5배 (기본 1회 지급 후 추가로 (배수-1)회분 지급)
                int multiplier = data.stageType == StageType.Boss ? 2 : 5;
                UIManager.Instance.ShowRewardedAd((adapter) =>
                {
                    rewardItemList.PlayMultiple(multiplier);
                    var extraReward = new Dictionary<ItemType, int>();
                    foreach (var kvp in data.rewardItemDic)
                        extraReward[kvp.Key] = kvp.Value * (multiplier - 1);
                    UserDataManager.AddItemCount(extraReward);
                    claimAgainButton.gameObject.SetActive(false);
                }, null, "reward_double");
            });

            exitButton.onClick.AddListener(() =>
            {
                ClosePopup();
                exitAction?.Invoke();
            });
        }

        //protected override void OnEnable()
        //{
        //    for (int i = 0; i < rectList.Count; i++)
        //    {
        //        int index = i;
        //        var rt = rectList[index];
        //        rt.localScale = Vector3.zero;

        //        var seq = DOTween.Sequence();
        //        seq.Append(rt.DOScale(1.1f, 0.35f).SetEase(Ease.OutBack))
        //           .Append(rt.DOScale(1.0f, 0.12f).SetEase(Ease.OutCubic))
        //           .SetDelay(index * 0.1f);
        //    }
        //}

        public void Initialize(bool _isXp, StageTableData _data, Dictionary<ItemType, int> _clearRewardDic, int _level, int _xp, Dictionary<ItemType, int> _levelUpReward, int _starCount, Action _restartAction, Action _nextAction, Action _exitAction = null)
        {
            data = _data;
            restartAction = _restartAction;
            nextAction = _nextAction;
            exitAction = _exitAction;
            //difficultyText.text = GameMgr.Instance.CurrentDifficultyType.ToString();
            stageClearText.text = _data.stageType == StageType.Normal ? string.Format(LocalizationManager.Instance.GetText("Stage"), _data.stage) : LocalizationManager.Instance.GetText("Boss Stage");

            normalStarList.ForEach(x=>x.SetActive(data.stageType == StageType.Normal));
            bossStarList.ForEach(x => x.SetActive(data.stageType == StageType.Boss));

            nextButton.gameObject.SetActive(_nextAction != null);
            claimAgainButton.gameObject.SetActive(_nextAction != null && _clearRewardDic != null);

            exitButton.gameObject.SetActive(_exitAction != null);

            starList.ForEach(x => x.SetActive(false));

            int maxXP = TableDataManager.Instance.TableLevelData.GetTableData(_level).xp;

            levelUI.SetLevelUI(_level, _xp, maxXP);

            if (_isXp)
            {
                rewardItemList.SetRewardItemList(_clearRewardDic);
                claimAgainButton.gameObject.SetActive(_clearRewardDic != null);

                if(data.stageType == StageType.Normal)
                {
                    for (int i = 0; i < _data.starMax; i++)
                    {
                        starList[i].SetActive(true);
                    }
                }
                else
                {
                    for (int i = 0; i < _starCount; i++)
                    {
                        starList[i].SetActive(true);
                    }
                }                

                rectList.ForEach(x => x.localScale = Vector3.zero);

                Action leveUpNext = () =>
                {
                    for (int i = 4; i < rectList.Count; i++)
                    {
                        int index = i;
                        var rt = rectList[index];

                        var seq = DOTween.Sequence();
                        seq.Append(rt.DOScale(1.1f, 0.35f).SetEase(Ease.OutBack).OnStart(()=>
                        { 
                            SoundManager.Instance.PlayFX(popAudioClip);
                        }))
                           .Append(rt.DOScale(1.0f, 0.12f).SetEase(Ease.OutCubic).OnComplete(() =>
                           {
                               if(index == 4)
                               {
                                   GameMgr.Instance.GameUI.GoldStateBox.SetBlockAutoUpdate(false);
                                   GameMgr.Instance.GameUI.SkillItemButtonList.ForEach(x => x.SetBlockAutoUpdate(false));
                               }
                           }))
                           .SetDelay(index * 0.1f);
                    }
                };

                DOVirtual.DelayedCall(delay, () =>
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int index = i;
                        var rt = rectList[index];

                        var seq = DOTween.Sequence();
                        seq.Append(rt.DOScale(1.1f, 0.35f).SetEase(Ease.OutBack).OnStart(() =>
                        {
                            SoundManager.Instance.PlayFX(popAudioClip);
                        }))
                           .Append(rt.DOScale(1.0f, 0.12f).SetEase(Ease.OutCubic).OnComplete(() =>
                           {
                               if (index == 3)
                               {
                                   if (UserDataManager.Level == _level)
                                   {
                                       levelUI.UpdateUI(UserDataManager.XP, leveUpNext);
                                   }
                                   else
                                   {
                                       levelUI.UpdateUI(maxXP, () =>
                                       {
                                           levelUI.SetLevelUI(UserDataManager.Level, 0, TableDataManager.Instance.TableLevelData.GetTableData(UserDataManager.Level).xp);

                                           DOVirtual.DelayedCall(0.5f, () =>
                                           {
                                               PopupManager.Instance.OpenPopup<LevelUpPopup>().Initialize(UserDataManager.Level, _levelUpReward, () =>
                                               {
                                                   levelUI.UpdateUI(UserDataManager.XP, leveUpNext);
                                               });
                                           });
                                       });
                                   }
                               }
                           }))
                           .SetDelay(index * 0.1f);
                    }
                });





                //StartCoroutine(PlayCoroutine());
            }
            else
            {
                rewardItemList.SetRewardItemList(null);
                //rewardItemList.gameObject.SetActive(false);
                claimAgainButton.gameObject.SetActive(false);
                GameMgr.Instance.GameUI.GoldStateBox.SetBlockAutoUpdate(false);
                GameMgr.Instance.GameUI.SkillItemButtonList.ForEach(x => x.SetBlockAutoUpdate(false));
            }           
        }

        private IEnumerator PlayCoroutine()
        {
            yield return new WaitForSeconds(delay);
            for (int i = 0; i < rectList.Count; i++)
            {
                rectList[i].gameObject.SetActive(true);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
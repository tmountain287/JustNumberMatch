using Common.Manager;
using Common.UI;
using DG.Tweening;
using InGame.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace Gostop.UI
{
    public enum InGameEffectType
    {
        BBUCK = 0,
        BBUCK_SELF = 1,
        BOMB = 2,
        DDADAK = 3,
        JJOCK = 4,
        SMALL_BOMB = 5,
        SWEEP = 6,
        STOP = 7,
        GO_1 = 8,
        GO_2 = 9,
        GO_3 = 10,
        GO_4 = 11,
        GO_5 = 12,
        GO_6 = 13,
        GO_7 = 14,
        GO_8 = 15,
        GO_9 = 16,
        BBUCK_SWEEP = 17,
        NOTHING = 18,
        NAGARI = 19,
        GODORI = 20,
        KWANG5 = 21,        
        STEAL_PEE = 22,
    }

    public class InGameUI : InGameEventHadlerMono
    {
        [SerializeField] private ObjectPool cardObjectPool = null;        
        [SerializeField] private Text pointValueText = null;
        [SerializeField] private Gambler[] gamblers = null;
        [SerializeField] private CardDeck cardDeck = null;
        [SerializeField] private TableCards tableCards = null;
        [SerializeField] private NoticePanel noticePanel = null;
        [SerializeField] private List<InGameEffectPool> effectList = null;
        [SerializeField] private MissionUI missionUI = null;
        [SerializeField] private GameObject winMultiple = null;
        [SerializeField] private Text winMultiipleText = null;

        [SerializeField] private SpinePlayerOneshot bankruptcySpine = null;

        [SerializeField] private GameObject pasanObj = null;
        [SerializeField] private GameObject glassEffect = null;

        [SerializeField] private List<AudioClip> bgmList = null;
        [SerializeField] private AudioClip fireBgm = null;

        [SerializeField] private List<GameObject> bgList = null;
        [SerializeField] private GameObject fireBg = null;

        [SerializeField] private GameObject firematchFlame = null;

        private List<Gambler> gamblerList = new();
        private Sequence sequenceDivede = null;

        public ObjectPool CardObjectPool { get => cardObjectPool; }

        private Gambler CurrentTurnGambler
        {
            get => GetGambler(CNetDocument.InGame.CurrentTurnIndex);
        }

        private Gambler CurrentOtherGambler
        {
            get => GetGamblerOther(CNetDocument.InGame.CurrentTurnIndex);
        }

        private Gambler MyGambler
        {
            get=> gamblerList.Where(x => x.GamblerType == Gambler.Type.MINE).FirstOrDefault();
        }

        private void Awake()
        {
            cardObjectPool.CreateObjectPool();            
            DOTween.SetTweensCapacity(1000, 200);
        }

        private void OnReturnInGame()
        {
            gamblerList.ForEach(gambler => gambler.Clear());
            cardDeck.Clear();
            tableCards.Clear();
            CardObjectPool.Clear();
        }

        private void OnChangeGame()
        {
            gamblerList.Clear();
            
            if (CNetDocument.InGame != null)
            {
                for (int i = 0; i < CNetDocument.InGame.InGamePlayerList.Count; i++)
                {
                    AddGambler(CNetDocument.InGame.InGamePlayerList[i]);
                }

                SetPointValue(CNetDocument.InGame.PointValue.Value);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            //추후
            if (CNetDocument.InGame != null)
            {
                for (int i = 0; i < CNetDocument.InGame.InGamePlayerList.Count; i++)
                {
                    AddGambler(CNetDocument.InGame.InGamePlayerList[i]);
                }

                CNetDocument.InGame.PointValue.OnValueChanged.AddListener(SetPointValue);

                SetPointValue(CNetDocument.InGame.PointValue.Value);

                CNetDocument.InGame.OnChangeGameEvent.AddListener(OnChangeGame);
            }
            //CNetDocument.InGame?.OnAddPlayerEvent.AddListener(AddGambler);
            //CNetDocument.InGame?.OnGameResultEvent.AddListener(OnGameResult);
            CardObjectPool.Clear();
            SetBG();
                        

            if (CNetDocument.NoticeInfoQueue.Count > 0)
            {
                PopupManager.Instance.OpenPopup<NoticePopup>().Initialize(CNetDocument.NoticeInfoQueue.Dequeue(), () =>
                {
                    if (!UserDataManager.UserData.isPrologueComplete)
                    {
                        PopupManager.Instance.OpenPopup<ProloguePopup>().Initialize(() =>
                        {
                            UserDataManager.UserData.isPrologueComplete = true;
                            UserDataManager.Save();
                        }, () =>
                        {
                            InGameManager.Instance.GameStart();
                        });
                    }
                    else
                    {
                        InGameManager.Instance.GameStart();
                    }
                });
            }
            else
            {
                if (!UserDataManager.UserData.isPrologueComplete)
                {
                    PopupManager.Instance.OpenPopup<ProloguePopup>().Initialize(() =>
                    {
                        UserDataManager.UserData.isPrologueComplete = true;
                        UserDataManager.Save();
                    }, () =>
                    {
                        InGameManager.Instance.GameStart();
                    });
                }
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            PopupManager.Instance?.AllClosePopup(PopupType.INGAME);

            if(CNetDocument.InGame != null)
            {
                CNetDocument.InGame.PointValue.OnValueChanged.RemoveListener(SetPointValue);
                CNetDocument.InGame.OnChangeGameEvent.RemoveListener(OnChangeGame);
                //CNetDocument.InGame.OnReturnInGameEvent.AddListener(OnReturnInGame);
            }
            //추후
            //CNetDocument.InGame?.OnReturnInGameEvent.RemoveListener(OnReturnInGame);

            for (int i = 0; i < gamblers.Length; i++)
            {
                gamblers[i].gameObject.SetActive(false);
            }


            //CNetDocument.InGame?.OnAddPlayerEvent.RemoveListener(AddGambler);
            //CNetDocument.InGame?.OnGameResultEvent.RemoveListener(OnGameResult);
        }

        public void SetBG()
        {
            bgList.ForEach(x => x.SetActive(false));

            fireBg.SetActive(CNetDocument.InGame.FireMatchMultiple > 1);
            bgList[(UserDataManager.Level - 1) % bgList.Count].SetActive(CNetDocument.InGame.FireMatchMultiple == 1);
            firematchFlame.SetActive(CNetDocument.InGame.FireMatchMultiple > 1);
        }

        public void PlayBGM()
        {
            if(CNetDocument.InGame.FireMatchMultiple > 1)
            {
                SoundManager.Instance.PlayBgm(fireBgm);
            }
            else
            {
                SoundManager.Instance.PlayBgm(bgmList[(UserDataManager.Level - 1) % bgmList.Count]);
            }
                
        }

        private void SetPointValue(long _value)
        {
            pointValueText.text = $"점 {_value.FormatKoreanUnits()}";
        }

        public void AddGambler(CInGamePlayer player)
        {
            Gambler gambler = gamblers[player.PlayerData.slotIndex];
            //추후
            //if (player.iPlayerRoomSlotIndex == CNetDocument.NXUser.NxUserInfo.iUserSlotIndex)
            //{
            //    gambler = gamblers[(int)Gambler.MainType.MINE];
            //}
            //else
            //{
            //    gambler = gamblers[(int)Gambler.MainType.OTHER];
            //}

            gambler.SetGambler(this, player);
            gamblerList.Add(gambler);
        }

        public Gambler GetGambler(int _index)
        {
            return gamblerList.Where(x => x.InGamePlayer.PlayerData.slotIndex == _index).FirstOrDefault();
        }

        public Gambler GetGamblerOther(int _index)
        {
            return gamblerList.Where(x => x.InGamePlayer.PlayerData.slotIndex != _index).FirstOrDefault();
        }

        public void SetStatusGambler()
        {
            CurrentTurnGambler.SetStatus(CurrentOtherGambler.CollectionCards);
            CurrentOtherGambler.SetStatus(CurrentTurnGambler.CollectionCards);
        }

        #region IngameHandlerEvent

        public override void HandleFireMatchEvent(object _sender, nxGoStopEvent _event)
        {
            nxFireMatchEvent handleEvent = (nxFireMatchEvent)_event;
            CharacterTableData characterTableData1 = TableDataManager.Instance.TableCharacterData.GetCharacterTableData(handleEvent.characterID1);
            CharacterTableData characterTableData2 = TableDataManager.Instance.TableCharacterData.GetCharacterTableData(handleEvent.characterID2);

            PopupManager.Instance.OpenPopup<InGameFireMatchPopup>().Initialize(characterTableData1, characterTableData2, handleEvent.useTicket);
        }

        public override void HandleInvalidGameEvent(object _sender, nxGoStopEvent _event)
        {
            PopupManager.Instance.OpenPopup<InGameInvalidGamePopup>();
        }

        public override void HandleNextLevelEvent(object _sender, nxGoStopEvent _event)
        {
            nxNextLevelEvent handleEvent = (nxNextLevelEvent)_event;

            PopupManager.Instance.OpenPopup<InGameLevelupPopup>(_initialize:(popup)=>
            {
                popup.popupType = PopupType.INGAME;
            }).Initialize(handleEvent.iLevel, TableDataManager.Instance.TableCharacterData.GetCharacterTableData(handleEvent.playerData.characterID));
            missionUI.Clear();
            cardDeck.Clear();
            tableCards.Clear();
            pasanObj.SetActive(false);
            glassEffect.SetActive(false);
            gamblers[(int)Gambler.Type.MINE].ResetSkillCard();
        }

        public override void HandleInitEvent(object _sender, nxGoStopEvent _event)
        {
            PlayBGM();
            SetBG();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            missionUI.Clear();
            cardDeck.Clear();
            tableCards.Clear();
            pasanObj.SetActive(false);
            glassEffect.SetActive(false);
            gamblers[(int)Gambler.Type.MINE].ResetSkillCard();
            cardDeck.CompleteShuffleAndMerge();

            if (sequenceDivede != null)
            {
                sequenceDivede.Kill();
                sequenceDivede = null;
            }

            if (CNetDocument.InGame.WinMultipleCount > 0)
            {
                winMultiipleText.text = $"x{(int)Mathf.Pow(2, CNetDocument.InGame.WinMultipleCount)}배";
            }

            if(CNetDocument.InGame.IsNagari)
            {
                winMultiipleText.text = "x2배";
            }

            winMultiple.SetActive(CNetDocument.InGame.WinMultipleCount > 0 || CNetDocument.InGame.IsNagari);
        }
        public override void HandleChangeTurnEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();           
        }

        public override void HandleShuffleCardEvent(object _sender, nxGoStopEvent _event)
        {
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            tableCards.Clear();
            pasanObj.SetActive(false);
            glassEffect.SetActive(false);
            cardDeck.StartShuffleAndMerge(CNetDocument.InGame.IsMissionCard, CNetDocument.InGame.FireMatchMultiple, () =>
            {
                //CNetDocument.InGame.OnEventCompleted();
            });
        }

        public override void HandleDivedeCardEvent(object _sender, nxGoStopEvent _event)
        {
            cardDeck.CompleteShuffleAndMerge();

            nxDivideCardEvent handleEvent = (nxDivideCardEvent)_event;

            sequenceDivede = DOTween.Sequence();

            void DrawCard(CardGroup _cardGroup, List<Card> _cardList, Action _onComplte = null)
            {
                sequenceDivede.AppendCallback(() =>
                {
                    _cardGroup.DrawCard(_cardList);
                });

                sequenceDivede.AppendInterval(0.1f); // 0.1초 대기
            }

            List<int> firstHolder = handleEvent.aPlayerHolder0;
            List<int> secondHolder = handleEvent.aPlayerHolder1;

            List<Card> firstCardList = new();
            List<Card> secondCardList = new();

            Gambler firstGambler = GetGambler(CNetDocument.InGame.CurrentFirstIndex);
            Gambler secondGambler = GetGamblerOther(CNetDocument.InGame.CurrentFirstIndex);


            if (CNetDocument.InGame.CurrentFirstIndex == 0)
            {
                for (int i = 0; i < secondHolder.Count; i++)
                {
                    secondHolder[i] = -1;
                }
            }
            else
            {
                for (int i = 0; i < firstHolder.Count; i++)
                {
                    firstHolder[i] = -1;
                }
            }

            DrawCard(tableCards, cardDeck.GetTopCard(handleEvent.aBoardHolder.Skip(0).Take(4).ToList()));            
            DrawCard(secondGambler.HandCards, cardDeck.GetTopCard(secondHolder.Skip(0).Take(5).ToList()));
            DrawCard(firstGambler.HandCards, cardDeck.GetTopCard(firstHolder.Skip(0).Take(5).ToList()));

            DrawCard(tableCards, cardDeck.GetTopCard(handleEvent.aBoardHolder.Skip(4).Take(4).ToList()));            
            DrawCard(secondGambler.HandCards, cardDeck.GetTopCard(secondHolder.Skip(5).Take(5).ToList()));
            DrawCard(firstGambler.HandCards, cardDeck.GetTopCard(firstHolder.Skip(5).Take(5).ToList()));

            sequenceDivede.AppendInterval(0.3f); // 0.1초 대기

            sequenceDivede.AppendCallback(() =>
            {
                gamblers[(int)Gambler.Type.MINE].HandCards.SortCard(0, ()=>
                {
                    //if(UserDataManager.PeeStealCount > 0)
                        gamblers[(int)Gambler.Type.MINE].SetSkillCard();
                });
                sequenceDivede.OnComplete(() =>
                {
                    //CNetDocument.InGame.OnEventCompleted();
                });
            });
            sequenceDivede.SetAutoKill(true);
            // Sequence 시작
            sequenceDivede.Play();
        }

        public override void HandleChangeBoardCardEvent(object _sender, nxGoStopEvent _event)
        {
            nxChangeBoardCardEvent handleEvent = (nxChangeBoardCardEvent)_event;
            Gambler firstGambler = GetGambler(CNetDocument.InGame.CurrentFirstIndex);
            tableCards.MoveToCard(handleEvent.DragInfo, firstGambler.CollectionCards, GameConstants.MoveDuration, _onComplete: () =>
            {
                SetStatusGambler();
            });
            cardDeck.MoveToCard(handleEvent.FlipCard, tableCards, GameConstants.FlipMoveDuration, _onComplete: () =>
            {
                CNetDocument.InGame.OnEventCompleted();
            });
        }

        public override void HandleSkillCardEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxSkillCardEvent handleEvent = (nxSkillCardEvent)_event;

            CurrentTurnGambler.PlayVoice(VoiceType.BBUCK_SELF);
            InGameEffectType inGameEffectType = InGameEffectType.STEAL_PEE;

            OnEffect(inGameEffectType, _characterID: CNetDocument.InGame.CurrentTurnPlayer.PlayerData.characterID);

            void CheckMission()
            {
                if (handleEvent.missionResult != null)
                {
                    if (handleEvent.missionResult.isSuccess)
                    {
                        missionUI.SetMissionSuccess(handleEvent.missionResult.iMatchSlotIndex == 0);

                        CurrentTurnGambler.PlayVoice(VoiceType.MISSION_SUCCESS);
                    }
                    else
                    {
                        //MyGambler.PlayVoice(VoiceType.MISSION_FAIL);
                        missionUI.SetMissionFail(() =>
                        {
                            gamblerList.ForEach(gambler =>
                            {
                                gambler.HandCards.RefreshMission();
                                gambler.CollectionCards.RefreshMission();
                            });
                            tableCards.RefreshMission();
                        });
                    }
                }
                else
                {
                    if (CNetDocument.InGame.NXRoundMission != null)
                    {
                        List<int> mission = handleEvent.aStealInfo.Select(val => CNetDocument.InGame.NXRoundMission.aMissionCard.IndexOf(val))
                                                                        .Where(index => index != -1)
                                                                        .ToList();

                        mission?.ForEach(m => missionUI.SetCheck(m, CNetDocument.InGame.CurrentTurnIndex == 0));
                    }
                }
            }
            SetStatusGambler();

            DOVirtual.DelayedCall(3.0f, () =>
            {
                CurrentOtherGambler.CollectionCards.MoveToCard(handleEvent.aStealInfo, CurrentTurnGambler.CollectionCards, GameConstants.StealDuration, GameConstants.StealDuration, () =>
                {
                    SetStatusGambler();

                    CurrentTurnGambler.OnEffect(handleEvent.collectionTypeList);
                    gamblerList.ForEach(g => g.OffEffect(handleEvent.removeWarnList));

                    CheckMission();
                });
            });
        }

        public override void HandleGeneralHitCardEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            nxGeneralHitCardEvent handleEvent = (nxGeneralHitCardEvent)_event;
         
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);

            CurrentTurnGambler.HitCard(handleEvent.HitCard, tableCards, () =>
            {
                CNetDocument.InGame.OnEventCompleted();
            });
        }

        public override void HandleBombHitCardEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);

            nxBombHitCardEvent handleEvent = (nxBombHitCardEvent)_event;

            CurrentTurnGambler.HitCard(handleEvent.aHitCard, tableCards, () =>
            {
                CurrentTurnGambler.ScoreBoard.SetShake(handleEvent.shakeCount);
                Transform target = tableCards.GetTransform(handleEvent.aHitCard[0]);
                CurrentTurnGambler.OnEffect(new List<CollectionType> { CollectionType.HUMMING });
                OnEffect(InGameEffectType.BOMB, target);
                CurrentTurnGambler.PlayVoice(InGameEffectType.BOMB);
                CurrentTurnGambler.HandCards.MakeCard(handleEvent.aToPlayerSlot);
                CNetDocument.InGame.OnEventCompleted();
            });
        }

        public override void HandleDDaddakBombHitCardEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxDDaDDkBombHitCardEvent handleEvent = (nxDDaDDkBombHitCardEvent)_event;
          
            CurrentTurnGambler.HitCard(handleEvent.aHitCard, tableCards, () =>
            {
                Transform target = tableCards.GetTransform(handleEvent.aHitCard[0]);
                CurrentTurnGambler.OnEffect(new List<CollectionType> { CollectionType.HUMMING });
                OnEffect(InGameEffectType.SMALL_BOMB, target);
                CurrentTurnGambler.HandCards.MakeCard(handleEvent.aToPlayerSlot);
                CNetDocument.InGame.OnEventCompleted();
            });            
        }

        public override void HandleBBucksweepHitCardEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);

            nxBBukSweepHitCardEvent handleEvent = (nxBBukSweepHitCardEvent)_event;
            CurrentTurnGambler.HitCard(handleEvent.HitCard, tableCards, () =>
            {
                CurrentTurnGambler.OnEffect(new List<CollectionType> { CollectionType.HUMMING });
                if (handleEvent.bSelf)
                {
                    Transform target = tableCards.GetTransform(handleEvent.HitCard);
                    OnEffect(InGameEffectType.BBUCK_SELF, target);
                    CurrentTurnGambler.PlayVoice(InGameEffectType.BBUCK_SELF);
                }
                else
                {
                    Transform target = tableCards.GetTransform(handleEvent.HitCard);
                    OnEffect(InGameEffectType.BBUCK_SWEEP, target);
                    CurrentTurnGambler.PlayVoice(InGameEffectType.BBUCK_SWEEP);
                    CNetDocument.InGame.OnEventCompleted();
                }
            });
        }

        public override void HandleBonusHitCardEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);

            nxBonusHitCardEvent handleEvent = (nxBonusHitCardEvent)_event;

            CurrentTurnGambler.HandCards.MoveToCard(handleEvent.HitCard, CurrentTurnGambler.CollectionCards, GameConstants.HitDuration, _onComplete: () =>
            {
                if (handleEvent.StealCardToDragInfo != -1)
                {
                    CurrentTurnGambler.PlayVoice(VoiceType.BONUS);
                }
                CurrentTurnGambler.OnEffect(new List<CollectionType> { CollectionType.HUMMING });
                SetStatusGambler();
                cardDeck.MoveToCard(handleEvent.FlipCard, CurrentTurnGambler.HandCards, GameConstants.FlipDragDuration, GameConstants.HitBonusDelay, _onComplete: () =>
                {
                    CurrentTurnGambler.HandCards.SortCard(0);

                    if (handleEvent.StealCardToDragInfo == -1)
                    {
                        CNetDocument.InGame.OnEventCompleted();
                    }
                    else
                    {
                        SetStatusGambler();
                        CurrentOtherGambler.CollectionCards.MoveToCard(handleEvent.StealCardToDragInfo, CurrentTurnGambler.CollectionCards, GameConstants.MoveDuration, _onComplete: () =>
                        {
                            SetStatusGambler();
                            CNetDocument.InGame.OnEventCompleted();
                        });
                    }
                });
            });
        }

        public override void HandleEmptyBombHitCardEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);

            nxEmptyBombHitCardEvent handleEvent = (nxEmptyBombHitCardEvent)_event;
            CurrentTurnGambler.HandCards.DisappearCard();
            CNetDocument.InGame.OnEventCompleted();
        }

        public override void HandleSelectShakeCardEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            nxSelectShakeCardEvent handleEvent = (nxSelectShakeCardEvent)_event;

            if (CurrentTurnGambler.GamblerType == Gambler.Type.MINE)
            {
                PopupManager.Instance.OpenPopup<InGameShakeSelectPopup>().Initialize(handleEvent.aShakeCard);
            }
            else
            {
                InGameManager.Instance.SendReqShakeResult(true);
            }

            CNetDocument.InGame.OnEventCompleted();
        }

        public override void HandleShakeEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);

            nxShakeEvent handleEvent = (nxShakeEvent)_event;

            PopupManager.Instance.OpenPopup<InGameShakePopup>().Initialize(handleEvent.aShakeCard);
            CurrentTurnGambler.PlayVoice(VoiceType.SHAKE);
            CurrentTurnGambler.ScoreBoard.SetShake(handleEvent.shakeCount);            
        }

        private int lastFlipCard = -1;

        public override void HandleGeneralFlipCardEvent(object _sender, nxGoStopEvent _event)
        {
            //noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxGeneralFlipCardEvent handleEvent = (nxGeneralFlipCardEvent)_event;
            lastFlipCard = handleEvent.FlipCard;
            cardDeck.HitCard(handleEvent.FlipCard, tableCards, () =>
            {                
                if(!handleEvent.isDragCard)
                {
                    Transform target = tableCards.GetTransform(handleEvent.FlipCard);
                    OnEffect(InGameEffectType.NOTHING, target);
                    CurrentTurnGambler.PlayVoice(InGameEffectType.NOTHING);
                }

                if (handleEvent.isGoodStart)
                {
                    CurrentTurnGambler.PlayVoice(VoiceType.GOOD_START);
                    CurrentTurnGambler.OnEffect(new List<CollectionType> { CollectionType.HUMMING });
                }
            });
        }

        public override void HandleBBuckFlipCardEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxBBukFlipCardEvent handleEvent = (nxBBukFlipCardEvent)_event;
            lastFlipCard = handleEvent.FlipCard;
            cardDeck.HitCard(handleEvent.FlipCard, tableCards, () =>
            {
                CurrentTurnGambler.ScoreBoard.SetBBuk(handleEvent.iBBukCount);
                Transform target = tableCards.GetTransform(handleEvent.FlipCard);
                OnEffect(InGameEffectType.BBUCK, target);

                if(CurrentTurnGambler.HandCards.CardList.Count == 9)
                {
                    CurrentTurnGambler.PlayVoice(VoiceType.BBUCK_1);
                }
                else
                {                    
                    CurrentTurnGambler.PlayVoice(handleEvent.iBBukCount == 3 ? VoiceType.BBUCK_3 : VoiceType.BBUCK);
                }                
            });
        }

        public override void HandleBbukRewardEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxBBukRewardEvent handleEvent = (nxBBukRewardEvent)_event;

            if (handleEvent.isBankruptcy)
            {
                CurrentOtherGambler.ScoreBoard.OnPasan(pasanObj);

                if(CNetDocument.InGame.CurrentTurnIndex == 1)
                {
                    glassEffect.SetActive(true);
                }
            }

            PopupManager.Instance.OpenPopup<InGameBbukRewardPopup>().Initialize(handleEvent.iBBukCount, handleEvent.iBBukReward, CurrentTurnGambler.GamblerType == Gambler.Type.MINE, handleEvent.isBankruptcy);
        }

        public override void HandleBbukRewardConfirmEvent(object _sender, nxGoStopEvent _event)
        {
            PopupManager.Instance.ClosePopup<InGameBbukRewardPopup>();
        }

        public override void HandleBBucksweepFlipCardEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxBBukSweepFlipCardEvent handleEvent = (nxBBukSweepFlipCardEvent)_event;
            lastFlipCard = handleEvent.FlipCard;
            cardDeck.HitCard(handleEvent.FlipCard, tableCards, () =>
            {
                CurrentTurnGambler.OnEffect(new List<CollectionType> { CollectionType.HUMMING });
                if (handleEvent.bSelf)
                {
                    Transform target = tableCards.GetTransform(handleEvent.FlipCard);                    
                    OnEffect(InGameEffectType.BBUCK_SELF, target);
                    CurrentTurnGambler.PlayVoice(InGameEffectType.BBUCK_SELF);
                }
                else
                {
                    Transform target = tableCards.GetTransform(handleEvent.FlipCard);
                    OnEffect(InGameEffectType.BBUCK_SWEEP, target);
                    CurrentTurnGambler.PlayVoice(InGameEffectType.BBUCK_SWEEP);
                }
            });
        }

        public override void HandleBonusFlipCardEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxBonusFlipCardEvent handleEvent = (nxBonusFlipCardEvent)_event;
            lastFlipCard = handleEvent.FlipCard;
            cardDeck.HitCard(handleEvent.FlipCard, handleEvent.SubIndex, tableCards, () =>
            {
                CNetDocument.InGame.OnEventCompleted();
            });
        }

        public override void HandleDDaddkFlipCardEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxDDaDDakFlipCardEvent handleEvent = (nxDDaDDakFlipCardEvent)_event;
            lastFlipCard = handleEvent.FlipCard;
            cardDeck.HitCard(handleEvent.FlipCard, tableCards, () =>
            {
                CurrentTurnGambler.OnEffect(new List<CollectionType> { CollectionType.HUMMING });
                Transform target = tableCards.GetTransform(handleEvent.FlipCard);
                OnEffect(InGameEffectType.DDADAK, target);
                CurrentTurnGambler.PlayVoice(InGameEffectType.DDADAK);
            });
        }

        public override void HandleJJokFlipCardEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxJJokFlipCardEvent handleEvent = (nxJJokFlipCardEvent)_event;
            lastFlipCard = handleEvent.FlipCard;
            cardDeck.HitCard(handleEvent.FlipCard, tableCards, () =>
            {
                CurrentTurnGambler.OnEffect(new List<CollectionType> { CollectionType.HUMMING });
                Transform target = tableCards.GetTransform(handleEvent.FlipCard);
                OnEffect(InGameEffectType.JJOCK, target);
                CurrentTurnGambler.PlayVoice(InGameEffectType.JJOCK);
            });
        }

        public override void HandleSelectCardEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);

            nxSelectCardEvent handleEvent = (nxSelectCardEvent)_event;

            if (CurrentTurnGambler.GamblerType == Gambler.Type.MINE)
            {
                PopupManager.Instance.OpenPopup<InGameCardSelectPopup>().Initialize(handleEvent.aSelectBoardSlot, CNetDocument.InGame.IsMissionCard);
            }
            else
            {
                noticePanel.OnNotice(NoticePanel.NoticeType.SelectCard);
            }

            //    CNetDocument.InGame.OnEventCompleted();
        }


        public override void HandleBoardsweepEvent(object _sender, nxGoStopEvent _event)
        {
            nxBoardSweepEvent handleEvent = (nxBoardSweepEvent)_event;
            CurrentTurnGambler.OnEffect(new List<CollectionType> { CollectionType.HUMMING });
            Transform target = tableCards.GetTransform(lastFlipCard);
            OnEffect(InGameEffectType.SWEEP, target);
            CurrentTurnGambler.PlayVoice(InGameEffectType.SWEEP);
        }

        public override void HandleDragCardEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxDragCardEvent handleEvent = (nxDragCardEvent)_event;
            
            void CheckMission()
            {
                if (handleEvent.missionResult != null)
                {
                    if (handleEvent.missionResult.isSuccess)
                    {
                        missionUI.SetMissionSuccess(handleEvent.missionResult.iMatchSlotIndex == 0);

                        CurrentTurnGambler.PlayVoice(VoiceType.MISSION_SUCCESS);
                    }
                    else
                    {
                        //MyGambler.PlayVoice(VoiceType.MISSION_FAIL);
                        missionUI.SetMissionFail(() =>
                        {
                            gamblerList.ForEach(gambler =>
                            {
                                gambler.HandCards.RefreshMission();
                                gambler.CollectionCards.RefreshMission();
                            });
                            tableCards.RefreshMission();
                        });
                    }
                }
                else
                {
                    if (CNetDocument.InGame.NXRoundMission != null)
                    {
                        List<int>  mission = handleEvent.aDragInfo.Select(val => CNetDocument.InGame.NXRoundMission.aMissionCard.IndexOf(val))
                                                                        .Where(index => index != -1)
                                                                        .ToList();

                        mission?.ForEach(m => missionUI.SetCheck(m, CNetDocument.InGame.CurrentTurnIndex == 0));
                    }
                }
            }


            int jockerCount = handleEvent.aDragInfo
                    .Select(index => CardDataInfo.CardDataInfoList.FirstOrDefault(card => card.Index == index))
                    .Count(card => card != null && card.MainType == CardMainType.JOCKER);

            if (handleEvent.aDragInfo.Count == 0 && handleEvent.aStealInfo.Count > 0)
            {
                SetStatusGambler();
                CurrentOtherGambler.CollectionCards.MoveToCard(handleEvent.aStealInfo, CurrentTurnGambler.CollectionCards, GameConstants.StealDuration, GameConstants.StealDuration, () =>
                {
                    SetStatusGambler();
                    CNetDocument.InGame.OnEventCompleted();
                });
            }
            else if (handleEvent.aDragInfo.Count > 0 && handleEvent.aStealInfo.Count > 0)
            {
                tableCards.MoveToCard(handleEvent.aDragInfo, CurrentTurnGambler.CollectionCards, GameConstants.DragDuration, GameConstants.StealDuration, _onComplete: () =>
                {
                    //if(jockerCount > 0)
                    //{
                    //    SoundManager.Instance.PlayFX(jockerCount == 1 ? stealPeeOne : stealPeeTwo);
                    //    handleEvent.collectionTypeList.Insert(0, CollectionType.HUMMING);
                    //}
                    SetStatusGambler();

                    CurrentTurnGambler.OnEffect(handleEvent.collectionTypeList);
                    gamblerList.ForEach(g => g.OffEffect(handleEvent.removeWarnList));

                    CheckMission();

                    CurrentOtherGambler.CollectionCards.MoveToCard(handleEvent.aStealInfo, CurrentTurnGambler.CollectionCards, GameConstants.StealDuration, GameConstants.StealDuration, () =>
                    {
                        SetStatusGambler();
                        CNetDocument.InGame.OnEventCompleted();
                    });
                });
            }
            else if (handleEvent.aDragInfo.Count > 0 && handleEvent.aStealInfo.Count == 0)
            {
                tableCards.MoveToCard(handleEvent.aDragInfo, CurrentTurnGambler.CollectionCards, GameConstants.DragDuration, GameConstants.StealDuration, _onComplete: () =>
                {
                    //if (jockerCount > 0)
                    //{
                    //    SoundManager.Instance.PlayFX(jockerCount == 1 ? stealPeeOne : stealPeeTwo);
                    //    handleEvent.collectionTypeList.Insert(0, CollectionType.HUMMING);
                    //}
                    gamblerList.ForEach(g => g.OffEffect(handleEvent.removeWarnList));
                    CurrentTurnGambler.OnEffect(handleEvent.collectionTypeList);
                    SetStatusGambler();
                    CheckMission();
                    CNetDocument.InGame.OnEventCompleted();
                });
            }
            else
            {
                
            }
        }

        public override void HandleSelectGukjunCardEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxSelectGukJunCardEvent handleEvent = (nxSelectGukJunCardEvent)_event;

            Gambler gambler = GetGambler(handleEvent.iMatchSolotIndex);

            if (gambler.GamblerType == Gambler.Type.MINE)
            {
                PopupManager.Instance.OpenPopup<InGameGuSelectPopup>().Initialize(handleEvent.iMatchSolotIndex, gambler.CollectionCards.MungCount == 7);
            }
            else
            {
                noticePanel.OnNotice(NoticePanel.NoticeType.SelectDoublePee);
            }

            CNetDocument.InGame.OnEventCompleted();
        }

        public override void HandleGukjunToPeeEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxGukJunToPeeEvent handleEvent = (nxGukJunToPeeEvent)_event;
            Gambler gambler = GetGambler(handleEvent.iMatchSolotIndex);

            if(gambler == null)
            {
                Debug.LogError($"겜블러가 없습니다. 슬롯인덱스 : {handleEvent.iMatchSolotIndex}");
                return;
            }

            Card card = gambler.CollectionCards.CardList.Where(x => x.NXCard.CardData.SubType == CardSubType.GUKJUN).FirstOrDefault();

            if (card != null)
            {
                if (handleEvent.iUse)
                {                
                    card.NXCard.CardData.MainType = CardMainType.PEE;
                    card.NXCard.CardData.SubType = CardSubType.DOUBLE_PEE;

                    gambler.CollectionCards.MoveToCard(card.NXCard.Sn, gambler.CollectionCards, GameConstants.MoveDuration, _onComplete: () =>
                    {
                        SetStatusGambler();
                        CNetDocument.InGame.OnEventCompleted();
                    });
                }
                else
                {
                    //카드 잠그기
                    card.Lock();
                }
            }
            else
            {
                Debug.LogError($"국준카드가 없습니다. 슬롯인덱스 : {handleEvent.iMatchSolotIndex}");
            }
        }

        public override void HandleSelectGostopEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxSelectGoStopEvent handleEvent = (nxSelectGoStopEvent)_event;

            if (CurrentTurnGambler.GamblerType == Gambler.Type.MINE)
            {
                PopupManager.Instance.OpenPopup<InGameGoSelectPopup>().Initialize(handleEvent.isbankruptcy, handleEvent.nTotalMoney, handleEvent.iGoCount);
            }
            else
            {
                noticePanel.OnNotice(NoticePanel.NoticeType.GoStop);
            }
            CNetDocument.InGame.OnEventCompleted();
        }

        public override void HandleGoEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxGoEvent handleEvent = (nxGoEvent)_event;

            InGameEffectType type = InGameEffectType.GO_1 + handleEvent.iGoCount - 1;
            OnEffect(type);
            CurrentTurnGambler.OnEffect(new List<CollectionType> { CollectionType.HUMMING });
            CurrentTurnGambler.PlayVoice(type);
            CurrentTurnGambler.ScoreBoard.SetGo(handleEvent.iGoCount);
            SetStatusGambler();
            CNetDocument.InGame.OnEventCompleted();
        }

        public override void HandleStopEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxStopEvent handleEvent = (nxStopEvent)_event;
            OnEffect(InGameEffectType.STOP, _characterID: CNetDocument.InGame.CurrentTurnPlayer.PlayerData.characterID);
            CurrentTurnGambler.PlayVoice(InGameEffectType.STOP);
        }

        public override void HandleNagariEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxNagariEvent handleEvent = (nxNagariEvent)_event;
            OnEffect(InGameEffectType.NAGARI);

            Gambler gambler = GetGambler(CNetDocument.InGame.CurrentFirstIndex);

            gambler.PlayVoice(InGameEffectType.NAGARI);
            CNetDocument.InGame.OnEventCompleted();
        }

        public override void HandleBoardPresidentEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxBoardPresidentEvent handleEvent = (nxBoardPresidentEvent)_event;
            PopupManager.Instance.OpenPopup<InGameBoardPresidentPopup>().Initialize(handleEvent.aPresidentCard);
            CNetDocument.InGame.OnEventCompleted();
        }

        public override void HandlePlayerPresidentEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxPlayerPresidentEvent handleEvent = (nxPlayerPresidentEvent)_event;

            PopupManager.Instance.OpenPopup<InGamePresidentPopup>().Initialize(handleEvent.aPresidentCard);

            Gambler gambler = GetGambler(handleEvent.iMatchSlotIndex);

            gambler.PlayVoice(VoiceType.PRESIDENT);

            CNetDocument.InGame.OnEventCompleted();
        }

        public override void HandleSelectPresidentEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxSelectPresidentEvent handleEvent = (nxSelectPresidentEvent)_event;

            Gambler gambler = GetGambler(handleEvent.iMatchSlotIndex);

            if (gambler.GamblerType == Gambler.Type.MINE)
            {
                PopupManager.Instance.OpenPopup<InGamePresidentSelectPopup>().Initialize(handleEvent.aPresidentCard);
            }
            else
            {
                noticePanel.OnNotice(NoticePanel.NoticeType.SelectPresident);
            }

            CNetDocument.InGame.OnEventCompleted();
        }

        public override void HandleSamePlayerPresidentEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxSamePlayerPresidentEvent handleEvent = (nxSamePlayerPresidentEvent)_event;
            PopupManager.Instance.OpenPopup<InGameSamePresidentPopup>().Initialize(handleEvent.aPresidentCard1, handleEvent.aPresidentCard2);
            CNetDocument.InGame.OnEventCompleted();
        }

        public override void HandleResultEvent(object _sender, nxGoStopEvent _event)
        {
            PopupManager.Instance.AllClosePopup(PopupType.INGAME);
            nxResultEvent handleEvent = (nxResultEvent)_event;

            float delayTime = 0f;

            if (handleEvent.netResultInfo.isBankruptcy)
            {
                Gambler pasenGambler = GetGamblerOther(handleEvent.netResultInfo.winSlotIndex);
                pasenGambler.ScoreBoard.OnPasan(pasanObj);
                delayTime = 2.0f;

                if(handleEvent.netResultInfo.winSlotIndex == 0)
                    glassEffect.SetActive(true);
            }

            DOVirtual.DelayedCall(delayTime, () =>
            {
                PlayerData winPlayer = CNetDocument.InGame.FindInGamePlayer(handleEvent.netResultInfo.winSlotIndex).PlayerData;
                PlayerData losePlayer = CNetDocument.InGame.FindInGamePlayerOther(handleEvent.netResultInfo.winSlotIndex).PlayerData;
                PopupManager.Instance.OpenPopup<InGameResultPopup>().Initialize(CNetDocument.InGame.IsNextAd, handleEvent.netResultInfo, winPlayer, losePlayer, CNetDocument.InGame.WinMultipleCount, CNetDocument.InGame.FireMatchMultiple, CNetDocument.InGame.IsNagari);
            });
        }

        public override void HandleBankruptcyEvent(object _sender, nxGoStopEvent _event)
        {
            missionUI.Clear();
            cardDeck.Clear();
            tableCards.Clear();
            pasanObj.SetActive(false);
            glassEffect.SetActive(false);
            gamblers[(int)Gambler.Type.MINE].ResetSkillCard();

            bankruptcySpine.PlayAnimation(() =>
            {
                PopupManager.Instance.OpenPopup<InGameBankruptcyPopup>().Initialize(ConfigData.RevivalMoney, ConfigData.AdRevivalMoney);
            });
        }

        public override void HandleRevivalEvent(object _sender, nxGoStopEvent _event)
        {
            PopupManager.Instance.OpenPopup<InGameRevivalPopup>();
        }

        public override void HandleRoundMissionEvent(object _sender, nxGoStopEvent _event)
        {
            noticePanel.Off();
            nxRoundMissionEvent handleEvent = (nxRoundMissionEvent)_event;

            PopupManager.Instance.OpenPopup<InGameMissionPopup>().Initialize(handleEvent.nxRoundMission, () =>
            {
                gamblerList.ForEach(gambler => gambler.HandCards.RefreshMission());
                tableCards.RefreshMission();
                missionUI.SetMissionUI(handleEvent.nxRoundMission);
                CNetDocument.InGame.OnEventCompleted();
            });
        }

        public override void HandleRoundMissionSuccessEvent(object _sender, nxGoStopEvent _event)
        {
            nxRoundMissionSuccessEvent handleEvent = (nxRoundMissionSuccessEvent)_event;
            DOVirtual.DelayedCall(2.0f, () =>
            {
                missionUI.SetMissionSuccess(handleEvent.iMatchSlotIndex == 0);
            });
            CNetDocument.InGame.OnEventCompleted();
        }

        public override void HandleRoundMissionFailEvent(object _sender, nxGoStopEvent _event)
        {
            //nxRoundMissionFailEvent handleEvent = (nxRoundMissionFailEvent)_event;
            DOVirtual.DelayedCall(2.0f, () =>
            {
                missionUI.SetMissionFail(()=>
                {
                    gamblerList.ForEach(gambler =>
                    {
                        gambler.HandCards.RefreshMission();
                        gambler.CollectionCards.RefreshMission();
                    });
                    tableCards.RefreshMission();
                });
            });
            CNetDocument.InGame.OnEventCompleted();
        }

        public override void HandleNfyAutoModeEvent(object _sender, nxGoStopEvent _event)
        {
            //nxAutoModeEvent handleEvent = (nxAutoModeEvent)_event;
            CNetDocument.InGame.OnEventCompleted();
        }
        #endregion

        //public void OnEventCompleted()
        //{
        //    CNetDocument.InGame?.OnEventCompleted();
        //}

        public void OnEffect(InGameEffectType _type, Transform _target = null, int _characterID = -1)
        {
            effectList[(int)_type].OnEffect(_target, _characterID);

            //Transform effect = effectTransform.GetChild((int)_type);
            //if (_target != null)
            //    effect.position = _target.position;
            //effect.gameObject.SetActive(true);
        }

        //private void Update()
        //{
        //    var activeTweens = DOTween.PlayingTweens();

        //    if (activeTweens != null && activeTweens.Count > 0)
        //    {
        //        Debug.Log($"현재 {activeTweens.Count}개의 Tween이 재생 중입니다.");
        //    }
        //    else
        //    {
        //        Debug.Log("활성화된 Tween이 없습니다.");
        //    }
        //}

        //private void Update()
        //{
        //    for (int i = 1; i <= 9; i++)
        //    {
        //        if (Input.GetKeyDown(i.ToString()))
        //        {
        //            OnEffect((InGameEffectType)System.Enum.Parse(typeof(InGameEffectType), $"GO_{i}"));
        //        }
        //    }
        //}
    }
}
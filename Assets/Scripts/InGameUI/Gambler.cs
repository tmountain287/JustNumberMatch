using Common.Manager;
using Common.UI;
using DG.Tweening;
using InGame;
using InGame.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public enum VoiceType
    {
        BBUCK,
        BBUCK_1,
        BBUCK_3,
        BBUCK_SELF,
        BBUCK_SWEEP,
        BI_KWANG_3,

        BOMB,
        BONUS,
        CHUNGDAN,
        DDADAK,
        FAIL,

        GO_1,
        GO_2,
        GO_3,
        GO_4,
        GO_5,
        GO_6,
        GO_7,
        GO_8,
        GO_9,
        GODORI,
        GOOD_START,
        HONGDAN,
        JJOCK,
        CHODAN,
        KWANG_3,
        KWANG_4,
        KWANG_5,

        MISSION_FAIL,
        MISSION_SUCCESS,
        NAGARI,
        NOTHING,
        PRESIDENT,
        SHAKE,
        STOP,

        //SMALL_BOMB,
        SWEEP,
        WIN,
    }

    public abstract class Gambler : InGameEventHadlerMono// InGameEventHadlerMono
    {
        public enum Type
        {
            MINE = 0,
            OTHER = 1,
        }

        #region Inspector Fields
        [Header("[  Base  ]")]
        //[SerializeField] protected ProfileUI profileUI = null;
        [SerializeField] protected List<GameObject> turnObjList = null;
        [SerializeField] protected GamblerScoreBoard scoreBoard = null;

        [Header("[  Card  ]")]
        [SerializeField] protected CollectionCards collectionCards = null;
        [SerializeField] protected HandCards handCards = null;
        [SerializeField] private List<CollectionWarnMark> collectionWarnMarkList = null;

        [Header("[  EffectPosition  ]")]
        [SerializeField] Transform effectTransform = null;
        [SerializeField] List<Transform> effectTransformPosList = null;
        #endregion

        private InGameUI gameUI = null;

        public abstract Type GamblerType { get; }
        public CollectionCards CollectionCards { get => collectionCards; }
        public HandCards HandCards { get => handCards; }

        public CInGamePlayer InGamePlayer { get; set; } = null;
        public GamblerScoreBoard ScoreBoard { get => scoreBoard; } 

        protected bool IsGoodAI { get; set; } = false;

        public virtual void SetSkillCard()
        {
            //TableDataManager.Instance.TableCharacterData.GetCharacterTableData(InGamePlayer.PlayerData.characterID)
        }

        public virtual void ResetSkillCard()
        {

        }

        public void PlayVoice(Enum sourceEnum)
        {
            GenderType genderType = TableDataManager.Instance.TableCharacterData.GetCharacterTableData(InGamePlayer.PlayerData.characterID).gengerType;
            
            if (Enum.TryParse(sourceEnum.ToString(), out VoiceType voiceType))
            {
                string path = $"Sound/Voice/{(GamblerType == Type.MINE ? "Mine" : "Other")}/{(genderType == GenderType.Male ? "Man" : "Woman")}/{voiceType}";

                Debug.Log(path);

                AudioClip audioClip = Resources.Load<AudioClip>(path);
                if (audioClip != null)
                {
                    SoundManager.Instance.PlayFX(audioClip);
                }
            }
        }

        public void OffEffect(List<CollectionType> _typeList)
        {
            _typeList.ForEach(x =>
            {
                collectionWarnMarkList.ForEach(m => m.SetMark(x, false));
            });
        }

        public void OnEffect(List<CollectionType> _typeList)
        {
            StartCoroutine(OnEffectCoroutine(_typeList));
        }

        IEnumerator OnEffectCoroutine(List<CollectionType> _typeList)
        {
            if (_typeList.Count == 0)
                yield break;
            CollectionType _type = _typeList[0];
            Transform effect = effectTransform.GetChild((int)_type);

            effect.transform.position = effectTransformPosList[(int)_type].position;

            effect.gameObject.SetActive(true);

            PlayVoice(_type);

            if (_type == CollectionType.GODORI)
            {
                gameUI.OnEffect(InGameEffectType.GODORI, _characterID: InGamePlayer.PlayerData.characterID);
            }

            if (_type == CollectionType.KWANG_5)
            {
                gameUI.OnEffect(InGameEffectType.KWANG5, _characterID: InGamePlayer.PlayerData.characterID);
            }

            _typeList.RemoveAt(0);
            yield return new WaitForSeconds(CollectionDataInfo.CollectionDataInfoList.Where(x => x.collectionType == _type).FirstOrDefault().aniTime);
            collectionWarnMarkList.ForEach(m => m.SetMark(_type, true));
            StartCoroutine(OnEffectCoroutine(_typeList));
        }

        //protected override void OnEnable()
        //{
        //    base.OnEnable();
        //    Clear();
        //    ScoreBoard.SetFire(CNetDocument.InGame.FireMatchMultiple > 1);
        //    ScoreBoard.SetFirstMark(InGamePlayer.PlayerData.slotIndex == CNetDocument.InGame.CurrentFirstIndex);
        //}

        protected override void OnDisable()
        {
            base.OnDisable();
            Clear();
            InGamePlayer.PlayerData.Money.OnValueChanged.RemoveListener(SetMoney);
            InGamePlayer.PlayerData.GaugeValue.OnValueChanged.RemoveListener(SetGauge);
        }

        public virtual void Clear()
        {
            for (int i = 0; i < effectTransform.childCount; i++)
            {
                effectTransform.GetChild(i).gameObject.SetActive(false);
            }
            CollectionCards.Clear();
            HandCards.Clear();
            collectionWarnMarkList.ForEach(m => m.Clear());
            ScoreBoard.Clear();
            turnObjList.ForEach(x => x.SetActive(false));
        }

        public void SetGambler(InGameUI _gameUI, CInGamePlayer _inGamePlayer)
        {
            gameUI = _gameUI;
            InGamePlayer = _inGamePlayer;
            ScroreBoardRefresh();
            scoreBoard.SetGauge(InGamePlayer.PlayerData.GaugeValue.Value / (float)ConfigData.FireGaugeMax, false);

            InGamePlayer.PlayerData.Money.OnValueChanged.AddListener(SetMoney);
            InGamePlayer.PlayerData.GaugeValue.OnValueChanged.AddListener(SetGauge);

            gameObject.SetActive(true);
        }

        public void SetMoney(long _value)
        {
            scoreBoard.SetMoney(_value);
        }

        public void SetGauge(int _value)
        {
            scoreBoard.SetGauge(_value / (float)ConfigData.FireGaugeMax);
        }

        public virtual void SetProfile()
        {
            scoreBoard.SetProfile(InGamePlayer.PlayerData.characterID);
        }

        public virtual void SetNickName()
        {
            scoreBoard.SetName(InGamePlayer.PlayerData.name);
        }


        public virtual void ScroreBoardRefresh()
        {
            scoreBoard.SetFire(false);
            scoreBoard.SetMoney(InGamePlayer.PlayerData.Money.Value, false);
            scoreBoard.SetName(InGamePlayer.PlayerData.name);
            scoreBoard.SetProfile(InGamePlayer.PlayerData.characterID);
        }

        void LeaveAction(int _index)
        {
            if (InGamePlayer.PlayerData.slotIndex == _index)
            {
                gameObject.SetActive(false);
                //CNetDocument.Room?.OnLeavePlayerEvent.RemoveListener(LeaveAction);
            }
        }

        public void HitCard(List<int> _nxCardList, CardGroup _cardGroup, Action _onComplete = null)
        {
            HandCards.HitCard(_nxCardList, _cardGroup, _onComplete);
        }

        public void HitCard(int _nxCard, CardGroup _cardGroup, Action _onComplete = null)
        {
            HandCards.HitCard(_nxCard, _cardGroup, _onComplete);
        }

        public void SetStatus(CollectionCards _otherCollectionCards)
        {
            collectionCards.SetCount(new() { collectionCards.KwangCount, collectionCards.MungCount, collectionCards.DdiCount, collectionCards.PeeCount });
            collectionCards.SetKwangBak(_otherCollectionCards.KwangCount >= 3 && collectionCards.IsKwangBak);
            collectionCards.SetMungBak(_otherCollectionCards.MungCount >= 7);
            collectionCards.SetPeeBak(_otherCollectionCards.PeeScore > 0 && collectionCards.IsPeeBak);
            ScoreBoard.SetScore(collectionCards.CollectionScore + InGamePlayer.PlayerData.goCount);
        }

        public virtual void SetActiveTurn(bool _flag)
        {
            turnObjList.ForEach(x => x.SetActive(_flag));
            if (_flag == true)
                handCards.SetCardOnClickAction();
        }

        #region IngameHandlerEvent
        public override void HandleFireMatchEvent(object _sender, nxGoStopEvent _event)
        {
            
        }

        public override void HandleInvalidGameEvent(object _sender, nxGoStopEvent _event)
        {
            
        }

        public override void HandleNextLevelEvent(object _sender, nxGoStopEvent _event)
        {
            nxNextLevelEvent handleEvent = (nxNextLevelEvent)_event;           
            Clear();
            ScroreBoardRefresh();
        }

        public override void HandleInitEvent(object _sender, nxGoStopEvent _event)
        {
            Clear();
            ScoreBoard.SetFire(CNetDocument.InGame.FireMatchMultiple > 1);
            ScoreBoard.SetFirstMark(InGamePlayer.PlayerData.slotIndex == CNetDocument.InGame.CurrentFirstIndex);            
        }
        public override void HandleChangeTurnEvent(object _sender, nxGoStopEvent _event)
        {
            nxChangeTurnEvent handleEvent = (nxChangeTurnEvent)_event;
            SetActiveTurn(InGamePlayer.PlayerData.slotIndex == handleEvent.iTurnMatchSlotIndex);

            if (InGamePlayer.PlayerData.slotIndex != handleEvent.iTurnMatchSlotIndex)
            {
                HandCards.SetDisableCards(true);
            }
        }

        public override void HandleShuffleCardEvent(object _sender, nxGoStopEvent _event)
        {
            
        }

        public override void HandleDivedeCardEvent(object _sender, nxGoStopEvent _event)
        {

        }

        public override void HandleChangeBoardCardEvent(object _sender, nxGoStopEvent _event)
        {
            
        }

        public override void HandleSkillCardEvent(object _sender, nxGoStopEvent _event)
        {
            SetActiveTurn(false);
            nxSkillCardEvent handleEvent = (nxSkillCardEvent)_event;
        }

        public override void HandleGeneralHitCardEvent(object _sender, nxGoStopEvent _event)
        {
            nxGeneralHitCardEvent handleEvent = (nxGeneralHitCardEvent)_event;
            SetActiveTurn(false);
        }

        public override void HandleBombHitCardEvent(object _sender, nxGoStopEvent _event)
        {
            SetActiveTurn(false);
            
            //SoundManager.Instance.PlayFX(bomb);           
        }

        public override void HandleDDaddakBombHitCardEvent(object _sender, nxGoStopEvent _event)
        {
            SetActiveTurn(false);
            
            nxDDaDDkBombHitCardEvent handleEvent = (nxDDaDDkBombHitCardEvent)_event;
        }

        public override void HandleBBucksweepHitCardEvent(object _sender, nxGoStopEvent _event)
        {
            SetActiveTurn(false);
        }

        public override void HandleBonusHitCardEvent(object _sender, nxGoStopEvent _event)
        {
            SetActiveTurn(false);

            nxBonusHitCardEvent handleEvent = (nxBonusHitCardEvent)_event;
        }

        public override void HandleEmptyBombHitCardEvent(object _sender, nxGoStopEvent _event)
        {
            SetActiveTurn(false);
            nxEmptyBombHitCardEvent handleEvent = (nxEmptyBombHitCardEvent)_event;
        }

        public override void HandleSelectShakeCardEvent(object _sender, nxGoStopEvent _event)
        {
           
        }

        public override void HandleShakeEvent(object _sender, nxGoStopEvent _event)
        {
            SetActiveTurn(false);
        }

        public override void HandleGeneralFlipCardEvent(object _sender, nxGoStopEvent _event)
        {
                        
        }

        public override void HandleBBuckFlipCardEvent(object _sender, nxGoStopEvent _event)
        {
            
        }

        public override void HandleBbukRewardEvent(object _sender, nxGoStopEvent _event)
        {
            
        }

        public override void HandleBbukRewardConfirmEvent(object _sender, nxGoStopEvent _event)
        {

        }

        public override void HandleBBucksweepFlipCardEvent(object _sender, nxGoStopEvent _event)
        {
            
        }

        public override void HandleBonusFlipCardEvent(object _sender, nxGoStopEvent _event)
        {
           
        }

        public override void HandleDDaddkFlipCardEvent(object _sender, nxGoStopEvent _event)
        {
           
        }

        public override void HandleJJokFlipCardEvent(object _sender, nxGoStopEvent _event)
        {
            
        }

        public override void HandleSelectCardEvent(object _sender, nxGoStopEvent _event)
        {
           
        }


        public override void HandleBoardsweepEvent(object _sender, nxGoStopEvent _event)
        {
            
        }

        public override void HandleDragCardEvent(object _sender, nxGoStopEvent _event)
        {
            
        }

        public override void HandleSelectGukjunCardEvent(object _sender, nxGoStopEvent _event)
        {           
            
        }

        public override void HandleGukjunToPeeEvent(object _sender, nxGoStopEvent _event)
        {
           
        }

        public override void HandleSelectGostopEvent(object _sender, nxGoStopEvent _event)
        {
            
        }

        public override void HandleGoEvent(object _sender, nxGoStopEvent _event)
        {
           
        }

        public override void HandleStopEvent(object _sender, nxGoStopEvent _event)
        {
           
        }

        public override void HandleNagariEvent(object _sender, nxGoStopEvent _event)
        {
            
        }

        public override void HandleBoardPresidentEvent(object _sender, nxGoStopEvent _event)
        {
           
        }

        public override void HandlePlayerPresidentEvent(object _sender, nxGoStopEvent _event)
        {
            
        }

        public override void HandleSelectPresidentEvent(object _sender, nxGoStopEvent _event)
        {
           
        }

        public override void HandleSamePlayerPresidentEvent(object _sender, nxGoStopEvent _event)
        {
           
        }

        public override void HandleResultEvent(object _sender, nxGoStopEvent _event)
        {
            
        }

        public override void HandleBankruptcyEvent(object _sender, nxGoStopEvent _event)
        {
            Clear();
            scoreBoard.SetFire(false);
        }

        public override void HandleRevivalEvent(object _sender, nxGoStopEvent _event)
        {

        }

        public override void HandleRoundMissionEvent(object _sender, nxGoStopEvent _event)
        {
           
        }

        public override void HandleRoundMissionSuccessEvent(object _sender, nxGoStopEvent _event)
        {
          
        }

        public override void HandleRoundMissionFailEvent(object _sender, nxGoStopEvent _event)
        {
           
        }

        public override void HandleNfyAutoModeEvent(object _sender, nxGoStopEvent _event)
        {
            
        }
        #endregion
    }
}
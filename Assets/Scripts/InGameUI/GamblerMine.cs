using Common.Manager;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class GamblerMine : Gambler
    {
        public override Type GamblerType { get => Type.MINE; }
        public bool UseMainSkill { get => useMainSkill; set => useMainSkill = value; }

        [SerializeField] private HandCardIcons handCardIcons = null;
        [SerializeField] private Button collectionOtherButton = null;
        [SerializeField] private Button collectionMineButton = null;

        [SerializeField] private List<Button> skillButtonList = null;

        [SerializeField] private MainSkillCard mainSkillCard = null;
        [SerializeField] private GameObject subSkillCard = null;
        [SerializeField] private GameObject stealItemButtonL = null;

        private bool useMainSkill = false;

        protected override void OnEnable()
        {
            base.OnEnable();
            UserDataManager.OnValueSelectIndexChanged.AddListener(SetProfile);
            UserDataManager.OnValueNickNameChanged.AddListener(SetNickName);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UserDataManager.OnValueSelectIndexChanged.AddListener(SetProfile);
            UserDataManager.OnValueNickNameChanged.AddListener(SetNickName);
        }

        public override void SetSkillCard()
        {
            mainSkillCard.SetMainSkillCard(
            () =>
            {
                mainSkillCard.gameObject.SetActive(false);
                InGameManager.Instance.SendSkillCard(CardMainType.PEE);
                useMainSkill = true;
                handCards.SortCard(0);

            });

            //subSkillCard.SetActive(true);
        }

        public override void ResetSkillCard()
        {
            mainSkillCard.gameObject.SetActive(false);
            useMainSkill = false;
            //subSkillCard.SetActive(false);
        }


        public override void SetProfile()
        {
            InGamePlayer.PlayerData.characterID = UserDataManager.SelectIndex;
            base.SetProfile();
        }

        public override void SetNickName()
        {
            InGamePlayer.PlayerData.name = UserDataManager.NickName;
            base.SetNickName();
        }

        private void Start()
        {
            for (int i = 0; i < skillButtonList.Count; i++)
            {
                int index = i;
                skillButtonList[i].onClick.AddListener(() =>
                {
                    InGameManager.Instance.SendSkillCard((CardMainType)index);
                });
            }

            collectionMineButton.onClick.AddListener(() =>
            {
                List<PlayerData> playerDatas = CNetDocument.InGame.InGamePlayerList.Select(x => x.PlayerData).ToList();
                PopupManager.Instance.OpenPopup<InGameCollectionPopup>()?.Initialize(playerDatas, 0);
            });

            collectionOtherButton.onClick.AddListener(() =>
            {
                List<PlayerData> playerDatas = CNetDocument.InGame.InGamePlayerList.Select(x => x.PlayerData).ToList();
                PopupManager.Instance.OpenPopup<InGameCollectionPopup>()?.Initialize(playerDatas, 1);
            });
        }

        public override void Clear()
        {
            base.Clear();
            stealItemButtonL.transform.DOLocalMoveX(-500, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
            {
                stealItemButtonL.gameObject.SetActive(false);
            });
            handCardIcons.gameObject.SetActive(false);
        }

        public override void SetActiveTurn(bool _flag)
        {
            skillButtonList.ForEach(x=>x.enabled = _flag);

            collectionOtherButton.gameObject.SetActive(_flag);
            collectionMineButton.gameObject.SetActive(_flag);

            if (handCards.CardList.Count == 10 && UserDataManager.Level <= 5 && CNetDocument.InGame.FireMatchMultiple == 1)
            {
                stealItemButtonL.gameObject.SetActive(true);
                stealItemButtonL.transform.DOKill();
                stealItemButtonL.transform.DOLocalMoveX(0, 0.3f).SetEase(Ease.OutBack);
            }
            else
            {
                stealItemButtonL.transform.DOKill();

                stealItemButtonL.transform.DOLocalMoveX(-500, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    stealItemButtonL.gameObject.SetActive(false);
                });
            }

            mainSkillCard.SetBlock(!_flag);

            if (_flag)
            {
                handCards.SetDisableCards(false);
                List<HandCardIcon.Type> typeList = new();

                List<NxCard> boardCardList = CNetDocument.InGame.BoardCards.Cards;
                List<NxCard> playerDragList = CNetDocument.InGame.InGamePlayerList.SelectMany(player => player.PlayerData.CollectCards.Cards ?? Enumerable.Empty<NxCard>()).ToList();
                
                handCards.CardList.ForEach(card =>
                {
                    HandCardIcon.Type cardType = HandCardIcon.Type.None;

                    if (card.NXCard.CardData.MainType == CardMainType.JOCKER || card.NXCard.CardData.MainType == CardMainType.BOMB)
                    {
                        cardType = HandCardIcon.Type.None;
                    }
                    else
                    {
                        int handInCount = handCards.CardList.Count(x => x.NXCard.SubIndex == card.NXCard.SubIndex);
                        int boardInCount = boardCardList.Count(x => x.SubIndex == card.NXCard.SubIndex);
                        int playerDragInCount = playerDragList.Count(x => x.SubIndex == card.NXCard.SubIndex && x.CardData.MainType != CardMainType.JOCKER);

                        if (handInCount >= 3)
                        {
                            cardType = boardInCount > 0 ? HandCardIcon.Type.Bomb : HandCardIcon.Type.Bell;
                        }
                        else if(handInCount == 2)
                        {
                            if(boardInCount == 2)
                            {
                                cardType = HandCardIcon.Type.Bomb;
                            }
                            else if(boardInCount == 1)
                            {
                                cardType = HandCardIcon.Type.Blue;
                            }
                            else
                            {
                                cardType = playerDragInCount > 0 ? HandCardIcon.Type.Gray : HandCardIcon.Type.None;
                            }
                        }
                        else
                        {
                            if (boardInCount >= 3)
                            {
                                cardType = HandCardIcon.Type.Red;
                            }
                            else if (boardInCount > 0)
                            {
                                cardType = playerDragInCount > 0 ? HandCardIcon.Type.Gray : HandCardIcon.Type.Blue;
                            }
                            else
                            {
                                cardType = HandCardIcon.Type.None;
                            }
                        }
                    }

                    typeList.Add(cardType);
                });

                if (handCards.CardList.Count <= 9 && !useMainSkill)// && UserDataManager.PeeStealCount > 0)
                {
                    typeList.Insert(0, HandCardIcon.Type.None);
                }

                //else if(handCards.CardList.Count < 9)
                //{
                //    typeList.Insert(0, HandCardIcon.Type.None);
                //    typeList.Insert(0, HandCardIcon.Type.None);
                //}

                handCardIcons.SetHandCardIcons(typeList);
            }
            else
            {
                handCardIcons.gameObject.SetActive(false);
            }

            base.SetActiveTurn(_flag);
        }
    }
}
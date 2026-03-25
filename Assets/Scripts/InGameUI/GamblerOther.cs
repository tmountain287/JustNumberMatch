using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gostop.UI
{
    public class GamblerOther : Gambler
    {
        public override Type GamblerType { get => Type.OTHER; }

        public override void ScroreBoardRefresh()
        {
            scoreBoard.SetFire(false);
            scoreBoard.SetMoney(InGamePlayer.PlayerData.Money.Value, false);

            int grade = UserDataManager.Level / 50;

            string gs = grade > 0 ? $"<size=24>{grade}단</size> " : "";

            scoreBoard.SetName($"{gs}{InGamePlayer.PlayerData.name}");
            scoreBoard.SetProfile(InGamePlayer.PlayerData.characterID);
        }

        public override void SetActiveTurn(bool _flag)
        {
            base.SetActiveTurn(_flag);
            if (_flag)
            {
                int hitSn = -1;

                if (IsGoodAI)
                {
                    List<NxCard> boardCardList = CNetDocument.InGame.BoardCards.Cards.OrderBy(card => card.CardData.Grade).ToList();
                    List<NxCard> handCardList = InGamePlayer.PlayerData.HandCards.Cards.OrderBy(card => card.CardData.Grade).ToList();
                    List<NxCard> playerDragList = CNetDocument.InGame.InGamePlayerList.SelectMany(player => player.PlayerData.CollectCards.Cards ?? Enumerable.Empty<NxCard>()).ToList();

                    //우선 조커가 있는지 찾기
                    NxCard jokerCard = handCardList.Where(x => x.CardData.MainType == CardMainType.JOCKER).FirstOrDefault();

                    PlayerData otherPlayerData = CNetDocument.InGame.CurrentOtherPlayer.PlayerData;

                    if (jokerCard != null && otherPlayerData.PeeCount > 0)
                    {
                        hitSn = jokerCard.Sn;
                    }
                    else
                    {
                        //이제 먹을게 있나 찾아보자
                        List<NxCard> matchedBoard = handCardList.Where(hCard => boardCardList.Any(bCard => bCard.SubIndex == hCard.SubIndex)).ToList();

                        //먹을게 있음
                        if (matchedBoard.Count > 0)
                        {
                            List<NxCard> chochoolList = new();
                            List<NxCard> boomList = new();
                            List<NxCard> fixList = new();

                            matchedBoard.ForEach(card =>
                            {
                                int handInCount = handCardList.Count(x => x.SubIndex == card.SubIndex);
                                int boardInCount = boardCardList.Count(x => x.SubIndex == card.SubIndex);
                                int playerDragInCount = playerDragList.Count(x => x.SubIndex == card.SubIndex && x.CardData.MainType != CardMainType.JOCKER);

                                if (handInCount >= 3)
                                {
                                    boomList.Insert(0, card);
                                }
                                else if (handInCount == 2)
                                {
                                    if (boardInCount == 2)
                                    {
                                        boomList.Add(card);
                                    }
                                    else if (boardInCount == 1)
                                    {
                                        chochoolList.Insert(0, card);
                                    }
                                }
                                else
                                {
                                    if (boardInCount >= 3)
                                    {
                                        fixList.Add(card);
                                    }
                                    else if (boardInCount > 0)
                                    {
                                        if (playerDragInCount > 0)// 굳은거
                                        {
                                            fixList.Add(card);
                                        }
                                        else
                                        {
                                            chochoolList.Add(card);
                                        }
                                    }
                                    else
                                    {

                                    }
                                }

                            });

                            if (chochoolList.Count > 0)
                            {
                                var bestCard = chochoolList
                                        .SelectMany(choCard => boardCardList
                                            .Where(boardCard => boardCard.SubIndex == choCard.SubIndex)
                                            .Select(boardCard => new
                                            {
                                                ChoCard = choCard,
                                                GradeSum = choCard.CardData.Grade + boardCard.CardData.Grade
                                            }))
                                        .OrderBy(pair => pair.GradeSum) // 가장 작은 합
                                        .Select(pair => pair.ChoCard)
                                        .FirstOrDefault();
                                hitSn = bestCard.Sn;
                            }
                            else if (boomList.Count > 0)
                            {
                                boomList = boomList.OrderBy(card => card.CardData.Grade).ToList();
                                hitSn = boomList[0].Sn;
                            }
                            else if (fixList.Count > 0)
                            {
                                fixList = fixList.OrderBy(card => card.CardData.Grade).ToList();
                                hitSn = fixList[0].Sn;
                            }
                        }
                        else //먹을게 없음
                        {
                            if (jokerCard != null)
                            {
                                hitSn = jokerCard.Sn;
                            }
                            else
                            {
                                NxCard emptyBomb = handCardList.Where(x => x.CardData.MainType == CardMainType.BOMB).FirstOrDefault();

                                if (emptyBomb != null)
                                {
                                    hitSn = emptyBomb.Sn;
                                }
                                else
                                {
                                    List<NxCard> fix2List = new();
                                    List<NxCard> boomList = new();
                                    List<NxCard> chochool2List = new();
                                    List<NxCard> fixList = new();
                                    List<NxCard> chochoolList = new();

                                    handCardList.ForEach(card =>
                                    {
                                        int handInCount = handCardList.Count(x => x.SubIndex == card.SubIndex);
                                        int playerDragInCount = playerDragList.Count(x => x.SubIndex == card.SubIndex && x.CardData.MainType != CardMainType.JOCKER);

                                        if (handInCount >= 3)
                                        {
                                            boomList.Insert(0, card);
                                        }
                                        else if (handInCount == 2)
                                        {
                                            if (playerDragInCount > 0)
                                            {
                                                fix2List.Add(card);
                                            }
                                            else
                                            {
                                                chochool2List.Add(card);
                                            }
                                        }
                                        else
                                        {
                                            if (playerDragInCount > 0)// 굳은거
                                            {
                                                fixList.Add(card);
                                            }
                                            else
                                            {
                                                chochoolList.Add(card);
                                            }
                                        }

                                    });

                                    if (fix2List.Count > 0)
                                    {
                                        fix2List = fix2List.OrderBy(card => card.CardData.Grade).ToList();
                                        hitSn = fix2List[0].Sn;
                                    }
                                    else if (boomList.Count > 0)
                                    {
                                        boomList = boomList.OrderByDescending(card => card.CardData.Grade).ToList();
                                        hitSn = boomList[0].Sn;
                                    }
                                    else if (chochool2List.Count > 0)
                                    {
                                        chochool2List = chochool2List.OrderByDescending(card => card.CardData.Grade).ToList();
                                        hitSn = chochool2List[0].Sn;
                                    }
                                    else if (fixList.Count > 0)
                                    {
                                        fixList = fixList.OrderByDescending(card => card.CardData.Grade).ToList();
                                        hitSn = fixList[0].Sn;
                                    }
                                    else if (chochoolList.Count > 0)
                                    {
                                        chochoolList = chochoolList.OrderByDescending(card => card.CardData.Grade).ToList();
                                        hitSn = chochoolList[0].Sn;
                                    }
                                }
                            }
                        }
                    }
                }

                DOVirtual.DelayedCall(Random.Range(0.1f, 0.5f), () =>
                {
                    //InGameManager.Instance.SendReqHitCard(InGamePlayer.PlayerData.HandCards.SnList[0]);
                    InGameManager.Instance.SendReqHitCard(hitSn == -1 ? InGamePlayer.PlayerData.HandCards.SnList[0] : hitSn);
                });
            }
        }


        #region IngameHandlerEvent
        public override void HandleInitEvent(object _sender, nxGoStopEvent _event)
        {
            base.HandleInitEvent(_sender, _event);

            LevelTableData data = TableDataManager.Instance.TableLevelData.GetLevelTableData(UserDataManager.Level);

            int aiRate = CNetDocument.InGame.FireMatchMultiple > 1 ? data.fireAIRate : data.normalAIRate;

            if (aiRate > 0)
            {
                IsGoodAI = Random.Range(0f, 100f) <= aiRate;
            }
            else
            {
                IsGoodAI= false;
            }
        }

        public override void HandleSelectCardEvent(object _sender, nxGoStopEvent _event)
        {
            nxSelectCardEvent handleEvent = (nxSelectCardEvent)_event;

            if (InGamePlayer.PlayerData.slotIndex == CNetDocument.InGame.CurrentTurnIndex)
            {
                CardData highestGradeCard = CardDataInfo.CardDataInfoList
                    .Where(card => handleEvent.aSelectBoardSlot.Contains(card.Index))
                    .OrderBy(card => card.Grade)
                    .FirstOrDefault();

                DOVirtual.DelayedCall(Random.Range(0.5f, 1.0f), () =>
                {
                    InGameManager.Instance.SendReqSelectBoardCard(highestGradeCard.Index);
                });
            }
        }

        public override void HandleSelectGukjunCardEvent(object _sender, nxGoStopEvent _event)
        {
            nxSelectGukJunCardEvent handleEvent = (nxSelectGukJunCardEvent)_event;

            if (InGamePlayer.PlayerData.slotIndex == handleEvent.iMatchSolotIndex)
            {
                DOVirtual.DelayedCall(Random.Range(0.5f, 1.0f), () =>
                {
                    bool flag = true;

                    if (IsGoodAI)
                    {
                        CInGamePlayer otherPlayer = CNetDocument.InGame.FindInGamePlayerOther(InGamePlayer.PlayerData.slotIndex);

                        if (InGamePlayer.PlayerData.CanGukjunToGo && InGamePlayer.PlayerData.CollectionScore < 7)
                        {
                            flag = true;
                        }
                        else if (otherPlayer.PlayerData.CollectionScore >= 7)
                        {
                            flag = true;
                        }
                        else if (!InGamePlayer.PlayerData.CanGo && InGamePlayer.PlayerData.ScoreGukjunToGo > InGamePlayer.PlayerData.MaxScore)
                        {
                            flag = true;
                        }
                        else
                        {
                            if (otherPlayer.PlayerData.CollectionScore >= 3 && InGamePlayer.PlayerData.MungCount == 7)
                            {
                                flag = false;
                            }
                            else if (otherPlayer.PlayerData.CollectionScore < 3 && otherPlayer.PlayerData.MungCount < 3 && InGamePlayer.PlayerData.HandCards.Count > 0)
                            {
                                flag = false;
                            }
                        }
                    }

                    InGameManager.Instance.SendReqSelectGukjunToPee(handleEvent.iMatchSolotIndex, flag);
                });
            }
        }

        public override void HandleSelectGostopEvent(object _sender, nxGoStopEvent _event)
        {
            nxSelectGoStopEvent handleEvent = (nxSelectGoStopEvent)_event;

            if (InGamePlayer.PlayerData.slotIndex == CNetDocument.InGame.CurrentTurnIndex)
            {
                DOVirtual.DelayedCall(Random.Range(0.5f, 1.0f), () =>
                {
                    InGameManager.Instance.SendReqSelectGoStop(CNetDocument.InGame.CurrentOtherPlayer.PlayerData.CollectionScore < 3);
                });
            }
        }

        public override void HandleSelectPresidentEvent(object _sender, nxGoStopEvent _event)
        {
            nxSelectPresidentEvent handleEvent = (nxSelectPresidentEvent)_event;

            if (InGamePlayer.PlayerData.slotIndex == handleEvent.iMatchSlotIndex)
            {
                DOVirtual.DelayedCall(2.0f, () =>
                {
                    InGameManager.Instance.SendReqSelectPresident(Random.value >= 0.1f);
                });
            }
        }
        #endregion
    }
}
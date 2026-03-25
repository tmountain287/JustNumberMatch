using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gostop.UI
{
    public abstract class HandCards : CardGroup
    {
        public void HitCard(int _nxCard, CardGroup _cardGroup, Action _onComplete = null)
        {
            Card card = GetCard(_nxCard);
            if (card != null)
            {
                RemoveCard(card);

                _cardGroup.ReceiveCard(card, GameConstants.HitDuration, _onComplete: () =>
                {
                    _onComplete?.Invoke();
                });
            }
        }

        public void HitCard(List<int> _nxCard, CardGroup _cardGroup, Action _onComplete = null)
        {
            int completedCount = 0;

            List<Card> cards = GetCard(_nxCard);

            for (int i = 0; i < cards.Count; i++)
            {
                int index = i;
                RemoveCard(cards[i], false);
                _cardGroup.ReceiveCard(cards[i], GameConstants.HitDuration, index * GameConstants.HitDuration, _onComplete: () =>
                {
                    completedCount++;
                    if (completedCount >= _nxCard.Count)
                    {
                        _onComplete?.Invoke();
                    }
                });

            }
            DelaySortCard();
        }

        public abstract Card MakeCard(int _nxCard);

        public void MakeCard(List<int> _nxCardList)
        {
            _nxCardList.ForEach(nxCard => MakeCard(nxCard));
        }

        public override Tuple<bool, Transform> GetTarget(NxCard _nxCard, int _offset = 0)
        {
            return new(true, cardPositionGroupList[CardList.Count + _offset].GetTransform(0));
        }

        public virtual void SetCardOnClickAction()
        {
            CardList.ForEach(card => card.OnClick = () =>
            {
                //NetworkManager.Instance.LobbySession.SendReqHitCard(card.NXCard.HolderSlot.iMainSlot);
                card.IsHit = true;
                InGameManager.Instance.SendReqHitCard(card.NXCard.Sn);
                CardList.ForEach(card => card.OnClick = null);
            });
        }

        public virtual void SetDisableCards(bool _flag) { }
    }
}
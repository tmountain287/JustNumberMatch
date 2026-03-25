using DG.Tweening;
using InGame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Util;
using System.Linq;

namespace Gostop.UI
{
    public class HandCardsOther : HandCards
    {
        public override Card GetCard(int _iSn)
        {
            Card card;
            if (_iSn != 50)
            {
                List<Card> cardList = CardList.Where(x => x.NXCard.Sn != 50).ToList();
                card = cardList[^1];
            }
            else
            {
                card = CardList.Where(x => x.NXCard.Sn == _iSn).LastOrDefault();
            }

            card.NXCard = new(_iSn);
            return card;
        }

        public override List<Card> GetCard(List<int> _iSnList)
        {
            List<Card> cardList = CardList.Where(x => x.NXCard.Sn != 50).ToList();

            List<Card> lastItems = cardList.GetRange(cardList.Count - _iSnList.Count, _iSnList.Count);

            for (int i = 0; i < lastItems.Count; i++)
            {
                lastItems[i].NXCard = new(_iSnList[i]);
            }

            return lastItems;
        }

        public override void DisappearCard()
        {
            Card card = GetCard(50);
            inGameUI.CardObjectPool.ReturnObjectToPool(card.gameObject);
            RemoveCard(card);
        }

        public override void SetCardOnClickAction()
        {
            CardList.ForEach(card => card.OnClick = null);
        }

        public override Card MakeCard(int _nxCard)
        {
            Card card = inGameUI.CardObjectPool.GetObjectFromPool<Card>();

            if(_nxCard == 50)            
                card.FlipCard(_nxCard);
            else
                card.SetBackside();

            var gt = GetTarget(null);

            Transform target = gt.Item2;
            
            card.transform.SetParent(target);
            card.transform.localScale = Vector3.one;
            card.transform.localPosition = Vector3.zero;
            card.transform.localRotation = Quaternion.identity;
            card.NXCard = new(_nxCard);
            CardList.Add(card);
            return card;
        }

        public override void SortCard(float _delay = 0.5f, Action _onComplete = null)
        {
            List<Card> cardList = CardList.Where(x => x.NXCard.Sn != 50).ToList();
            List<Card> addCardList = CardList.Where(x => x.NXCard.Sn == 50).ToList();
            cardList.AddRange(addCardList);
             
            for (int i = 0; i < CardList.Count; i++)
            {
                Transform target = GetTarget(i);
                cardList[i].TweenLocalMoveToTarget(target, 0, 0);
            }
        }
    }
}
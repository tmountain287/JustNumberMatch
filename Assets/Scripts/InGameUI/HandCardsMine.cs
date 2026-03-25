using DG.Tweening;
using InGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using Util;

namespace Gostop.UI
{
    public class HandCardsMine : HandCards
    {
        [SerializeField] private GamblerMine gamblerMine = null;

        public override Card MakeCard(int _nxCard)
        {
            Card card = inGameUI.CardObjectPool.GetObjectFromPool<Card>();
            card.FlipCard(_nxCard);

            int offSet = 0;

            if (CardList.Count <= 9 && !gamblerMine.UseMainSkill)// && UserDataManager.PeeStealCount > 0)
                offSet = 1;

            var gt = GetTarget(null, offSet);

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
            CardList = CardList.OrderBy(x => x.NXCard.Sn).ToList();

            int offSet = 0;

            if (CardList.Count <= 9 && !gamblerMine.UseMainSkill)// && UserDataManager.PeeStealCount > 0)
                offSet = 1;

            int completedCount = 0;

            for (int i = 0; i < CardList.Count; i++)
            {
                Transform target = GetTarget(i + offSet);
                
                CardList[i].TweenLocalMoveToTarget(target, 0.1f, _delay, _onComplete:()=>
                {
                    completedCount++;
                    if (completedCount >= CardList.Count)
                    {
                        _onComplete?.Invoke();
                    }
                });
            }
        }

        public override void SetDisableCards(bool _flag)
        {
            CardList.ForEach(card => card.SetDisable(_flag));
        }
    }
}

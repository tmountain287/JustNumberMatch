using InGame;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using DG.Tweening;
using Util;

namespace UI.Popup
{
    public class CardGroup : MonoBehaviour
    {
        [SerializeField] protected List<CardPositionGroup> cardPositionGroupList = null;
        [SerializeField] protected InGameUI inGameUI = null;
        [SerializeField] private ObjectPool sparkObjectPool = null;

        public List<Card> CardList { get; set; } = new();

        private void OnValidate()
        {
            if (cardPositionGroupList == null)
            {
                cardPositionGroupList = new List<CardPositionGroup>();
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform t = transform.GetChild(i);
                    if (!t.gameObject.activeSelf)
                        continue;
                    CardPositionGroup c = t.GetComponent<CardPositionGroup>();
                    if (c != null)
                    {
                        cardPositionGroupList.Add(c);
                    }
                }
            }

            if (inGameUI == null)
            {
                inGameUI = transform.GetComponentInParent<InGameUI>();
            }
        }

        private void Awake()
        {
            if (sparkObjectPool != null)
                sparkObjectPool.CreateObjectPool();
        }

        public virtual void DrawCard(List<Card> _cardList, Action _onComplete = null)
        {
            int completedCount = 0;
            List<Card> tempCardList = new();

            for (int i = 0; i < _cardList.Count; i++)
            {
                //_cardList[i].FlipCard(_cardList[i].NXCard.Sn);//, CNetDocument.CurrentInGame().IsMissionCard(_cardList[i].NXCard.iSn));

                var gt = GetTarget(_cardList[i].NXCard);

                Transform target = gt.Item2;

                //float distance = Vector3.Distance(_cardList[i].transform.position, target.position);
                //float duration = 0.2f;

                AddCard(_cardList[i]);

                int index = i;

                _cardList[i].TweenDrawToTarget(inGameUI.CardObjectPool.transform, target, 0.1f, 0, _onComplete: () =>
                {
                    completedCount++;
                    if (completedCount >= _cardList.Count)
                    {
                        _onComplete?.Invoke();
                    }
                });
            }
        }

        //public void ScaleReceiveCard(Card _card, nxSlot _nxSlot, float _speed, float _scale, float _delay = 0, Action _onComplete = null)
        //{
        //    Transform target = GetTarget(_nxSlot);

        //    float distance = Vector3.Distance(_card.transform.position, target.position);
        //    float duration = distance / _speed;

        //    _card.NXCard.HolderSlot = _nxSlot;
        //    Debug.Log($"{gameObject.name} :  {_card.NXCard.HolderSlot.iMainSlot} {_card.NXCard.HolderSlot.iSubSlot} 카드 추가");
        //    _card.ScaleTweenMoveToTarget(inGameUI.CardObjectPool.transform, target, duration, _scale,  _delay, () =>
        //    {
        //        _onComplete?.Invoke();
        //        SortCard();
        //    });
        //    AddCard(_card);
        //}

        public void MoveCardEX(Card _card, float _duration, float _delay = 0, Action _onComplete = null)
        {
            var gt = GetTarget(_card.NXCard, -1);

            Transform target = gt.Item2;

            if (target == null)
            {
                //Debug.LogWarning($"{gameObject.name} :  {_nxSlot.iMainSlot} {_nxSlot.iSubSlot} 이 없습니다.");
                return;
            }

            _card.TweenMoveToTarget(inGameUI.CardObjectPool.transform, target, _duration, _delay, () =>
            {
                SortCard();
                _onComplete?.Invoke();
            });

            AddCard(_card);
        }

        public void MoveCard(Card _card, float _duration, float _delay = 0, Action _onComplete = null)
        {
            var gt = GetTarget(_card.NXCard);

            Transform target = gt.Item2;

            if (target == null)
            {
                //Debug.LogWarning($"{gameObject.name} :  {_nxSlot.iMainSlot} {_nxSlot.iSubSlot} 이 없습니다.");
                return;
            }

            _card.TweenMoveToTarget(inGameUI.CardObjectPool.transform, target, _duration, _delay, () =>
            {
                SortCard();
                _onComplete?.Invoke();
            });

            AddCard(_card);
        }

        public void ReceiveCard(Card _card, float _duration, float _delay = 0, bool _isFire = true, Action _onComplete = null)
        {
            var gt = GetTarget(_card.NXCard);

            Transform target = gt.Item2;

            if (target == null)
            {
                Debug.LogWarning($"{gameObject.name} :  {_card.NXCard.Sn} 이 없습니다.");
                return;
            }

            _card.ScaleTweenMoveToTarget(inGameUI.CardObjectPool.transform, target, _duration, _delay, gt.Item1, () =>
            {
                SortCard();
                _onComplete?.Invoke();
                //if(_isFire && sparkObjectPool!=null)
                //{
                //    GameObject go = sparkObjectPool.GetObjectFromPool();
                //    go.transform.position = _card.transform.position;
                //}
            });

            AddCard(_card);
        }

        public void DrawCard(Card _card, float _speed, float _delay = 0, Action _onComplete = null)
        {
            var gt = GetTarget(_card.NXCard);

            Transform target = gt.Item2;

            float distance = Vector3.Distance(_card.transform.position, target.position);
            float duration = 0.1f;

            _card.TweenDrawToTarget(inGameUI.CardObjectPool.transform, target, duration, _delay, _onComplete);
            AddCard(_card);
        }

        public void MoveToCard(List<int> _nxDragInfo, CardGroup _toCardGroup, float _duration, float _delay = 0, Action _onComplete = null)
        {
            if (_nxDragInfo.Count == 0)
            {
                _onComplete?.Invoke();
                return;
            }

            int completedCount = 0;

            for (int i = 0; i < _nxDragInfo.Count; i++)
            {
                int index = i;
                Card card = GetCard(_nxDragInfo[i]);

                if (card == null)
                {
                    Debug.LogWarning($"{gameObject.name} :  {_nxDragInfo[i]} 이 없습니다.");
                    return;
                }
                RemoveCard(card, false);
                _toCardGroup.MoveCard(card, _duration, _delay * index, () =>
                {
                    completedCount++;
                    if (completedCount >= _nxDragInfo.Count)
                    {
                        DelaySortCard();
                        _onComplete?.Invoke();
                    }
                });
            }
            DelaySortCard();
        }

        public void MoveToCard(int _iSn, CardGroup _toCardGroup, float _duration, float _delay = 0, Action _onComplete = null)
        {
            Card card = GetCard(_iSn);

            if (card == null)
            {
                Debug.LogWarning($"{gameObject.name} :  {_iSn} 이 없습니다.");
                return;
            }

            if (this == _toCardGroup)
            {
                _toCardGroup.MoveCardEX(card, _duration, _delay, _onComplete);
            }
            else
            {
                _toCardGroup.MoveCard(card, _duration, _delay, _onComplete);
            }

            RemoveCard(card);
            DelaySortCard();
        }

        public virtual void SortCard(float _delay = 0.5f, Action _onComplete = null)
        {

        }

        public virtual void DisappearCard()
        {
            Card card = CardList.Where(x => x.IsHit).FirstOrDefault();
            inGameUI.CardObjectPool.ReturnObjectToPool(card.gameObject);
            RemoveCard(card);
        }

        public virtual Tuple<bool, Transform> GetTarget(NxCard _nxCard, int _offset = 0)
        {
            return null;
        }

        public Transform GetTarget(int _mainSlot, int _subSlot = 0)
        {
            return cardPositionGroupList[_mainSlot].GetTransform(_subSlot);
        }

        public virtual Transform GetTransform(int _iSn)
        {
            Card card = GetCard(_iSn);
            if (card == null) return null;
            return card.GetComponent<Transform>();
        }

        public virtual Card GetCard(int _iSn)
        {
            return CardList.Where(x => x.NXCard.Sn == _iSn).FirstOrDefault();
        }

        public virtual List<Card> GetCard(List<int> _iSnList)
        {
            return CardList.Where(x => _iSnList.Contains(x.NXCard.Sn)).ToList();
        }


        public virtual void AddCard(Card _card)
        {
            CardList.Add(_card);
        }

        public virtual void RemoveCard(Card _card, bool _isSort = true)
        {
            CardList.Remove(_card);
            if (_isSort)
                DelaySortCard();
        }

        public void DelaySortCard(float _delay = 0.5f)
        {
            SortCard(_delay);
        }

        public virtual void Clear()
        {
            CardList.ForEach(card =>
            {
                card.Clear();
            });
            CardList.Clear();
        }

        public void RefreshMission()
        {
            CardList.ForEach(x => x.RefreshMission());
        }
    }
}
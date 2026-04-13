using DG.Tweening;
using UI.Popup;
using InGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;
using System;
using Common.Manager;

namespace UI.Popup
{
    public class CardDeck : MonoBehaviour
    {
        [SerializeField] private RectTransform container = null;
        [SerializeField] private ObjectPool cardObjectPool;
        [SerializeField] private CardPositionGroup cardPositionGroup = null;
        [SerializeField] private Transform tableTransform = null;

        [SerializeField] private AudioClip shuffleSound = null;

        private int cardCount = 50;
        private float cardWidth = 117f;
        private float cardHeight = 162f;
        private List<Card> cardList = new();

        private Sequence mainSequence = null;
        private List<Sequence> cardSequences = new List<Sequence>();

        private void OnValidate()
        {
            
        }

        public void MakeDeck(int _count = 50)
        {
            Clear();

            for (int i = 0; i < _count; i++)
            {
                MakeCard();
            }
        }

        public Card MakeCard()
        {
            Card card = cardObjectPool.GetObjectFromPool<Card>();
            card.SetBackside();
            Transform target = cardPositionGroup.GetTransform(cardList.Count);
            card.transform.SetParent(target);
            card.transform.localScale = Vector3.one;
            card.transform.localPosition = Vector3.zero;
            AddCard(card);
            return card;
        }

        public void AddCard(Card _card)
        {
            cardList.Add(_card);
        }

        public Card PopCard()
        {
            Card card = cardList[^1];
            cardList.RemoveAt(cardList.Count - 1);
            return card;
        }

        public void ResetSequence()
        {
            if (mainSequence != null && mainSequence.IsActive())
            {
                mainSequence.Kill(true);
            }

            foreach (var cardSequence in cardSequences)
            {
                if (cardSequence.IsActive())
                    cardSequence.Kill(true); // 완전히 제거
            }

            cardSequences.Clear();

            // 추가로 카드 트윈도 제거
            foreach (var card in cardList)
            {
                if (card != null && card.Rect != null)
                {
                    DOTween.Kill(card.Rect);
                }
            }
        }

        public void StartShuffleAndMerge(Func<int, bool> _isMissionCardFunc, int _multiple, Action _onComplete)
        {
            Clear();            

            for (int i = 0; i < cardCount; i++)
            {
                Card card = cardObjectPool.GetObjectFromPool<Card>();
                card.Initialize(_isMissionCardFunc, _multiple);
                card.transform.SetParent(tableTransform);
                card.transform.localScale = Vector3.one;// * 0.65f;
                RectTransform cardRect = card.GetComponent<RectTransform>();

                // 초기 랜덤 위치 배치
                float randomX = UnityEngine.Random.Range(-container.rect.width / 2 + cardWidth / 2, container.rect.width / 2 - cardWidth / 2) + container.localPosition.x;
                float randomY = UnityEngine.Random.Range(-container.rect.height / 2 + cardHeight / 2, container.rect.height / 2 - cardHeight / 2) + container.localPosition.y;
                cardRect.anchoredPosition = new Vector2(randomX, randomY);

                AddCard(card);
            }

            // 메인 시퀀스 초기화
            mainSequence = DOTween.Sequence();

            for (int k = 0; k < cardList.Count; k++)
            {
                int cardIndex = k;
                Card card = cardList[k];
                Sequence cardSequence = DOTween.Sequence();
                cardSequences.Add(cardSequence); // 리스트에 추가

                // 카드의 섞임 위치를 컨테이너 안에서만 유지
                for (int i = 0; i < 3; i++)
                {
                    Vector2 randomPosition = new Vector2(
                        UnityEngine.Random.Range(-container.rect.width / 4, container.rect.width / 4) + container.localPosition.x,
                        UnityEngine.Random.Range(-container.rect.height / 4, container.rect.height / 4 + container.localPosition.y)
                    );

                    float randomRotation = UnityEngine.Random.Range(-30f, 30f);

                    // 카드의 위치와 회전을 부드럽게 이동
                    cardSequence.Append(card.Rect.DOAnchorPos(randomPosition, 0.2f).OnComplete(()=>
                    {
                        if (cardIndex == 0)
                            SoundManager.Instance.PlayFX(shuffleSound);
                    }).SetEase(Ease.OutQuad));
                    cardSequence.Join(card.Rect.DORotate(new Vector3(0, 0, randomRotation), 0.2f).SetEase(Ease.OutQuad));
                }

                // 섞임 이후 중앙으로 모이는 연출
                cardSequence.AppendCallback(() =>
                {
                    Transform target = cardPositionGroup.GetTransform(cardIndex);
                    target.localEulerAngles = PlayerPrefsManager.Instance.GetPlayerPrefsValue(PrefsKey.BOARD_SORT) == 1 ? Vector3.zero : new Vector3(0, 0, UnityEngine.Random.Range(-6, 6));
                    card.transform.SetParent(target);
                });

                cardSequence.Append(card.Rect.DOLocalMove(Vector3.zero, 0.2f).SetEase(Ease.InOutQuad)); // 중앙으로 이동
                cardSequence.Join(card.Rect.DORotate(Vector3.zero, 0.2f).SetEase(Ease.InOutQuad)); // 회전 초기화

                cardSequence.OnComplete(() =>
                {
                    Transform target = cardPositionGroup.GetTransform(cardIndex);
                    card.transform.SetParent(target);
                    card.transform.localScale = Vector3.one;
                    card.transform.localPosition = Vector3.zero;
                    card.transform.localEulerAngles = Vector3.zero;
                });
                cardSequence.SetAutoKill(true);
                // 메인 시퀀스에 개별 카드의 시퀀스를 추가
                mainSequence.Join(cardSequence);
            }

            mainSequence.OnComplete(() =>
            {
                _onComplete?.Invoke();
            });

            mainSequence.Play();
        }

        // ? 애니메이션 즉시 완료 함수 추가 ?
        public void CompleteShuffleAndMerge()
        {
            if (mainSequence != null && mainSequence.IsActive())
            {
                mainSequence.Complete(true);  // 메인 시퀀스 즉시 완료
            }

            // 개별 카드 애니메이션도 즉시 완료
            foreach (var cardSequence in cardSequences)
            {
                if (cardSequence != null && cardSequence.IsActive())
                {
                    cardSequence.Complete(true);
                }
            }

            ResetSequence();
        }

        public Card GetTopCard(int _iSn = -1, int _subIndex = -1)
        {
            Card card = PopCard();
            card.NXCard = new(_iSn, _subIndex);
            return card;
        }

        //public List<Card> GetTopCard(int _count)
        //{
        //    List<Card> tempCardList = new();
        //    for (int i = 0; i < _count; i++)
        //    {
        //        Card card = GetTopCard();
        //        card.NXCard = new();
        //        card.NXCard.HolderSlot.iMainSlot = (sbyte)i;
        //        tempCardList.Add(card);
        //    }

        //    return tempCardList;
        //}

        public List<Card> GetTopCard(List<int> _nxCardList)
        {
            List<Card> tempCardList = new();
            for (int i = 0; i < _nxCardList.Count; i++)
            {
                Card card = GetTopCard(_nxCardList[i]);
                tempCardList.Add(card);
            }

            return tempCardList;
        }

        public void MoveToCard(int _iSn, CardGroup _cardGroup, float _duration, float _delay = 0, Action _onComplete = null)
        {
            Card card = GetTopCard(_iSn);
            _cardGroup.MoveCard(card, _duration, _delay, _onComplete);
        }

        public void HitCard(int _iSn, CardGroup _cardGroup, Action _onComplete = null)
        {
            Card card = GetTopCard(_iSn);
            _cardGroup.ReceiveCard(card, GameConstants.FlipDuration, _onComplete: _onComplete);
        }

        public void HitCard(int _iSn, int _subIndex, CardGroup _cardGroup, Action _onComplete = null)
        {
            Card card = GetTopCard(_iSn, _subIndex);
            _cardGroup.ReceiveCard(card, GameConstants.FlipDuration, _onComplete: _onComplete);
        }

        public void Clear()
        {
            ResetSequence();
           
            cardList.Clear();
            cardObjectPool.Clear();
        }

        public void SortCardPostionGroupList(bool _flag)
        {
            if (_flag)
            {
                cardPositionGroup.RectList.ForEach(r => r.localEulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(-6, 6)));
            }
            else
            {
                cardPositionGroup.RectList.ForEach(r => r.localEulerAngles = Vector3.zero);
            }
        }
    }
}
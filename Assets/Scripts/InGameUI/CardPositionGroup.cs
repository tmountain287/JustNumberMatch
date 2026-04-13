using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;

namespace UI.Popup
{
    [Serializable]
    public class SortInfo
    {
        public RectTransform rectTransform = null;
        public float width;    //카드 사이즈
        public float oriSpacing;
    }

    public class CardPositionGroup : MonoBehaviour
    {
        [SerializeField] private CardGroup cardGroup = null;
        [SerializeField] private List<RectTransform> rectList = null;
        [SerializeField] private HorizontalLayoutGroup layoutGroup = null;
        [SerializeField] private SortInfo sortInfo = null;
        [SerializeField] private CardPositionGroupCount cardPositionGroupCount = null;
        [SerializeField] private CollectionWarnMark collectionWarnMark = null;

        public List<RectTransform> RectList { get => rectList; }

        private void OnValidate()
        {
            if (rectList == null)
            {
                rectList = new();
                for (int i = 0; i < transform.childCount; i++)
                {
                    RectList.Add(transform.GetChild(i).Find("Pos").GetComponent<RectTransform>());
                }
            }
        }

        public Transform GetTransform(int _index)
        {
            if (_index < 0) return null;
            return RectList[_index].transform;
        }

        private Coroutine coroutine = null;

        public void SortCardBySubSlot(float _delay = 0.5f, int _cardCount = 0)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }

            coroutine = StartCoroutine(SortCoroutine(_delay));

            if(_cardCount > 0)
            {
                SetLayoutGroupSpacing(_cardCount);
            }

            //SetMark(_cardCount > 0 ? RectList[_cardCount - 1].transform : null);
        }

        IEnumerator SortCoroutine(float _delay = 0.5f, int _cardCount = 0)
        {
            List<Card> cards = new List<Card>();

            while (true)
            {
                cards.Clear();
                RectList.ForEach(rect =>
                {
                    Card card = rect.GetComponentInChildren<Card>(false);
                    if (card != null && cardGroup.CardList.Contains(card))
                        cards.Add(card);
                });
                
                bool anyPlaying = cards.Any(card => card != null && card.IsPlaying);

                if (!anyPlaying)
                    break;
                
                SetMark(cards.Count > 0 ? cards[^1].transform : null);
                yield return null; // 다음 프레임까지 대기
            }

            cards.Clear();
            RectList.ForEach(rect =>
            {
                Card card = rect.GetComponentInChildren<Card>(false);
                if (card != null && cardGroup.CardList.Contains(card))
                    cards.Add(card);
            });

            SetLayoutGroupSpacing(cards.Count);
                
            for (int i = 0; i < cards.Count; i++)
            {
                int index = i;
                Transform target = GetTransform(i);
                cards[i].TweenLocalMoveToTarget(target, 0.2f, 0, _onUpdate: () =>
                {
                    if (index == cards.Count - 1 && cards.Count == _cardCount)
                    {
                        SetMark(cards.Count > 0 ? cards[^1].transform : null);
                    }

                }, _onComplete: () =>
                {
                    if (index == cards.Count - 1 && cards.Count == _cardCount)
                    {
                        SetMark(cards.Count > 0 ? cards[^1].transform : null);
                    }
                });
            }
            yield return null; // 다음 프레임까지 대기
            SetMark(cards.Count > 0 ? cards[^1].transform : null);
        }

        public void SetCount(int _count)
        {
            if (cardPositionGroupCount != null)
            {
                cardPositionGroupCount.SetCount(_count);
            }
        }

        public void Clear()
        {
            if (cardPositionGroupCount != null)
            {
                cardPositionGroupCount.gameObject.SetActive(false);
            }

            if (layoutGroup == null)
            {
                layoutGroup.spacing = sortInfo.oriSpacing;
            }
        }

        public void SetMark(Transform _parent)
        {
            if (cardPositionGroupCount != null)
            {
                cardPositionGroupCount.SetTransform(_parent);
            }

            if (collectionWarnMark != null)
            {
                collectionWarnMark.SetTransform(_parent);
            }
        }

        public void SetLayoutGroupSpacing(int _count)
        {
           
            if (layoutGroup == null)
                return;

            if((_count - 1) * (sortInfo.width + sortInfo.oriSpacing) + sortInfo.width  <= sortInfo.rectTransform.rect.width || _count == 0)
            {
                layoutGroup.spacing = sortInfo.oriSpacing;
            }
            else
            {
                float spacing = (sortInfo.rectTransform.rect.width - sortInfo.width) / (_count - 1) - sortInfo.width;
                layoutGroup.spacing = Mathf.Floor(spacing);
            }
        }
    }
}
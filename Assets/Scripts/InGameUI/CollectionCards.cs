using Common.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI.Popup
{
    public class CollectionCards : CardGroup
    {
        [SerializeField] private GameObject kwangBak = null;
        [SerializeField] private GameObject mungBak = null;
        [SerializeField] private GameObject peeBak = null;

        public NxCardList CollectCards
        {
            get
            {
                return new(CardList.Select(x => x.NXCard).ToList());
            }
        }

        public bool IsKwangBak { get => KwangCount == 0; }
        public bool IsPeeBak { get => PeeCount < 8 && PeeCount > 0; }

        public int KwangCount { get => CollectCards.GetCardsTotalScore(0); }
        public int MungCount { get => CollectCards.GetCardsTotalScore(1); }
        public int DdiCount { get => CollectCards.GetCardsTotalScore(2); }
        public int PeeCount { get => CollectCards.GetCardsTotalScore(3); }

        public int MungScore
        {
            get => Mathf.Max(0, MungCount - 4);
        }

        public int DdiScore
        {
            get => Mathf.Max(0, DdiCount - 4);
        }

        public int PeeScore
        {
            get => Mathf.Max(0, PeeCount - 9);
        }

        public int CollectionScore
        {
            get
            {
                int sum = MungScore + DdiScore + PeeScore;
                List<CollectionType> CollectionTypeList = CollectionDataInfo.GetCollectionTypeList(CollectCards.SnList);
                CollectionTypeList.ForEach(ct =>
                {
                    CollectionData cd = CollectionDataInfo.CollectionDataInfoList.Where(x => x.collectionType == ct).FirstOrDefault();
                    if (cd != null)
                        sum += cd.score;
                });

                return sum;
            }
        }

        private void OnEnable()
        {
            ResolutionManager.Instance.OnChangeResolution.AddListener(ResoutionSort);
        }

        private void OnDisable()
        {
            if (ResolutionManager.Instance != null)
                ResolutionManager.Instance.OnChangeResolution.RemoveListener(ResoutionSort);
        }

        public override void SortCard(float _delay = 0.5f, Action _onComplete = null)
        {
            for(int i=0;i< cardPositionGroupList.Count;i++)
            {
                cardPositionGroupList[i].SortCardBySubSlot(_delay, CardList.Count(x => i == x.NXCard.CardData.MainIndex));
            }
        }

        public void ResoutionSort()
        {
            for (int i = 0; i < cardPositionGroupList.Count; i++)
            {
                int cardCount = CardList.Count(x => i == x.NXCard.CardData.MainIndex);
                cardPositionGroupList[i].SetLayoutGroupSpacing(cardCount);
                cardPositionGroupList[i].SetMark(cardCount > 0 ? cardPositionGroupList[i].RectList[cardCount - 1].transform : null);
            }
        }

        public override Tuple<bool, Transform> GetTarget(NxCard _nxCard, int _offset = 0)
        {
            int count = CardList.Count(x => _nxCard.CardData.MainIndex == x.NXCard.CardData.MainIndex) + _offset;
            return new(false, cardPositionGroupList[_nxCard.CardData.MainIndex].GetTransform(Math.Max(0, count)));
        }

        public void SetCount(List<int> _list)
        {
            for (int i = 0; i < cardPositionGroupList.Count; i++)
            {
                cardPositionGroupList[i].SetCount(_list[i]);
            }
        }

        public void SetKwangBak(bool _flag)
        {
            kwangBak.SetActive(_flag);
        }

        public void SetMungBak(bool _flag)
        {
            mungBak.SetActive(_flag);
        }

        public void SetPeeBak(bool _flag)
        {
            peeBak.SetActive(_flag);
        }

        public override void Clear()
        {
            base.Clear();
            kwangBak.SetActive(false);
            peeBak.SetActive(false);
            mungBak.SetActive(false);
            for (int i = 0; i < cardPositionGroupList.Count; i++)
            {
                cardPositionGroupList[i].Clear();
            }
        }
    }
}
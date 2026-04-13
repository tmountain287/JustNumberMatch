using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Common.Manager;

namespace UI.Popup
{
    public class TableCards : CardGroup
    {
        int[] arr = new int[13];

        public override void Clear()
        {
            base.Clear();
            arr = Enumerable.Repeat(-1, 13).ToArray();
        }

        public override void AddCard(Card _card)
        {
            int subIndex = _card.NXCard.SubIndex;

            int index = Array.IndexOf(arr, subIndex);
            if (index == -1)
            {
                int index2 = Array.IndexOf(arr, -1);
                arr[index2] = subIndex;
            }

            CardList.Add(_card);
        }

        public override void RemoveCard(Card _card, bool _isSort = true)
        {
            int subIndex = _card.NXCard.SubIndex;
            if (CardList.Count(x => x.NXCard.SubIndex == subIndex) == 1)   //보드에 한장밖에 없으면 arr값을 -1로 해준다
            {
                int index = Array.IndexOf(arr, subIndex);
                if (index != -1)
                    arr[index] = -1;
            }

            base.RemoveCard(_card, _isSort);
        }

        public void AllFlipCard(List<sbyte> _snList)
        {

        }

        public override void SortCard(float _delay = 0.5f, Action _onComplete = null)
        {
            cardPositionGroupList.ForEach(cpg => cpg.SortCardBySubSlot(_delay));            
        }

        public override Tuple<bool, Transform> GetTarget(NxCard _nxCard, int _offset = 0)
        {
            int subIndex = _nxCard.SubIndex;

            int index = Array.IndexOf(arr, subIndex);
            bool has = index != -1;
            if (index == -1)
            {
                index = Array.IndexOf(arr, -1);
            }

            int count = CardList.Count(x => subIndex == x.NXCard.SubIndex);
            Transform t = cardPositionGroupList[index].GetTransform(count);
            cardPositionGroupList[index].transform.SetAsLastSibling();
            t.localEulerAngles = PlayerPrefsManager.Instance.GetPlayerPrefsValue(PrefsKey.BOARD_SORT) == 1 ? Vector3.zero : new Vector3(0, 0, UnityEngine.Random.Range(-7, 7));
            return new(has, t);
        }

        public void SortCardPostionGroupList(bool _flag)
        {
            if(_flag)
            {
                cardPositionGroupList.ForEach(x => x.RectList.ForEach(r => r.localEulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(-7, 7))));                
            }
            else
            {
                cardPositionGroupList.ForEach(x => x.RectList.ForEach(r => r.localEulerAngles = Vector3.zero));
            }
        }
    }
}
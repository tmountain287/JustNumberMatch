using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common.UI;

namespace UI.Popup
{
    public class InGameSamePresidentPopup : BasePopup
    {
        [SerializeField] private List<Card> cardList1 = null;
        [SerializeField] private List<Card> cardList2 = null;

        public void Initialize(List<int> _cardList1, List<int> _cardList2)
        {
            _cardList1.Sort();
            _cardList2.Sort();

            for (int i = 0; i < _cardList1.Count; i++)
            {
                cardList1[i].SetCard(_cardList1[i]);
            }

            for (int i = 0; i < _cardList2.Count; i++)
            {
                cardList2[i].SetCard(_cardList2[i]);
            }
        }
    }
}
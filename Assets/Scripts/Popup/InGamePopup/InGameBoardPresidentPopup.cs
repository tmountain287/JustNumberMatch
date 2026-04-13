using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common.UI;

namespace UI.Popup
{
    public class InGameBoardPresidentPopup : BasePopup
    {
        [SerializeField] private List<Card> cardList = null;

        public void Initialize(List<int> _cardList)
        {
            _cardList.Sort();
            
            for (int i = 0; i < _cardList.Count; i++)
            {
                cardList[i].SetCard(_cardList[i]);
            }
        }
    }
}
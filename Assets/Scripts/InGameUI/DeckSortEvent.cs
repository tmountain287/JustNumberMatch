using Common.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Popup
{
    public class DeckSortEvent : PlayerPrefsChangeEvent
    {
        [SerializeField] private CardDeck cardDeck = null;

        protected override void SetChangeEvent()
        {
            cardDeck.SortCardPostionGroupList(PlayerPrefsManager.Instance.GetPlayerPrefsValue(PrefsKey.BOARD_SORT) == 0);
        }
    }
}

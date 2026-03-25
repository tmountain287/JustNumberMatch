using Common.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gostop.UI
{
    public class TableSortEvent : PlayerPrefsChangeEvent
    {
        [SerializeField] private TableCards tableCards = null;

        protected override void SetChangeEvent()
        {
            tableCards.SortCardPostionGroupList(PlayerPrefsManager.Instance.GetPlayerPrefsValue(PrefsKey.BOARD_SORT) == 0);
        }
    }
}

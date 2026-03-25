using Common.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class HandCardSortEvent : PlayerPrefsChangeEvent
    {
        [SerializeField] private bool isReverse = true;
        [SerializeField] private HorizontalLayoutGroup layoutGroup = null;

        protected override void SetChangeEvent()
        {
            layoutGroup.reverseArrangement = PlayerPrefsManager.Instance.GetPlayerPrefsValue(PrefsKey.HANDS_SORT) == (isReverse ? 0 : 1);
        }
    }
}

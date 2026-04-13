using Common.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class HandCardSortEvent : PlayerPrefsChangeEvent
    {
        [SerializeField] private bool isReverse = true;
        [SerializeField] private HorizontalLayoutGroup layoutGroup = null;
        [SerializeField] private GameObject stealButtonPivotR = null;
        [SerializeField] private GameObject stealButtonPivotL = null;

        protected override void SetChangeEvent()
        {
            int sort = (int)PlayerPrefsManager.Instance.GetPlayerPrefsValue(PrefsKey.HANDS_SORT);
            layoutGroup.reverseArrangement = sort == (isReverse ? 0 : 1);

            if (stealButtonPivotR != null && stealButtonPivotL != null)
            {
                stealButtonPivotR.SetActive(sort == 1);
                stealButtonPivotL.SetActive(sort != 1);
            }
        }
    }
}
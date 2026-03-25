using InGame;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class MissionContent : MonoBehaviour
    {
        [SerializeField] private List<GameObject> mulNumList = null;
        [SerializeField] private List<Card> missionCardList = null;
        [SerializeField] private RectTransform rectTransform = null;

        private void OnDisable()
        {
            gameObject.SetActive(false);
        }

        public void SetMissionContent(netRoundMission _netRoundMission)
        {
            gameObject.SetActive(false);

            if (_netRoundMission == null) return;

            missionCardList.ForEach(card => card.gameObject.SetActive(false));
            
            mulNumList.ForEach(num => num.SetActive(false));

            if (_netRoundMission.iMultiple > 0)
            {
                mulNumList[_netRoundMission.iMultiple - 2].SetActive(true);
            }
         
            for (int i = 0; i < _netRoundMission.aMissionCard.Count; i++)
            {
                missionCardList[i].SetCard(_netRoundMission.aMissionCard[i]);
                missionCardList[i].gameObject.SetActive(true);
            }

            gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using Common.UI;

namespace UI.Popup
{
    public class InGameCardSelectPopup : BasePopup
    {
        [SerializeField] private List<Card> selectCardList = null;

        public void Initialize(List<int> _cardList, Func<int, bool> _isMissionCardFunc)
        {
            for(int i = 0; i < _cardList.Count; i++)
            {
                int selectSlot = _cardList[i];
                selectCardList[i].SetCard(_cardList[i]);
                selectCardList[i].RefreshMission(_isMissionCardFunc.Invoke(_cardList[i]));
                selectCardList[i].OnClick = () =>
                {
                    selectCardList.ForEach(card => card.OnClick = null);

                    //추후
                    //NetworkManager.Instance.LobbySession.SendReqSelectBoardCard(selectSlot);
                    InGameManager.Instance.SendReqSelectBoardCard(selectSlot);
                    ClosePopup();
                };
            }
        }
    }
}
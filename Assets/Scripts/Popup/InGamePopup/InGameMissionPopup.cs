using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Common.UI;

namespace UI.Popup
{
    public class InGameMissionPopup : BasePopup
    {
        [SerializeField] private Text title = null;
        [SerializeField] private MissionContent missionContent = null;
        
        private Tween delayCall = null;
        private Action onClose = null;

        public void Initialize(netRoundMission _netRoundMission, Action _onClose)
        {
            title.text = _netRoundMission.strMissionName;
            missionContent.SetMissionContent(_netRoundMission);
          
            onClose = _onClose;
            delayCall = DOVirtual.DelayedCall(1.0f, () =>
            {
                ClosePopup();
            });
        }

        public override void Close(Action _onClose = null)
        {
            if (delayCall != null)
            {
                delayCall.Kill();
                delayCall = null;
            }
        
            base.Close(onClose);
        }
    }
}
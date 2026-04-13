using Common.UI;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class InGameBbukRewardPopup : BasePopup
    {
        [SerializeField] private Button okButton = null;
        [SerializeField] private Text text = null;
        [SerializeField] private Text goldText = null;

        [SerializeField] private GameObject bankruptcy = null;
        [SerializeField] private Button nextLevelButton = null;

        Tween delayCall = null;

        protected override void Start()
        {
            base.Start();
            okButton.onClick.AddListener(() =>
            {
                //NetworkManager.Instance.LobbySession.SendReqSelectGukjunToPee(true);
                InGameManager.Instance.SendReqBbukReward();
                //ClosePopup();
            });

            nextLevelButton.onClick.AddListener(() =>
            {
                //NetworkManager.Instance.LobbySession.SendReqSelectGukjunToPee(true);
                InGameManager.Instance.SendReqNextLevel();
                //ClosePopup();
            });
        }

        public void Initialize(int _count, long _reward, bool _isMy, bool _isBankruptcy)
        {
            bankruptcy.SetActive(_isMy && _isBankruptcy);
            okButton.gameObject.SetActive(!(_isMy && _isBankruptcy));

            if (_count == 1)
            {
                text.text = "<color=#FF9200>첫뻑</color> 보상";
            }
            else if (_count == 2)
            {
                text.text = "<color=#FF9200\\>2연뻑</color> 보상";
            }
            else if (_count == 3)
            {
                text.text = "<color=#FF9200\\>3연뻑</color> 보상";
            }

            goldText.color = _isMy ? Color.yellow : Color.red;
            goldText.text = _isMy ? $"+{_reward.FormatKoreanUnits()}" : $"-{_reward.FormatKoreanUnits()}";

            //if(!(_isMy && _isBankruptcy))
            //{
            //    delayCall = DOVirtual.DelayedCall(2.0f, () =>
            //    {
            //        ClosePopup();
            //    });
            //}            
        }

        public override void Close(Action _onClose = null)
        {
            if (delayCall != null)
            {
                delayCall.Kill();
                delayCall = null;
            }
            base.Close(_onClose);
        }
    }
}
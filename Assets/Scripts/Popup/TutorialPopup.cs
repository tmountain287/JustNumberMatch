using Common.Manager;
using Common.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace JustOneMatch.UI
{
    public class TutorialPopup : BasePopup
    {
        [SerializeField] private LocalChangeTextEvent changeTextEvent = null;

        private Action closeAction = null;
        

        protected override void Start()
        {
            closeButton.onClick.AddListener(() =>
            {
                closeAction?.Invoke();
                ClosePopup();
            });
        }

        public void Initialize(string _localString, Action _closeAction)
        {
            changeTextEvent.EntryKey = _localString;
            closeAction = _closeAction;
        }
    }
}
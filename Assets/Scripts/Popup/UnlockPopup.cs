using Common.Manager;
using Common.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JustOneMatch.UI
{
    public class UnlockPopup : BasePopup
    {
        [SerializeField] private List<GameObject> objList = null;
        [SerializeField] private LocalChangeTextEvent text = null;
        [SerializeField] private List<string> strings = null;

        private Action closeAction = null;

        protected override void Start()
        {
            closeButton.onClick.AddListener(() =>
            {
                ClosePopup(closeAction);

            });
        }

        public void Initialize(int _index, Action _closeAction)
        {
            closeAction = _closeAction;

            for (int i = 0; i < objList.Count; i++)
            {
                objList[i].SetActive(i == _index);
            }

            text.EntryKey = strings[_index];
        }
    }
}
using Common.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class InGameLevelupPopup : BasePopup
    {
        [SerializeField] private Text levelText = null;
        [SerializeField] private Text nameText = null;
        //[SerializeField] private CharImage character = null;
        [SerializeField] private Button confirmButton = null;

        [SerializeField] private Text nextText = null;
        //[SerializeField] private Text fakeText = null;
        //[SerializeField] private Text dialogueText = null;
        
        private Action onClick = null;

        protected override void Start()
        {
            base.Start();
            confirmButton.onClick.AddListener(() =>
            {
                onClick?.Invoke();
            });
        }
        

        public void Initialize(int _level, bool _isDebug = false)
        {
            levelText.text = _level.ToString();
            //nameText.text = _data.name;

            onClick = _isDebug ? () =>
            {
                ClosePopup();
            }
            : () => 
            {
                InGameManager.Instance.SendReqNextLevelConfirm();
                ClosePopup();
            };
           // character.SetImage(_data.id);
           // nameText.text = _data.name;
            nextText.text = _level > 1 ? "다음상대 : " : "첫 상대 : ";
            
           // StartTyping(_data.dialogue.Replace("\"", "").Replace("\\n", "\n"));
        }

        public void Initialize(int _level, Action _onClick)
        {
            levelText.text = _level.ToString();
            //nameText.text = _data.name;

            onClick = () =>
            {
                _onClick?.Invoke();
                ClosePopup();
            };
           // character.SetImage(_data.id);
          //  nameText.text = _data.name;
            nextText.text = _level > 1 ? "다음상대 : " : "첫 상대 : ";

            // StartTyping(_data.dialogue.Replace("\"", "").Replace("\\n", "\n"));
        }

        //public void StartTyping(string message)
        //{
        //    fakeText.text = message;

        //    StopAllCoroutines(); // 중복 방지
        //    StartCoroutine(TypingCoroutine(message));
        //}

        //private IEnumerator TypingCoroutine(string message)
        //{
        //    dialogueText.text = "";
        //    foreach (char c in message)
        //    {
        //        dialogueText.text += c;
        //        yield return new WaitForSeconds(0.03f);
        //    }
        //}
    }
}
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Common.UI;

namespace Gostop.UI
{
    public class InGameLevelupPopup : BasePopup
    {
        [SerializeField] private Text levelText = null;
        [SerializeField] private Text nameText = null;
        [SerializeField] private Image character = null;
        [SerializeField] private Button confirmButton = null;

        [SerializeField] private Text fakeText = null;
        [SerializeField] private Text dialogueText = null;
        
        private Action onClick = null;

        protected override void Start()
        {
            base.Start();
            confirmButton.onClick.AddListener(() =>
            {
                onClick?.Invoke();
            });
        }
        

        public void Initialize(int _level, CharacterTableData _data, bool _isDebug = false)
        {
            levelText.text = _level.ToString();
            nameText.text = _data.name;

            onClick = _isDebug ? () =>
            {
                ClosePopup();
            }
            : () => 
            {
                InGameManager.Instance.SendReqNextLevelConfirm();
                ClosePopup();
            };
            
            CharacterResManager.Instance.SetImage(character, _data.resource, CharacterImage.Type.LevelUp);
            StartTyping(_data.dialogue.Replace("\"", "").Replace("\\n", "\n"));
        }

        public void StartTyping(string message)
        {
            fakeText.text = message;

            StopAllCoroutines(); // 중복 방지
            StartCoroutine(TypingCoroutine(message));
        }

        private IEnumerator TypingCoroutine(string message)
        {
            dialogueText.text = "";
            foreach (char c in message)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(0.03f);
            }
        }


    }
}
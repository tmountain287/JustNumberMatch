using Common.Manager;
using UnityEngine;
using UnityEngine.UI;
using Common.UI;
using System;

namespace JustOneMatch.UI
{
    public class NickNameChangePopup : BasePopup
    {
        [SerializeField] private InputField inputField = null;
        [SerializeField] private Button changeButton = null;

        private Action closeAction = null;

        protected override void Start()
        {
            closeButton.onClick.AddListener(() =>
            {
                ClosePopup(closeAction);
            });

            inputField.onValidateInput += OnValueChanged;
            changeButton.onClick.AddListener(() =>
            {
                if(inputField.text == UserDataManager.NickName || inputField.text == "")
                {
                    ClosePopup();
                }
                else
                {
                    if(TableDataManager.Instance.TableSlangData.ContainsSlang(inputField.text))
                    {
                        PopupManager.Instance.OpenMessageBoxPopup("", LocalizationManager.Instance.GetText("inappropriate characters"));
                    }
                    else
                    {                        
                        UserDataManager.NickName = inputField.text;
                        ClosePopup(() =>
                        {
                            UIManager.Instance.ShowLoading();
                            UserDataManager.Save(true, () =>
                            {
                                UIManager.Instance.HideLoading();
                                PopupManager.Instance.OpenMessageBoxPopup("", LocalizationManager.Instance.GetText("NicknameChanged"));
                            });
                        });
                    }                        
                }
            });
        }
        private char OnValueChanged(string text, int charIndex, char addedChar)
        {
            int predictedLength = text.Length;
            int composing = Input.compositionString.Length;

            // 현재 길이 + 조합 중 문자 포함 시 10자 초과되면 차단
            if ((text.Length + composing) >= 10)
                return '\0';

            return addedChar;
            //string filtered = Regex.Replace(input, @"[^가-힣a-zA-Z0-9]", "");

            //// characterLimit 적용 후 substring
            //if (filtered.Length > inputField.characterLimit)
            //{
            //    filtered = filtered.Substring(0, inputField.characterLimit);
            //}

            //if (inputField.text != filtered) 

            //{
            //    int pos = Mathf.Min(filtered.Length, inputField.caretPosition);
            //    inputField.SetTextWithoutNotify(filtered);
            //    inputField.caretPosition = pos;
            //}
        }

        public char? GetInvalidChar(string input)
        {
            foreach (char c in input)
            {
                if (!(IsKorean(c) || IsEnglishLetter(c) || char.IsDigit(c)))
                {
                    return c; // 유효하지 않은 문자 반환
                }
            }
            return null; // 모두 유효한 경우
        }

        private bool IsKorean(char c)
        {
            return (c >= '\uAC00' && c <= '\uD7A3'); // 한글 완성형 (가~힣)
        }

        private bool IsEnglishLetter(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        }

        public void Initialize(Action _closeAction)
        {
            closeAction = _closeAction;
            inputField.text = UserDataManager.NickName;
        }
    }
}
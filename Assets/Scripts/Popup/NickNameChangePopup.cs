using Common.Manager;
using UnityEngine;
using UnityEngine.UI;
using Common.UI;

namespace Gostop.UI
{
    public class NickNameChangePopup : BasePopup
    {
        [SerializeField] private InputField inputField = null;
        [SerializeField] private Button changeButton = null;

        protected override void Start()
        {
            base.Start();
            inputField.onValidateInput += OnValueChanged;
            changeButton.onClick.AddListener(() =>
            {
                if(inputField.text == UserDataManager.NickName || inputField.text == "")
                {
                    ClosePopup();
                }
                else
                {
                    var result = GetInvalidChar(inputField.text);
                    if (result != null)
                    {
                        PopupManager.Instance.OpenMessageBoxPopup("알림", "<color=#FF6600>유효하지 않은 문자</color>가\n포함되었습니다.");
                    }
                    else if(TableDataManager.Instance.TableSlangData.ContainsSlang(inputField.text))
                    {
                        PopupManager.Instance.OpenMessageBoxPopup("알림", "<color=#FF6600>적절하지 않은 문자</color>가\n포함되었습니다.");
                    }
                    else
                    {
                        InGameManager.Instance.SendReqNickNameChange(inputField.text);
                        UserDataManager.NickName = inputField.text;
                        ClosePopup(() =>
                        {
                            UIManager.Instance.ShowLoading();
                            UserDataManager.Save(true, () =>
                            {
                                _ = NetworkManager.Instance.UpdtateNickName(UserDataManager.NickName);
                                UIManager.Instance.HideLoading();
                                PopupManager.Instance.OpenMessageBoxPopup("알림", "<color=#FF6600>별명</color>이 변경되었습니다.");
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

        public void Initialize()
        {
            inputField.text = UserDataManager.NickName;
        }
    }
}
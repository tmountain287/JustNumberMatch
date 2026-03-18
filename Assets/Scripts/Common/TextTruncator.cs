using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UI
{
    public class TextTruncator : MonoBehaviour
    {
        [SerializeField] private Text text = null;
        [SerializeField] private string ellipsStr = "...";

        private void OnValidate()
        {
            if(text == null)
            {
                text = GetComponent<Text>();
            }
        }

        public void SetText(string _str)
        {
            text.text = ProcessText(_str, text, text.rectTransform.rect.width);
        }

        private string ProcessText(string originalText, Text uiText, float maxWidth)
        {
            if (uiText == null)
            {
                Debug.LogError("Text component is not assigned.");
                return originalText;
            }

            // TextGenerator 및 설정 가져오기
            TextGenerator textGen = new TextGenerator();
            TextGenerationSettings settings = uiText.GetGenerationSettings(Vector2.zero);

            // 원래 텍스트의 너비 계산
            float originalTextWidth = textGen.GetPreferredWidth(originalText, settings);

            // 너비를 초과하지 않으면 그대로 반환
            if (originalTextWidth <= maxWidth)
            {
                return originalText;
            }

            // `...`의 너비 계산
            float ellipsisWidth = textGen.GetPreferredWidth(ellipsStr, settings);
            float allowedWidth = maxWidth - ellipsisWidth;

            if (allowedWidth <= 0)
            {
                return "..."; // `...`만 표시 가능
            }

            // 텍스트를 반복적으로 줄여가며 적합한 텍스트 찾기
            for (int i = originalText.Length; i >= 0; i--)
            {
                string substring = originalText.Substring(0, i);
                float substringWidth = textGen.GetPreferredWidth(substring, settings);

                if (substringWidth <= allowedWidth)
                {
                    return substring + ellipsStr;
                }
            }

            return "..."; // 모든 텍스트가 너무 길 경우
        }
    }
}
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace Common.UI
{
    [RequireComponent(typeof(Text))]
    public class NumberIncrement : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private bool isAutoReset = true;
        [SerializeField] private Text numberText = null;
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private bool useZero = true;
        [SerializeField] private bool useKoreaUnit = false;
        #endregion

        private long currentValue = 0;

        public float Duration { get => duration; set => duration = value; }

        private void OnValidate()
        {
            if(numberText == null)
            {
                numberText = GetComponent<Text>();
            }
        }

        private void OnDisable()
        {
            if(isAutoReset)
            {
                SetNumber(0, false);
            }
        }

        public void SetNumber(long _value, bool _ani = true)
        {
            if (_ani)
            {
                DOTween.To(() => currentValue, x =>
                {
                    currentValue = x;
                    numberText.text = currentValue == 0 && !useZero ? "" : (useKoreaUnit ? currentValue.FormatKoreanUnits(false) : currentValue.FormatComma());
                }, _value, Duration).OnComplete(()=>
                {
                    currentValue = _value;
                    numberText.text = currentValue == 0 && !useZero ? "" : (useKoreaUnit ? currentValue.FormatKoreanUnits(false) : currentValue.FormatComma());
                }).SetEase(Ease.Linear);
            }
            else
            {
                currentValue = _value;
                numberText.text = currentValue == 0 && !useZero ? "" : (useKoreaUnit ? currentValue.FormatKoreanUnits(false) : currentValue.FormatComma());
            }
        }
    }
}
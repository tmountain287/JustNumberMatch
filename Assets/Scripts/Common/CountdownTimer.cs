using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;


namespace Common.UI
{
    [RequireComponent(typeof(Text))]
    public class CountdownTimer : MonoBehaviour
    {
        [SerializeField] private Text timerText = null;

        private void OnValidate()
        {
            if (timerText == null)
            {
                timerText = GetComponent<Text>();
            }
        }

        public void StartTimer(float _startTime, Action _onComplete = null)
        {
            DOTween.To(() => _startTime, x => _startTime = x, 0, _startTime)
                .OnUpdate(() =>
                {
                    timerText.text = FormatTime(_startTime);
                })
                .OnComplete(() =>
                {
                    timerText.text = "00:00:00";
                    _onComplete?.Invoke();
                });
        }
        
        private string FormatTime(float timeInSeconds)
        {
            int hours = (int)timeInSeconds / 3600; // 전체 초에서 시간 계산
            int minutes = ((int)timeInSeconds % 3600) / 60; // 남은 초에서 분 계산
            int seconds = (int)timeInSeconds % 60; // 나머지 초 계산
            return $"{hours:00}:{minutes:00}:{seconds:00}";
        }
    }
}

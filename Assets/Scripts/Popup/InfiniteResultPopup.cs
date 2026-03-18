using Common.Manager;
using Common.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace JustOneMatch.UI
{
    public class InfiniteResultPopup : BasePopup
    {
        [SerializeField] private Text prebestScoreText = null;
        [SerializeField] private Text bestScoreText = null;
        [SerializeField] private Text scoreText = null;

        [SerializeField] private Button restartButton = null;
        [SerializeField] private Button exitButton = null;

        [SerializeField] private GameObject prebestObj = null;
        [SerializeField] private GameObject bestObj = null;

        [SerializeField] private GameObject newRecordFlag = null;
        [SerializeField] private List<RectTransform> rectList = null;
        [SerializeField] private AudioClip popAudioClip = null;

        private Action restartAction = null;
        private Action exitAction = null;

        protected override void Start()
        {
            base.Start();
            restartButton.onClick.AddListener(() =>
            {
                ClosePopup();
                restartAction?.Invoke();
            });

            exitButton.onClick.AddListener(() =>
            {
                ClosePopup();
                exitAction?.Invoke();
            });
        }

        public void Initialize(long _score, long _bestScore, bool _isBest, Action _restartAction, Action _exitAction = null)
        {
            restartAction = _restartAction;
            exitAction = _exitAction;

            bestScoreText.text = _bestScore.FormatComma();
            
            if (newRecordFlag != null)
                newRecordFlag.SetActive(_isBest);

            prebestObj.SetActive(_isBest);
            bestObj.SetActive(!_isBest);

            if (_isBest)
            {
                prebestScoreText.text = _bestScore.FormatComma();
            }
            else
            {
                bestScoreText.text = _bestScore.FormatComma();
            }

            scoreText.text = _score.FormatComma();

            DOVirtual.DelayedCall(delay, () =>
            {
                if (rectList == null) return;

                for (int i = 0; i < rectList.Count; i++)
                {
                    int index = i;
                    var rt = rectList[index];

                    if (!rt.gameObject.activeSelf)
                        continue;

                    rt.localScale = Vector3.zero;

                    var seq = DOTween.Sequence();
                    seq.Append(rt.DOScale(1.1f, 0.35f).SetEase(Ease.OutBack).OnStart(() =>
                    {
                        if (popAudioClip != null)
                            SoundManager.Instance.PlayFX(popAudioClip);
                    }))
                       .Append(rt.DOScale(1.0f, 0.12f).SetEase(Ease.OutCubic))
                       .SetDelay(index * 0.1f);
                }
            });
        }
    }
}

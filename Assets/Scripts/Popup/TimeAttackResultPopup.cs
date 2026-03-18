using Common.Manager;
using Common.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JustOneMatch.UI
{
    public class TimeAttackResultPopup : BasePopup
    {       
        [SerializeField] private GameObject newRecordText = null;
        [SerializeField] private Text stageClearText = null;
        
        [SerializeField] private Button restartButton = null;        
        [SerializeField] private Button exitButton = null;

        [SerializeField] private GameObject prebestObj = null;
        [SerializeField] private GameObject bestObj = null;

        [SerializeField] private Text prebestTimeText = null;
        [SerializeField] private Text bestTimeText = null;

        [SerializeField] private Text totalTimeText = null;
        [SerializeField] private Text avgTimeText = null;
        [SerializeField] private Text fastTimeText = null;
        [SerializeField] private Text longTimeText = null;
        [SerializeField] private Text ticketCountText = null;

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

        public void Initialize(DifficultyType _difficultyType, SolveRunResultMs _result, bool _isBest, long _preBest, Action _restartAction, Action _exitAction = null)
        {
            restartAction = _restartAction;
            exitAction = _exitAction;
            stageClearText.text = _difficultyType.ToString();
            newRecordText.SetActive(_isBest);
            newRecordFlag.SetActive(_isBest);

            prebestObj.SetActive(_isBest);
            bestObj.SetActive(!_isBest);

            if (_isBest)
            {
                prebestTimeText.text = _preBest.FormatFromMs();
            }
            else
            {
                bestTimeText.text = _preBest.FormatFromMs();
            }

            totalTimeText.text = _result.TotalMs.FormatFromMs();
            avgTimeText.text = ((long)_result.AvgMs).FormatFromMs();
            fastTimeText.text = _result.MinMs.FormatFromMs();
            longTimeText.text = _result.MaxMs.FormatFromMs();

            ticketCountText.text = $"x{ConfigData.NeedTimeAttckTicketCountDic[_difficultyType]}";

            DOVirtual.DelayedCall(delay, () =>
            {
                for (int i = 0; i < rectList.Count; i++)
                {
                    int index = i;
                    var rt = rectList[index];

                    if (!rt.gameObject.activeSelf)
                    {
                        continue;
                    }

                    rt.localScale = Vector3.zero;

                    var seq = DOTween.Sequence();
                    seq.Append(rt.DOScale(1.1f, 0.35f).SetEase(Ease.OutBack).OnStart(() =>
                    {
                        SoundManager.Instance.PlayFX(popAudioClip);
                    }))
                       .Append(rt.DOScale(1.0f, 0.12f).SetEase(Ease.OutCubic))
                       .SetDelay(index * 0.1f);
                }
            });
        }
    }
}
using Common.Manager;
using Common.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JustOneMatch.UI
{
    public class SurvivalModePopup : BasePopup
    {
        [SerializeField] private Text bestScoreText = null;
        [SerializeField] private Text lastScoreText = null;
        [SerializeField] private Button startButton = null;

        private Action startAction = null;

        protected override void Start()
        {
            base.Start();
            startButton.onClick.AddListener(() =>
            {
                ClosePopup(startAction);
            });
        }

        public void Initialize(Action _startAction)
        {
            startAction = _startAction;

            bestScoreText.text = UserDataManager.UserData.infiniteBestScore.FormatComma();
            lastScoreText.text = UserDataManager.UserData.infiniteLastScore.FormatComma();
        }
    }
}
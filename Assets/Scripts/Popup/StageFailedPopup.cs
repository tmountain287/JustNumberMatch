using Common.Manager;
using Common.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace JustOneMatch.UI
{
    public class StageFailedPopup : BasePopup
    {
        [SerializeField] private Button okButton = null;

        private Action onOkAction = null;

        protected override void Start()
        {            
            okButton.onClick.AddListener(() =>
            {
                ClosePopup();
                onOkAction.Invoke();
            });

            closeButton.onClick.AddListener(() =>
            {               
                ClosePopup();
                UIManager.Instance.ShowUI(Common.UI.BaseUI.Type.STAGE);
                PopupManager.Instance.OpenPopup<StagePopup>()
                    .Initialize(GameMgr.Instance.CurrentTableData.difficultyType,
                                GameMgr.Instance.CurrentTableData.stage);
            });
        }

        public void Initialize(Action _onOkAction)
        {
            onOkAction = _onOkAction;
            var stageData = GameMgr.Instance?.CurrentStageTableData;
            if (stageData != null)
                GameAnalyticsHelper.LogStageFail(stageData.difficultyType.ToString().ToLower(), stageData.id, "time_over");
        }
    }
}
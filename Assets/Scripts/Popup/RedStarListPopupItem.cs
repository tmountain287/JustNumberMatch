using Common.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JustOneMatch.UI
{
    public class RedStarListPopupItem : MonoBehaviour
    {
        [SerializeField] private Text stageLabelText;
        [SerializeField] private List<GameObject> starObjList;
        [SerializeField] private Button goButton;
        private int stageTableId;
        private Action<int> onGoClicked;

        private void Awake()
        {
            if (goButton != null)
                goButton.onClick.AddListener(OnGoClicked);
        }

        public void SetData(DifficultyType _difficultyType, BossStarInfo _bossStarInfo, Action<int> _onGo)
        {
            stageTableId = _bossStarInfo.stageID;
            onGoClicked = _onGo;

            StageTableData stage = TableDataManager.Instance.TableStageData.GetTableData(_difficultyType, _bossStarInfo.stageID);
            if (stage == null)
                return;

            stageLabelText.text = string.Format(LocalizationManager.Instance.GetText("GateStage"), stage.stage);
        }

        void OnGoClicked()
        {
            onGoClicked?.Invoke(stageTableId);
        }
    }
}

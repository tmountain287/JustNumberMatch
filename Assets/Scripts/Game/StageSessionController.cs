using Common.Manager;
using JustOneMatch.UI;
using System;
using System.Collections;
using UnityEngine;

public sealed class StageSessionController : GameSessionController
{   
    private IStageSequence seq;
    public override void ReadySession(IStageSequence sequence)
    {
        seq = sequence;
        StartSession();
    }

    public override void StartSession()
    {       
        GameMgr.Instance.OnStageCleared += OnStageCleared;

        //gm.CurrentGameMode = GameModeType.STAGE;
        //gm.StageCleared -= OnStageCleared;
        //gm.StageCleared += OnStageCleared;

        seq.Reset();
        seq.MoveNext();                 // 단일이든 리스트든 현재 1판을 가리킴
        GameAnalyticsHelper.LogStageStart(StageTableData.difficultyType.ToString().ToLower(), StageTableData.id, seq.Current?.stage.ToString());
        GameMgr.Instance.SetEquation(seq.Current);
        //UIManager.Instance.ShowUI(BaseUI.Type.GAME);
       // sw.Restart();

        // HUD: 진행도 표기하고 싶으면 1/1 고정
        // gm.GameUI.SetProgress(0, seq.Count);
    }

    public override void ChangeSequence()
    {
    }

    public override void StopSession()
    {
        //   gm.StageCleared -= OnStageCleared;
        GameMgr.Instance.OnStageCleared -= OnStageCleared;     
    }

    private void OnStageCleared()
    {
        int level = UserDataManager.Level;
        int xp = UserDataManager.XP;
        GameMgr.Instance.GameUI.GoldStateBox.SetBlockAutoUpdate(true);
        GameMgr.Instance.GameUI.SkillItemButtonList.ForEach(x => x.SetBlockAutoUpdate(true));
        var clearResult = UserDataManager.ClearStage(StageTableData);

        Action nextAction = null;
        Action exitAction = null;
        Action popupAction = null;

        popupAction = () =>
        {
            PopupManager.Instance.OpenPopup<ResultPopup>().Initialize(clearResult.Item1, StageTableData, clearResult.Item2, level, xp, clearResult.Item3, 0, () =>
            {
                GameMgr.Instance.StartStageMode(StageTableData);
            }, nextAction, exitAction);
        };

        StageTableData nextPlayable = StageNextPlayableHelper.FindNextPlayableStage(StageTableData);
        if (nextPlayable != null)
        {
            nextAction = () => GameMgr.Instance.StartStageMode(nextPlayable);
        }
        
        exitAction = () =>
        {
            UIManager.Instance.ShowUI(Common.UI.BaseUI.Type.STAGE);
            PopupManager.Instance.OpenPopup<StagePopup>().Initialize(StageTableData.difficultyType, StageTableData.id);
        };        

        if(clearResult.Item1)
        {
            GameAnalyticsHelper.LogStageComplete(StageTableData.difficultyType.ToString().ToLower(), StageTableData.id, true);
            GameAnalyticsHelper.SetMaxStageCleared(UserDataManager.UserData.clearStageInfoDic[StageTableData.difficultyType]);
            if (clearResult.Item2 != null && clearResult.Item2.TryGetValue(ItemType.Gold, out int goldEarn))
                GameAnalyticsHelper.LogEarnVirtualCurrency("gold", goldEarn, "stage_clear");
            UserDataManager.Save(_onComplete: () =>
            {
                popupAction.Invoke();
            });
        }
        else
        {
            popupAction.Invoke();
        }       
    }
}
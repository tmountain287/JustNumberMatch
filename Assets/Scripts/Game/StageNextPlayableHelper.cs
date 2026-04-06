using System.Linq;
using UnityEngine;

/// <summary>
/// 결과 팝업 "다음" 등에서 이어갈 스테이지를 고를 때 사용.
/// 보스는 레드 스타를 이미 최대로 맞춘 경우(재도전 불가) 다음 일반/미완료 보스로 건너뜀.
/// </summary>
public static class StageNextPlayableHelper
{
    public static StageTableData FindNextPlayableStage(StageTableData currentStage)
    {
        if (currentStage == null || UserDataManager.UserData == null)
            return null;

        if (TableDataManager.Instance?.TableStageData?.StageTableDataDic == null)
            return null;

        if (!TableDataManager.Instance.TableStageData.StageTableDataDic.TryGetValue(currentStage.difficultyType, out var list) ||
            list == null || list.Count == 0)
            return null;

        int idx = list.FindIndex(x => x.id == currentStage.id);
        if (idx < 0 || idx >= list.Count - 1)
            return null;

        int clearedId = UserDataManager.UserData.clearStageInfoDic[currentStage.difficultyType];

        for (int i = idx + 1; i < list.Count; i++)
        {
            StageTableData s = list[i];
            if (s.stageType != StageType.Boss)
                return s;

            if (!ShouldSkipBossWithMaxRedStars(s, clearedId))
                return s;
        }

        return null;
    }

    /// <summary>이미 최대 레드 스타로 클리어된 보스면 true (목록에서 건너뛸 것).</summary>
    static bool ShouldSkipBossWithMaxRedStars(StageTableData boss, int clearedStageTableId)
    {
        if (boss.id > clearedStageTableId)
            return false;

        if (!UserDataManager.UserData.bossStarInfoDic.TryGetValue(boss.difficultyType, out var infos) || infos == null)
            return true;

        BossStarInfo info = infos.FirstOrDefault(x => x.stageID == boss.id);
        if (info == null)
            return true;
        return info.starCount >= boss.starMax;
    }
}

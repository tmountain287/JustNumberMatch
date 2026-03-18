using UnityEngine.SocialPlatforms;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSocialIOS : PlatformSocialBase
{
    private HashSet<string> completedAchievementList = new();
    private Dictionary<string, long> lastReportedScores = new();

    public override void Initialize()
    {
        Social.localUser.Authenticate(success =>
        {
            if (success)
            {
                Debug.Log("[GameCenter] Login Success");
                CheckAchievements();
            }
            else
            {
                Debug.Log("[GameCenter] Login Failed");
            }
        });
    }

    public override void ShowAchievementsUI(Action onFail = null)
    {
        if (Social.localUser.authenticated)
            Social.ShowAchievementsUI();
        else
            onFail?.Invoke();
    }

    public override void ShowLeaderboardsUI(Action onFail = null)
    {
        if (Social.localUser.authenticated)
            Social.ShowLeaderboardUI();
        else
            onFail?.Invoke();
    }

    public override void CheckAchievements()
    {
        Social.LoadAchievements(achievements =>
        {
            if (achievements == null)
            {
                Debug.Log("[GameCenter] 업적 불러오기 실패");
                return;
            }

            foreach (var a in achievements)
            {
                if (a.completed)
                    completedAchievementList.Add(a.id);
            }
        });
    }

    public override void UnlockAchievement(string achievementId)
    {
        if (!Social.localUser.authenticated)
            return;

        if (!completedAchievementList.Contains(achievementId))
        {
            Social.ReportProgress(achievementId, 100.0, success =>
            {
                if (success)
                {
                    completedAchievementList.Add(achievementId);
                    Debug.Log($"[업적] 달성 완료: {achievementId}");
                }
                else
                {
                    Debug.Log($"[업적] 달성 실패: {achievementId}");
                }
            });
        }
    }

    public override void IncrementAchievement(string achievementId, int steps = 1)
    {
        
    }
    

    public override void ReportScore(long score, string leaderboardId)

    {
        if (!Social.localUser.authenticated)
            return;

        if (lastReportedScores.TryGetValue(leaderboardId, out var lastScore) && lastScore == score)
            return;

        Social.ReportScore(score, leaderboardId, success =>
        {
            if (success)
            {
                lastReportedScores[leaderboardId] = score;
                Debug.Log($"[리더보드] 점수 {score} 업로드 성공 ({leaderboardId})");
            }
            else
            {
                Debug.Log($"[리더보드] 점수 {score} 업로드 실패 ({leaderboardId})");
            }
        });
    }
}
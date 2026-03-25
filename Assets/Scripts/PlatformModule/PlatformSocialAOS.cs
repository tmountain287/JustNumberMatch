#if UNITY_ANDROID //&& !UNITY_EDITOR
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlatformSocialAOS : PlatformSocialBase
{
    private HashSet<string> completedAchievementList = new();
    private Dictionary<string, long> lastReportedScores = new();

    public override void Initialize()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            if (status == SignInStatus.Success)
            {
                Debug.Log("[GPG] Login Success");

                CheckAchievements();
                if (!completedAchievementList.Any(x => x == "CgkIofrI1qsSEAIQCA"))
                    IncrementAchievement("CgkIofrI1qsSEAIQCA");
            }
            else
            {
                Debug.Log($"[GPG] Login Failed: {status}");
            }
        });
    }

    public override void ShowAchievementsUI(Action onFail = null)
    {
        if (!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            onFail?.Invoke();
            return;
        }

        PlayGamesPlatform.Instance.ShowAchievementsUI(uiStatus =>
        {
            if (uiStatus != UIStatus.Valid)
            {
                Debug.Log($"[GPG] ShowAchievementsUI failed: {uiStatus}");
                onFail?.Invoke();
            }
        });
    }

    public override void ShowLeaderboardsUI(Action onFail = null)
    {
        if (!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            onFail?.Invoke();
            return;
        }

        PlayGamesPlatform.Instance.ShowLeaderboardUI(null, uiStatus =>
        {
            if (uiStatus != UIStatus.Valid)
            {
                Debug.Log($"[GPG] ShowAchievementsUI failed: {uiStatus}");
                onFail?.Invoke();
            }
        });
    }

    public override void CheckAchievements()
    {
        if (!PlayGamesPlatform.Instance.IsAuthenticated())
            return;

        PlayGamesPlatform.Instance.LoadAchievements(achievements =>
        {
            if (achievements == null)
            {
                Debug.Log("[GPG] 업적 불러오기 실패");
                return;
            }

            foreach (var a in achievements)
            {
                if (a.completed)
                {
                    completedAchievementList.Add(a.id);
                }
            }
        });
    }

    public override void UnlockAchievement(string achievementId)
    {
        if (!PlayGamesPlatform.Instance.IsAuthenticated())
            return;

        if (!completedAchievementList.Contains(achievementId))
        {
            PlayGamesPlatform.Instance.ReportProgress(achievementId, 100.0, success =>
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
        if (!PlayGamesPlatform.Instance.IsAuthenticated())
            return;

        PlayGamesPlatform.Instance.IncrementAchievement(achievementId, steps, success =>
        {
            if (success)
            {
                Debug.Log($"[업적] 증가 성공: {achievementId} (+{steps})");
            }
            else
            {
                Debug.Log($"[업적] 증가 실패: {achievementId}");
            }
        });
    }

    public override void ReportScore(long score, string leaderboardId)
    {
        if (!PlayGamesPlatform.Instance.IsAuthenticated())
            return;

        if (lastReportedScores.TryGetValue(leaderboardId, out var lastScore) && lastScore == score)
            return;

        PlayGamesPlatform.Instance.ReportScore(score, leaderboardId, success =>
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
#endif

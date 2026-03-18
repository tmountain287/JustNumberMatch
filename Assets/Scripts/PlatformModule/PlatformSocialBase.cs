using System;

public abstract class PlatformSocialBase
{
    public abstract void Initialize();
    public abstract void ShowAchievementsUI(Action onFail = null);
    public abstract void ShowLeaderboardsUI(Action onFail = null);
    public abstract void CheckAchievements();
    public abstract void UnlockAchievement(string achievementId);
    public abstract void IncrementAchievement(string achievementId, int steps = 1);
    public abstract void ReportScore(long score, string leaderboardId);
}

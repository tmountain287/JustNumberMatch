using UnityEngine;

public class PlatformSocialManager : MonoSingletonDont<PlatformSocialManager>
{
    private PlatformSocialBase platform;
    private bool init = false;
    public void Initialize()
    {
        if (init) return;
#if UNITY_ANDROID && !UNITY_EDITOR
        platform = new PlatformSocialAOS();
#elif UNITY_IOS && !UNITY_EDITOR
        platform = new PlatformSocialIOS();
#else
        platform = null;
#endif
        platform?.Initialize();
        init = true;
    }
    
    public void ShowAchievementsUI(System.Action onFail = null) => platform?.ShowAchievementsUI(onFail);
    public void ShowLeaderboardsUI(System.Action onFail = null) => platform?.ShowLeaderboardsUI(onFail);
    public void CheckAchievements() => platform?.CheckAchievements();
    public void UnlockAchievement(string achievementId) => platform?.UnlockAchievement(achievementId);
    public void ReportScore(long score, string leaderboardId) => platform?.ReportScore(score, leaderboardId);
}

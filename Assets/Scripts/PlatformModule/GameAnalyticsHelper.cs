using Firebase.Analytics;
using UnityEngine;

/// <summary>
/// Firebase Analytics 커스텀 이벤트·유저 속성·화면 조회 로깅을 한곳에서 처리합니다.
/// Firebase SDK 상수 대신 공식 이벤트/파라미터 문자열을 사용해 버전 호환성을 높였습니다.
/// 에디터에서는 Firebase 서버로 전송되지 않지만, LogInEditor=true 시 콘솔에 출력해 호출 여부를 확인할 수 있습니다.
/// </summary>
public static class GameAnalyticsHelper
{
    // Firebase 공식 이벤트/파라미터 이름 (상수 미정의 SDK 대응)
    private const string EventLogin = "login";
    private const string EventSignUp = "sign_up";
    private const string EventEarnVirtualCurrency = "earn_virtual_currency";
    private const string EventSpendVirtualCurrency = "spend_virtual_currency";
    private const string EventPurchase = "purchase";
    private const string EventViewItem = "view_item";
    private const string EventSelectContent = "select_content";
    private const string EventTutorialBegin = "tutorial_begin";
    private const string EventTutorialComplete = "tutorial_complete";
    private const string EventScreenView = "screen_view";
    private const string ParamMethod = "method";
    private const string ParamVirtualCurrencyName = "virtual_currency_name";
    private const string ParamValue = "value";
    private const string ParamCurrency = "currency";
    private const string ParamTutorialId = "tutorial_id";
    private const string ParamScreenName = "screen_name";
    private const string ParamScreenClass = "screen_class";

#if UNITY_EDITOR
    /// <summary>true면 에디터에서 이벤트/유저속성을 콘솔에 출력합니다. 빌드에는 영향 없음.</summary>
    public static bool LogInEditor = true;
#endif

    #region App & Lobby

    public static void LogAppOpen(string source = "normal")
    {
        LogEvent("app_open", "source", source);
    }

    public static void LogLobbyEntered(float flowDurationSec = 0f)
    {
        if (flowDurationSec > 0f)
            LogEvent("lobby_entered", "flow_duration_sec", flowDurationSec);
        else
            LogEvent("lobby_entered");
    }

    public static void LogVersionCheckResult(string result, string currentVersion, string serverVersion)
    {
        LogEvent("version_check_result",
            "result", result,
            "current_version", currentVersion ?? "",
            "server_version", serverVersion ?? "");
    }

    #endregion

    #region Auth

    public static void LogLogin(string method)
    {
        FirebaseAnalytics.LogEvent(EventLogin, new Parameter[] { new Parameter(ParamMethod, method ?? "anonymous") });
    }

    public static void LogSignUp(string method)
    {
        FirebaseAnalytics.LogEvent(EventSignUp, new Parameter[] { new Parameter(ParamMethod, method ?? "") });
    }

    public static void LogLogout()
    {
        LogEvent("logout");
    }

    #endregion

    #region Game Mode & Stage

    public static void LogGameModeStart(string gameMode, string difficulty = "", int stageId = -1)
    {
        if (stageId >= 0)
            LogEvent("game_mode_start", "game_mode", gameMode, "difficulty", difficulty, "stage_id", stageId);
        else if (!string.IsNullOrEmpty(difficulty))
            LogEvent("game_mode_start", "game_mode", gameMode, "difficulty", difficulty);
        else
            LogEvent("game_mode_start", "game_mode", gameMode);
    }

    public static void LogStageStart(string difficulty, int stageId, string equationId = null)
    {
        if (!string.IsNullOrEmpty(equationId))
            LogEvent("stage_start", "difficulty", difficulty, "stage_id", stageId, "equation_id", equationId);
        else
            LogEvent("stage_start", "difficulty", difficulty, "stage_id", stageId);
    }

    public static void LogStageComplete(string difficulty, int stageId, bool success, float clearTimeSec = -1f, int attemptCount = -1)
    {
        if (clearTimeSec >= 0f && attemptCount >= 0)
            LogEvent("stage_complete", "difficulty", difficulty, "stage_id", stageId, "success", success ? 1 : 0, "clear_time_sec", clearTimeSec, "attempt_count", attemptCount);
        else if (clearTimeSec >= 0f)
            LogEvent("stage_complete", "difficulty", difficulty, "stage_id", stageId, "success", success ? 1 : 0, "clear_time_sec", clearTimeSec);
        else
            LogEvent("stage_complete", "difficulty", difficulty, "stage_id", stageId, "success", success ? 1 : 0);
    }

    public static void LogStageFail(string difficulty, int stageId, string failReason)
    {
        LogEvent("stage_fail", "difficulty", difficulty, "stage_id", stageId, "fail_reason", failReason ?? "unknown");
    }

    public static void LogTimeAttackComplete(string difficulty, int score, int clearCount, float durationSec, bool isBest)
    {
        LogEvent("time_attack_complete", "difficulty", difficulty, "score", score, "clear_count", clearCount, "duration_sec", durationSec, "is_best", isBest ? 1 : 0);
    }

    public static void LogBossStageComplete(string difficulty, int stageId, bool success, float clearTimeSec = -1f)
    {
        if (clearTimeSec >= 0f)
            LogEvent("boss_stage_complete", "difficulty", difficulty, "stage_id", stageId, "success", success ? 1 : 0, "clear_time_sec", clearTimeSec);
        else
            LogEvent("boss_stage_complete", "difficulty", difficulty, "stage_id", stageId, "success", success ? 1 : 0);
    }

    public static void LogSurvivalSessionEnd(long score, int roundsCleared, float durationSec)
    {
        LogEvent("survival_session_end", "score", (int)Mathf.Clamp(score, 0, int.MaxValue), "rounds_cleared", roundsCleared, "duration_sec", durationSec);
    }

    #endregion

    #region Equation & Skill

    public static void LogEquationSubmit(string resultType, bool isCorrect)
    {
        LogEvent("equation_submit", "result_type", resultType ?? "unknown", "is_correct", isCorrect ? 1 : 0);
    }

    public static void LogHintUsed(int stageId, string hintType = "")
    {
        if (!string.IsNullOrEmpty(hintType))
            LogEvent("hint_used", "stage_id", stageId, "hint_type", hintType);
        else
            LogEvent("hint_used", "stage_id", stageId);
    }

    public static void LogSkillUsed(string skillIdOrItemType, int stageId = -1)
    {
        if (stageId >= 0)
            LogEvent("skill_used", "skill_id", skillIdOrItemType ?? "", "stage_id", stageId);
        else
            LogEvent("skill_used", "skill_id", skillIdOrItemType ?? "");
    }

    #endregion

    #region Economy

    public static void LogEarnVirtualCurrency(string virtualCurrencyName, long value, string source)
    {
        FirebaseAnalytics.LogEvent(EventEarnVirtualCurrency, new Parameter[] {
            new Parameter(ParamVirtualCurrencyName, virtualCurrencyName ?? "gold"),
            new Parameter(ParamValue, value),
            new Parameter("item_id", source ?? "unknown")
        });
    }

    public static void LogSpendVirtualCurrency(string itemName, string virtualCurrencyName, long value)
    {
        FirebaseAnalytics.LogEvent(EventSpendVirtualCurrency, new Parameter[] {
            new Parameter("item_name", itemName ?? ""),
            new Parameter(ParamVirtualCurrencyName, virtualCurrencyName ?? "gold"),
            new Parameter(ParamValue, value)
        });
    }

    public static void LogRewardReceived(string rewardType, long amount, string source)
    {
        LogEvent("reward_received", "reward_type", rewardType ?? "", "amount", amount, "source", source ?? "");
    }

    #endregion

    #region Ads

    public static void LogAdImpression(string adUnit, string format)
    {
        LogEvent("ad_impression", "ad_unit", adUnit ?? "", "format", format ?? "");
    }

    public static void LogAdClick(string adUnit, string format)
    {
        LogEvent("ad_click", "ad_unit", adUnit ?? "", "format", format ?? "");
    }

    public static void LogRewardedAdStart(string placement)
    {
        LogEvent("rewarded_ad_start", "placement", placement ?? "");
    }

    public static void LogRewardedAdComplete(string placement, string rewardType = "")
    {
        if (!string.IsNullOrEmpty(rewardType))
            LogEvent("rewarded_ad_complete", "placement", placement ?? "", "reward_type", rewardType);
        else
            LogEvent("rewarded_ad_complete", "placement", placement ?? "");
    }

    public static void LogRewardedAdSkip(string placement)
    {
        LogEvent("rewarded_ad_skip", "placement", placement ?? "");
    }

    #endregion

    #region IAP

    public static void LogPurchaseBegin(string productId, string price = null)
    {
        if (!string.IsNullOrEmpty(price))
            LogEvent("purchase_begin", "product_id", productId ?? "", "price", price);
        else
            LogEvent("purchase_begin", "product_id", productId ?? "");
    }

    public static void LogPurchase(string currency, double value, string productId, string transactionId = null)
    {
        var p = new Parameter[] {
            new Parameter(ParamCurrency, currency ?? "KRW"),
            new Parameter(ParamValue, value),
            new Parameter("item_id", productId ?? "")
        };
        if (!string.IsNullOrEmpty(transactionId))
        {
            var list = new Parameter[p.Length + 1];
            p.CopyTo(list, 0);
            list[p.Length] = new Parameter("transaction_id", transactionId);
            FirebaseAnalytics.LogEvent(EventPurchase, list);
        }
        else
        {
            FirebaseAnalytics.LogEvent(EventPurchase, p);
        }
    }

    public static void LogPurchaseFail(string productId, string reason)
    {
        LogEvent("purchase_fail", "product_id", productId ?? "", "reason", reason ?? "");
    }

    public static void LogAdRemovedPurchase(string productId)
    {
        LogEvent("ad_removed_purchase", "product_id", productId ?? "");
    }

    #endregion

    #region Shop & UI

    public static void LogViewItem(string itemListOrScreenName)
    {
        LogEvent(EventViewItem, "item_list_id", itemListOrScreenName ?? "");
    }

    public static void LogSelectContent(string contentType, string itemId)
    {
        FirebaseAnalytics.LogEvent(EventSelectContent, new Parameter[] {
            new Parameter("content_type", contentType ?? ""),
            new Parameter("item_id", itemId ?? "")
        });
    }

    public static void LogShopOpen()
    {
        LogEvent("shop_open");
    }

    public static void LogMissionOpen()
    {
        LogEvent("mission_open");
    }

    public static void LogMissionComplete(int missionId, string rewardType)
    {
        LogEvent("mission_complete", "mission_id", missionId, "reward_type", rewardType ?? "");
    }

    public static void LogSettingOpen()
    {
        LogEvent("setting_open");
    }

    public static void LogUserInfoOpen()
    {
        LogEvent("user_info_open");
    }

    #endregion

    #region Push

    public static void LogPushPermissionResult(bool granted)
    {
        LogEvent("push_permission_result", "granted", granted ? 1 : 0);
    }

    public static void LogPushReceived(string messageId, string campaign = null)
    {
        if (!string.IsNullOrEmpty(campaign))
            LogEvent("push_received", "message_id", messageId ?? "", "campaign", campaign);
        else
            LogEvent("push_received", "message_id", messageId ?? "");
    }

    public static void LogPushClick(string messageId, string campaign = null)
    {
        if (!string.IsNullOrEmpty(campaign))
            LogEvent("push_click", "message_id", messageId ?? "", "campaign", campaign);
        else
            LogEvent("push_click", "message_id", messageId ?? "");
    }

    #endregion

    #region Tutorial & Level & Share

    public static void LogTutorialBegin(string tutorialName)
    {
        FirebaseAnalytics.LogEvent(EventTutorialBegin, new Parameter[] { new Parameter(ParamTutorialId, tutorialName ?? "") });
    }

    public static void LogTutorialComplete(string tutorialName)
    {
        FirebaseAnalytics.LogEvent(EventTutorialComplete, new Parameter[] { new Parameter(ParamTutorialId, tutorialName ?? "") });
    }

    public static void LogShare(string contentType, string method)
    {
        LogEvent("share", "content_type", contentType ?? "", "method", method ?? "");
    }

    public static void LogAchievementUnlock(string achievementId)
    {
        LogEvent("achievement_unlock", "achievement_id", achievementId ?? "");
    }

    public static void LogLevelUp(int level, int previousLevel)
    {
        LogEvent("level_up", "level", level, "previous_level", previousLevel);
    }

    public static void LogException(string description, bool fatal = false)
    {
        LogEvent("exception", "description", description ?? "", "fatal", fatal ? 1 : 0);
    }

    #endregion

    #region Screen View

    public static void LogScreenView(string screenName, string screenClass = null)
    {
        if (string.IsNullOrEmpty(screenName)) return;
#if UNITY_EDITOR
        if (LogInEditor)
            Debug.Log($"[GameAnalytics] ScreenView: {screenName}" + (string.IsNullOrEmpty(screenClass) ? "" : $" (class: {screenClass})"));
#endif
        if (!string.IsNullOrEmpty(screenClass))
            FirebaseAnalytics.LogEvent(EventScreenView, new Parameter[] {
                new Parameter(ParamScreenName, screenName),
                new Parameter(ParamScreenClass, screenClass)
            });
        else
            FirebaseAnalytics.LogEvent(EventScreenView, new Parameter[] { new Parameter(ParamScreenName, screenName) });
    }

    #endregion

    #region User Properties

    public static void SetUserProperty(string name, string value)
    {
        if (string.IsNullOrEmpty(name)) return;
#if UNITY_EDITOR
        if (LogInEditor)
            Debug.Log($"[GameAnalytics] UserProperty: {name}={value ?? ""}");
#endif
        FirebaseAnalytics.SetUserProperty(name, value ?? "");
    }

    public static void SetDifficultyPreference(string difficulty)
    {
        SetUserProperty("difficulty_preference", difficulty ?? "");
    }

    public static void SetMaxStageCleared(int maxStage)
    {
        SetUserProperty("max_stage_cleared", maxStage.ToString());
    }

    public static void SetUserLevel(int level)
    {
        SetUserProperty("user_level", level.ToString());
    }

    public static void SetLoginMethod(string method)
    {
        SetUserProperty("login_method", method ?? "anonymous");
    }

    public static void SetTotalPlayCount(int count)
    {
        SetUserProperty("total_play_count", count.ToString());
    }

    public static void SetLastPlayMode(string gameMode)
    {
        SetUserProperty("last_play_mode", gameMode ?? "");
    }

    public static void SetHasRemovedAds(bool value)
    {
        SetUserProperty("has_removed_ads", value ? "true" : "false");
    }

    #endregion

    #region Private

    private static void LogEvent(string name, params object[] keyValuePairs)
    {
        if (string.IsNullOrEmpty(name)) return;
        try
        {
#if UNITY_EDITOR
            if (LogInEditor)
            {
                if (keyValuePairs == null || keyValuePairs.Length == 0)
                    Debug.Log($"[GameAnalytics] Event: {name}");
                else
                {
                    var sb = new System.Text.StringBuilder();
                    for (int i = 0; i < keyValuePairs.Length; i += 2)
                        sb.Append($" {keyValuePairs[i]}={keyValuePairs[i + 1]}");
                    Debug.Log($"[GameAnalytics] Event: {name}{sb}");
                }
            }
#endif
            if (keyValuePairs == null || keyValuePairs.Length == 0)
            {
                FirebaseAnalytics.LogEvent(name);
                return;
            }
            if (keyValuePairs.Length % 2 != 0)
            {
                Debug.LogWarning($"[GameAnalytics] LogEvent {name}: keyValuePairs length must be even.");
                return;
            }
            var parameters = new Parameter[keyValuePairs.Length / 2];
            for (int i = 0; i < parameters.Length; i++)
            {
                string key = keyValuePairs[i * 2]?.ToString() ?? "";
                object val = keyValuePairs[i * 2 + 1];
                if (val is int intVal)
                    parameters[i] = new Parameter(key, intVal);
                else if (val is long longVal)
                    parameters[i] = new Parameter(key, longVal);
                else if (val is double doubleVal)
                    parameters[i] = new Parameter(key, doubleVal);
                else
                    parameters[i] = new Parameter(key, val?.ToString() ?? "");
            }
            FirebaseAnalytics.LogEvent(name, parameters);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[GameAnalytics] LogEvent failed: {name}, {e.Message}");
        }
    }

    #endregion
}

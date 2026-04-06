using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class DailyTimedIconController
{
    private readonly string _keyLastShowTime;
    private readonly Func<bool> _condition;
    private readonly double _showDurationHours;

    public DailyTimedIconController(string id, Func<bool> condition, double showDurationHours)
    {
        _keyLastShowTime = $"DailyIcon_LastShowTime_{id}";
        _condition = condition;
        _showDurationHours = showDurationHours;
    }

    public bool CanBeTriggeredNow()
    {
        if (!_condition()) return false;

        // 오늘 이미 트리거했으면 false
        if (PlayerPrefs.HasKey(_keyLastShowTime))
        {
            var last = GetLastShowTime();
            if (last.Date == Now.Date) return false;
        }
        return true;
    }

    /// ✅ 특정 상황에서 호출: "오늘 1회 아이콘 표시 시작"
    public bool TryMake()
    {
        if (!CanBeTriggeredNow()) return false;

        PlayerPrefs.SetString(_keyLastShowTime, Now.ToString("O"));
        PlayerPrefs.Save();
        return true;
    }

    public bool IsActive()
    {
        if (!PlayerPrefs.HasKey(_keyLastShowTime)) return false;

        var last = GetLastShowTime();
        // 오늘 트리거된 건만 유효
        if (last.Date != Now.Date) return false;

        return Now <= last.AddHours(_showDurationHours);
    }

    public TimeSpan GetRemainingTime()
    {
        if (!IsActive()) return TimeSpan.Zero;

        TimeSpan remain = GetExpireTime() - Now;
        return remain > TimeSpan.Zero ? remain : TimeSpan.Zero;
    }

    public DateTime GetExpireTime()
    {
        return GetLastShowTime().AddHours(_showDurationHours);
    }

    private DateTime GetLastShowTime()
    {
        string raw = PlayerPrefs.GetString(_keyLastShowTime);
        return DateTime.Parse(raw, null, System.Globalization.DateTimeStyles.RoundtripKind);
    }

    private DateTime Now => DateTime.Now; // 서버시간으로 바꾸려면 여기만 교체
}

public static class DailyIconRegistry
{
    private static readonly Dictionary<string, DailyTimedIconController> _map = new();

    public static void Register(string id, DailyTimedIconController controller)
    {
        Debug.Log(id);
        if( _map.ContainsKey(id)) return;
        _map.Add(id, controller);
    }

    public static DailyTimedIconController Get(string id)
    {
        return _map[id];
    }

    /// ✅ 모든 DailyIcon 컨트롤러 + PlayerPrefs 값 삭제
    public static void ClearAll()
    {
        foreach (var id in _map.Keys)
        {
            string key = $"DailyIcon_LastShowTime_{id}";
            if (PlayerPrefs.HasKey(key))
                PlayerPrefs.DeleteKey(key);
        }

        PlayerPrefs.Save();
        _map.Clear();
    }

    public static void Clear(string id)
    {
        // PlayerPrefs 키 삭제
        string key = $"DailyIcon_LastShowTime_{id}";
        if (PlayerPrefs.HasKey(key))
            PlayerPrefs.DeleteKey(key);

        PlayerPrefs.Save();

        // Registry에서도 제거
        _map.Remove(id);
    }
}

public static class DailyIconBootstrap
{
    public static void Init()
    {
        DailyIconRegistry.Register(
            "DailyBonus",
            new DailyTimedIconController(
                id: "DailyBonus",
                condition: () => UserDataManager.Level >= 0,
                showDurationHours: 3
            )
        );

        // 필요하면 더 등록...
    }
}
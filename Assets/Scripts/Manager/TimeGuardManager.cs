using Common.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

namespace Common.Manager
{
    public class TimeGuardManager : MonoSingletonDont<TimeGuardManager>
    {
        // PlayerPrefs 키 prefix
        private const string KEY_LAST_UTC = "TIMEGUARD_LAST_UTC_";
        private const string KEY_LAST_MONO = "TIMEGUARD_LAST_MONO_";

        // 허용 오차(ms): 벽시계 vs 모노타임 차이가 이 이상이면 시간조작 의심
        private const double ALLOW_DELTA_MS = 300000; // 5분

        #region 기본 저장/로드

        /// <summary>
        /// 특정 키에 대해 "지금 시각"을 저장 (UTC + realtimeSinceStartup)
        /// 예: "HintCooldown", "DailyReward" 등
        /// </summary>
        public void SaveNow(string key)
        {
            // UTC 벽시계
            long nowUtcTicks = DateTime.UtcNow.Ticks;
            PlayerPrefs.SetString(KEY_LAST_UTC + key, nowUtcTicks.ToString());

            // 모노토닉 시간
            float mono = Time.realtimeSinceStartup;
            PlayerPrefs.SetFloat(KEY_LAST_MONO + key, mono);

            PlayerPrefs.Save();
        }

        /// <summary>
        /// 해당 키가 이전에 저장된 적 있는지
        /// </summary>
        public bool HasSaved(string key)
        {
            return PlayerPrefs.HasKey(KEY_LAST_UTC + key) &&
                   PlayerPrefs.HasKey(KEY_LAST_MONO + key);
        }

        #endregion

        #region 경과 시간 계산

        /// <summary>
        /// 마지막 SaveNow(key) 이후 지난 시간을 ms 단위로 반환.
        /// suspicious = true 면 시간 조작 의심.
        /// 저장 데이터 없으면 0ms.
        /// </summary>
        public long GetElapsedMs(string key, out bool suspicious)
        {
            suspicious = false;

            string utcKey = KEY_LAST_UTC + key;
            string monoKey = KEY_LAST_MONO + key;

            if (!PlayerPrefs.HasKey(utcKey) || !PlayerPrefs.HasKey(monoKey))
            {
                return 0;
            }

            string savedUtcStr = PlayerPrefs.GetString(utcKey, "0");
            if (!long.TryParse(savedUtcStr, out long savedUtcTicks))
            {
                return 0;
            }

            DateTime savedUtc = new DateTime(savedUtcTicks, DateTimeKind.Utc);
            float savedMono = PlayerPrefs.GetFloat(monoKey, 0f);

            DateTime nowUtc = DateTime.UtcNow;
            float nowMono = Time.realtimeSinceStartup;

            // 벽시계 기준 경과
            TimeSpan diffWall = nowUtc - savedUtc;
            double wallMs = diffWall.TotalMilliseconds;

            // 모노토닉 기준 경과
            double monoMs = (nowMono - savedMono) * 1000.0;

            if (wallMs < 0) wallMs = 0;
            if (monoMs < 0) monoMs = 0;

            // 두 기준의 차이가 너무 크면 시간조작 의심
            double diffBetween = Math.Abs(wallMs - monoMs);
            if (diffBetween > ALLOW_DELTA_MS)
            {
                suspicious = true;
            }

            // 최종 사용하는 경과 시간
            double resultMs;

            if (!suspicious)
            {
                // 정상 → 벽시계 기준 사용 (앱이 꺼져있던 시간도 포함)
                resultMs = wallMs;
            }
            else
            {
                // 조작 의심 → 지나치게 늘어난 시간은 막기 위해
                // 두 값 중 더 작은 쪽만 인정 (혹은 monoMs만 써도 됨)
                resultMs = Math.Min(wallMs, monoMs);
            }

            return (long)resultMs;
        }

        #endregion
    }
}
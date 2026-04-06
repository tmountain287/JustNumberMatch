using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public struct SplitMix64
{
    private ulong state;
    public SplitMix64(ulong seed) { state = seed; }
    public ulong Next()
    {
        ulong z = (state += 0x9E3779B97F4A7C15UL);
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }
    public int NextInt(int maxExclusive) => (int)(Next() % (uint)maxExclusive);
}

public static class LocalUserId
{
    const string PlayerPrefsKey = "LocalUserID.v1"; // 버전 태그 권장

    public static string GetOrCreate()
    {
        //if (PlayerPrefs.HasKey(PlayerPrefsKey))
        //    return PlayerPrefs.GetString(PlayerPrefsKey);

        // 128-bit 난수 생성 (crypto-grade)
        byte[] buf = new byte[16];
        RandomNumberGenerator.Fill(buf);
        string id = BytesToHex(buf); // 예: "A3F1...16바이트*2=32글자"

        //PlayerPrefs.SetString(PlayerPrefsKey, id);
        //PlayerPrefs.Save();
        return id;
    }

    public static string BytesToHex(byte[] bytes)
    {
        char[] c = new char[bytes.Length * 2];
        int b;
        for (int i = 0; i < bytes.Length; i++)
        {
            b = bytes[i] >> 4;
            c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
            b = bytes[i] & 0xF;
            c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
        }
        return new string(c);
    }
}

public static class StableSeed
{
    // userId + salt 를 SHA-256으로 해시 → 상위 8바이트를 ulong 시드로
    public static ulong Seed64(string key, string salt = "LevelOrderSaltV1")
    {
        using var sha = SHA256.Create();
        var h = sha.ComputeHash(Encoding.UTF8.GetBytes($"{salt}:{key}"));
        ulong s = 0;
        for (int i = 0; i < 8; i++) s = (s << 8) | h[i];
        return s;
    }
}

public static class ShuffleUtil
{
    // 제자리 섞기 (원본을 바꿈)
    public static void ShuffleInPlace<T>(IList<T> list, SplitMix64 rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.NextInt(i + 1); // 0..i
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // 복사본을 섞어 반환 (원본 보존)
    public static List<T> ShuffledCopy<T>(IEnumerable<T> src, SplitMix64 rng)
    {
        var list = src.ToList();
        ShuffleInPlace(list, rng);
        return list;
    }
}

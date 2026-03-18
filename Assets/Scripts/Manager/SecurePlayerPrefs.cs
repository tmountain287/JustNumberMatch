using Newtonsoft.Json;
using System;
using System.Text;
using UnityEngine;

public static class SecurePlayerPrefs
{
    private static readonly string secretKey = "dnjsgksmszlfhq123456";

    public static void SetInt(string key, int value)
    {
        SetString(key, value.ToString());
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        string encrypted = PlayerPrefs.GetString(key, null);
        if (string.IsNullOrEmpty(encrypted))
            return defaultValue;

        string decrypted = Decrypt(encrypted);
        if (int.TryParse(decrypted, out int result))
            return result;

        return defaultValue;
    }

    public static void SetFloat(string key, float value)
    {
        SetString(key, value.ToString());
    }

    public static float GetFloat(string key, float defaultValue = 0f)
    {
        string encrypted = PlayerPrefs.GetString(key, null);
        if (string.IsNullOrEmpty(encrypted))
            return defaultValue;

        string decrypted = Decrypt(encrypted);
        if (float.TryParse(decrypted, out float result))
            return result;

        return defaultValue;
    }

    public static void SetString(string key, string value)
    {
        string encrypted = Encrypt(value);
        PlayerPrefs.SetString(key, encrypted);
    }

    public static string GetString(string key, string defaultValue = "")
    {
        string encrypted = PlayerPrefs.GetString(key, null);
        if (string.IsNullOrEmpty(encrypted))
            return defaultValue;

        return Decrypt(encrypted);
    }

    public static string Encrypt(UserData userData)
    {
        string plainText = JsonConvert.SerializeObject(userData);
        byte[] bytes = Encoding.UTF8.GetBytes(plainText);
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] ^= (byte)secretKey[i % secretKey.Length];
        }
        return Convert.ToBase64String(bytes);
    }

    public static string Encrypt(string plainText)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(plainText);
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] ^= (byte)secretKey[i % secretKey.Length];
        }
        return Convert.ToBase64String(bytes);
    }

    public static string Decrypt(string cipherText)
    {
        try
        {
            byte[] bytes = Convert.FromBase64String(cipherText);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] ^= (byte)secretKey[i % secretKey.Length];
            }
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return "";
        }
    }
}

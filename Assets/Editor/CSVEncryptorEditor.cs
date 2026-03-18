
using UnityEditor;
using UnityEngine;
using System.IO;
using System;

public class CSVEncryptorEditor : EditorWindow
{
    private const string rawCSVPath = "Assets/rawRes/TableDatas/";
    private const string outputSOPath = "Assets/Resources/EncryptedCSVs/";
    private const string encryptionKey = "secret_key";

    [MenuItem("Tools/CSV Encryptor/Encrypt All CSVs")]
    public static void EncryptAllCSVs()
    {
        if (!Directory.Exists(rawCSVPath))
        {
            Debug.LogError($"❌ 원본 CSV 폴더가 없습니다: {rawCSVPath}");
            return;
        }

        if (!Directory.Exists(outputSOPath))
        {
            Directory.CreateDirectory(outputSOPath);
        }

        string[] csvFiles = Directory.GetFiles(rawCSVPath, "*.csv");
        if (csvFiles.Length == 0)
        {
            Debug.LogWarning("❗ CSV 파일이 없습니다.");
            return;
        }

        foreach (string filePath in csvFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath).ToLower();
            string plainText = File.ReadAllText(filePath);

            string base64 = XORAndEncrypt(plainText, encryptionKey); // ✅ 바이트 기반 XOR + Base64

            EncryptedCSVData asset = ScriptableObject.CreateInstance<EncryptedCSVData>();
            asset.name = $"Encrypted_{fileName}";
            asset.fileName = fileName;
            asset.encryptedData = base64;

            string assetPath = outputSOPath + asset.name + ".asset";
            AssetDatabase.CreateAsset(asset, assetPath);

            Debug.Log($"✅ 암호화 완료: {fileName}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ 모든 CSV 암호화 완료! 총 파일 수: {csvFiles.Length}");
    }

    private static string XORAndEncrypt(string plainText, string key)
    {
        byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
        byte[] result = new byte[dataBytes.Length];

        for (int i = 0; i < dataBytes.Length; i++)
        {
            result[i] = (byte)(dataBytes[i] ^ keyBytes[i % keyBytes.Length]);
        }

        return Convert.ToBase64String(result);
    }
}
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "EncryptedCSVData", menuName = "ScriptableObjects/EncryptedCSVData")]
public class EncryptedCSVData : ScriptableObject
{
    [Tooltip("파일 이름 또는 식별용 이름 (예: 'level', 'monster')")]
    public string fileName;

    [TextArea(5, 20)]
    public string encryptedData; // Base64 문자열

    [SerializeField]
    private string key = "secret_key";

    public string GetDecryptedText()
    {
        try
        {
            if (string.IsNullOrEmpty(encryptedData))
            {
                Debug.LogError($"❌ 복호화 실패: 빈 데이터 - {fileName}");
                return null;
            }

            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] resultBytes = new byte[encryptedBytes.Length];

            for (int i = 0; i < encryptedBytes.Length; i++)
                resultBytes[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);

            string decrypted = System.Text.Encoding.UTF8.GetString(resultBytes);

            // 추가 디버깅 로그
            //Debug.Log($"✅ 복호화 성공: {fileName}, 줄 수 = {decrypted.Split('\n').Length}");
            return decrypted;
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ 복호화 예외: {fileName}\n{ex}");
            return null;
        }
    }
}

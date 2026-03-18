using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

public static class BackendApi
{
    public static async Task<string> PostJson(string url, string json, Dictionary<string, string>? headers = null, int timeoutSeconds = 15)
    {
        using var req = new UnityWebRequest(url, "POST");
        byte[] body = Encoding.UTF8.GetBytes(json ?? "{}");
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        if (headers != null)
        {
            foreach (var kv in headers)
                req.SetRequestHeader(kv.Key, kv.Value);
        }

        req.timeout = timeoutSeconds;

        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        // 성공
        if (req.result == UnityWebRequest.Result.Success &&
            req.responseCode >= 200 && req.responseCode < 300)
        {
            return req.downloadHandler.text;
        }

        // 실패 → 상태코드, 에러, 본문 같이 전달
        string errorText = req.downloadHandler?.text;
        throw new BackendApiException(
            (int)req.responseCode,
            req.error ?? "Request failed",
            errorText
        );
    }
}

// 커스텀 예외 정의
public class BackendApiException : Exception
{
    public int StatusCode { get; }
    public string? ResponseBody { get; }

    public BackendApiException(int statusCode, string message, string? responseBody)
        : base($"HTTP {statusCode}: {message}\nBody: {Truncate(responseBody, 300)}")
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    private static string Truncate(string? s, int max)
    {
        if (string.IsNullOrEmpty(s)) return s ?? "";
        return s.Length <= max ? s : s.Substring(0, max) + "...";
    }
}
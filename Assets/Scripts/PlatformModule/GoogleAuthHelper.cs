#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Google.Apis.Auth; // ✅ GoogleJsonWebSignature 사용

public class GoogleAuthHelper
{
    public async Task<(string idToken, string email, string name, string picture, string userId)> GetGoogleUserAsync(
        string clientId, string clientSecret, string redirectUri)
    {
        try
        {
            string[] scopes = { "openid", "email", "profile" };
            string scopeParam = string.Join(" ", scopes);

            string authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                             $"response_type=code&client_id={Uri.EscapeDataString(clientId)}" +
                             $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                             $"&scope={Uri.EscapeDataString(scopeParam)}" +
                             $"&access_type=offline&prompt=consent";

            Debug.Log("[Google OAuth] 브라우저 열기: " + authUrl);
            Application.OpenURL(authUrl);

            string code = await WaitForAuthCode(redirectUri);
            if (string.IsNullOrEmpty(code))
            {
                Debug.LogError("Authorization code 수신 실패");
                return default;
            }

            string idToken = await ExchangeAuthCodeForIdToken(code, clientId, clientSecret, redirectUri);
            if (string.IsNullOrEmpty(idToken))
            {
                Debug.LogError("ID Token 획득 실패");
                return default;
            }

            // ✅ ID Token 검증 및 정보 추출
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
            string email = payload.Email;
            string name = payload.Name;
            string picture = payload.Picture;
            string userId = payload.Subject; // 고유 ID (sub)

            Debug.Log($"✅ 로그인 성공: {email}, {name}, {userId}");
            return (idToken, email, name, picture, userId);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Google OAuth] 오류: {ex.Message}");
            return default;
        }
    }

    private async Task<string> WaitForAuthCode(string redirectUri)
    {
        string authCode = null;

        using (var http = new HttpListener())
        {
            http.Prefixes.Add(redirectUri.EndsWith("/") ? redirectUri : redirectUri + "/");
            http.Start();

            Debug.Log($"[Google OAuth] 로컬 서버 대기 중: {redirectUri}");

            var context = await http.GetContextAsync();

            var query = context.Request.Url.Query;
            var queryParams = ParseQuery(query);
            authCode = queryParams.TryGetValue("code", out string code) ? code : null;

            string html = "<html><head><meta charset='utf-8'></head><body>✅ 로그인 성공! 창을 닫으세요.</body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(html);
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();

            http.Stop();
        }

        return authCode;
    }

    private async Task<string> ExchangeAuthCodeForIdToken(string authCode, string clientId, string clientSecret, string redirectUri)
    {
        using (var client = new HttpClient())
        {
            var parameters = new Dictionary<string, string>
            {
                { "code", authCode },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" }
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync("https://oauth2.googleapis.com/token", content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Debug.LogError($"[Google OAuth] 토큰 요청 실패: {json}");
                return null;
            }

            var jObj = JObject.Parse(json);
            return jObj["id_token"]?.ToString();
        }
    }

    private Dictionary<string, string> ParseQuery(string query)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        if (query.StartsWith("?")) query = query.Substring(1);

        foreach (var part in query.Split('&'))
        {
            var kv = part.Split('=');
            if (kv.Length == 2)
                dict[Uri.UnescapeDataString(kv[0])] = Uri.UnescapeDataString(kv[1]);
        }

        return dict;
    }
}
#endif

using backend_cli.BackEnd;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

public class NetErrorCode
{
    public static readonly string None = "";
    public static readonly string HttpRequestException = "HttpRequestException";
    public static readonly string ECONNABORTED = "ECONNABORTED";
    public static readonly string NotFoundException = "NotFoundException";
    public static readonly string Unknown = "NotFoundException";
}

public static class WebHelper
{
    public static async UniTask<(BackendReturnObject, T)> GetAsync<T>(string url, string queryString, string jwtToken = "", int _timeOut = 10, CancellationToken cancellationToken = default)
    {
        string fullUrl = string.IsNullOrEmpty(queryString) ? url : $"{url}?{queryString}";
        return await SendRequestAsync<T>(UnityWebRequest.kHttpVerbGET, fullUrl, null, jwtToken, _timeOut, cancellationToken);
    }

    public static async UniTask<(BackendReturnObject, T)> PostAsync<T>(string url, object body, string jwtToken, int _timeOut = 10, CancellationToken cancellationToken = default)
    {
        string json = ObjectToJson(body);
        return await SendRequestAsync<T>(UnityWebRequest.kHttpVerbPOST, url, json, jwtToken, _timeOut, cancellationToken);
    }

    public static async UniTask<(BackendReturnObject, T)> DeleteAsync<T>(string url, string jwtToken, int _timeOut = 10, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<T>(UnityWebRequest.kHttpVerbDELETE, url, null, jwtToken, _timeOut, cancellationToken);
    }

    private static async UniTask<(BackendReturnObject, T)> SendRequestAsync<T>(
    string method, string url, string jsonBody, string jwtToken,
    int _timeOut = 10, CancellationToken cancellationToken = default)
    {
        using (UnityWebRequest request = new(url, method))
        {
            request.timeout = _timeOut;
            request.downloadHandler = new DownloadHandlerBuffer();

            if (method == UnityWebRequest.kHttpVerbPOST && !string.IsNullOrEmpty(jsonBody))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
                Debug.Log($"[WebHelper] POST {url} Body JSON: {jsonBody}");
            }
            else
            {
                Debug.Log($"[WebHelper] GET {url} Body JSON: {jsonBody}");
            }

            if (!string.IsNullOrEmpty(jwtToken))
            {
                request.SetRequestHeader("Authorization", "Bearer " + jwtToken);
            }

            BackendReturnObject bro;
            T returnValue = default;

            try
            {
                await request.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);

#if UNITY_2020_1_OR_NEWER
                bool isError = request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError;
#else
            bool isError = request.isNetworkError || request.isHttpError;
#endif 

                if (isError)
                {
                    if (request.error != null && request.error.ToLower().Contains("timeout"))
                    {
                        bro = new BackendReturnObject(408, NetErrorCode.ECONNABORTED, "timeout error", null);
                    }
                    else
                    {
                        bro = new BackendReturnObject((int)request.responseCode, NetErrorCode.HttpRequestException, request.error, null);
                    }
                }
                else
                {
                    try
                    {
                        Debug.Log(request.downloadHandler.text);
                        bro = JsonToObject<BackendReturnObject>(request.downloadHandler.text);
                        returnValue = JsonToObject<T>(bro.returnValue);
                    }
                    catch (Exception ex)
                    {
                        bro = new BackendReturnObject(500, NetErrorCode.Unknown, ex.Message, null);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("[WebHelper] Request canceled by CancellationToken");
                bro = new BackendReturnObject(-1, NetErrorCode.ECONNABORTED, "Request Canceled", null);
            }
            catch (Exception ex)
            {
                Debug.Log($"[WebHelper] Unexpected error: {ex}");
                bro = new BackendReturnObject(500, NetErrorCode.Unknown, ex.Message, null);
            }

            return (bro, returnValue);
        }
    }


    public static T JsonToObject<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }

    public static string ObjectToJson(object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }
}

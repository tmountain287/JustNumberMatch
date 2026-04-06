using Common.Manager;
using Common.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Purchasing;

public enum ServerType
{
    Local = 0,
    Service = 1,
}

[Serializable]
public class SeverUrlInfo
{
    public ServerType severType;
    public string serverUrl;
}

[Serializable]
public class VersionInfo
{
    public string android_latest;
    public string android_min;
    public string ios_latest;
    public string ios_min;
    public bool force_update;
    public bool maintenance;
}

[Serializable]
public sealed class VerifyGoogleIdTokenResult
{
    [JsonProperty("ok")] public bool Ok { get; set; }

    [JsonProperty("googleUser")] public GoogleUser? GoogleUser { get; set; }

    // 있으면 Firebase 커스텀 토큰 로그인에 사용 가능 (없어도 성공일 수 있음)
    [JsonProperty("firebaseCustomToken")] public string? FirebaseCustomToken { get; set; }

    // 실패 시 서버가 내려주는 필드들
    [JsonProperty("error")] public string? Error { get; set; }

    [JsonProperty("details")] public string? Details { get; set; }
}

[Serializable]
public sealed class GoogleUser
{
    [JsonProperty("uid")] public string? Uid { get; set; }
    [JsonProperty("email")] public string? Email { get; set; }
    [JsonProperty("name")] public string? Name { get; set; }
    [JsonProperty("picture")] public string? Picture { get; set; }
}

[Serializable]
public sealed class VerifyAppleIdTokenResult
{
    [JsonProperty("ok")] public bool Ok { get; set; }
    [JsonProperty("appleUser")] public AppleUser? AppleUser { get; set; }
    [JsonProperty("firebaseCustomToken")] public string? FirebaseCustomToken { get; set; }
    [JsonProperty("error")] public string? Error { get; set; }
    [JsonProperty("details")] public string? Details { get; set; }
}

[Serializable]
public sealed class AppleUser
{
    [JsonProperty("uid")] public string? Uid { get; set; }
    [JsonProperty("email")] public string? Email { get; set; }
    [JsonProperty("name")] public string? Name { get; set; }
    [JsonProperty("picture")] public string? Picture { get; set; }
}

public class NetworkManager : MonoSingleton<NetworkManager>
{
    [SerializeField] private List<SeverUrlInfo> serverUrlInfoList = null;
    [SerializeField] private ServerType serverType = ServerType.Local;

    private string serverUrl;

    public void SetSeverUri()
    {
        SeverUrlInfo severUrlInfo = serverUrlInfoList.Where(x => x.severType == serverType).FirstOrDefault();

        if (severUrlInfo != null)
        {
            serverUrl = severUrlInfo.serverUrl;
        }
    }

    private void Awake()
    {
        SetSeverUri();
    }

    private void OnValidate()
    {
        SetSeverUri();
    }

    public void CheckVersion(
        Action<VersionInfo> onSuccess,
        Action<string> onFail)
    {
        StartCoroutine(CoCheckVersion(onSuccess, onFail));
    }

    private const string versionUrl = "https://justonematch-d2cfe.web.app/version.json";

    private int timeoutSeconds = 5;    // 요청 타임아웃 시간
    private int retryCount = 1;        // 재시도 횟수

    private IEnumerator CoCheckVersion(
        Action<VersionInfo> onSuccess,
        Action<string> onFail)
    {
        int retries = retryCount;

        while (retries-- > 0)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(versionUrl))
            {
                www.timeout = timeoutSeconds;

                yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                bool isSuccess = (www.result == UnityWebRequest.Result.Success);
#else
                bool isSuccess = !www.isNetworkError && !www.isHttpError;
#endif

                if (isSuccess)
                {
                    string json = www.downloadHandler.text?.TrimStart('\uFEFF'); // BOM 제거
                    Debug.Log($"[VersionChecker] 성공: {json}");

                    try
                    {
                        VersionInfo info = JsonConvert.DeserializeObject<VersionInfo>(json);
                        onSuccess?.Invoke(info);
                    }
                    catch (Exception e)
                    {
                        onFail?.Invoke("JSON Parse Error: " + e.Message);
                    }
                    yield break;
                }
                else
                {
                    Debug.LogWarning($"[VersionChecker] 실패: {www.error}, 남은 재시도 {retries}");
                }
            }

            // 재시도 간 딜레이
            yield return new WaitForSeconds(1f);
        }

        // 모든 재시도 실패
        onFail?.Invoke("버전 체크 실패 (모든 재시도 실패)");
    }


    public async Task<SaveResult> SaveUserDataAsync(
    bool checkDevice = true,            // ✅ 추가
    bool forceConflict = false,
    bool useTimeout = false,
    int timeoutMs = 1000
)
    {
        var saveTask = FirestoreDiag.Instance.TrySaveAsync(
            FirebaseUserData.UID,
            SystemInfo.deviceUniqueIdentifier,
            SecurePlayerPrefs.Encrypt(UserDataManager.UserData),
            checkDevice,              // ✅ 추가
            forceConflict
        );

        Task completedTask;

        if (useTimeout)
        {
            var timeoutTask = Task.Delay(timeoutMs);
            completedTask = await Task.WhenAny(saveTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                Debug.LogWarning($"[SaveUserDataAsync] Timeout ({timeoutMs}ms 경과, 응답 없음)");
                return null;
            }
        }
        else
        {
            completedTask = saveTask;
        }

        var result = await saveTask;
        return result;
    }

    public async Task<bool> ApplyOverwriteAsync(string _newRecord)
    {
        var result = await FirestoreDiag.Instance.ApplyOverwriteAsync(FirebaseUserData.UID, SystemInfo.deviceUniqueIdentifier, _newRecord);
        UserDataManager.SetForceUserData(JsonConvert.DeserializeObject<UserData>(SecurePlayerPrefs.Decrypt(_newRecord)));
        return result;
    }

    public async Task<(bool success, VerifyGoogleIdTokenResult result, string error)> LinkAccountAsync()
    {        
        try
        {
            string token = PlatformLoginReceiver.Instance.Token ?? "";
            string res = await BackendApi.PostJson(
                $"{serverUrl}verifyGoogleIdToken",
                $"{{\"idToken\":\"{EscapeJson(token)}\"}}"
            );

            var parsed = JsonConvert.DeserializeObject<VerifyGoogleIdTokenResult>(res);
            if (parsed?.Ok == true)
                return (true, parsed, "Linked successfully");

            return (false, null, parsed?.Error ?? "Unknown server error");
        }
        catch (BackendApiException apiEx)
        {
            Debug.Log($"[LinkAccount] Backend error {apiEx.StatusCode}: {apiEx.Message}");
            return (false, null, $"Server error {apiEx.StatusCode}: {apiEx.ResponseBody}");
        }
        catch (Exception ex)
        {
            Debug.Log($"[LinkAccount] Exception: {ex}");
            return (false, null, $"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>Apple idToken 서버 검증 (Cloud Function verifyAppleIdToken)</summary>
    public async Task<(bool success, VerifyAppleIdTokenResult result, string error)> VerifyAppleIdTokenAsync(string idToken)
    {
        if (string.IsNullOrEmpty(idToken))
            return (false, null, "Missing idToken");
        try
        {
            string jsonBody = $"{{\"idToken\":\"{EscapeJson(idToken)}\"}}";
            string res = await BackendApi.PostJson($"{serverUrl}verifyAppleIdToken", jsonBody);
            var parsed = JsonConvert.DeserializeObject<VerifyAppleIdTokenResult>(res);
            if (parsed?.Ok == true)
                return (true, parsed, null);
            return (false, parsed, parsed?.Error ?? "Invalid token");
        }
        catch (BackendApiException apiEx)
        {
            Debug.Log($"[VerifyAppleIdToken] Backend error {apiEx.StatusCode}: {apiEx.Message}");
            return (false, null, $"Server error {apiEx.StatusCode}");
        }
        catch (Exception ex)
        {
            Debug.Log($"[VerifyAppleIdToken] Exception: {ex}");
            return (false, null, ex.Message);
        }
    }

    private static string EscapeJson(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }

    /// <summary>Google Play 결제 검증 (Cloud Function verifyPurchase)</summary>
    public async Task<(bool ok, string msg)> VerifyPurchase(string myAccountId, string itemId, string purchaseToken)
    {
        string payload =
            $"{{\"packageName\":\"{Application.identifier}\"," +
            $"\"productId\":\"{itemId}\"," +
            $"\"purchaseToken\":\"{purchaseToken}\"," +
            $"\"accountId\":\"{myAccountId}\"," +
            $"\"acknowledge\":true}}";

        string res = await BackendApi.PostJson($"{serverUrl}verifyPurchase", payload);

        JObject json;
        try { json = JObject.Parse(res); }
        catch { return (false, "server_response_parse_fail"); }

        if (json["error"] != null)
        {
            string errorMsg = json["error"]?.ToString() ?? "Unknown error";
            string details = json["details"]?.ToString();
            if (!string.IsNullOrEmpty(details)) errorMsg += $" ({details})";
            Debug.LogError($"❌ 서버 에러: {errorMsg}");
            return (false, errorMsg);
        }

        bool ok =
            (json["ok"]?.Type == JTokenType.Boolean && json["ok"]!.Value<bool>()) ||
            string.Equals(json["status"]?.ToString(), "ok", StringComparison.OrdinalIgnoreCase);

        if (!ok)
            return (false, res);

        var purchase = (JObject)json["purchase"];
        if (purchase == null)
            return (false, "purchase_missing");

        int purchaseState = purchase["purchaseState"]?.Value<int>() ?? -1;
        if (purchaseState != 0)
            return (false, $"purchaseState_not_purchased:{purchaseState}");

        return (true, res);
    }

    /// <summary>Apple App Store 결제 검증 (Cloud Function verifyApplePurchase). receiptData는 base64 영수증.</summary>
    public async Task<(bool ok, string errorMsg)> VerifyApplePurchase(string myAccountId, string productId, string receiptDataBase64)
    {
        if (string.IsNullOrEmpty(receiptDataBase64))
            return (false, "Missing receiptData");
        try
        {
            string payload = $"{{\"receiptData\":\"{EscapeJson(receiptDataBase64)}\"," +
                             $"\"productId\":\"{EscapeJson(productId ?? "")}\"," +
                             $"\"accountId\":\"{EscapeJson(myAccountId ?? "")}\"}}";
            string res = await BackendApi.PostJson($"{serverUrl}verifyApplePurchase", payload);
            JObject json;
            try { json = JObject.Parse(res); }
            catch { return (false, "server_response_parse_fail"); }
            if (json["error"] != null)
                return (false, json["error"]?.ToString() ?? "Unknown error");
            bool ok = json["ok"]?.Value<bool>() == true || string.Equals(json["status"]?.ToString(), "ok", StringComparison.OrdinalIgnoreCase);
            return (ok, ok ? null : (json["error"]?.ToString() ?? res));
        }
        catch (BackendApiException apiEx)
        {
            Debug.LogError($"[VerifyApplePurchase] Backend error: {apiEx.StatusCode} {apiEx.Message}");
            return (false, $"Server error {apiEx.StatusCode}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[VerifyApplePurchase] Exception: {ex}");
            return (false, ex.Message);
        }
    }

    public async UniTask RecoveryPendingItem()
    {
        // 네트워크 없으면 복구 자체 의미 없음
        if (Application.internetReachability == NetworkReachability.NotReachable)
            return;

        // IAP 준비될 때까지 대기
        float timeout = 5f;
        float elapsed = 0f;
        while (!Common.Manager.IAPManager.Instance.InitComplete && elapsed < timeout)
        {
            await UniTask.Delay(100);
            elapsed += 0.1f;
        }
        if (!Common.Manager.IAPManager.Instance.InitComplete)
            return;

        PendingOrder pending;

        while ((pending = Common.Manager.IAPManager.Instance.GetPendingOrder()) != null)
        {
            // 1) pending에서 receipt 확보
            string receipt = pending.Info?.Receipt ?? string.Empty;
            if (string.IsNullOrEmpty(receipt))
            {
                Debug.LogWarning("[IAP] pending receipt empty - skip");
                continue;
            }

            string store = "unknown";
            try { store = JObject.Parse(receipt)?["Store"]?.ToString() ?? "unknown"; } catch { }

            // 2) iOS: Apple 결제 검증
            if (store == "AppleAppStore")
            {
                if (!TryExtractAppleReceiptInfo(receipt, out var receiptDataBase64, out var receiptProductIdApple))
                {
                    Debug.LogWarning("[IAP] Apple receipt parse fail - keep pending");
                    continue;
                }
                string productIdApple = !string.IsNullOrEmpty(receiptProductIdApple) ? receiptProductIdApple : GetProductIdFromOrder(pending);
                if (string.IsNullOrEmpty(productIdApple) || string.IsNullOrEmpty(receiptDataBase64))
                {
                    Debug.LogWarning("[IAP] Apple productId/receiptData empty - keep pending");
                    continue;
                }
                Debug.Log($"[IAP] 복구 처리(Apple): {productIdApple}");
                var (okApple, rawApple) = await NetworkManager.Instance.VerifyApplePurchase(FirebaseUserData.UID, productIdApple, receiptDataBase64);
                if (okApple)
                {
                    GrantRewardByProductId(productIdApple);
                    UserDataManager.Save(false);
                    Common.Manager.IAPManager.Instance.ConfirmPending(pending);
                    Debug.Log($"[IAP] 복구 완료/Confirm(Apple): {productIdApple}");
                }
                else
                    Debug.LogWarning($"[IAP] 복구 검증 실패(keep pending): {productIdApple} / {rawApple}");
                continue;
            }

            // 3) Android: receipt에서 productId / token 추출
            if (!TryExtractGooglePlayInfo(receipt, out var receiptProductId, out var purchaseToken, out var orderId, out var storeOut))
            {
                Debug.LogWarning($"[IAP] receipt parse fail - keep pending");
                continue;
            }

            // productId는 “pending에서 얻은 id”가 더 정확할 때가 있어서 우선순위 정해도 됨
            string productId = !string.IsNullOrEmpty(receiptProductId)
                ? receiptProductId
                : GetProductIdFromOrder(pending);

            if (string.IsNullOrEmpty(productId) || string.IsNullOrEmpty(purchaseToken))
            {
                Debug.LogWarning("[IAP] productId/token empty - keep pending");
                continue;
            }

            Debug.Log($"[IAP] 복구 처리: {productId} / orderId={orderId} / store={storeOut}");

            // 3) 서버 검증 (여기서 서버가 idempotent 처리해야 안전)
            var (ok, raw) = await NetworkManager.Instance.VerifyPurchase(FirebaseUserData.UID, productId, purchaseToken);

            if (ok)
            {
                // 4) ✅ 보상 지급 (서버가 “이미 지급됨”이어도 ok=true로 내려줄 수 있음)
                //    - 가장 안전한 건 “지급 자체도 서버에서 처리하고, 클라는 결과만 반영”인데
                //      지금 구조 유지라면 최소한 서버가 중복 지급 방지해줘야 함.
                GrantRewardByProductId(productId);

                UserDataManager.Save(false);

                // 5) ✅ Confirm (이제 pending 정리)
                Common.Manager.IAPManager.Instance.ConfirmPending(pending);

                Debug.Log($"[IAP] 복구 완료/Confirm: {productId}");
            }
            else
            {
                // 실패 시 Confirm하지 않음 -> 다음 실행 때 다시 시도 가능
                Debug.LogWarning($"[IAP] 복구 검증 실패(keep pending): {productId} / {raw}");
            }
        }
    }

    private static bool TryExtractAppleReceiptInfo(string receipt, out string receiptDataBase64, out string productId)
    {
        receiptDataBase64 = "";
        productId = "";
        try
        {
            var wrapper = JObject.Parse(receipt);
            if (wrapper["Store"]?.ToString() != "AppleAppStore") return false;
            receiptDataBase64 = wrapper["Payload"]?.ToString() ?? "";
            if (string.IsNullOrEmpty(receiptDataBase64)) return false;
            productId = wrapper["TransactionID"]?.ToString() ?? "";
            return true;
        }
        catch { return false; }
    }

    private void GrantRewardByProductId(string productId)
    {
        var data = TableDataManager.Instance.TableProductCatalogData.GetProductCatalogData(productId);
        if (data == null)
        {
            Debug.LogWarning($"[IAP] product catalog not found: {productId}");
            return;
        }

        if(data.isPremium)
        {
            UserDataManager.IsAdsFree = true;
            UserDataManager.AddItemCount(data.itemType, data.value);
        }
    }

    private bool TryExtractGooglePlayInfo(string receipt, out string productId, out string token, out string orderId, out string store)
    {
        productId = token = orderId = store = "";

        try
        {
            // receipt wrapper
            var wrapper = JObject.Parse(receipt);
            store = wrapper["Store"]?.ToString() ?? "";

            // fake는 복구 대상 아님(에디터 테스트)
            if (store == "fake")
                return false;

            var payloadRaw = wrapper["Payload"]?.ToString();
            if (string.IsNullOrEmpty(payloadRaw)) return false;

            var payload = JObject.Parse(payloadRaw);
            var jsonStr = payload["json"]?.ToString();
            if (string.IsNullOrEmpty(jsonStr)) return false;

            var purchase = JObject.Parse(jsonStr);

            productId = purchase["productId"]?.ToString() ?? "";
            token = purchase["purchaseToken"]?.ToString() ?? "";
            orderId = purchase["orderId"]?.ToString() ?? ""; // 가끔 없을 수 있음

            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GetProductIdFromOrder(Order order)
    {
        var item = order?.CartOrdered?.Items()?.FirstOrDefault();
        return item?.Product?.definition?.id ?? string.Empty;
    }
}
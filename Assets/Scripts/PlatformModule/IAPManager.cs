using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Common.Manager
{
    public class IAPManager : MonoSingletonDont<IAPManager>
    {
        private StoreController storeController; // Unity IAP v5

        private bool initCalled = false;

        private bool isUGSInitialized = false;
        private bool isUGSInitializing = false;

        [SerializeField] private float retryInterval = 3.0f;

        private bool isConnected = false;
        private bool productsFetched = false;

        // productId -> callback
        private readonly Dictionary<string, Action<bool, string>> callbacks = new();

        // 중복 검증 방지 키(트랜잭션/영수증 해시)
        private readonly HashSet<string> verifyingKeys = new();

        // 복구/재시도용 PendingOrder 큐
        private readonly Stack<PendingOrder> pendingOrders = new();
        private readonly HashSet<string> queuedKeys = new();

        /// <summary>
        /// v5에서 "실사용 가능" 기준:
        /// StoreController 생성 + Connect 완료 + ProductsFetched(1회 이상)
        /// </summary>
        public bool InitComplete => storeController != null && isConnected && productsFetched;

        public StoreController StoreController { get => storeController; }
        private Dictionary<string, string> priceCache = new();
        #region Public API

        public bool IsKorea
        {
            get
            {
                if (PlayerPrefs.HasKey("Korea"))
                    return PlayerPrefs.GetInt("Korea") == 1;
                else
                {
                    SystemLanguage currentLanguage = Application.systemLanguage;
                    return currentLanguage == SystemLanguage.Korean;
                }
            }
        }

        public void Initialize()
        {
            if (initCalled) return;
            initCalled = true;

            if (!isUGSInitialized)
                StartCoroutine(CheckAndInitializeUGS());
        }

        public void BuyProduct(string productId, Action<bool, string> cb)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                string message = LocalizationManager.Instance.GetText("OfflineRecovery");
                cb ?.Invoke(false, message);
                return;
            }


            if (!InitComplete)
            {
                cb?.Invoke(false, "iap_not_ready");
                return;
            }

            if (string.IsNullOrEmpty(productId))
            {
                cb?.Invoke(false, "invalid_product_id");
                return;
            }

            // v5: product는 StoreController 내부 캐시에서 조회
            var product = storeController.GetProductById(productId);
            if (product == null)
            {
                cb?.Invoke(false, "product_not_found");
                return;
            }

            callbacks[productId] = cb;

            string priceStr = priceCache.TryGetValue(productId, out var p) ? p : null;
            GameAnalyticsHelper.LogPurchaseBegin(productId, priceStr);
            Debug.Log($"[IAP] PurchaseProduct: {productId}");
            storeController.PurchaseProduct(product);
        }

        /// <summary>
        /// v5: 복구/미처리 구매 다시 조회
        /// 결과는 OnPurchasesFetched에서 PendingOrder로 들어옴
        /// </summary>
        public void CheckForRestoredPurchases()
        {
            if (storeController == null)
                return;

            if (!isConnected || !productsFetched)
            {
                Debug.LogWarning("[IAP] CheckForRestoredPurchases ignored: not ready");
                return;
            }

            Debug.Log("[IAP] FetchPurchases() for restore check");
            storeController.FetchPurchases();
        }

        /// <summary>
        /// 복구/재시도에서 PendingOrder 하나 꺼내기
        /// </summary>
        public PendingOrder GetPendingOrder()
        {
            if (pendingOrders.Count > 0)
                return pendingOrders.Pop();
            return null;
        }

        /// <summary>
        /// 복구/재시도에서 Confirm
        /// </summary>
        public void ConfirmPending(PendingOrder pending)
        {
            if (pending == null || storeController == null) return;
            storeController.ConfirmPurchase(pending);
        }

        #endregion

        #region UGS Init

        private IEnumerator CheckAndInitializeUGS()
        {
            while (!isUGSInitialized)
            {
                if (Application.internetReachability != NetworkReachability.NotReachable && !isUGSInitializing)
                {
                    isUGSInitializing = true;

                    var initTask = UnityServices.InitializeAsync();
                    while (!initTask.IsCompleted)
                        yield return null;

                    if (initTask.Exception != null)
                    {
                        Debug.LogError("[UGS] 초기화 실패: " + initTask.Exception);
                        isUGSInitializing = false;
                    }
                    else
                    {
                        Debug.Log("[UGS] 초기화 완료");
                        isUGSInitialized = true;
                        isUGSInitializing = false;

                        InitializePurchasing();
                        yield break;
                    }
                }

                yield return new WaitForSeconds(retryInterval);
            }
        }

        #endregion

        #region IAP v5 Init

        public async void InitializePurchasing()
        {
            try
            {
                storeController = UnityIAPServices.StoreController();

                // 이벤트 연결(먼저 연결)
                storeController.OnStoreDisconnected += OnStoreDisconnected;
                storeController.OnPurchasePending += OnPurchasePending;
                storeController.OnPurchaseFailed += OnPurchaseFailed;
                storeController.OnProductsFetched += OnProductsFetched;
                storeController.OnPurchasesFetched += OnPurchasesFetched;
                storeController.OnProductsFetchFailed += OnProductsFetchFailed;
                storeController.OnPurchasesFetched += OnPurchasesFetched;
                storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;

                await storeController.Connect();
                isConnected = true;

                // 상품 정의 생성
                var defs = new List<ProductDefinition>();
                foreach (var p in TableDataManager.Instance.TableProductCatalogData.ProductCatalogDataList)
                    defs.Add(new ProductDefinition(p.id, p.productType));

                Debug.Log($"[IAP] FetchProducts: {defs.Count}");
                storeController.FetchProducts(defs);
            }
            catch (Exception e)
            {
                Debug.LogError($"[IAP] InitializePurchasing failed:\n{e}");
                isConnected = false;
                productsFetched = false;
            }
        }

        private void OnStoreDisconnected(StoreConnectionFailureDescription storeConnectionFailureDescription)
        {
            Debug.LogWarning("[IAP] Store disconnected");
            // 필요하면 재시도/재연결 로직
        }

        private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription purchasesFetchFailureDescription)
        {
            Debug.LogWarning("[IAP] Store FetchFailed");
            // 필요하면 재시도/재연결 로직
        }

        private void OnProductsFetchFailed(ProductFetchFailed productFetchFailed)
        {
            Debug.LogWarning("[IAP] Store productFetchFailed");
            // 필요하면 재시도/재연결 로직
        }

        private void OnProductsFetched(List<Product> products)
        {
            productsFetched = true;
            Debug.Log($"[IAP] ProductsFetched: {products?.Count ?? 0}");

            // ✅ 가격 캐시 채우기
            priceCache.Clear();
            if (products != null)
            {
                foreach (var p in products)
                {
                    var id = p?.definition?.id;
                    if (string.IsNullOrEmpty(id)) continue;

                    var priceStr = p.metadata?.localizedPriceString ?? "";
                    priceCache[id] = priceStr;
                }
            }

            bool isKRW = IsKRWFromProducts(products);

            if (isKRW)
            {
                Debug.Log("[Region] KRW detected → Korea Store");
                PlayerPrefs.SetInt("Korea", 1);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.Log("[Region] Non-KRW → Global Store");
                PlayerPrefs.SetInt("Korea", 0);
                PlayerPrefs.Save();
            }

            // 복구/미처리 구매 조회
            storeController.FetchPurchases();
        }

        private bool IsKRWFromProducts(List<Product> products)
        {
            if (products == null) return false;

            foreach (var p in products)
            {
                var price = p.metadata?.localizedPriceString;
                if (string.IsNullOrEmpty(price)) continue;

                // 한국 스토어면 거의 100% ₩ 포함
                if (price.Contains("₩"))
                    return true;
            }

            return false;
        }

        public bool TryGetLocalizedPriceString(string productId, out string price)
        {
            price = "";
            if (string.IsNullOrEmpty(productId)) return false;

            return priceCache.TryGetValue(productId, out price) && !string.IsNullOrEmpty(price);
        }

        private void OnPurchasesFetched(Orders orders)
        {
            try
            {
                var allOrders = GetAllOrdersSafe(orders);
                Debug.Log($"[IAP] PurchasesFetched: {allOrders.Count}");

                foreach (var o in allOrders)
                {
                    if (o is PendingOrder pending)
                    {
                        var productId = GetProductIdFromOrder(pending);
                        Debug.Log($"[IAP] 복구된(Pending) 주문 발견: {productId}");
                        EnqueuePending(pending); // ✅ v4의 hasReceipt/pending push 역할
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[IAP] OnPurchasesFetched parse failed:\n{e}");
            }
        }

        #endregion

        #region Purchase Handlers

        private void OnPurchaseFailed(FailedOrder failed)
        {
            var productId = GetProductIdFromOrder(failed);
            string reasonStr = failed != null ? failed.FailureReason.ToString() : "unknown";
            GameAnalyticsHelper.LogPurchaseFail(productId, reasonStr);
            Debug.LogWarning($"[IAP] PurchaseFailed: {productId} reason={failed.FailureReason}");

            if (callbacks.TryGetValue(productId, out var cb))
            {
                cb(false, reasonStr);
                callbacks.Remove(productId);
            }
        }

        private void OnPurchasePending(PendingOrder pending)
        {
            // 복구/재시도용 큐잉 (원치 않으면 제거 가능)
            EnqueuePending(pending);

            var productId = GetProductIdFromOrder(pending);

            // 중복 검증 방지 키: TransactionID 우선, 없으면 receipt hash
            var key = MakeVerifyKey(pending);
            if (!string.IsNullOrEmpty(key) && verifyingKeys.Contains(key))
                return;

            if (!string.IsNullOrEmpty(key))
                verifyingKeys.Add(key);

            _ = VerifyAndConfirmAsync(pending, productId, key);
        }

        #endregion

        #region Verify

        private async System.Threading.Tasks.Task VerifyAndConfirmAsync(PendingOrder pending, string productId, string verifyKey)
        {
            try
            {
                // 0) receipt 확보
                string receipt = pending?.Info?.Receipt ?? "";
                if (string.IsNullOrEmpty(receipt))
                {
                    InvokeCb(productId, false, "receipt_empty");
                    return;
                }

                // 1) fake 처리(에디터 테스트 등)
                string store = "unknown";
                try { store = JObject.Parse(receipt)?["Store"]?.ToString() ?? "unknown"; } catch { }

                if (store == "fake")
                {
                    Debug.Log($"[IAP] Fake receipt -> confirm: {productId}");
                    storeController.ConfirmPurchase(pending);
                    InvokeCb(productId, true, productId);
                    return;
                }

                // 2) Apple App Store: Payload가 base64 영수증
                if (store == "AppleAppStore")
                {
                    var appleWrapper = JObject.Parse(receipt);
                    string receiptDataBase64 = appleWrapper["Payload"]?.ToString() ?? "";
                    if (string.IsNullOrEmpty(receiptDataBase64))
                    {
                        InvokeCb(productId, false, "payload_empty");
                        return;
                    }
                    var (isSuccessApple, errorApple) = await NetworkManager.Instance.VerifyApplePurchase(FirebaseUserData.UID, productId, receiptDataBase64);
                    if (isSuccessApple)
                    {
                        Debug.Log($"[IAP] Apple Verify OK -> confirm: {productId}");
                        storeController.ConfirmPurchase(pending);
                        InvokeCb(productId, true, productId);
                    }
                    else
                    {
                        Debug.LogWarning($"[IAP] Apple Verify FAIL (keep pending): {productId} / {errorApple}");
                        InvokeCb(productId, false, string.IsNullOrEmpty(errorApple) ? "verify_failed" : errorApple);
                    }
                    return;
                }

                // 3) Google Play: receipt 파싱 -> Payload(JSON) 파싱
                JObject receiptWrapper;
                try
                {
                    receiptWrapper = JObject.Parse(receipt);
                }
                catch
                {
                    InvokeCb(productId, false, "receipt_parse_fail");
                    return;
                }

                string payloadRaw = receiptWrapper["Payload"]?.ToString();
                if (string.IsNullOrEmpty(payloadRaw))
                {
                    InvokeCb(productId, false, "payload_empty");
                    return;
                }

                // 3) Payload (GooglePlay JSON 구조) 파싱 -> json 필드
                JObject payloadObj;
                try
                {
                    payloadObj = JObject.Parse(payloadRaw);
                }
                catch
                {
                    InvokeCb(productId, false, "payload_parse_fail");
                    return;
                }

                string jsonStr = payloadObj["json"]?.ToString();
                if (string.IsNullOrEmpty(jsonStr))
                {
                    InvokeCb(productId, false, "payload_json_empty");
                    return;
                }

                // 4) purchaseToken 추출
                JObject purchaseData;
                try
                {
                    purchaseData = JObject.Parse(jsonStr);
                }
                catch
                {
                    InvokeCb(productId, false, "purchaseData_parse_fail");
                    return;
                }

                string purchaseToken = purchaseData["purchaseToken"]?.ToString();
                if (string.IsNullOrEmpty(purchaseToken))
                {
                    InvokeCb(productId, false, "purchaseToken_empty");
                    return;
                }

                // (선택) receipt에 들어있는 productId가 기대값과 다른 경우 방어
                string receiptProductId = purchaseData["productId"]?.ToString();
                if (!string.IsNullOrEmpty(receiptProductId) && receiptProductId != productId)
                {
                    Debug.LogWarning($"[IAP] productId mismatch: arg={productId}, receipt={receiptProductId}");
                    // 여기서 return 할지, 경고만 할지는 정책에 맞게 선택
                    // InvokeCb(productId, false, "productId_mismatch"); return;
                }

                // 5) 서버 검증 (기존 로직 유지)
                var (isSuccess, error) = await NetworkManager.Instance.VerifyPurchase(FirebaseUserData.UID, productId, purchaseToken);

                if (isSuccess)
                {
                    Debug.Log($"[IAP] Verify OK -> confirm: {productId}");
                    storeController.ConfirmPurchase(pending);
                    InvokeCb(productId, true, productId);
                }
                else
                {
                    // 실패 시 Confirm 안 함(= pending 유지)
                    Debug.LogWarning($"[IAP] Verify FAIL (keep pending): {productId} / {error}");
                    InvokeCb(productId, false, string.IsNullOrEmpty(error) ? "verify_failed" : error);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[IAP] Verify exception: {productId}\n{e}");
                InvokeCb(productId, false, "verify_exception");
            }
            finally
            {
                if (!string.IsNullOrEmpty(verifyKey))
                    verifyingKeys.Remove(verifyKey);
            }
        }


        private void InvokeCb(string productId, bool ok, string msg)
        {
            if (string.IsNullOrEmpty(productId))
                return;

            if (ok)
            {
                var product = storeController?.GetProductById(productId);
                string currency = product?.metadata?.isoCurrencyCode ?? "KRW";
                decimal priceDecimal = product?.metadata?.localizedPrice ?? 0m;
                double value = (double)priceDecimal;
                GameAnalyticsHelper.LogPurchase(currency, value, productId);
            }

            if (callbacks.TryGetValue(productId, out var cb))
            {
                cb(ok, msg);
                callbacks.Remove(productId);
            }
        }

        #endregion

        #region Helpers

        private void EnqueuePending(PendingOrder pending)
        {
            if (pending == null) return;

            var key = MakeQueueKey(pending);
            if (string.IsNullOrEmpty(key)) return;

            if (queuedKeys.Contains(key))
                return;

            queuedKeys.Add(key);
            pendingOrders.Push(pending);

            var productId = GetProductIdFromOrder(pending);
            Debug.Log($"[IAP] Pending queued: {productId} / key={key}");
        }

        private static string GetProductIdFromOrder(Order order)
        {
            var item = order?.CartOrdered?.Items()?.FirstOrDefault();
            return item?.Product?.definition?.id ?? string.Empty;
        }

        private static string MakeVerifyKey(PendingOrder pending)
        {
            var tx = pending?.Info?.TransactionID;
            if (!string.IsNullOrEmpty(tx))
                return "tx:" + tx;

            var receipt = pending?.Info?.Receipt ?? "";
            if (!string.IsNullOrEmpty(receipt))
                return "rcpt:" + Sha1(receipt);

            return string.Empty;
        }

        private static string MakeQueueKey(PendingOrder pending)
        {
            return MakeVerifyKey(pending);
        }

        private static string Sha1(string s)
        {
            using var sha = SHA1.Create();
            var bytes = Encoding.UTF8.GetBytes(s);
            var hash = sha.ComputeHash(bytes);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private static List<Order> GetAllOrdersSafe(Orders orders)
        {
            var list = new List<Order>();
            if (orders == null) return list;

            // Orders가 IEnumerable<Order>면 그대로
            if (orders is IEnumerable<Order> enumerable)
            {
                list.AddRange(enumerable);
                return list;
            }

            // orders.All 같은 컬렉션 프로퍼티가 있는 경우 방어적으로 접근
            try
            {
                var prop = orders.GetType().GetProperty("All");
                if (prop != null)
                {
                    var val = prop.GetValue(orders);
                    if (val is IEnumerable<Order> e2)
                        list.AddRange(e2);
                }
            }
            catch { }

            return list;
        }

        #endregion
    }
}

using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Common.Manager
{ 
    public class IAPManager : MonoSingletonDont<IAPManager>, IDetailedStoreListener
    {
        private IStoreController storeController;
        private IExtensionProvider storeExtensionProvider;

        private bool init = false;

        private bool isUGSInitialized = false;
        private bool isUGSInitializing = false;

        private float retryInterval = 3.0f;

        private Stack<Product> pendingProducts = new();

        public int PendingProductCount { get=>pendingProducts.Count; }


        public Product GetPendingProduct()
        {
            if (pendingProducts.Count > 0)
                return pendingProducts.Pop();

            return null;
        }

        public void Initialize()
        {
            if (init) return;

            if (!isUGSInitialized)
            {
                StartCoroutine(CheckAndInitializeUGS());
            }

            init = true;
        }

        private IEnumerator CheckAndInitializeUGS()
        {
            while (!isUGSInitialized)
            {
                if (Application.internetReachability != NetworkReachability.NotReachable && !isUGSInitializing)
                {
                    isUGSInitializing = true;

                    var initTask = UnityServices.InitializeAsync();

                    // await를 못 쓰므로, 유니티식으로 대기
                    while (!initTask.IsCompleted)
                        yield return null;

                    if (initTask.Exception != null)
                    {
                        Debug.LogError("[UGS] 초기화 실패: " + initTask.Exception.Message);
                        isUGSInitializing = false;
                    }
                    else
                    {
                        Debug.Log("[UGS] 초기화 완료됨");
                        isUGSInitialized = true;

                        // ✅ 여기서 다음 단계 호출 (예: IAP 초기화)
                        InitializePurchasing();
                        yield break;
                    }
                }

                yield return new WaitForSeconds(retryInterval);
            }
        }

        private Action<bool, string> onPurchaseCallback = null;
        private string pendingProductId = null;

        public void InitializePurchasing()
        {
            if (InitComplete)
                return;

#if UNITY_IOS
            var module = StandardPurchasingModule.Instance(AppStore.AppleAppStore);
#elif UNITY_ANDROID
            var module = StandardPurchasingModule.Instance(AppStore.GooglePlay);
#else
            var module = StandardPurchasingModule.Instance(AppStore.NotSpecified);
#endif

            var builder = ConfigurationBuilder.Instance(module);

            foreach (var p in TableDataManager.Instance.TableProductCatalogData.ProductCatalogDataList)
            {
                var ids = new IDs();

#if UNITY_ANDROID
                ids.Add(p.id, GooglePlay.Name);
#endif
#if UNITY_IOS
                ids.Add(p.id, AppleAppStore.Name);
#endif
                // 공용 키(p.id)로 접근하고, 각 스토어는 storeSpecificId로 조회
                builder.AddProduct(p.id, p.productType, ids);
            }
            UnityPurchasing.Initialize(this, builder);
        }

        public bool InitComplete
        {
            get =>
            storeController != null && storeExtensionProvider != null;
        }

        public void BuyProduct(string productId, Action<bool, string> onResult)
        {
            if (!InitComplete)
            {
                onResult?.Invoke(false, "IAP 초기화 오류");
                return;
            }

            var product = storeController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                pendingProductId = productId;
                onPurchaseCallback = onResult;
                storeController.InitiatePurchase(product);
            }
            else
            {
                onResult?.Invoke(false, "IAP 알 수 없는 오류");
            }
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            storeController = controller;
            storeExtensionProvider = extensions;
            Debug.Log("[IAP] initComplete");

            //foreach (var product in controller.products.all)
            //{
            //    Debug.Log($"상품 ID: {product.definition.id}");
            //    Debug.Log($"지역 통화 가격: {product.metadata.localizedPriceString}");
            //    Debug.Log($"통화 코드: {product.metadata.isoCurrencyCode}");
            //    Debug.Log($"소수형 가격: {product.metadata.localizedPrice}");
            //}

            //StartCoroutine(InitCompleteCoroutine());

            CheckForRestoredPurchases();
        }

        public void CheckForRestoredPurchases()
        {
            if (storeController == null || storeController.products == null)
                return;

            foreach (var product in storeController.products.all)
            {
                if (product.hasReceipt && !product.definition.type.Equals(ProductType.Subscription))
                {
                    if (!pendingProducts.Contains(product)) // 중복 방지
                    {
                        Debug.Log($"[IAP] 복구된 상품 발견: {product.definition.id}");
                        pendingProducts.Push(product);
                    }
                }
            }
        }

        private IEnumerator InitCompleteCoroutine()
        {
            yield return new WaitForSeconds(0.1f); //펜딩 아이템을 모으기위해 딜레이를
            Debug.Log("[IAP] initComplete");
            
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.Log($"[IAP] InitFailed: {error} | {message}");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            
        }


        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            Debug.Log($"[IAP] 구매 성공(Pending): {args.purchasedProduct.definition.id}");

            if (args.purchasedProduct.definition.id == pendingProductId)
            {
                // 서버 검증 등 비동기 처리 필요 → 완료되면 ConfirmPendingPurchase 호출
                ServerVerify(args.purchasedProduct);
            }
            else
            {
                
            }

            return PurchaseProcessingResult.Pending;
        }


        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.LogWarning($"[IAP] 구매 실패: {product.definition.id}, 이유: {failureDescription.reason}, 메시지: {failureDescription.message}");

            onPurchaseCallback?.Invoke(false, failureDescription.reason.ToString());

            pendingProductId = null;
            onPurchaseCallback = null;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.LogWarning($"[IAP] 구매 실패: {product.definition.id}, {failureReason}");
            onPurchaseCallback?.Invoke(false, failureReason.ToString());

            pendingProductId = null;
            onPurchaseCallback = null;
        }
        

        private async void ServerVerify(Product product)
        {
            JObject root = JObject.Parse(product.receipt);
            string store = root["Store"]?.ToString();

            if(store == "fake")
            {
                storeController.ConfirmPendingPurchase(product);
                onPurchaseCallback?.Invoke(true, product.definition.id);

                pendingProductId = null;
                onPurchaseCallback = null;
            }
            else
            {
                var (bro, result) = await NetworkManager.Instance.InAppVerify(product.receipt);

                if (bro.IsSuccess())
                {
                    storeController.ConfirmPendingPurchase(product);
                    onPurchaseCallback?.Invoke(true, product.definition.id);

                    pendingProductId = null;
                    onPurchaseCallback = null;
                }
                else
                {
                    onPurchaseCallback?.Invoke(false, bro.message);
                }
            }
        }  
        
        public void ConfirmPendingPurchase(Product product)
        {
            storeController.ConfirmPendingPurchase(product);
        }
    }
}
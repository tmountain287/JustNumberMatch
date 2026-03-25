using backend_cli.BackEnd;
using Cysharp.Threading.Tasks;
using Gostop.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Common.Manager
{
    public enum ServerType
    {
        Debug = -1,
        Local = 0,
        Service = 1,
    }

    [Serializable]
    public class SeverUrlInfo
    {
        public ServerType severType;
        public string backend_url;
        public string service_url;

        public SeverUrlInfo(ServerType severType, string backend_url, string service_url)
        {
            this.severType = severType;
            this.backend_url = backend_url;
            this.service_url = service_url;
        }
                
    }

    public class NetworkManager : MonoSingleton<NetworkManager>
    {
        [SerializeField] private string backend_url = "";
        [SerializeField] private string service_url = "";
        
        [SerializeField] private ServerType serverType = ServerType.Debug;

        private string JwtToken 
        { 
            get
            {
                return PlayerPrefs.GetString("jwtToken", "");
                
            }
            set
            {
                PlayerPrefs.SetString("jwtToken", value);
                PlayerPrefs.Save();
            }
        }

        private bool isServerCheck = false;

        public string US_KEY { get; set; } = string.Empty;

        public bool IsLogined { get => !string.IsNullOrEmpty(US_KEY); }
        
        public string GetLastSegment(string packageOrBundleId)
        {
            if (string.IsNullOrWhiteSpace(packageOrBundleId)) return string.Empty;
            int lastDot = packageOrBundleId.LastIndexOf('.');
            return (lastDot >= 0 && lastDot + 1 < packageOrBundleId.Length)
                ? packageOrBundleId.Substring(lastDot + 1)
                : packageOrBundleId;
        }

        public void SetSeverUri()
        {
            string appName = GetLastSegment(Application.identifier);

            List<SeverUrlInfo> serverUrlInfoList = new List<SeverUrlInfo>()
            {
                {new(ServerType.Debug, $"http://localhost:7000/{appName}_backend", $"http://localhost:7000/{appName}_service") },
                {new(ServerType.Local, $"https://smallgame.co.kr:7000/{appName}_backend", $"https://smallgame.co.kr:7000/{appName}_service") },
                {new(ServerType.Service, $"https://kingdomapi.kr/{appName}_backend", $"https://kingdomapi.kr/{appName}_service") },
            };

            SeverUrlInfo severUrlInfo = serverUrlInfoList.Where(x => x.severType == serverType).FirstOrDefault();

            if (severUrlInfo != null)
            {
                backend_url = severUrlInfo.backend_url;
                service_url = severUrlInfo.service_url;
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

        public static string GetDeviceName()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return SystemInfo.deviceModel; // 예: "samsung SM-G991N"
#elif UNITY_IOS && !UNITY_EDITOR
            return UnityEngine.iOS.Device.generation.ToString(); // 예: "iPhone13Pro"
#else
            return SystemInfo.deviceName + " - " + SystemInfo.deviceModel;
#endif
        }

        public string GetRegionName()
        {
            string tz = TimeZoneInfo.Local.Id;

            return tz switch
            {
                "Korea Standard Time" => "Seoul",
                "Tokyo Standard Time" => "Tokyo",
                "China Standard Time" => "Beijing",
                "Pacific Standard Time" => "LosAngeles",
                "Eastern Standard Time" => "NewYork",
                _ => tz // fallback to raw timezone ID
            };
        }

        public string GetCountryName()
        {
            RegionInfo region = new RegionInfo(CultureInfo.CurrentCulture.Name);
            return region.EnglishName; // "South Korea", "United States"
        }      

        public async UniTask<(BackendReturnObject, VersionInfo)> CheckAppVersion(int _timeOut = 10)
        {
            string protocol = "/version";

            string queryString = $"app_id={Uri.EscapeDataString(Application.identifier)}&platform={Uri.EscapeDataString(AppDefine.Platform)}";
            var (bro, result) = await WebHelper.GetAsync<VersionInfo>($"{service_url}{protocol}", queryString, "", _timeOut);

            if (bro.IsSuccess() && result != null)
            {
                PlayerPrefs.SetString("Version", result.maxVersion);
                PlayerPrefs.Save();
            }
            return (bro, result);
        }

        public async UniTask<(BackendReturnObject, ServiceInfo)> CheckServer(int _timeOut = 10)
        {
            string protocol = "/status";
      
            string queryString = $"app_id={Uri.EscapeDataString(Application.identifier)}&platform={Uri.EscapeDataString(AppDefine.Platform)}";
            var (bro, result) = await WebHelper.GetAsync<ServiceInfo>($"{service_url}{protocol}", queryString, "", _timeOut);

            if (bro.IsSuccess() && result != null)
            {
                isServerCheck = result.status == "check";
            }
                
            return (bro, result);
        }

        public async UniTask<(BackendReturnObject, List<AppPopUpInfo>)> CheckNotice(int _timeOut = 10)
        {
            string protocol = "/popups";

            string queryString = $"app_id={Uri.EscapeDataString(Application.identifier)}&platform={Uri.EscapeDataString(AppDefine.Platform)}";
            var (bro, result) = await WebHelper.GetAsync<List<AppPopUpInfo>>($"{service_url}{protocol}", queryString, "", _timeOut);

            return (bro, result);
        }

        public async UniTask<(BackendReturnObject, LogInResponse)> Login(string _uuid, int _timeOut = 10)
        {
            string protocol = "/member/log_in";

            LogInRequest request = new();
            request.app_id = Application.identifier;
            request.uuid = _uuid;
            request.us_id = PlayerPrefs.GetString("us_id", "");
            request.federationType = FederationType.GOOGLE;
            request.region = GetRegionName();
            request.language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            request.os = RuntimeInformation.OSDescription;
            request.device = GetDeviceName();
            request.country = GetCountryName();
            request.platform = AppDefine.Platform;

            var (bro, result) = await WebHelper.PostAsync<LogInResponse>($"{backend_url}{protocol}", request, JwtToken, _timeOut);

            if (bro.IsSuccess() && result != null)
            {
                JwtToken = result.access_token;
                US_KEY = result.us_key;
                
                PlayerPrefs.SetString("us_id", result.us_id);
                PlayerPrefs.Save();

                if (UserDataManager.UserData != null)
                {
                    SendPushToken();
                }
            }

            return (bro, result);
        }

        public async UniTask<(BackendReturnObject, GameRecord)> SaveRecord(int _timeOut = 10)
        {
            string protocol = "/record/game_record";

            SaveGameRecordRequest request = new SaveGameRecordRequest();
            request.gameRecord = SecurePlayerPrefs.Encrypt(JsonUtility.ToJson(UserDataManager.UserData));

            var (bro, result) = await WebHelper.PostAsync<GameRecord>($"{backend_url}{protocol}", request, JwtToken, _timeOut);
            return (bro, result);
        }

        public async UniTask<(BackendReturnObject, GameRecord)> SaveRecordMerge(UserData _userData)
        {
            string protocol = "/record/game_record/merge";

            SaveGameRecordRequest request = new SaveGameRecordRequest();
            request.gameRecord = SecurePlayerPrefs.Encrypt(JsonUtility.ToJson(_userData));

            var (bro, result) = await WebHelper.PostAsync<GameRecord>($"{backend_url}{protocol}", request, JwtToken);
            
            UserDataManager.SetForceUserData(_userData);
            return (bro, result);
        }

        public async UniTask<(BackendReturnObject, LogInResponse)> LinkAccount(string _idToken, string _federationType)
        {
            string protocol = "/member/change_federation";

            ChangeFederationRequest request = new();
            request.token = _idToken;
            request.federationType = _federationType;// FederationType.GOOGLE;
            request.region = GetRegionName();
            request.language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            request.os = RuntimeInformation.OSDescription;
            request.device = GetDeviceName();
            request.country = GetCountryName();

            var (bro, result) = await WebHelper.PostAsync<LogInResponse>($"{backend_url}{protocol}", request, JwtToken);
            if (bro.IsSuccess() && result != null)
            {
                JwtToken = result.access_token;
                PlayerPrefs.SetString("us_id", result.us_id);
                PlayerPrefs.Save();
            }
            return (bro, result);
        }

        public async UniTask<(BackendReturnObject, string)> PushAgree(string _pushToken, bool _isAgree)
        {
            string protocol = "/api/push_agree";

            PushAgreeRequest request = new();
            request.pushToken = _pushToken;
            request.pushAgree = _isAgree;

            var (bro, result) = await WebHelper.PostAsync<string>($"{backend_url}{protocol}", request, JwtToken);
            
            return (bro, result);
        }

        public async UniTask<(BackendReturnObject, InappProduct)> InAppVerify(string _receipt, int _timeOut = 10)
        {
            string protocol = "/iap/verify";

            IpaVerifyRequest request = new();
            request.receipt = _receipt;

            var (bro, result) = await WebHelper.PostAsync<InappProduct>($"{backend_url}{protocol}", request, JwtToken, _timeOut);

            return (bro, result);
        }

        public async UniTask<(BackendReturnObject, string)> LimitPurchase(string _productId)
        {
            string protocol = "/iap/monthly_total";
            string queryString = $"productId={_productId}";
            var (bro, result) = await WebHelper.GetAsync<string>($"{backend_url}{protocol}", queryString, JwtToken);

            return (bro, result);
        }

        public async UniTask<(BackendReturnObject, string)> AdReward(string _adType, string _rewardType, int _rewardValue = 0)
        {
#if UNITY_EDITOR
            return (null, null);
#endif
            string protocol = "/ad/reward";

            AdRewardRequest request = new AdRewardRequest();
            request.adType = _adType;
            request.rewardType = _rewardType;
            request.rewardAmount = _rewardValue;

            var (bro, result) = await WebHelper.PostAsync<string>($"{backend_url}{protocol}", request, JwtToken);

            return (bro, result);
        }
        public async UniTask<(BackendReturnObject, string)> SendItemLog(string _rewardType, int _rewardValue = 0)
        {
#if UNITY_EDITOR
            return (null, null);
#endif
            string protocol = "/api/gold_ticket_usage_log";

            GoldTicketUsageLogRequest request = new GoldTicketUsageLogRequest();
            request.usageType = _rewardType;
            request.amount = _rewardValue;

            var (bro, result) = await WebHelper.PostAsync<string>($"{backend_url}{protocol}", request, JwtToken);

            return (bro, result);
        }

        public async UniTask<(BackendReturnObject, string)> AccountDelete()
        {
            string protocol = "/member";

            var (bro, result) = await WebHelper.DeleteAsync<string>($"{backend_url}{protocol}", JwtToken);

            return (bro, result);
        }

        public async UniTask<(BackendReturnObject, string)> UpdtateNickName(string _nickName)
        {
            string protocol = "/api/update_nickname";
            UpdateNicknameRequest request = new UpdateNicknameRequest();
            request.nickName = _nickName;
            var (bro, result) = await WebHelper.PostAsync<string>($"{backend_url}{protocol}", request, JwtToken);

            return (bro, result);
        }



        public void SendPushToken()
        {
            StartCoroutine(WaitForPushToken());
        }

        public IEnumerator WaitForPushToken()
        {
            while (string.IsNullOrEmpty(FirebasePushReceiver.Instance.PushToken))
            {
                yield return null;
            }

            _ = PushAgree(FirebasePushReceiver.Instance.PushToken, UserDataManager.IsPush);
        }

        public void ClearData()
        {
            PlayerPrefs.DeleteKey("us_id");
            PlayerPrefs.DeleteKey("jwtToken");
            US_KEY = "";
        }

        public async void BuyProductAsync(string _item, Action<bool, string> _action)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                _action?.Invoke(false, "온라인 상태에서만\n상품 구매가 가능합니다.\n네트워크 연결 후\n다시 시도해 주세요.");
                return;
            }

            async void BuyProduct()
            {
                var(bro, result) = await LimitPurchase(_item);

                if (bro.IsSuccess())
                {
                    IAPManager.Instance.BuyProduct(_item, (success, result) => _action?.Invoke(success, result));
                }
                else
                {
                    string message = "";
                    if(bro.errorCode == "ExceedPurchaseLimit")
                    {
                        message = "한달 구매 한도를 넘어서\n구매할수 없습니다.";
                    }
                    else
                    {
                        message = $"지금은 구매 할 수 없습니다.({bro.statusCode})\n잠시 후 다시 시도해주세요.";
                    }
                    _action?.Invoke(false, message);
                }
            }

            if(IsLogined)
            {
                UserDataManager.Save(true, ()=> BuyProduct());
            }
            else
            {
                var (bro, result) = await Login(SystemInfo.deviceUniqueIdentifier);
                if (bro.IsSuccess() && result != null)
                {
                    if (result.record != null && result.record.last_us_key != US_KEY && result.record.record != SecurePlayerPrefs.Encrypt(JsonUtility.ToJson(UserDataManager.UserData)))
                    {
                        //데이터 충돌
                        Debug.Log("데이터 충돌");
                        UIManager.Instance.HideLoading();
                        UserData beforeData = JsonUtility.FromJson<UserData>(SecurePlayerPrefs.Decrypt(result.record.record));
                        PopupManager.Instance.OpenPopup<SelectUserPopup>().Initialize(beforeData, UserDataManager.UserData, async (isCurrent) =>
                        {
                            UIManager.Instance.ShowLoading();
                            await HandleUserChoiceAsync();
                            UIManager.Instance.HideLoading();
                            if (!isCurrent)
                            {
                                PopupManager.Instance.AllClosePopup(PopupType.NONE);
                                InGameManager.Instance.RestartGameReady();
                                InGameManager.Instance.GameStart();
                            }
                        });
                    }
                    else
                    {
                        await HandleUserChoiceAsync();
                        BuyProduct();
                    }
                }
                else
                {
                    _action?.Invoke(false, $"지금은 구매 할 수 없습니다.({bro.statusCode})\n잠시 후 다시 시도해주세요.");
                }
            }
        }

        async UniTask HandleUserChoiceAsync()
        {
            await RecoveryPendingItem();
            await SaveRecord(1);
        }

        public async UniTask RecoveryPendingItem()
        {
            float timeout = 2f;
            float elapsed = 0f;
            float interval = 0.1f;

            while (!IAPManager.Instance.InitComplete && elapsed < timeout)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(interval));
                elapsed += interval;
            }

            if (IAPManager.Instance.InitComplete)
            {
                Product product;
                while ((product = IAPManager.Instance.GetPendingProduct()) != null)
                {
                    Debug.Log($"[IAP] 복구 처리 중: {product.definition.id}");                    
                    var (bro, result) = await InAppVerify(product.receipt);

                    if (bro.IsSuccess())
                    {
                        ProductCatalogData data = TableDataManager.Instance.TableProductCatalogData.GetProductCatalogData(product.definition.id);

                        if (data.isPremium)
                        {
                            UserDataManager.BuyPremium();
                        }

                        if (data.gold > 0)
                        {
                            UserDataManager.AddGold(data.gold);
                            _ = SendItemLog("GoldInApp", data.gold);
                        }
                        if(data.fireTicket> 0)
                        {
                            UserDataManager.AddFireTicket(data.fireTicket);
                            _ = NetworkManager.Instance.SendItemLog("FireTicketInApp", data.fireTicket);

                        }
                        if (data.peeSteal > 0)
                        {
                            UserDataManager.PeeStealCount = UserDataManager.PeeStealCount + data.peeSteal;
                            _ = NetworkManager.Instance.SendItemLog("PeeStealTicketInApp", data.peeSteal);

                        }
                        UserDataManager.Save(false);
                        IAPManager.Instance.ConfirmPendingPurchase(product);
                    }
                }
            }
        }

        public async UniTask RecoveryPendingItem(Action<string> _onResult)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                _onResult?.Invoke("온라인 상태에서만\n구매복구가 가능합니다.\n네트워크 연결 후\n다시 진행해 주세요.");
                return;
            }

            if (IAPManager.Instance.InitComplete)
            {
                IAPManager.Instance.CheckForRestoredPurchases();

                if (IAPManager.Instance.PendingProductCount > 0)
                {
                    Product product;
                    while ((product = IAPManager.Instance.GetPendingProduct()) != null)
                    {
                        Debug.Log($"[IAP] 복구 처리 중: {product.definition.id}");
                        var (bro, result) = await InAppVerify(product.receipt);

                        if (bro.IsSuccess())
                        {
                            ProductCatalogData data = TableDataManager.Instance.TableProductCatalogData.GetProductCatalogData(product.definition.id);

                            if (data.isPremium)
                            {
                                UserDataManager.BuyPremium();
                            }

                            if (data.gold > 0)
                            {
                                UserDataManager.AddGold(data.gold);
                                _ = NetworkManager.Instance.SendItemLog("GoldInApp", data.gold);
                            }
                            if (data.fireTicket > 0)
                            {
                                UserDataManager.AddFireTicket(data.fireTicket);
                                _ = NetworkManager.Instance.SendItemLog("FireTicketInApp", data.fireTicket);
                            }
                            if (data.peeSteal > 0)
                            {
                                UserDataManager.PeeStealCount = UserDataManager.PeeStealCount + data.peeSteal;
                                _ = NetworkManager.Instance.SendItemLog("PeeStealTicketInApp", data.peeSteal);

                            }
                            UserDataManager.Save(false);
                            IAPManager.Instance.ConfirmPendingPurchase(product);
                        }
                    }

                    _onResult?.Invoke("구매 복구완료");
                }
                else
                {
                    _onResult?.Invoke("복구할 상품이 없습니다.");
                }
            }
            else
            {
                _onResult?.Invoke("상품 목록 초기화에\n실패했습니다.\n잠시 후 다시 진행해 주세요.");
            }
        }
    }
}
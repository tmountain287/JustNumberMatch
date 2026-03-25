using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace backend_cli.BackEnd
{
    [Serializable]
    public class GameRecord
    {
        public string us_id;
        public string record;
        public string last_us_key;// 최신에 업데이트한 us_key
        public string last_date;// 최신 업데이트 날짜
    }

    [Serializable]
    public class LogInResponse
    {
        public string access_token;
        public string refresh_token;
        public string uuid;
        [JsonProperty("record")]
        private string _recordJson; // 원본 문자열 저장

        [JsonIgnore]
        public GameRecord record;   // 파싱된 객체

        public string us_id;
        public string us_key;
        public string us_nick;
        public string game_coin;
        public string push_agree;

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            if (!string.IsNullOrEmpty(_recordJson))
            {
                try
                {
                    record = JsonConvert.DeserializeObject<GameRecord>(_recordJson);
                }
                catch (Exception)
                {
                    //Debug.LogError($"[LogInResponse] Failed to parse record: {_recordJson}\n{ex.Message}");
                    record = null;
                }
            }
        }
    }

    [Serializable]
    public class VersionInfo
    {
        public string minVersion;
        public string maxVersion;
    }

    [Serializable]
    public class ServiceInfo
    {
        public string status;
        public string noticeUrl;
    }

    [Serializable]
    public class TotalPurchaseInfo
    {
        public int total;
    }

    [Serializable]
    public class InappProduct
    {
        public string name; // 상품 이름
        public string productId; // 상품 ID
        public int price; // 가격
        public string currency; // 통화 단위 (예: "KRW", "USD")
        public string description; // 상품 설명
                                   //public UserItem userItem; // 구매한 사용자 아이템 정보(보류)
    }

    [Serializable]
    public class AppPopUpInfo
    {
        public string appId;
        public int popupId;
        public string title;
        public int popupVer;         // 팝업 버전
        public string imageUrl;
        public string linkUrl;
        public string alive;         // 'Y' or 'N', 사용 유무
    }

    [Serializable]
    public class ShowNoticeInfo
    {
        public string today;
        public int id;
        public int poupVer;

        public ShowNoticeInfo(string today, int id, int poupVer)
        {
            this.today = today;
            this.id = id;
            this.poupVer = poupVer;
        }
    }

    [Serializable]
    public class ShowNoticeInfoList
    {
        public List<ShowNoticeInfo> showNoticeInfos = new();
    }
}
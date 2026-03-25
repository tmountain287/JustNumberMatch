

using System;

namespace backend_cli.BackEnd
{
    public class LogInRequest
    {
        public LogInRequest()
        {

        }
        public LogInRequest(string app_id, string uuid, string us_id)
        {
            this.app_id = app_id;
            this.uuid = uuid;
            this.us_id = us_id;
        }

        public string app_id;// 앱 id
        public string uuid;// 기기 uuid
        public string us_id;// 사용자 아이디
        public string federationType;// 페더레이션 타입
        public string region;// 지역
        public string language;// 언어
        public string os;// 운영체제
        public string device;// 디바이스
        public string country;// 국가
        public string platform;
    }

    // 페더레이션 인증 요청
    public class AuthorizeFederationRequest

    {
        public string token { get; set; }
        public string federationType { get; set; }
        public string region { get; set; }
        public string language { get; set; }
        public string os { get; set; }
        public string device { get; set; }
        public string country { get; set; }
    }

    // 페더레이션 변경 요청
    public class ChangeFederationRequest
    {
        public string token { get; set; }// Google : authCode, GPGS : idToken, Apple : identityToken 
        public string federationType { get; set; }
        public string region { get; set; }
        public string language { get; set; }
        public string os { get; set; }
        public string device { get; set; }
        public string country { get; set; }
    }

    public static class IpaPlatform
    {
        public static int Google = 0;// 구글 인앱 결제
        public static int Apple = 1;// 애플 인앱 결제
        public static int OneStore = 2;// 원스토어 인앱 결제
    }

    public class IpaVerifyRequest
    {
        public string platform { get; set; }// 게임 로그 타입(google : 0, apple : 1)        
        public string receipt { get; set; } // 영수증 토큰 ex) dipjcnhompnmpbkokmjjdfam.AO-XXXwKJo931-qI0crOddpS1hKm2-vOfXGS2cxJfRsiEepNF4lfjqYTU6deUs6_hAhofJdBD9Az_CbfKm6xQJ5hAhofJdVNXH7zGzxfWt0gd2y3_pehztA

    }

    // 게임 정보 입력 요청
    public class SaveGameRecordRequest
    {
        public string gameRecord { get; set; }
    }

    public class AddGameLogRequest
    {
        public string logType { get; set; }//게임 로그 타입
        public string gameLog { get; set; }// 게임 로그
    }

    public class PushAgreeRequest
    {
        public string pushToken;
        public bool pushAgree;
    }

    public class AdRewardRequest
    {
        public string adType;
        public string rewardType;
        public int rewardAmount;
    }

    public class GoldTicketUsageLogRequest
    {
        public string usageType;
        public int amount;
    }

    public class UpdateNicknameRequest
    {
        public string nickName;
    }
}
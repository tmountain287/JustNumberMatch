using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backend_cli.BackEnd
{
    /*
     * 200 번대 : 성공
     * 300 번대 : 리다이렉션
     * 400 번대 : 클라이언트 에러(클라이언트에서의 요청에 에러가 있다.)
     * 500 번대 : 서버 에러(클라이언트의 요청은 유효한데 서버가 처리에 실패하였다
     */
    public class NetErrorCode
    {
        // 200
        public static String Success = "Success";

        // 400
        public static String accessTokenError = "accessTokenError";// 기기 로컬에 액세스 토큰이 존재하지 않는데 토큰 로그인 시도를 한 경우
        public static String BadParameterException = "BadParameterException";// 입력한 파라미터에 오류가 있음
        public static String HttpRequestException = "HttpRequestException";// 네트워크의 상태가 일시적으로 불안정하여 호출/응답에 실패할 경우
        public static String InitializeFail = "InitializeFail";// Client App ID 혹은 Signature Key가 null 혹은string.Empty인 경우
        public static String InvalidParameterValue = "InvalidParameterValue"; // 유효하지 않은 파라미터
        public static String UndefinedParameterException = "UndefinedParameterException";// 정의되지 않은 파라미터
        public static String ValidationException = "ValidationException";// 입력 값이 데이터 필드의 예상 데이터 형식, 범위 또는 패턴과 일치하지 않는 경우 유효성 검사 예외
        public static String GoogleOAuthException = "GoogleOAuthException";// 구글 OAuth(오스) 에러
        public static String InvalidRequest = "InvalidRequest";// 400 ~ 500 사이의 http error

        // 401
        public static String BadUnauthorizedException = "BadUnauthorizedException";// 인증이 필요한 API 엔드포인트에 인증 없이 접근하려는 경우

        // 402
        public static String AbnormalReceipt = "AbnormalReceipt";// 비정상적 영수증

        // 403
        public static String Forbidden = "Forbidden";// 금지된 상황

        // 404
        public static String NotFoundException = "NotFoundException";// 찾을 수 없는 에러를 나타냄

        // 405
        public static String MethodNotAllowedParameterException = "MethodNotAllowedParameterException";// 이용할 수 없는 파라미터 값으로 인하여 method가 허용되지 않는 경우

        // 408
        public static String ECONNABORTED = "ECONNABORTED";// 서버에서 타임아웃 오류 발생(최대 20초)

        // 409
        public static String DuplicatedParameterException = "DuplicatedParameterException";// 중복된 파라미터 오류
        public static String UsedReceipt = "UsedReceipt";// 사용한 영수증

        // 410
        public static String GoneResourceException = "GoneResourceException";// 리소스 사용 만료

        // 412
        public static String PreconditionFailed = "PreconditionFailed";// 조건 실패

        // 500
        public static String ServerErrorException = "ServerErrorException";
        public static String InternalServerError = "InternalServerError";

        // 502
        public static String BadGateway = "BadGateway";

        // 503
        public static String ETIMEDOUT = "ETIMEDOUT";// 요청에 대한 시간이 오래 걸릴 때

        // 504
        public static String DatabaseError = "DatabaseError";

        // 기타
        public static String UNKNOWN = "UNKNOWN";
    }
}

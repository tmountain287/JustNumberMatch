using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backend_cli.BackEnd
{
    public class BackendReturnObject
    {
        public BackendReturnObject() { }
        public BackendReturnObject(int statusCode, string errorCode, string message, string returnValue = "")
        {
            this.statusCode = statusCode;
            this.errorCode = errorCode;
            this.message = message;
            this.returnValue = returnValue;
        }

        public int statusCode { get; set; }// 서버에서 넘겨주는 http 상태 코드
        public string errorCode { get; set; }// 에러 정보를 알려주는 코드
        public string message { get; set; }// 서버에서 넘겨주는 성공 / 실패 세부 정보
        public string returnValue { get; set; }// 요청 성공 시 서버에서 넘겨준 json 형태의 데이터
        public string code { get; set; }// Federation code(해당 에러 케이스의 고유 ID)
        public string errorMessage { get; set; }// Federation errorMessage(해당 에러 세부 사항)
        public string errorDataString { get; set; }// Federation errorDataString(에러에 관련된 세부 데이터 json)

        public string GetCode()
        {
            return code;
        }

        public string GetErrorCode()
        {
            return errorCode;
        }

        public string GetErrorMessage()
        {
            return errorMessage;
        }

        public string GetMessage()
        {
            return message;
        }

        //public float GetReadCapacity();
        public string GetReturnValue()
        {
            return returnValue;
        }

        public int GetStatusCode()
        {
            return statusCode;
        }

        // 서버에서 리턴값을 전달 했는지 확인한다.
        // 전달할 값이 있다면 returnValue에 json형태로 전달된다. null일 경우 서버에서 전달할 리턴값이 없는 것이다.
        public bool HasReturnValue()
        {
            bool bError = false;

            if (returnValue != null)
                bError = true;

            return bError;
        }

        public bool IsDeviceBlockError()
        {
            bool bError = false;

            if (statusCode == 403 && errorCode.Equals(NetErrorCode.Forbidden) && message.Contains("Forbidden blocked device"))
                bError = true;

            return bError;
        }

        // 클라이언트 에러인지 확인(400 번대)
        public bool IsClientError()
        {
            bool bError = false;

            if (400 <= statusCode && statusCode < 500)
                bError = true;

            return bError;
        }

        // 서버 에러인지 확인(500 번대)
        public bool IsServerError()
        {
            bool bError = false;

            if (500 <= statusCode && statusCode < 600)
                bError = true;

            return bError;
        }

        // 요청이 성공했는지 실패했는지 확인 (성공: 200번대 statusCode, 실패: 300 이상의 statusCode)
        public bool IsSuccess()
        {
            bool bError = false;

            if (200 <= statusCode && statusCode < 300)
                bError = true;

            return bError;
        }
    }
}

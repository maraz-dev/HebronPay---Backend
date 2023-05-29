using HebronPay.Responses.Enums;

namespace HebronPay.Responses
{
    public class ApiResponse
    {
        public string Message { get; set; }
        public string code { get; set; }

        public object data { get; set; }

        public ApiError error { get; set; }

    }

    public class ApiError
    {
        public string message { get; set; }
    }

    public class ReturnedResponse
    {


        public ApiResponse ErrorResponse(string message, object data)
        {
            var apiResp = new ApiResponse();
            apiResp.data = data;
            apiResp.Message = ApiResponseEnum.failure.ToString();
            apiResp.code = "400";
            var error = new ApiError();
            error.message = message;
            apiResp.error = error;

            return apiResp;
        }

        public ApiResponse CorrectResponse(object data)
        {
            var apiResp = new ApiResponse();
            apiResp.data = data;
            apiResp.Message = ApiResponseEnum.success.ToString();
            apiResp.code = "200";
            apiResp.error = null;

            return apiResp;
        }
    }
}

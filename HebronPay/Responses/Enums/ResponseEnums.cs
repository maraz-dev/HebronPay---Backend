using System.ComponentModel;

namespace HebronPay.Responses.Enums
{
    public enum EmailValidationEnum
    {
        [Description("invalid")] invalid = 1,
        [Description("valid")] valid,

    }

    public enum ApiResponseEnum
    {
        [Description("failure")] failure = 1,
        [Description("success")] success,

    }
}

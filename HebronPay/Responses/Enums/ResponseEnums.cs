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

    public enum HebronPayTransactionTypeEnum
    {
        [Description("pending")] pending = 1,
        [Description("credit")] credit = 2,
        [Description("debit")] debit = 3,
 

    }

    public enum CurrencyEnum
    {
        [Description("NGN")] NGN = 1,


    }
    public enum FlutterWaveResponseEnum
    {
        [Description("success")] success = 1,

    }
}

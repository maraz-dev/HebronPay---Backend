using System.Text.Json.Serialization;
using System;

namespace HebronPay.Model.FlutterWave.SubAccout
{
    public class CreateSubAccountRequestModel
    {
        public string account_name { get; set; }
        public string email { get; set; }
        public string mobilenumber { get; set; }
        public string country { get; set; }
        public string account_reference { get; set; }
        public string bank_code { get; set; }
    }

    public class CreateSubAccountResponseModel
    {
        public int id { get; set; }

        public string account_reference { get; set; }

        public string account_name { get; set; }

        public string barter_id { get; set; }

        public string email { get; set; }

        public string mobilenumber { get; set; }

        public string country { get; set; }

        public string nuban { get; set; }

        public string bank_name { get; set; }

        public string bank_code { get; set; }
 
        public string status { get; set; }

        public DateTimeOffset created_at { get; set; }
    }





    public class GetWalletBalanceModel
    {
        public string currency { get; set; }
        public string available_balance { get; set; }
        public string ledger_balance { get; set; }
    }


}

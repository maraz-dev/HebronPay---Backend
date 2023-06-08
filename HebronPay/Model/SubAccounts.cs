using System;

namespace HebronPay.Model
{
    public class SubAccount
    {
        public int id { get; set; }
        public int flutterwaveSubAccountId { get; set; }

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
}

using System;

namespace HebronPay.Model.Transactions
{
    public class GenerateTicketModel
    {
        public string description { get; set; }
        public double amount { get; set; }
        

    }


    public class WithdrawModel
    {
        public string account_number { get; set; }
        public string account_name { get; set; }
        public string account_bank { get; set; }
        public string narration { get; set; }
        public double amount { get; set; }


    }

    public class FundWalletModel
    {
        public double amount { get; set; }


    }

    public class GetTransactionResponse : HebronPayTransaction
    {
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

using System;

namespace HebronPay.Model.FlutterWave.Transfer
{
    public class InitiateTransferRequest
    {
        public string account_bank { get; set; } //the bank code for the recipient of the money
        public string account_number { get; set; } //the account number of the recipient of the money
        public double amount { get; set; } //the amount to be transfered
        public string narration { get; set; } //descirption of the ticket/transaction
        public string currency { get; set; } //for now, always set to "NGN"
        public string reference { get; set; } //transaction referece
        public string debit_currency { get; set; }  //for now, always set to "NGN"
        public string debit_subaccount { get; set; } //reference of the sender's sub account
    }

    public class ResolveAccountDetailsRequest
    {
        public string account_number{ get; set; }
        public string account_bank{ get; set; }
    }

    public class ResolveAccountDetailsResponse
    {
        public string account_number { get; set; }
        public string account_name { get; set; }
    }

    public class Bank
    {
        public int id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    }


    public class InitiateTransferResponse
    {
        public int id { get; set; }
        public string account_number { get; set; }
        public string bank_code { get; set; }
        public string full_name { get; set; }
        public DateTime created_at { get; set; }
        public string currency { get; set; }
        public string debit_currency { get; set; }
        public int amount { get; set; }
        public int fee { get; set; }
        public string status { get; set; }
        public string reference { get; set; }
        public object meta { get; set; }
        public string narration { get; set; }
        public string complete_message { get; set; }
        public int requires_approval { get; set; }
        public int is_approved { get; set; }
        public string bank_name { get; set; }
    }

    
}

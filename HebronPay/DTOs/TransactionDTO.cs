namespace HebronPay.DTOs
{
    public class GetTransactionDTO
    {
        public int id { get; set; }
        public string description { get; set; }
        public double amount { get; set; }
        public string reference { get; set; }
        public string type { get; set; }  // THIS WILL BE PENDING   
        public string date { get; set; }
        public string time { get; set; }
        public string username { get; set; }

    }

    public class ConfirmTransactionDTO
    {
        public int id { get; set; }
        public string description { get; set; }
        public double amount { get; set; }
        public string reference { get; set; }
        public string type { get; set; }  // THIS WILL BE PENDING   
        public string date { get; set; }
        public string time { get; set; }


    }

}

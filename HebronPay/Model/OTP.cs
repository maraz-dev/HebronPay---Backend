using System.Collections.Generic;

namespace HebronPay.Model
{
    public class OTP
    {
        public int Id { get; set; }
        public int pin { get; set; }
        public string email { get; set; }
    }

    public class Content
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class From
    {
        public string email { get; set; }
    }

    public class Personalization
    {
        public List<To> to { get; set; }
        public string subject { get; set; }
    }

    public class SendEmailRequest
    {
        public List<Personalization> personalizations { get; set; }
        public From from { get; set; }
        public List<Content> content { get; set; }
    }

    public class To
    {
        public string email { get; set; }
    }
}

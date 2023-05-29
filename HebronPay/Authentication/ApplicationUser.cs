using Microsoft.AspNetCore.Identity;

namespace HebronPay.Authentication
{
    public class ApplicationUser : IdentityUser
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string DateofBirth { get; set; }
        public bool isOtpVerified { get; set; }
        public bool isKycVerified { get; set; }

    }
}

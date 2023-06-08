using HebronPay.Model;
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

        public int subAccountId { get; set; }
        public virtual SubAccount subAccount { get; set; }
       
        public int hebronPayWalletId { get; set; }
        public virtual HebronPayWallet hebronPayWallet { get; set; }

    }
}

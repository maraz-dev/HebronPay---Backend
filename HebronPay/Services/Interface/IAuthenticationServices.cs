using HebronPay.Authentication;
using HebronPay.Responses;
using System.Threading.Tasks;

namespace HebronPay.Services.Interface
{
    public interface IAuthenticationServices
    {

        public Task<ApiResponse> CheckValidations(ValidateModel model);
        public Task<ApiResponse> SignUpUser(SignUpModel model);
        public Task<ApiResponse> Login(LoginModel model);
        public Task<ApiResponse> GetUserDetails(string username);
        public Task<ApiResponse> SetPin(string username, SetPinModel model);
        public Task<ApiResponse> ChangePin(string username, ChangePinModel model);
        public Task<ApiResponse> CreateOTP(string email);
        public Task<ApiResponse> SendOTP(string email);
        public Task<ApiResponse> ValidateOTP(string inputPin, string email);
        //public Task<ApiResponse> ForgotPassword(string email, string newPassword, string confirmPassword);
        public Task<ApiResponse> ForgotPassword(ForgotPasswordModel model);
        public Task<ApiResponse> ChangePassword(string username, ChangePasswordModel model);

      //  public Task<ApiResponse> ChangePassword(string username, string currentPassword, string newPassword, string confirmPassword);
        public Task<ApiResponse> UpdateUserWallet(string username);
        public Task<ApiResponse> getSubAccountBalance(string username);

        public ApiResponse ValidatePassword(string password);
        public ApiResponse ValidatePin(string pin);
    }
}

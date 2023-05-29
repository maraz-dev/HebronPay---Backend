using HebronPay.Authentication;
using HebronPay.Responses;
using System.Threading.Tasks;

namespace HebronPay.Services.Interface
{
    public interface IAuthenticationServices
    {
        public Task<ApiResponse> SignUpUser(SignUpModel model);
        public Task<ApiResponse> CreateOTP(string email);
        public Task<ApiResponse> SendOTP(string email);
        public Task<ApiResponse> ValidateOTP(int inputPin, string email);
    }
}

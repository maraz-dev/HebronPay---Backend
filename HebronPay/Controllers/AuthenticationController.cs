using HebronPay.Authentication;
using HebronPay.Responses;
using HebronPay.Responses.Enums;
using HebronPay.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HebronPay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationServices _authenticationServices;
        public AuthenticationController(IAuthenticationServices authenticationServices)
        {
            _authenticationServices = authenticationServices;
        }

        
        [HttpPost("SignUp")]
        public async Task<ActionResult<ApiResponse>> SignUp(SignUpModel signUpModel)
        {

            var response = await _authenticationServices.SignUpUser(signUpModel);
            if (response.Message == ApiResponseEnum.success.ToString())
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }

        }

        [HttpPost("SendOTP")]
        public async Task<ActionResult<ApiResponse>> SendOTP(string email)
        {

            var response = await _authenticationServices.SendOTP(email);
            if (response.Message == ApiResponseEnum.success.ToString())
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }

        }
    }
}

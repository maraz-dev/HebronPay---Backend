using HebronPay.Authentication;
using HebronPay.FlutterwaveServices.Interface;
using HebronPay.Model.FlutterWave.SubAccout;
using HebronPay.Responses;
using HebronPay.Responses.Enums;
using HebronPay.Services.Interface;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IFlutterwaveServices _flutterwaveServices;
        public AuthenticationController(IAuthenticationServices authenticationServices, IFlutterwaveServices flutterwaveServices)
        {
            _authenticationServices = authenticationServices;
            _flutterwaveServices = flutterwaveServices;
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

        [HttpPost("Login")]
        public async Task<ActionResult<ApiResponse>> Login(LoginModel loginModel)
        {

            var response = await _authenticationServices.Login(loginModel);
            if (response.Message == ApiResponseEnum.success.ToString())
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }

        }

        [Authorize]
        [HttpPost("SetPin")]
        public async Task<ActionResult<ApiResponse>> SetPin(SetPinModel model)
        {

            var response = await _authenticationServices.SetPin(User.Identity.Name,model);
            if (response.Message == ApiResponseEnum.success.ToString())
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }

        }


        [Authorize]
        [HttpPost("ChangePin")]
        public async Task<ActionResult<ApiResponse>> ChangePin(ChangePinModel model)
        {

            var response = await _authenticationServices.ChangePin(User.Identity.Name, model);
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


        [HttpPost("ValidateOTP")]
        public async Task<ActionResult<ApiResponse>> ValidateOTP(string inputPin, string email)
        {

            var response = await _authenticationServices.ValidateOTP(inputPin,email);
            if (response.Message == ApiResponseEnum.success.ToString())
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }

        }


       /* [HttpPost("createsubAccount")]
        public async Task<ActionResult<ApiResponse>> createSubAccount(CreateSubAccountRequestModel model)
        {

            var response = await _flutterwaveServices.createSubAccount(model);
            var payoutSubAccount = response.data;
            return Ok(payoutSubAccount);
            

        }

        */

        [HttpPost("ForgotPassword")]
        public async Task<ActionResult<ApiResponse>> ForgotPassword(ForgotPasswordModel model)
        {

            var response = await _authenticationServices.ForgotPassword(model);
            if (response.Message == ApiResponseEnum.success.ToString())
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }

        }

        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<ActionResult<ApiResponse>> ChangePassword(ChangePasswordModel model)
        {

            var response = await _authenticationServices.ChangePassword(User.Identity.Name, model);
            if (response.Message == ApiResponseEnum.success.ToString())
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }

        }


        [Authorize]
        [HttpGet("GetSubAccountBalance")]
        public async Task<ActionResult<ApiResponse>> GetSubAccountBalance()
        {

            var response = await _authenticationServices.getSubAccountBalance(User.Identity.Name);
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

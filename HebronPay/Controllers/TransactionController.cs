using HebronPay.Authentication;
using HebronPay.FlutterwaveServices.Interface;
using HebronPay.Responses.Enums;
using HebronPay.Responses;
using HebronPay.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HebronPay.Model.Transactions;
using Microsoft.AspNetCore.Authorization;
using HebronPay.Model.FlutterWave.Transfer;
using HebronPay.Model;

namespace HebronPay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IAuthenticationServices _authenticationServices;
        private readonly ITransactionServices _transactionServices;
        private readonly IFlutterwaveServices _flutterwaveServices;
        public TransactionController(IAuthenticationServices authenticationServices, IFlutterwaveServices flutterwaveServices, ITransactionServices transactionServices)
        {
            _authenticationServices = authenticationServices;
            _flutterwaveServices = flutterwaveServices;
            _transactionServices = transactionServices; 
        }

        //YOU NEED TO MAKE CHANGE HERE OOOOOO, CHECK SERVICE CLASS
        [Authorize]
        [HttpPost("GenerateTicket")]
        public async Task<ActionResult<ApiResponse>> GenerateTicket(GenerateTicketModel model)
        {

            var response = await _transactionServices.generateTicket(User.Identity.Name,model);
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
        [HttpPost("InitiateTransfer")]
        public async Task<ActionResult<ApiResponse>> InitiateTransfer(InitiateTransferRequest model)
        {

            var response = await _transactionServices.initiateTransfer(model);
            if (response.Message == ApiResponseEnum.success.ToString())
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }

        }


        //YOU NEED TO MAKE CHANGE HERE OOOOOO, CHECK SERVICE CLASS
        [Authorize]
        [HttpDelete("DeleteTicket")]
        public async Task<ActionResult<ApiResponse>> DeleteTicket(string reference)
        {

            var response = await _transactionServices.deleteTicket(reference);
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
        [HttpGet("GetPendingTransactions")]
        public async Task<ActionResult<ApiResponse>> GetPendingTransactions()
        {

            var response = await _transactionServices.getPendingTransactions(User.Identity.Name);
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
        [HttpGet("GetTransaction")]
        public async Task<ActionResult<ApiResponse>> GetTransaction(string reference)
        {

            var response = await _transactionServices.getTransactionDetails(User.Identity.Name,reference);
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
        [HttpGet("GetSubAccount")]
        public async Task<ActionResult<ApiResponse>> GetSubAccount()
        {

            var response = await _transactionServices.getSubAccountDetails(User.Identity.Name);
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
        [HttpPost("FundWallet")]
        public async Task<ActionResult<ApiResponse>> FundWallet(FundWalletModel model)
        {

            var response = await _transactionServices.fundWallet(User.Identity.Name, model);
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
        [HttpPost("ConfirmTicketPayment")]
        public async Task<ActionResult<ApiResponse>> ConfirmTicketPayment(HebronPayTransaction model)
        {

            var response = await _transactionServices.confirmTicketPayment(User.Identity.Name, model);
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
        [HttpGet("GetAllTransactions")]
        public async Task<ActionResult<ApiResponse>> GetAllTransactions()
        {

            var response = await _transactionServices.getAllTransactions();
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
        [HttpGet("GetUsersTransactions")]
        public async Task<ActionResult<ApiResponse>> GetUsersTransactions()
        {

            var response = await _transactionServices.getUsersTransactions(User.Identity.Name);
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
        [HttpPost("ResolveBankAccountDetails")]
        public async Task<ActionResult<ApiResponse>> ResolveBankAccountDetails(ResolveAccountDetailsRequest model)
        {

            var response = await _transactionServices.resolveBankAccount(model);
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
        [HttpGet("GetBanks")]
        public async Task<ActionResult<ApiResponse>> GetBanks()
        {

            var response = await _transactionServices.getAllBanks();
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
        [HttpPost("WithdrawFromWallet")]
        public async Task<ActionResult<ApiResponse>> WithdrawFromWallet(WithdrawModel model)
        {

            var response = await _transactionServices.withdraw(User.Identity.Name,model);
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

using AutoMapper;
using HebronPay.Authentication;
using HebronPay.FlutterwaveServices.Interface;
using HebronPay.Model;
using HebronPay.Model.FlutterWave.SubAccout;
using HebronPay.Model.FlutterWave.Transfer;
using HebronPay.Model.Transactions;
using HebronPay.Responses;
using HebronPay.Responses.Enums;
using HebronPay.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HebronPay.Services.Implementation
{
    public class TransactionServices :ITransactionServices
    {
        private readonly UserManager<ApplicationUser> userManager;
        private ApplicationDbContext _context;
        private readonly IFlutterwaveServices _flutterwaveServices;

        public TransactionServices(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IFlutterwaveServices flutterwaveServices)
        {
            this.userManager = userManager;
            _context = context;
            _flutterwaveServices = flutterwaveServices;


        }

        //PLEASEEEEEE DO NOT FORGET TO ADD A CHECK FOR WHETHER THE AMOUNT >= WALLET BALANCE FOR INSUFFICIENT FUNDS
        
        public async Task<ApiResponse> generateTicket(string username, GenerateTicketModel model)
        {

            //this method is to create a hebron pay wallet transaction basiacally

            //PLEASEEEEEE DO NOT FORGET TO ADD A CHECK FOR WHETHER THE AMOUNT >= WALLET BALANCE FOR INSUFFICIENT FUNDS

            ReturnedResponse returnedResponse = new ReturnedResponse();
            
            //var user = await userManager.FindByNameAsync(username);
            var user = await _context.Users
                .Where(u => u.UserName == username)
                .Include(u=>u.hebronPayWallet)
                .FirstAsync();
            var userHebronPayWallet = user.hebronPayWallet;

            try
            {
                HebronPayTransaction transaction = new HebronPayTransaction
                {
                    amount = model.amount,
                    reference = generateRandomString(20),
                    date = DateTime.Today.ToString("dd-MM-yyyy"),
                    time = DateTime.Now.ToString("hh:mm tt"),
                    description = model.description,
                    type = HebronPayTransactionTypeEnum.pending.GetEnumDescription(),
                    hebronPayWalletId = userHebronPayWallet.id,
                    hebronPayWallet = userHebronPayWallet,
                };

                await _context.HebronPayTransactions.AddAsync(transaction);
                await _context.SaveChangesAsync();

                //userHebronPayWallet.hebronPayTransactions.Add(transaction);

                return returnedResponse.CorrectResponse(transaction);

            }

            catch (Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);
            }
            
        }

        public string generateRandomString(int length)
        {

            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new string(result);
        }

        public async Task<ApiResponse> getPendingTransactions(string username)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();

            try
            {
                //get the user's details and the user's wallet details.
                var user = await _context.Users
                                .Where(u => u.UserName == username)
                                .Include(u => u.hebronPayWallet)
                                .FirstAsync();
                var userHebronPayWallet = user.hebronPayWallet;


                //use the user's wallet details to get pending transactions on the wallet.
                var transactions = await _context.HebronPayTransactions.Where(t => t.type == HebronPayTransactionTypeEnum.pending.GetEnumDescription() && t.hebronPayWalletId == userHebronPayWallet.id).ToListAsync();

                return returnedResponse.CorrectResponse(transactions);
            }
            catch(Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);
            }


        }

        public async Task<ApiResponse> deleteTicket(string reference)
        {
            //PLEASEEEE REFUND THE MONEY BACK TO THE USER'S HEBRON PAY WALLET
            ReturnedResponse returnedResponse = new ReturnedResponse();
            var ticket = await _context.HebronPayTransactions.Where(t => t.reference == reference).OrderBy(t=>t.id).LastAsync();

            try
            {
                _context.HebronPayTransactions.Remove(ticket);
                await _context.SaveChangesAsync();
                return returnedResponse.CorrectResponse("successfully deleted ticket");
            }
            catch(Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);
            }
        }

        public async Task<ApiResponse> getSubAccountDetails(string username)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();

            try
            {
                var user = await _context.Users.Where(u => u.UserName == username).Include(u => u.subAccount).Include(u => u.hebronPayWallet).FirstAsync();
                var userSubAccount = user.subAccount;
                return returnedResponse.CorrectResponse(userSubAccount);
            }
            catch(Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);
            }


        
        
        }

        public async Task<ApiResponse> getTransactionDetails(string username, string reference)
        {

            //this method retuns the transaction details AND the details of the sub account of the user that will be debited for the transaction
            ReturnedResponse returnedResponse = new ReturnedResponse();

            var mapper = new Mapper(MapperConfig.GetMapperConfiguration());



            try
            {
                var user = await _context.Users.Where(u => u.UserName == username).Include(u => u.subAccount).Include(u => u.hebronPayWallet).FirstAsync();
                var hebronPayWallet = user.hebronPayWallet;
                var userSubAccount = user.subAccount;

                var transaction = await _context.HebronPayTransactions.Where(t => t.hebronPayWalletId == hebronPayWallet.id && t.reference == reference).OrderBy(t => t.id).LastAsync();
                if (transaction == null)
                {
                    return returnedResponse.ErrorResponse("No such transaction exists",null);
                }

                var transactionResponse = mapper.Map<GetTransactionResponse>(transaction);

                transactionResponse.flutterwaveSubAccountId = userSubAccount.flutterwaveSubAccountId;
                transactionResponse.account_reference = userSubAccount.account_reference;
                transactionResponse.account_name = userSubAccount.account_name;
                transactionResponse.barter_id = userSubAccount.barter_id;
                transactionResponse.email = userSubAccount.email;
                transactionResponse.mobilenumber = userSubAccount.mobilenumber;
                transactionResponse.country = userSubAccount.country;
                transactionResponse.nuban = userSubAccount.nuban;
                transactionResponse.bank_name = userSubAccount.bank_name;
                transactionResponse.bank_code = userSubAccount.bank_code;
                transactionResponse.status = userSubAccount.status;

                


                return returnedResponse.CorrectResponse(transactionResponse);

            }
            catch (Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);
            }

        }

        public async Task<ApiResponse> initiateTransfer(InitiateTransferRequest model)
        {
            ReturnedResponse returnedResponse = new ReturnedResponse();

            try
            {

                model.debit_currency = CurrencyEnum.NGN.GetEnumDescription();
                model.currency = CurrencyEnum.NGN.GetEnumDescription();

                var flutterwaveResponse = await _flutterwaveServices.initiateTransfer(model);

                if (flutterwaveResponse.status != FlutterWaveResponseEnum.success.GetEnumDescription())
                {
                    return returnedResponse.ErrorResponse(flutterwaveResponse.message, null);
                }

                InitiateTransferResponse transferData = JsonConvert.DeserializeObject<InitiateTransferResponse>(flutterwaveResponse.data.ToString());
                return returnedResponse.CorrectResponse(transferData);

            }

            catch (Exception e)
            {
                return returnedResponse.ErrorResponse(e.Message, null);
            }



        }
    }
}

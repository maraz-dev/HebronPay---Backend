using HebronPay.Authentication;
using HebronPay.Model;
using HebronPay.Model.FlutterWave.Transfer;
using HebronPay.Model.Transactions;
using HebronPay.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HebronPay.Services.Interface
{
    public interface ITransactionServices
    {
        public Task<ApiResponse> generateTicket(string username, GenerateTicketModel model);
        public Task<ApiResponse> confirmTicketPayment(string receiverUsername, HebronPayTransaction pendingTransaction);
        public Task<ApiResponse> deleteTicket(string reference);
        public Task<ApiResponse> getPendingTransactions(string username);
        public Task<ApiResponse> getTransactionDetails(string username,string reference);
        public Task<ApiResponse> getSubAccountDetails(string username);
        public Task<ApiResponse> initiateTransfer(InitiateTransferRequest model);
        public Task<ApiResponse> fundWallet(string username, FundWalletModel model);
        public Task<ApiResponse> withdraw(string username, WithdrawModel model);


        //this endpoint is to get ALLLLL transactions
        public Task<ApiResponse> getAllTransactions();

        //this endpoint is to get all transactions of a SPECIFIC USER
        public Task<ApiResponse> getUsersTransactions(string username);

        //this endpoint is to get all pending transactions of a SPECIFIC USER
        public Task<ApiResponse> getUsersPendingTransactions(string username);


        //this endpoint is to get all banks 
        public Task<ApiResponse> getAllBanks();

        //this endpoint is to get user account details
        public Task<ApiResponse> resolveBankAccount(ResolveAccountDetailsRequest model);
        
        
        
        //send email asyync
        public Task<ApiResponse> sendEmailAsync(string email, string subject, string message);

        //get transactions in a day
        public Task<ApiResponse> generateEod(string username);

    }
}

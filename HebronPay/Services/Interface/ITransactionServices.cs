using HebronPay.Authentication;
using HebronPay.Model.FlutterWave.Transfer;
using HebronPay.Model.Transactions;
using HebronPay.Responses;
using System.Threading.Tasks;

namespace HebronPay.Services.Interface
{
    public interface ITransactionServices
    {
        public Task<ApiResponse> generateTicket(string username, GenerateTicketModel model);
        public Task<ApiResponse> deleteTicket(string reference);
        public Task<ApiResponse> getPendingTransactions(string username);
        public Task<ApiResponse> getTransactionDetails(string username,string reference);
        public Task<ApiResponse> getSubAccountDetails(string username);
        public Task<ApiResponse> initiateTransfer(InitiateTransferRequest model);
    }
}

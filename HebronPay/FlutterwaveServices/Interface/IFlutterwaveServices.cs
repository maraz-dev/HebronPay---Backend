using HebronPay.Model.FlutterWave.SubAccout;
using HebronPay.Model.FlutterWave.Transfer;
using HebronPay.Responses;
using System.Threading.Tasks;

namespace HebronPay.FlutterwaveServices.Interface
{
    public interface IFlutterwaveServices
    {
        public Task<FlutterWaveResponse> createSubAccount(CreateSubAccountRequestModel model);
        public Task<FlutterWaveResponse> initiateTransfer(InitiateTransferRequest model);
        public Task<FlutterWaveResponse> getWalletBalance(string account_reference);
    }
}

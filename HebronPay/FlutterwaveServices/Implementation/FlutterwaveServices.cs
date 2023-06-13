using HebronPay.FlutterwaveServices.Interface;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using HebronPay.Responses;
using HebronPay.Model.FlutterWave.SubAccout;
using System.Net.Http.Json;
using Microsoft.OpenApi.Writers;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using HebronPay.Model.FlutterWave.Transfer;

namespace HebronPay.FlutterwaveServices.Implementation
{
    public class FlutterwaveService : IFlutterwaveServices
    {
        public FlutterwaveService()
        {

        }

        public async Task<FlutterWaveResponse> createSubAccount(CreateSubAccountRequestModel model)
        {
            HttpClient client = setclient();

            string path = "payout-subaccounts";
            try
            {
                HttpResponseMessage Res = await client.PostAsJsonAsync(path, model);
                if (Res.IsSuccessStatusCode)
                {

                    var flutterwaveResponse = await Res.Content.ReadFromJsonAsync<FlutterWaveResponse>();


                    return flutterwaveResponse;
                }

                else
                {
                    var flutterwaveResponse = await Res.Content.ReadFromJsonAsync<FlutterWaveResponse>();
                    return flutterwaveResponse;
                }

            }
            catch (Exception e)
            {
                return null;
            }


        }

        public async Task<FlutterWaveResponse> getBankAccountDetails(ResolveAccountDetailsRequest model)
        {
            HttpClient client = setclient();

            string path = "accounts/resolve";
            try
            {
                HttpResponseMessage Res = await client.PostAsJsonAsync(path, model);
                if (Res.IsSuccessStatusCode)
                {

                    var flutterwaveResponse = await Res.Content.ReadFromJsonAsync<FlutterWaveResponse>();


                    return flutterwaveResponse;
                }

                else
                {
                    var flutterwaveResponse = await Res.Content.ReadFromJsonAsync<FlutterWaveResponse>();
                    return flutterwaveResponse;
                }

            }
            catch (Exception e)
            {
                return null;
            }

        }

        public async Task<FlutterWaveResponse> getBanks()
        {
            //throw new NotImplementedException();
            HttpClient client = setclient();
            string country = "NG";

            string path = $"banks/{country}";

            try
            {
                HttpResponseMessage Res = await client.GetAsync(path);
                if (Res.IsSuccessStatusCode)
                {

                    var flutterwaveResponse = await Res.Content.ReadFromJsonAsync<FlutterWaveResponse>();


                    return flutterwaveResponse;
                }

                else
                {
                    var flutterwaveResponse = await Res.Content.ReadFromJsonAsync<FlutterWaveResponse>();
                    return flutterwaveResponse;
                }

            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<FlutterWaveResponse> getWalletBalance(string account_reference)
        {
            //throw new NotImplementedException();
            HttpClient client = setclient();
            

            string path = $"payout-subaccounts/{account_reference}/balances?currency=NGN";



            try
            {
                HttpResponseMessage Res = await client.GetAsync(path);
                if (Res.IsSuccessStatusCode)
                {

                    var flutterwaveResponse = await Res.Content.ReadFromJsonAsync<FlutterWaveResponse>();


                    return flutterwaveResponse;
                }

                else
                {
                    var flutterwaveResponse = await Res.Content.ReadFromJsonAsync<FlutterWaveResponse>();
                    return flutterwaveResponse;
                }

            }
            catch (Exception e)
            {
                return null;
            }

        }

        public async Task<FlutterWaveResponse> initiateTransfer(InitiateTransferRequest model)
        {
            HttpClient client = setclient();

            string path = "transfers";
            try
            {
                HttpResponseMessage Res = await client.PostAsJsonAsync(path, model);
                if (Res.IsSuccessStatusCode)
                {

                    var flutterwaveResponse = await Res.Content.ReadFromJsonAsync<FlutterWaveResponse>();


                    return flutterwaveResponse;
                }

                else
                {
                    var flutterwaveResponse = await Res.Content.ReadFromJsonAsync<FlutterWaveResponse>();
                    return flutterwaveResponse;
                }

            }
            catch (Exception e)
            {
                return null;
            }

        }

        public HttpClient setclient()
        {

            HttpClient client = new HttpClient();
            string baseUrl = "https://api.flutterwave.com/v3/";
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer FLWSECK-d439586d86533056270f282797e0c42d-18867bef15avt-X");

            return client;
        }




    }
}

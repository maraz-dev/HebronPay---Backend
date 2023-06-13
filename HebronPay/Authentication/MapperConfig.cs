using AutoMapper;
using HebronPay.Model;
using HebronPay.Model.FlutterWave.SubAccout;
using HebronPay.Model.Transactions;

namespace HebronPay.Authentication
{
    public class MapperConfig
    {
        public static MapperConfiguration GetMapperConfiguration()
        {
            var config = new MapperConfiguration(cfg =>
            {
                //cfg.CreateMap<SubAccount, CreateSubAccountResponseModel>();
                cfg.CreateMap<CreateSubAccountResponseModel, SubAccount>();
                cfg.CreateMap<ApplicationUser, LoginResponseModel>();
                cfg.CreateMap<AuthorizationToken, LoginResponseModel>();
                cfg.CreateMap<SubAccount, GetTransactionResponse>();
                cfg.CreateMap<HebronPayTransaction, GetTransactionResponse>();
                cfg.CreateMap<SignUpModel, ValidateModel>();




            }
                   );
            return config;
        }
    }
}

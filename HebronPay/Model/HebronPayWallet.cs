using System;
using System.Collections.Generic;

namespace HebronPay.Model
{
    public class HebronPayWallet
    {
        public int id { get; set; }
        public double walletBalance { get; set; }

        public int walletPin { get; set; }

        //public List<HebronPayTransaction> hebronPayTransactions { get; set; }
    }


}

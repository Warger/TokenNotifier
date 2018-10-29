using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TokenNotifier.Models;

namespace TokenNotifier.ViewModels
{
    public class TransferForView
    {
        public Transfer Transfer { get; set; }
        public string OutWallet { get; set; }
        public string OutWalletName { get; set; }
        public string InWallet { get; set; }
        public string InWalletName { get; set; }
        public string TokenName { get; set; }
        public string SupplyPercentage { get; set; }

        public TransferForView(Transfer t = null, Wallet inW = null, Wallet outW = null, string tokenName = "", string supplyPercentage = "")
        {
            inW = inW == null ? new Wallet { Address = "", Name = "" } : inW;
            outW = outW == null ? new Wallet { Address = "", Name = "" } : outW;

            Transfer = t;
            OutWallet = outW.GetUrl;
            OutWalletName = outW.NameOrAddress;
            InWallet = inW.GetUrl;
            InWalletName = inW.NameOrAddress;
            TokenName = tokenName;
            SupplyPercentage = supplyPercentage;
        }
    }
}

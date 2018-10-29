using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TokenNotifier.Models;

namespace TokenNotifier.ViewModels
{
    public class WalletForView
    {
        public Wallet Wallet { get; set; }
        public List<TransferForView> TransferList { get; set; }

        public WalletForView(Wallet w, List<TransferForView> tl)
        {
            Wallet = w;
            TransferList = tl;
        }
    }
}

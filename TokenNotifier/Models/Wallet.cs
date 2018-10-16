using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TokenNotifier.Models
{
    public class Wallet
    {
        public int WalletID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Comment { get; set; }

        public string NameOrAddress
        {
            get
            {
                return Name == null ? Address : Name;
            }
        }
    }
}

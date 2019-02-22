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
        public Exchange Exchange { get; set; }
        public int? ExchangeId { get; set; }

        public string NameOrAddress
        {
            get
            {
                return Name == null || Name.Length==0 ? Address : Name;
            }
        }

        public string GetExcange
        {
            get
            {
                return Exchange == null ? "No Exchange" : Exchange.Title;
            }
        }

        public string GetUrl
        {
            get
            {
                return "https://etherscan.io/address/" + Address ;
            }
        }
    }
}

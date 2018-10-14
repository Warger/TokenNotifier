using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TokenNotifier.Models
{
    public class Token
    {
        public int TokenID { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Contract { get; set; }
        public bool IsObserved { get; set; }
        public double PercentForNotification { get; set; }
        public ulong TotalSupply { get; set; }
        public int Decimals { get; set; }
    }
}

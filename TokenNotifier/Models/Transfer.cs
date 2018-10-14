using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TokenNotifier.Models
{
    public class Transfer
    {
        [Key]
        public int TrasferID { get; set; }
        public string IncomingAddress { get; set; }
        public string OutgoingAddress { get; set; }
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public string Token { get; set; }
        public double UsdValue { get; set; }
        public string TransactionHash { get; set; }
    }
}

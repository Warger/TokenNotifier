using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TokenNotifier.Models;

namespace TokenNotifier.ViewModels
{
    public class ExchangeForView
    {
        public Exchange Exchange { get; set; }
        public List<ExchangeToken> Tokens { get; set; }
    }
}

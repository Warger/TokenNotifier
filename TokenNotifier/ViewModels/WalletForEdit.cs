using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TokenNotifier.Models;

namespace TokenNotifier.ViewModels
{
    public class WalletForEdit
    {
        public Wallet Wallet { get; set; }
        public List<SelectListItem> ExchangeOptions { get; set; }
    }
}

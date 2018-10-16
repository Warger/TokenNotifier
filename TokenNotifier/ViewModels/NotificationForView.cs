using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TokenNotifier.Models;

namespace TokenNotifier.ViewModels
{
    public class NotificationForView
    {
        public Notification Notification { get; set; }
        public string WalletAddress { get; set; }
        public string WalletName { get; set; }

        public NotificationForView (Notification n, string wa, string wn)
        {
            Notification = n;
            WalletAddress = wa;
            WalletName = wn;
        }
    }
}

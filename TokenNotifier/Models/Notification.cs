﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TokenNotifier.Models
{
    public enum Actions
    {
        BigDailySum, Other
    }

    public class Notification
    {
        public int NotificationID { get; set; }
        public string Description { get; set; }
        public Actions? Action { get; set; }
        public Wallet LinkedWallet { get; set; }
    }
}

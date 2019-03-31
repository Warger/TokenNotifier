using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TokenNotifier.Models
{
    public class TwitterAccount
    {
        public int TwitterAccountId { get; set; }
        public string Name { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Template { get; set; }
    }
}

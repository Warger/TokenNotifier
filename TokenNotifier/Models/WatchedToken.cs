using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TokenNotifier.Models
{
    public class WatchedToken
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Comment { get; set; }
        public int Counter { get; set; }
        public DateTime DateTime { get; set; }
        public Exchange Exchange { get; set; }
    }
}

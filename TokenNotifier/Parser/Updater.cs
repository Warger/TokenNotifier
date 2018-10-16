using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TokenNotifier.Data;

namespace TokenNotifier.Parser
{
    public class Updater
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private bool isProcessing = false;

        public Updater(IServiceScopeFactory serviceScopeFactory) : base()
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void Execute()
        {
            if (isProcessing)
                return;

            try
            {
                using (WebClient wc = new WebClient())
                {
                //var json = wc.DownloadString("https://localhost:44377/Transfers/Update");
                  var json = wc.DownloadString("https://ec2-18-216-22-23.us-east-2.compute.amazonaws.com/Transfers/Update");
                }
            }
            catch (Exception e)
            {
                return;
            }
        }

        public async Task Update(DbCryptoContext _context)
        {
            isProcessing = true;

            Parser p = new Parser();
            p.Update(_context);
            
            isProcessing = false;
        }
    }
}

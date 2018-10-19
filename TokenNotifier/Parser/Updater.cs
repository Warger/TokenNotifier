using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger logger;
        private bool isProcessing = false;

        public Updater(IServiceScopeFactory serviceScopeFactory, ILogger<Updater> _logger) : base()
        {
            _serviceScopeFactory = serviceScopeFactory;
            logger = _logger;
        }

        public async Task Execute()
        {
            if (isProcessing)
                return;

            try
            {
                /*
                using (WebClient wc = new WebClient())
                {
                  var json = wc.DownloadString("https://ec2-18-216-22-23.us-east-2.compute.amazonaws.com/Transfers/Update");
                }
                */
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    logger.LogDebug("Caling await Update(scope.ServiceProvider) ...");
                    await Update(scope.ServiceProvider);
                }
            }
            catch (Exception e)
            {
                logger.LogDebug("Exception on Update method: "+e.Message);
                isProcessing = false;
                return;
            }
        }

        public async Task Update(IServiceProvider serviceProvider)
        {
            isProcessing = true;
            Parser p = new Parser();
            p.Update(serviceProvider.GetRequiredService<DbCryptoContext>(), logger);
            isProcessing = false;
        }
    }
}

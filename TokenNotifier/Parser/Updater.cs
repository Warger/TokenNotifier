using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task Execute()
        {
            if (isProcessing)
                return;

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                await Update(scope.ServiceProvider);
            }
        }

        public async Task Update(IServiceProvider serviceProvider)
        {
            isProcessing = true;
            var _context = serviceProvider.GetRequiredService<DbCryptoContext>();

            Parser p = new Parser();
            p.Update(_context);
            
            isProcessing = false;
        }
    }
}

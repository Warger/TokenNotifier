using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TokenNotifier.Models;

namespace TokenNotifier.Data
{
    public static class DbInitializer
    {
        public static void Initialize(DbCryptoContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            var users = new User[]
            {
                new User(){Login="Test", Password="Test"},
            };

            foreach (User c in users)
            {
                context.Users.Add(c);
            }
            context.SaveChanges();
        }
    }
}

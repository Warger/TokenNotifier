using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TokenNotifier.Models;
using TokenNotifier.Parser;

namespace TokenNotifier.Controllers
{
    public class HomeController : Controller
    {
        ILogger<HomeController> Logger { get; set; }

        public HomeController(ILogger<HomeController> logger, Updater u)
        {
            //   Updater u = .GetRequiredService<DbCryptoContext>();
            this.Logger = logger;

            RecurringJob.AddOrUpdate(() => u.Execute(), Cron.MinuteInterval(10));
        }

        public IActionResult Index()
        {
            Logger.LogInformation("Home open");
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

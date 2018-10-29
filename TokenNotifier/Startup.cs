using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TokenNotifier.Data;
using Hangfire;
using Hangfire.MySql.Core;
using TokenNotifier.Parser;
using Microsoft.Extensions.Logging;
using ReflectionIT.Mvc.Paging;

namespace TokenNotifier
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<DbCryptoContext>
               (options => options.UseMySql(Configuration.GetConnectionString("TokenNotifierContext")));

            services.AddSingleton<Updater>();

            services.AddHangfire(x => x.UseStorage(new MySqlStorage(Configuration.GetConnectionString("TokenNotifierContext"))));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddPaging();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.C:\Users\warge\Source\Repos\Warger\TokenNotifier\TokenNotifier\Startup.cs
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Create a logging provider based on the configuration information passed through the appsettings.json
            // You can even provide your custom formatting.
            loggerFactory.AddAWSProvider(this.Configuration.GetAWSLoggingConfigSection(),
                formatter: (logLevel, message, exception) => $"[{DateTime.UtcNow}] {logLevel}: {message}");

            // Create a logger instance from the loggerFactory
            var logger = loggerFactory.CreateLogger<Program>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

          //  LoggerFactory.AddLog4Net();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new MyAuthorizationFilter() }
            });
            app.UseHangfireServer();
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

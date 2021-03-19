using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CampaignSync.Api.SyncService.Common;
using CampaignSync.Api.SyncService.Common.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HostService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SetupLog.Log("**********************************************************");
            SetupLog.Log("Initializing App");

            var isService = !(Debugger.IsAttached || args.Contains("--console"));
            var webHostArgs = args.Where(arg => arg != "--console").ToArray();

            string url = new UrlConfiguration().GetAppUrl();
            var configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
              .AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: true)
              .Build();

            SetupLog.Log("Url - got - " + url);

            SetupLog.Log("Now buildwebost");

            var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
            var pathToContentRoot = Path.GetDirectoryName(pathToExe);
            //pathToContentRoot = "";
            var host = BuildWebHost(url, webHostArgs, configuration, pathToContentRoot);
            SetupLog.Log("Host Initialized and running");

            if (isService)
            {
                try
                {
                    host.RunAsService();
                }
                catch (Exception ex)
                {
                    SetupLog.Log("Host Initialized and running" + ex.Message);
                }

            }
            else
            {
                host.Run();
            }

            SetupLog.Log("Host is runinng..");
        }

        public static IWebHost BuildWebHost(string url, string[] args, IConfiguration configuration, string contentPath) =>

            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseContentRoot(contentPath)
                .UseConfiguration(configuration)
                .ConfigureAppConfiguration((hostingcontext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: true);
                })
                .UseKestrel(options =>
                {
                    options.Limits.MaxRequestBodySize = null;
                })
                .UseUrls(url)
                .Build();


    }
}

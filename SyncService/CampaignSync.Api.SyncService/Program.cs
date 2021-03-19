using CampaignSync.Api.SyncService.Common;
using CampaignSync.Api.SyncService.Common.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CampaignSync.Api.SyncService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string url = new UrlConfiguration().GetAppUrl();
            BuildWebHost(url, args).Run();
        }

        public static IWebHost BuildWebHost(string url, string[] args) =>
             WebHost.CreateDefaultBuilder(args)
              .UseStartup<Startup>()
              //.UseContentRoot(Directory.GetCurrentDirectory())
              .ConfigureLogging((hostingContext, logging) =>
              {
                  logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                  logging.AddConsole();
                  logging.AddDebug();
              })
              .UseKestrel(options =>
              {
                  options.AddServerHeader = false;
                  options.Limits.MaxRequestBodySize = null;
              })
              .UseUrls(url)
            .UseDefaultServiceProvider(options =>
                    options.ValidateScopes = false)
              .Build();
    }
}

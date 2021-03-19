using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace CampaignSync.Api.SyncService.Common
{
    public class UrlConfiguration
    {
        public string GetAppUrl()
        {
            var enviornment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            enviornment = enviornment == null ? "Development" : enviornment;

            string filePath = Directory.GetCurrentDirectory() + $"/appsettings.{enviornment}.json";

            var builder = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddJsonFile(filePath, optional: true, reloadOnChange: true)
               .AddEnvironmentVariables();

            var configuration = builder.Build();
            string url = "";

            if (configuration["MySettings:DefaultSettings:AppUrls:ApiUrl"] != null)
            {
                url = configuration["MySettings:DefaultSettings:AppUrls:ApiUrl"];
            }

            return url;
        }
    }
}

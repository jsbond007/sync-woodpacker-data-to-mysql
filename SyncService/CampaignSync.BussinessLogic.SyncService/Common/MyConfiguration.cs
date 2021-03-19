using Microsoft.Extensions.Options;


namespace CampaignSync.BussinessLogic.SyncService.Common
{
    public class MyConfiguration
    {
        public MySettings settings { get; set; }
        public MyConfiguration(IOptions<MySettings> options)
        {
            this.settings = options.Value;
        }

    }

    public class MySettings
    {
        public DefaultSettings DefaultSettings { get; set; } = new DefaultSettings();
        public string CampaignURL { get; set; }
        public string ProcessTime { get; set; }

    }

    public class DefaultSettings
    {
        public string ValidIssuer { get; set; }
        public string ValidAudience { get; set; }
        public ApplicationUrls AppUrls { get; set; } = new ApplicationUrls();
    }

    public class ApplicationUrls
    {
        public string ApiUrl { get; set; }
        public string UIUrl { get; set; }
    }

}

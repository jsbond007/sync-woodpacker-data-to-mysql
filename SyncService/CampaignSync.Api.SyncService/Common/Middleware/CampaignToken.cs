using CampaignSync.DataAccess.SyncService.Tables;

namespace CampaignSync.Api.SyncService.Common.Middleware
{
    public class CampaignToken
    {
        public string AccessToken { get; set; }

        public int ExpiresIn { get; set; }

        public IUser UserDetails { get; set; }
    }
}

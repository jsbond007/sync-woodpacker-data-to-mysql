
using ServiceStack.DataAnnotations;

namespace CampaignSync.DataAccess.SyncService.Tables
{
    [Alias("user")]
    public class User: IUser
    {
        public int id { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public string name { get; set; }
    }

    public interface IUser
    {
        int id { get; set; }
        string userName { get; set; }
        string password { get; set; }
        string name { get; set; }
    }
}

using ServiceStack.DataAnnotations;
using System.Collections.Generic;

namespace CampaignSync.DataAccess.SyncService.Tables
{
    [Alias("company")]
    public class Company : ICompany
    {
        public Company()
        {
            this.Campaigns = new List<Campaign>();
        }
        [Alias("companyid")]
        public int CompanyId { get; set; }
        [Alias("companyname")]
        public string CompanyName { get; set; }
        [Alias("apiKey")]
        public string APIKey { get; set; }
        [Ignore]
        public List<Campaign> Campaigns { get; set; }
    }

    public interface ICompany
    {
        int CompanyId { get; set; }
        string CompanyName { get; set; }
        string APIKey { get; set; }
        List<Campaign> Campaigns { get; set; }
    }
}

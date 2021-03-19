
using ServiceStack.DataAnnotations;

namespace CampaignSync.DataAccess.SyncService.Tables
{
    [Alias("campaign")]
    public class Campaign : ICampaign
    {
        public int id { get; set; }
        public int company_id { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string created { get; set; }
        public string from_name { get; set; }
        public bool gdpr_unsubscribe { get; set; }
        public string folder_name { get; set; }
        public int folder_id { get; set; }
        public string from_email { get; set; }
        public int per_day { get; set; }
        public string bcc { get; set; }
        public string cc { get; set; }
        public int interested { get; set; }
        public int not_interested { get; set; }
        public int maybe_later { get; set; }
        public int replied { get; set; }
        public int autoreplied { get; set; }
        public int bounced { get; set; }
        [Alias("check_stats")]
        public int check { get; set; }
        public int clicked { get; set; }
        public int delivery { get; set; }
        public int invalid { get; set; }
        public int opened { get; set; }
        public int prospects { get; set; }
        public int queue { get; set; }
        public int sent { get; set; }
        [Ignore]
        public stats stats { get; set; }

        [Ignore]
        public decimal OpenPercentage
        {
            get
            {
                return (decimal)delivery == 0 ? 0 : (decimal)this.opened * 100 / (decimal)this.delivery;
            }
        }

        [Ignore]

        public decimal RepliedPercentage
        {
            get
            {
                return (decimal)delivery == 0 ? 0 : (decimal)this.replied * 100 / (decimal)this.delivery;
            }
        }
        public void UpdateFromStats(stats stats)
        {
            if (stats != null)
            {
                this.interested = stats.interested;
                this.not_interested = stats.not_interested;
                this.maybe_later = stats.maybe_later;
                this.replied = stats.replied;
                this.bounced = stats.bounced;
                this.check = stats.check;
                this.clicked = stats.clicked;
                this.delivery = stats.delivery;
                this.invalid = stats.invalid;
                this.opened = stats.opened;
                this.prospects = stats.prospects;
                this.queue = stats.queue;
                this.sent = stats.sent;
            }
        }

    }

    public class stats
    {
        public int interested { get; set; }
        public int not_interested { get; set; }
        public int maybe_later { get; set; }
        public int replied { get; set; }
        public int autoreplied { get; set; }
        public int bounced { get; set; }
        public int check { get; set; }
        public int clicked { get; set; }
        public int delivery { get; set; }
        public int invalid { get; set; }
        public int opened { get; set; }
        public int prospects { get; set; }
        public int queue { get; set; }
        public int sent { get; set; }
    }
    public interface ICampaign
    {
        int id { get; set; }
        int company_id { get; set; }
        string name { get; set; }
        string status { get; set; }
        string created { get; set; }
        string from_name { get; set; }
        bool gdpr_unsubscribe { get; set; }
        string folder_name { get; set; }
        int folder_id { get; set; }
        string from_email { get; set; }
        int per_day { get; set; }
        string bcc { get; set; }
        string cc { get; set; }
        int interested { get; set; }
        int not_interested { get; set; }
        int maybe_later { get; set; }
        int replied { get; set; }
        int autoreplied { get; set; }
        int bounced { get; set; }
        int check { get; set; }
        int clicked { get; set; }
        int delivery { get; set; }
        int invalid { get; set; }
        int opened { get; set; }
        int prospects { get; set; }
        int queue { get; set; }
        int sent { get; set; }
        decimal RepliedPercentage { get; }
        decimal OpenPercentage { get; }
    }
}

using CampaignSync.BussinessLogic.SyncService.Common;
using CampaignSync.DataAccess.SyncService.Common;
using CampaignSync.DataAccess.SyncService.Tables;
using ServiceStack.OrmLite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CampaignSync.BussinessLogic.SyncService.Repositories
{
    public interface ICampaignRepository
    {
        Task AddOrUpdate(Campaign campaign, int companyId);
        Task<IEnumerable<ICompany>> Get(string name);
    }
    public class CampaignRepository : BaseRepository<Campaign>, ICampaignRepository
    {
        public CampaignRepository(BaseValidationErrorCodes errorCodes,
            DatabaseContext dbContext
            )
            : base(errorCodes, dbContext)
        {
        }

        public async Task<ICampaign> Get(int campId)
        {
            return await this.Connection.SingleAsync<Campaign>(i => i.id == campId);
        }

        public async Task<IEnumerable<ICompany>> Get(string name)
        {
            string typeOperator = " like '%" + (name == null ? "NULL" : name.ToString().Replace("'", "''")) + "%'";
            var query = this.Connection.From<Company>()
                        .LeftJoin<Company, Campaign>((co, ca) => co.CompanyId == ca.company_id)
                        .Select<Company, Campaign>((co, ca) => new
                        {
                            co,
                            ca
                        });

            if (!string.IsNullOrEmpty(name))
                query.WhereExpression = $"where company.companyname {typeOperator} OR campaign.name {typeOperator}";

            var dynamicResult = await this.Connection.SelectAsync<dynamic>(query);
            var company = Mapper<Company>.MapList(dynamicResult);

            var result = company.Select(x => new
            {
                CompanyId = x.CompanyId,
                CompanyName = x.CompanyName,
                APIKey = x.APIKey
            }).GroupBy(i => new
            {
                i.CompanyId,
                i.CompanyName,
                i.APIKey,

            }).Select(i => new Company
            {
                CompanyId = i.Key.CompanyId,
                CompanyName = i.Key.CompanyName,
                APIKey = i.Key.APIKey,
            }).ToList().OrderBy(i => i.CompanyName);

            var campains = Mapper<Campaign>.MapList(dynamicResult);


            foreach (var item in result)
            {
                var camps = campains.Where(i => i.company_id == item.CompanyId).OrderBy(i => i.name);
                item.Campaigns.AddRange(camps);
            }
            return result;
        }

        public async Task AddOrUpdate(Campaign campaign, int companyId)
        {
            try
            {
                if (campaign != null)
                {
                    campaign.company_id = companyId;
                    campaign.UpdateFromStats(campaign.stats);
                    var result = await this.Get(campaign.id);

                    if (result != null)
                        await this.Connection.UpdateAsync(campaign);
                    else
                        await this.Connection.InsertAsync(campaign);
                }

            }
            catch
            {

                throw;
            }
        }

    }
}

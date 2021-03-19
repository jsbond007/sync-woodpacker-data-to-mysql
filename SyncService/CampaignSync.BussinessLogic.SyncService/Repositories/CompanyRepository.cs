using CampaignSync.BussinessLogic.SyncService.Common;
using CampaignSync.DataAccess.SyncService.Common;
using CampaignSync.DataAccess.SyncService.Tables;
using ServiceStack.OrmLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignSync.BussinessLogic.SyncService.Repositories
{
    public interface ICompanyRepository
    {
        Task<IEnumerable<ICompany>> Get();
    }
    public class CompanyRepository : BaseRepository<Company>, ICompanyRepository
    {
        public CompanyRepository(BaseValidationErrorCodes errorCodes,
            DatabaseContext dbContext
            )
            : base(errorCodes, dbContext)
        {
        }

        public async Task<IEnumerable< ICompany>> Get()
        {
            return await this.Connection.SelectAsync<Company>();
        }
    }
}

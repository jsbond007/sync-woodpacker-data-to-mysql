using CampaignSync.BussinessLogic.SyncService.Common;
using CampaignSync.DataAccess.SyncService.Common;
using CampaignSync.DataAccess.SyncService.Tables;
using ServiceStack.OrmLite;
using System.Threading.Tasks;

namespace CampaignSync.BussinessLogic.SyncService.Repositories
{
    public interface IUserRepository
    {
        Task<IUser> CheckLogin(string username, string password);
    }
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(BaseValidationErrorCodes errorCodes,
            DatabaseContext dbContext
            )
            : base(errorCodes, dbContext)
        {
        }

        public async Task<IUser> CheckLogin(string username, string password)
        {
            var result = await this.Connection.SingleAsync<User>(i => i.userName == username && i.password == password);
            if (result != null)
                result.password = null;
            return result;
        }

    }
}

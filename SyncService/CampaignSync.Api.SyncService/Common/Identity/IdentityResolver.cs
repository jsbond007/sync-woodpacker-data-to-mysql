using CampaignSync.BussinessLogic.SyncService.Repositories;
using CampaignSync.DataAccess.SyncService.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace CampaignSync.Api.SyncService.Common.Identity
{
    public class IdentityResolver
    {

        private IUserRepository _userRepository;
        public IdentityResolver(IUserRepository userRepository
            )
        {
            this._userRepository = userRepository;
        }

        public async Task<ClaimsIdentity> CheckUserLogin(string userName, string password)
        {
            IUser user = null;

            user = await this._userRepository.CheckLogin(userName, password);
            if (!string.IsNullOrEmpty(user.userName))
            {
                CampaignIdentity myIdentity = new CampaignIdentity(new GenericIdentity(
                                                        user.userName),
                                                        new List<Claim>(),
                                                        "Standard", "name", "role",
                                                        user);
                return myIdentity;
            }

            return null;
        }
    }
}

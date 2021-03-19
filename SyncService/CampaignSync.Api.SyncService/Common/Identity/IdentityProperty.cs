using CampaignSync.DataAccess.SyncService.Tables;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CampaignSync.Api.SyncService.Common.Identity
{
    public class IdentityProperty:IUser
    {
        public IdentityProperty(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor != null
                && httpContextAccessor.HttpContext != null
                && httpContextAccessor.HttpContext.User != null
                && httpContextAccessor.HttpContext.User.Identity != null
                )
            {
                var identity = (httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity);
                var idClaim = identity.Claims.FirstOrDefault(i => i.Type == "id");
                if (identity != null && idClaim != null)
                {
                    this.SetIdentiyFromClaims(identity);
                }
            }
        }
        public void SetIdentiyFromClaims(ClaimsIdentity identity)
        {
            this.id = Convert.ToInt32(identity.Claims.FirstOrDefault(i => i.Type == "id").Value);
            this.userName = identity.Claims.FirstOrDefault(i => i.Type == "userName").Value;
            this.name = identity.Claims.FirstOrDefault(i => i.Type == "name").Value;
        }

        public int id { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public string name { get; set; }
    }
}

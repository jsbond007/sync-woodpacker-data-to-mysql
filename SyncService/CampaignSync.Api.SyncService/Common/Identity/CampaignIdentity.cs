using CampaignSync.DataAccess.SyncService.Tables;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;

namespace CampaignSync.Api.SyncService.Common.Identity
{
    public class CampaignPrincipal : ClaimsPrincipal
    {
        public CampaignPrincipal(CampaignIdentity identity) : base()
        {
            this._identity = identity;
            identity.Principal = this;
            this.AddIdentity(identity);

        }

        private IIdentity _identity;
        public override IIdentity Identity
        {
            get
            {
                return _identity;
            }
        }

    }
    public class CampaignIdentity : ClaimsIdentity
    {
        public CampaignIdentity(IIdentity identity,
               IEnumerable<Claim> claims,
               string authenticationType,
               string nameType,
               string roleType,
                IUser user = null) : base(identity, claims, authenticationType, nameType, roleType)
        {
            if (user != null)
            {
                this.User = user;
                this.AddUserClaims(user);
            }
        }
        private IUser _user;

        public CampaignPrincipal Principal { get; set; }

        public IUser User
        {
            get { return _user; }
            set { _user = value; }
        }

        public void AddUserClaims(IUser user)
        {

            this.AddClaim(new Claim("id", user.id.ToString()));
            this.AddClaim(new Claim("userName", string.IsNullOrEmpty(user.userName) ? "" : user.userName.ToString()));
            this.AddClaim(new Claim("name", user.name.ToString()));

        }

        public static CampaignIdentity ToIdentity(ClaimsIdentity identity,
            HttpContext httpContext)
        {
            CampaignIdentity myIdentity = new CampaignIdentity(identity,
                                                    identity.Claims,
                                                    identity.AuthenticationType,
                                                    identity.NameClaimType,
                                                    identity.RoleClaimType
                                                    );

            return myIdentity;

        }
    }
}

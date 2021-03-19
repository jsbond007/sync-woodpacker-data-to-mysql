using CampaignSync.DataAccess.SyncService.Tables;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CampaignSync.Api.SyncService.Common.Identity
{
    public class CampaignJwtToken: JwtSecurityToken
    {
        public CampaignJwtToken(CampaignJwtPayLoad payload) : base(new JwtHeader(), payload)
        {

        }

        public CampaignJwtToken(IUser employees, string issuer, string audience, IEnumerable<Claim> claims, DateTime? notBefore, DateTime? expires, SigningCredentials signingCredentials) : base(new JwtHeader(signingCredentials), new CampaignJwtPayLoad(employees, issuer, audience, claims, notBefore, expires))
        {

        }
    }

    public class CampaignJwtPayLoad : JwtPayload
    {
        public CampaignJwtPayLoad(IUser user, string issuer, string audience, IEnumerable<Claim> claims, DateTime? notBefore, DateTime? expires) : base(issuer, audience, claims, notBefore, expires)
        {
            this._user = user;
        }
        private IUser _user;

        public IUser User
        {
            get { return _user; }
            private set { _user = value; }
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CampaignSync.Api.SyncService.Common.Identity
{
    public class CampaignJwtTokenHandler: JwtSecurityTokenHandler
    {
        private HttpContext _httpContext;
        public CampaignJwtTokenHandler(HttpContextAccessor httpContextAccessor)
        {
            this._httpContext = httpContextAccessor.HttpContext;
        }

        public override ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {

            ClaimsPrincipal principal = base.ValidateToken(token, validationParameters, out validatedToken);
            var parsedToken = base.ReadJwtToken(token);
            CampaignIdentity identity = CampaignIdentity.ToIdentity(principal.Identity as ClaimsIdentity, this._httpContext);
            CampaignPrincipal ndCrmPrincipal = new CampaignPrincipal(identity);

            return ndCrmPrincipal;

        }
    }
}

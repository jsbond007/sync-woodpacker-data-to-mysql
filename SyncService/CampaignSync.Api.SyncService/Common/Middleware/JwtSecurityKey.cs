using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CampaignSync.Api.SyncService.Common.Middleware
{
    public static class JwtSecurityKey
    {
        public readonly static string SecretKey = "agfsSHGJDF123454@3^%@^#%@(@DFSHDGFkdfdshkjgf486712312";

        public static SymmetricSecurityKey Create()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));
        }

        public static SigningCredentials GetSigningCredentials()
        {
            return new SigningCredentials(Create(), SecurityAlgorithms.HmacSha256);
        }
    }
}

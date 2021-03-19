using CampaignSync.Api.SyncService.Common.Identity;
using CampaignSync.Api.SyncService.Common.Middleware;
using CampaignSync.BussinessLogic.SyncService.Repositories;
using CampaignSync.DataAccess.SyncService.Tables;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace CampaignSync.Api.SyncService.TokenProvider
{
    public class TokenProviderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenProviderOptions _options;
        private readonly ILogger _logger;
        private readonly JsonSerializerSettings _serializerSettings;

        public TokenProviderMiddleware(
            RequestDelegate next,
            IOptions<TokenProviderOptions> options,
            ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<TokenProviderMiddleware>();

            _options = options.Value;
            ThrowIfInvalidOptions(_options);

            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            if (options.Value.IdentityResolver == null)
            {

            }
        }

        public Task Invoke(Microsoft.AspNetCore.Http.HttpContext context)
        {
            // If the request path doesn't match, skip
            if (!context.Request.Path.Equals(_options.Path, StringComparison.Ordinal))
            {
                return _next(context);
            }

            // Request must be POST with Content-Type: application/x-www-form-urlencoded
            if (!context.Request.Method.Equals("POST")
               || !context.Request.HasFormContentType)
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsync("Bad request.");
            }

            _logger.LogInformation("Handling request: " + context.Request.Path);

            return ValidateLogin(context);
        }

        private async Task ValidateLogin(Microsoft.AspNetCore.Http.HttpContext context)
        {
            var Password = context.Request.Form["Password"];
            var Email = context.Request.Form["Email"];

            var userRepository = (IUserRepository)context.RequestServices.GetService(typeof(IUserRepository));

            ClaimsIdentity identity = null;
            IUser user = null;

            user = await userRepository.CheckLogin(Email, Password);
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                //await userRepository.IncreaseAttempts(username);
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("username and password required");
                return;
            }
            if (user == null)
            {
                //await userRepository.IncreaseAttempts(username);
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Email and Password are wrong");
                return;
            }
            identity = await this._options.IdentityResolver(Email, Password);
            if (identity == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("incorrect username or password");
                return;
            }
            identity = new CampaignIdentity(new GenericIdentity(user.userName), new List<Claim>(), "Standard", "name", "role", user);

            var response = await this.GenerateToken(Email, user, identity);

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response, _serializerSettings));
        }

        private async Task<CampaignToken> GenerateToken(string username, IUser user, ClaimsIdentity identity)
        {
            var now = DateTime.UtcNow;

            /* Specifically add the jti (nonce), iat (issued timestamp), and sub (subject/user) claims. */
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, await _options.NonceGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(now).ToString(), ClaimValueTypes.Integer64),
            };

            /* You can add other claims here, if you want: */
            if (identity != null && identity.Claims.Count() > 0)
            {
                claims.AddRange(identity.Claims);
            }

            /* Create the JWT and write it to a string */
            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: DateTime.Now.AddMonths(2),
                signingCredentials: _options.SigningCredentials
                );

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new CampaignToken
            {
                AccessToken = encodedJwt,
                ExpiresIn = 60,
                UserDetails = user
            };

            return response;
        }

        private static void ThrowIfInvalidOptions(TokenProviderOptions options)
        {
            if (string.IsNullOrEmpty(options.Path))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Path));
            }

            if (string.IsNullOrEmpty(options.Issuer))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Issuer));
            }

            if (string.IsNullOrEmpty(options.Audience))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Audience));
            }

            if (options.Expiration == TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(TokenProviderOptions.Expiration));
            }

            if (options.IdentityResolver == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.IdentityResolver));
            }

            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.SigningCredentials));
            }

            if (options.NonceGenerator == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.NonceGenerator));
            }
        }

        /// <summary>
        /// Get this datetime as a Unix epoch timestamp (seconds since Jan 1, 1970, midnight UTC).
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <returns>Seconds since Unix epoch.</returns>
        public static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
    }

    public static class TokenProviderAppBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="TokenProviderMiddleware"/> middleware to the specified <see cref="IApplicationBuilder"/>, which enables token generation capabilities.
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="options">A  <see cref="TokenProviderOptions"/> that specifies options for the middleware.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseSimpleTokenProvider(this IApplicationBuilder app, TokenProviderOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<TokenProviderMiddleware>(Options.Create(options));
        }
    }
}

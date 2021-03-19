using CampaignSync.Api.SyncService.Common.Identity;
using CampaignSync.Api.SyncService.Common.Middleware;
using CampaignSync.BussinessLogic.SyncService.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CampaignSync.Api.SyncService.Common
{
    public static class ConfigureJWTAuthServices
    {
        private readonly static string secretKey = "JWT!Secret#As#perMYChoiCE!123";
        public readonly static SymmetricSecurityKey SigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
        {
            var sp = services.BuildServiceProvider();
            var httpContextAccessor = sp.GetService<HttpContextAccessor>();
            CampaignJwtTokenHandler jwtHandler = new CampaignJwtTokenHandler(httpContextAccessor);

            MyConfiguration Configuration = sp.GetService<MyConfiguration>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options => {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            ValidIssuer = Configuration.settings.DefaultSettings.ValidIssuer,
                            ValidAudience = Configuration.settings.DefaultSettings.ValidAudience,
                            IssuerSigningKey = JwtSecurityKey.Create()
                        };

                        options.Events = new JwtBearerEvents
                        {
                            OnAuthenticationFailed = context =>
                            {
                                Console.WriteLine("OnAuthenticationFailed: " + context.Exception.Message);
                                return Task.CompletedTask;
                            },
                            OnTokenValidated = context =>
                            {
                                Console.WriteLine("OnTokenValidated: " + context.SecurityToken);
                                return Task.CompletedTask;
                            }
                        };

                        options.SecurityTokenValidators.RemoveAt(0);
                        options.SecurityTokenValidators.Add(jwtHandler);
                    });

            return services;
        }
    }
}

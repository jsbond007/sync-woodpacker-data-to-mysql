using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CampaignSync.Api.SyncService.Common.Identity;
using CampaignSync.Api.SyncService.Common.Middleware;
using CampaignSync.Api.SyncService.TokenProvider;
using CampaignSync.BussinessLogic.SyncService.Common;
using CampaignSync.BussinessLogic.SyncService.Services;
using CampaignSync.DataAccess.SyncService.Tables;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CampaignSync.Api.SyncService
{
    public class Startup
    {

        public Startup(IHostingEnvironment env, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            Configuration = configuration;
            CurrentEnvironment = env;
        }

        public Microsoft.Extensions.Configuration.IConfiguration Configuration { get; }
        private IHostingEnvironment CurrentEnvironment { get; set; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCommonService(Configuration);
            services.AddSingleton<IdentityResolver>();
            services.AddTransient<IUser, IdentityProperty>();
            services.AddCampainSyncServiceRepositories();
            services.AddCampainSyncServices();
            services.AddMvc();
            services.AddSwaggerService();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            SetupLog.Log("Configuring log");
            string date = DateTime.UtcNow.ToLongDateString();
            loggerFactory.AddFile($"Logs/{date}.txt");

            SetupLog.Log("Configure log");
            if (env.IsDevelopment())
            {
                //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
                //loggerFactory.AddDebug();
                app.UseDeveloperExceptionPage();
            }

            app.Use((r, s) =>
            {
                var headers = r.Request.Headers;
                if (headers.ContainsKey("origin"))
                {
                    string headerOrigin = headers["origin"];
                    Console.WriteLine("Starting Request - from - " + headerOrigin);
                }

                return s.Invoke();
            });
            ServiceStack.Licensing.RegisterLicense(@"6058-e1JlZjo2MDU4LE5hbWU6Q29uUXN5cyBJVCBQdnQuIEx0ZC4sVHlwZTpPcm1MaXRlQnVzaW5lc3MsSGFzaDpsbHVmU3cxaGtHZ1ZhY2xMTnZkTWxRR0w3Rm5TY3Q2U1oydVhQR3VCQkF1SVZHL1k3TXozZllWZ0Jod25pMlU1VnVDYXBXRUdXOXA2akNGbjE2QXNYMVIyMExyV1FSSHNXdTYxK0dDU2MxU0tzVzNMZzBYK3E5WGFqZzAyL0RwTzJnRzJQSCtxbDE5aytieERVbEd0MTlqRDdtdUFlYjNOcm4wckUxMEhpWU09LEV4cGlyeToyMDE5LTA0LTI1fQ==");
            app.UseCors(builder =>
               builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials()
               );

            DefaultFilesOptions options = new DefaultFilesOptions();
            options.DefaultFileNames.Add("index.html");
            options.DefaultFileNames.Add("Index.html");
            app.UseDefaultFiles(options);

            app.UseStaticFiles();
            var scope = app.ApplicationServices.CreateScope();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            var syncService = app.ApplicationServices.GetService<ISyncService>();
            syncService.Timer();

            var myConfiguration = scope.ServiceProvider.GetRequiredService<MyConfiguration>();
            var identityResolver = scope.ServiceProvider.GetRequiredService<IdentityResolver>();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "LOGISTIC API V2");
            });
            app.UseAuthentication();
            app.UseSimpleTokenProvider(new TokenProviderOptions
            {
                Path = "/token",
                Audience = myConfiguration.settings.DefaultSettings.ValidAudience,
                Issuer = myConfiguration.settings.DefaultSettings.ValidIssuer,
                SigningCredentials = JwtSecurityKey.GetSigningCredentials(),
                IdentityResolver = identityResolver.CheckUserLogin,
                Expiration = DateTime.UtcNow.AddDays(30).TimeOfDay
            });


            app.UseMvc();
        }
    }
}


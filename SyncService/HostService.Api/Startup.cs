using System;
using CampaignSync.Api.SyncService.Common.Identity;
using CampaignSync.Api.SyncService.Common.Middleware;
using CampaignSync.Api.SyncService.TokenProvider;
using CampaignSync.BussinessLogic.SyncService.Common;
using CampaignSync.BussinessLogic.SyncService.Services;
using CampaignSync.DataAccess.SyncService.Tables;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HostService.Api
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
            })
			;
            //ServiceStack.Licensing.RegisterLicense(@"provide your license key");
			
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CampaignSync Api V2");
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

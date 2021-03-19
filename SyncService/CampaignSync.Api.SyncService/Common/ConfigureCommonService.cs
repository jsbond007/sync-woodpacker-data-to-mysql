using CampaignSync.Api.SyncService.Common;
using CampaignSync.BussinessLogic.SyncService.Common;
using CampaignSync.BussinessLogic.SyncService.Services;
using CampaignSync.DataAccess.SyncService.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.MySql;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ConfigureCommonService
    {
        public static IConfiguration Configuration { get; private set; }
        private static string _connectionString;
        public static IServiceCollection AddCommonService(this IServiceCollection services,
            IConfiguration configuration)
        {
            Configuration = configuration;
            services.AddSingleton<IConfiguration>(Configuration);

            services.AddOptions();


            var section = Configuration.GetSection("MySettings");

            services.Configure<MySettings>(section);
            services.AddSingleton<MyConfiguration>();

            _connectionString = Configuration.GetConnectionString("DefaultConnection");

            var sqlDbFactory = new OrmLiteConnectionFactory(_connectionString, MySqlDialectProvider.Instance);
            services.AddSingleton<OrmLiteConnectionFactory>(sqlDbFactory);

            services.AddSingleton<HttpContextAccessor>();
            services.AddSingleton<BaseValidationErrorCodes>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ICustomExceptionService, CustomExceptionService>();

            services.AddTransient<DatabaseContext>();
            services.AddJwtAuthentication();

            services.AddCors();

            services.AddMvc();

            return services;
        }

        public static void AddSwaggerService(this IServiceCollection services)
        {
            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(options =>
            {
                options.OperationFilter<AuthorizationHeaderParameterOperationFilter>();
                options.SwaggerDoc("v1", new Info
                {
                    Title = "CampaigmSync",
                    Version = "v1",
                    Description = "REST API for CampaigmSync",
                    TermsOfService = "None"
                });
                options.DescribeAllEnumsAsStrings();
            });
        }

        public static void AddCustomMvc(this IServiceCollection services)
        {
            //UserStampFilterAttribute


            services.AddMvc(config =>
            {
                /* right now injected IdentityUser globally and using in BaseRepository and handling there
                *config.Filters.Add(new UserStampFilterAttribute());
                */

            })

            .AddJsonOptions(opt =>
            {
                opt.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                var resolver = opt.SerializerSettings.ContractResolver;
                if (resolver != null)
                {
                    var res = resolver as DefaultContractResolver;
                    res.NamingStrategy = null;
                }

            }).AddControllersAsServices();
        }
    }
    public class AuthorizationHeaderParameterOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
            var isAuthorized = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is AuthorizeFilter);
            var allowAnonymous = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is IAllowAnonymousFilter);

            if (isAuthorized && !allowAnonymous)
            {
                if (operation.Parameters == null)
                    operation.Parameters = new List<IParameter>();

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "Authorization",
                    In = "header",
                    Description = "access token",
                    Required = true,
                    Type = "string"
                });
            }
        }
    }
}

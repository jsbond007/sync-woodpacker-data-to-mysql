
using CampaignSync.BussinessLogic.SyncService.Repositories;
using CampaignSync.BussinessLogic.SyncService.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CampainSyncRepositoryCollectionExtension
    {
        public static IServiceCollection AddCampainSyncServiceRepositories(this IServiceCollection services)
        {
            services.AddTransient<ICompanyRepository, CompanyRepository>();
            services.AddTransient<ICampaignRepository, CampaignRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            return services;
        }
        public static IServiceCollection AddCampainSyncServices(this IServiceCollection services)
        {

            services.AddScoped<ISyncService, CampaignSyncService>();
            return services;
        }
    }
}

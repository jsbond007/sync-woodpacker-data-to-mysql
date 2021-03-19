using Microsoft.AspNetCore.Mvc;


namespace CampaignSync.Api.SyncService.Common
{
    public abstract class BaseRepositoryController<TRepo> : ControllerBase where TRepo : class
    {
        public TRepo Repository { get; set; }
        public BaseRepositoryController(TRepo repository)
        {
            this.Repository = repository;
        }
    }
}

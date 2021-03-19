
using CampaignSync.Api.SyncService.Common;
using CampaignSync.BussinessLogic.SyncService.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CampaignSync.Api.SyncService.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class CampaignController : BaseRepositoryController<ICampaignRepository>
    {
        public CampaignController(ICampaignRepository repository) : base(repository)
        {
        }

        /// <summary>
        /// To Get All Records from NotificationTemplate Table.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> Get(string name)
        {
            try
            {
                var result = await this.Repository.Get(name);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

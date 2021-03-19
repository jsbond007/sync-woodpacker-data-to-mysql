
using CampaignSync.BussinessLogic.SyncService.Common;
using CampaignSync.BussinessLogic.SyncService.Repositories;
using CampaignSync.DataAccess.SyncService.Tables;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace CampaignSync.BussinessLogic.SyncService.Services
{
    public interface ISyncService
    {
        void Timer();
    }
    public class CampaignSyncService : ISyncService
    {
        bool alreadyRunning = false;
        private ICompanyRepository _companyRepository;
        private ICampaignRepository _campaignRepository;
        private MySettings _settings;
        private ICustomExceptionService _customExceptionService;
        public CampaignSyncService(ICompanyRepository companyRepository, MyConfiguration configuration, ICampaignRepository campaignRepository, ICustomExceptionService customExceptionService)
        {
            this._companyRepository = companyRepository;
            this._settings = configuration.settings;
            this._campaignRepository = campaignRepository;
            _customExceptionService = customExceptionService;
        }

        private static System.Timers.Timer timer;

        public void Timer()
        {

            timer = new System.Timers.Timer();
            timer.Interval = Convert.ToInt32(_settings.ProcessTime);
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;


        }
        private async void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (!alreadyRunning)
                    await SyncedDate();
            }
            catch (System.Exception ex)
            {
                _customExceptionService.LogException(ex);
            }

        }
        public async Task SyncedDate()
        {
            try
            {

                alreadyRunning = true;
                var companies = await this._companyRepository.Get();

                foreach (var result in companies)
                {
                    var campaign = await this.CampRequest("rest/v1/campaign_list", result.APIKey);
                    foreach (var item in campaign)
                    {
                        var campList = await this.CampRequest("rest/v1/campaign_list?id=" + item.id, result.APIKey);
                        await this._campaignRepository.AddOrUpdate(campList.FirstOrDefault(), result.CompanyId);
                    }

                }
            }
            catch (System.Exception ex)
            {
                _customExceptionService.LogException(ex);
            }
            finally
            {
                alreadyRunning = false;
            }
        }

        public async Task<List<Campaign>> CampRequest(string requestUrl, string apiKey)
        {
            List<Campaign> campaigns = new List<Campaign>();
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(this._settings.CampaignURL + requestUrl);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Accept = "*/*";
                httpWebRequest.Method = "GET";
                httpWebRequest.Headers.Add("Authorization", apiKey);

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    string result = streamReader.ReadToEnd();
                    campaigns = JsonConvert.DeserializeObject<List<Campaign>>(result);
                }
            }
            catch (Exception ex)
            {
                _customExceptionService.LogException(ex);

            }
            return campaigns;

        }
    }
}

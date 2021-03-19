using CampaignSync.DataAccess.SyncService.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignSync.Api.SyncService.Controllers
{
    [Route("api")]
    public class LoginController : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromBody]LoginModel loginModel)
        {
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri("http://" + this.Request.Host.Value);
            List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("Email", loginModel.UserName.ToString()));
            values.Add(new KeyValuePair<string, string>("Password", loginModel.Password.ToString()));
            System.Net.Http.FormUrlEncodedContent content = new System.Net.Http.FormUrlEncodedContent(values);
            var response = await client.PostAsync("/token", content);

            string result = response.Content.ReadAsStringAsync().Result;

            object dataObject = result;
            try
            {
                dataObject = Newtonsoft.Json.Linq.JObject.Parse(result);
            }

            catch
            {
            }

            return StatusCode((int)response.StatusCode, dataObject);
        }
    }
}

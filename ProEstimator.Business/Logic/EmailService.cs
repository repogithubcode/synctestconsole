using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using ProEstimatorData;

namespace ProEstimator.Business.Logic
{
    public class EmailService
    {
        public async Task<FunctionResult> Delete(string email)
        {
            string apiKey = ConfigurationManager.AppSettings["SendGridAPI"];
            var client = new SendGrid.SendGridClient(apiKey);
            string queryParams = "{\"email\":\"" + email + "\"}";
            string url = "https://api.sendgrid.com/api/unsubscribes.delete.json";
            var response = await client.RequestAsync(SendGrid.BaseClient.Method.POST, null, queryParams, url);
            if (response.IsSuccessStatusCode)
            {
                return new FunctionResult();
            }
            else
            {
                return new FunctionResult(response.StatusCode.ToString());
            }
        }

        public async Task<FunctionResult> Add(string email)
        {
            string apiKey = ConfigurationManager.AppSettings["SendGridAPI"];
            var client = new SendGrid.SendGridClient(apiKey);
            string queryParams = "{\"email\":\"" + email + "\"}";
            string url = "https://api.sendgrid.com/api/unsubscribes.add.json";
            var response = await client.RequestAsync(SendGrid.BaseClient.Method.POST, null, queryParams, url);
            if (response.IsSuccessStatusCode)
            {
                return new FunctionResult();
            }
            else
            {
                return new FunctionResult(response.StatusCode.ToString());
            }
        }
    }
}

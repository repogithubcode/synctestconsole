using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Proestimator.Models.LicensePlateReader;
using ProEstimatorData;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.Logic
{
    public class LicensePlateReaderService
    {
        public static async Task<PlateReaderResponse> ReadLicensePlateImage(PlateReaderRequest request)
        {
            try
            {
                var apiEndpoint = ConfigurationManagerUtilities.GetAppSetting("LicensePlateReaderApiEndpoint");
                var apiToken = ConfigurationManagerUtilities.GetAppSetting("LicensePlateReaderApiToken");

                if (string.IsNullOrWhiteSpace(apiEndpoint))
                    return null;

                using (var client = new HttpClient())
                {
                    var jsonSerializerSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };

                    var strJson = JsonConvert.SerializeObject(request, jsonSerializerSettings);
                    var content = new StringContent(strJson, Encoding.UTF8, "application/json");
                    client.DefaultRequestHeaders.Add("Authorization", $"Token {apiToken}");

                    var responseString = await client.PostAsync(apiEndpoint, content);

                    var strData = await responseString.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<PlateReaderResponse>(strData);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "LicensePlateReaderService ReadLicensePlateImage");
            }

            return null;
        }
    }
}

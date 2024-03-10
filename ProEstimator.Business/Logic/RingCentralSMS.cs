using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RingCentral;

using ProEstimatorData;

namespace ProEstimator.Business.Logic
{
    public static class RingCentralSMS
    {

        public static async Task<RingCentralSMSResult> SendSMS(string userName, string extension, string password, string fromNumber, string toNumber, string body)
        {
            try
            {
                string clientID = System.Configuration.ConfigurationManager.AppSettings["RingCentralClientID"];
                string clientSecret = System.Configuration.ConfigurationManager.AppSettings["RingCentralClientSecret"];
                bool useProduction = InputHelper.GetBoolean(System.Configuration.ConfigurationManager.AppSettings["RingCentralProduction"]);

                RestClient client = new RestClient(
                    clientID
                    , clientSecret
                    , useProduction
                );

                var authorizeResult = await client.Authorize(userName, extension, password);
            
                var clientExtension = client.Restapi().Account().Extension();

                var requestBody = new
                {
                    text = body,
                    from = new { phoneNumber = fromNumber },
                    to = new object[] { new { phoneNumber = toNumber } }
                };

                //var response = clientExtension.Sms().Post(requestBody);
                var response = await clientExtension.Sms().Post(requestBody);

                if (string.IsNullOrEmpty(response.deliveryErrorCode))
                {
                    return new RingCentralSMSResult(true, "Returned ID: " + response.id);
                }
                else
                {
                    return new RingCentralSMSResult(false, "Returned ID: " + response.id + "  Error: " + response.deliveryErrorCode);
                }
            }
            catch (Flurl.Http.FlurlHttpException flurlEx)
            {
                return new RingCentralSMSResult(false, "Error: " + flurlEx.Message + Environment.NewLine + flurlEx.Call.ErrorResponseBody + Environment.NewLine + Environment.NewLine + "Request: " + flurlEx.Call.RequestBody);
            }
            catch (Exception ex)
            {
                return new RingCentralSMSResult(false, "Error: " + ex.Message);
            }
        }
    }

    public class RingCentralSMSResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public RingCentralSMSResult(bool success, string errorMessage)
        {
            Success = success;
            ErrorMessage = errorMessage;
        }
    }
}

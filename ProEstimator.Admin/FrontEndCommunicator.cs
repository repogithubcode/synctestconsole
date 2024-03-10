using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData;

namespace ProEstimator.Admin
{
    /// <summary>
    /// Used to communicate with the front end site, for refreshing caches etc. 
    /// </summary>
    public class FrontEndCommunicator
    {
        private string GetFrontEndRoot()
        {
            string frontEndRoot = "https://proestimator.web-est.com/";

            try
            {
                frontEndRoot = System.Configuration.ConfigurationManager.AppSettings.Get("FrontEndRootUrl").ToString();
            }
            catch
            {
                ErrorLogger.LogError("Admin, FrontEndRootUrl config not set.", "Admin FrontEndRootUrl");
            }

            return frontEndRoot;
        }

        private string EncodeString(string input)
        {
            string encrypted = ProEstimatorData.Encryptor.Encrypt(input);
            string encoded = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Encode(encrypted);
            return encoded;
        }

        private string PostToFrontEnd(string path)
        {
            try
            {
                string frontEndRoot = GetFrontEndRoot();
                string link = System.IO.Path.Combine(frontEndRoot, path);

                System.Net.WebClient client = new System.Net.WebClient();

                string result = client.DownloadString(link);
                return result;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "AdminController PostToFrontEnd");
                return ex.Message;
            }
        }

        public void RefreshContractsOnSite(int loginID)
        {
            string loginIDEncoded = EncodeString(loginID.ToString());
            PostToFrontEnd("Hooks/RefreshContractCache?one=" + loginIDEncoded);
        }
    }
}
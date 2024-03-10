using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimator.Business.Logic;
using ProEstimator.Business.Model;
using ProEstimatorData;
using ProEstimatorData.DataModel;

namespace Proestimator.Controllers.Login.Commands
{
    /// <summary>
    /// A login link can contain a Login ID and a place to redirect to, separated by :: and then encrypted.
    /// If a good encrypted redirect link is passed, this command will Execute as True and the RedirectLink is filled.
    /// </summary>
    public class ProcessLoginLinkCommand : CommandBase
    {

        private SiteLoginManager _siteLoginManager;
        private string _encryptedLink;
        private string _computerKey;

        public string RedirectLink { get; private set; }

        public ProcessLoginLinkCommand(SiteLoginManager siteLoginManager, string encryptedLink, string computerKey)
        {
            _siteLoginManager = siteLoginManager;
            _encryptedLink = encryptedLink;
            _computerKey = computerKey;
        }

        public override bool Execute()
        {
            try
            {
                string decrypted = Encryptor.Decrypt(_encryptedLink);

                string[] pieces = decrypted.Split("::".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                int loginID = InputHelper.GetInteger(pieces[0]);
                string redirectUrl = pieces[1];

                if (loginID > 0)
                {
                    List<ActiveLogin> activeLogins = _siteLoginManager.GetActiveLoginsForLoginID(loginID);
                    if (activeLogins?.Count > 0)
                    {
                        string computerKey = _computerKey;
                        ActiveLogin loginMatch = activeLogins.FirstOrDefault(o => o.ComputerKey == computerKey);
                        if (loginMatch != null)
                        {
                            RedirectLink = "/" + loginMatch.SiteUserID + redirectUrl;
                            return true;
                        }
                        else
                        {
                            RedirectLink = redirectUrl;
                            return false;
                        }
                    }
                    else
                    {
                        RedirectLink = redirectUrl;
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "ProcessLoginLink");
            }

            return false;
        }

    }
}
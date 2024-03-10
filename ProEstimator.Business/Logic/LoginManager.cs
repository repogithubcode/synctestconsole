using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using System.Windows.Controls;

namespace ProEstimator.Business.Logic
{
    // T is a class that inherits from ActiveLogin
    public class LoginManager<T> where T : ActiveLogin
    {

        public List<T> ActiveLogins 
        {
            get
            {
                LoadActiveLogins();
                return _activeLogins;
            }
            private set
            {
                LoadActiveLogins();
                _activeLogins = value;
            }
        }

        private List<T> _activeLogins = null;
        private object _activeLoginsLock = new object();

        // Active Logins are stored in server memory and persisted to the database.  Load them here if they are not in the memory.
        private void LoadActiveLogins()
        {
            lock (_activeLoginsLock)
            {
                if (_activeLogins == null)
                {
                    _activeLogins = new List<T>();

                    DBAccess db = new DBAccess();
                    DBAccessTableResult tableResult = db.ExecuteWithTable("ActiveLogin_GetAll", new SqlParameter("AdminSite", _isAdmin));

                    if (tableResult.Success)
                    {
                        foreach (DataRow row in tableResult.DataTable.Rows)
                        {
                            T activeLogin = LoadActiveLogin(row);
                            _activeLogins.Add(activeLogin);
                        }
                    }
                }
            }
        }

        private bool _isAdmin = false;

        public LoginManager(bool isAdmin)
        {
            _isAdmin = isAdmin;
        }

        /// <summary>
        /// Returns True if the passed UserID and ComputerKey have entered their correct password
        /// </summary>
        public bool IsUserAuthorized(int siteUserID, string computerKey)
        {
            if (siteUserID <= 0 || computerKey.Length != 10)
            {
                return false;
            }

            return GetActiveLogin(siteUserID, computerKey) != null;
        }

        /// <summary>
        /// Get an ActiveLogin record for the passed SiteUserID and ComputerKey.  
        /// This first makes sure all ActiveLogins are loaded and will load them if necessary.
        /// </summary>
        public T GetActiveLogin(int siteUserID, string computerKey, bool doNotCallRetrieved = false)
        {
            T activeLogin = ActiveLogins.FirstOrDefault(o => o.SiteUserID == siteUserID && o.ComputerKey == computerKey);

            if (activeLogin != null && !doNotCallRetrieved)
            {
                ActiveLoginRetrieved(activeLogin);
            }

            return activeLogin;
        }

        private void SetAutoSaveforTechSupportSiteUser(int loginID)
        {
            // Admin active login If IsImpersonated = true and IsImpersonation is true
            ActiveLogin activeLoginIsImpersonation = GetImpersonatedActiveLoginForLoginID(loginID);
            if (activeLoginIsImpersonation != null)
            {
                List<ActiveLogin> activeLogins = GetActiveLoginsForLoginID(loginID);
                foreach (ActiveLogin eachActiveLogin in activeLogins)
                {
                    eachActiveLogin.AdminIsImpersonating = activeLoginIsImpersonation.IsImpersonated;
                    eachActiveLogin.AutoSaveTurnedOnTechSupport = activeLoginIsImpersonation.AutoSaveTurnedOnTechSupport;
                    eachActiveLogin.AutoSaveTurnedOnSiteUser = activeLoginIsImpersonation.AutoSaveTurnedOnSiteUser;
                }
            }
        }


        protected virtual void ActiveLoginRetrieved(ActiveLogin activeLogin) { }

        /// <summary>
        /// When a Site User enters the correct credentails, this saves their info on the server as being authorized.
        /// </summary>
        public void AuthorizeSiteUserID(int siteUserID, string computerKey, bool isImpersonated, string ipAddress, string userAgent, DeviceType deviceType, string browser,Boolean autoSaveTurnedOnTechSupport = true, Boolean autoSaveTurnedOnSiteUser = true)
        {
            if (siteUserID <= 0 || computerKey.Length != 10)
            {
                return;
            }

            // If the user is already logged in, end that session
            LogOutUserAllSessions(siteUserID, isImpersonated, computerKey);

            // Create a new active login for the user
            SiteUser siteUser = SiteUser.Get(siteUserID);

            T activeLogin = InstantiateNewActiveLogin();
            activeLogin.SiteUserID = siteUser.ID;
            activeLogin.StartTime = DateTime.Now;
            activeLogin.LoginID = siteUser.LoginID;
            activeLogin.ComputerKey = computerKey;
            activeLogin.IsImpersonated = isImpersonated;
            activeLogin.IPAddress = ipAddress;
            activeLogin.ComputerName = userAgent;
            activeLogin.LastActivity = DateTime.Now;
            activeLogin.DeviceType = deviceType;
            activeLogin.Browser = browser;
            activeLogin.Save();

            lock (_activeLoginsLock)
            {
                activeLogin.AutoSaveTurnedOnTechSupport = autoSaveTurnedOnTechSupport;
                activeLogin.AutoSaveTurnedOnSiteUser = autoSaveTurnedOnSiteUser;
                ActiveLogins.Add(activeLogin);

                // Admin active login If IsImpersonated = true and IsImpersonation is true
                SetAutoSaveforTechSupportSiteUser(activeLogin.LoginID);
            }

            SiteActiveLogin siteActiveLogin = activeLogin as SiteActiveLogin;
            if (siteActiveLogin != null)
            {
                siteActiveLogin.NeedsInvoiceCheck = true;
            }

            ActiveLoginRetrieved(activeLogin);
        }

        protected virtual T InstantiateNewActiveLogin() { return null; }


        public T DeactivateSiteUser(int siteUserID, string computerKey, ActiveLoginDeleteKey logoutReason = ActiveLoginDeleteKey.LoggedOut)
        {
            T activeLogin = GetActiveLogin(siteUserID, computerKey);
            if (activeLogin != null)
            {
                activeLogin.Delete(logoutReason);
                lock (_activeLoginsLock)
                {
                    ActiveLogins.Remove(activeLogin);
                }
            }
            return activeLogin;
        }

        public void LogOutUserAllSessions(int siteUserID, bool isImpersonated, string computerKey)
        {
            lock(_activeLoginsLock)
            {
                List<T> activeLogins = ActiveLogins.Where(o => o.SiteUserID == siteUserID && o.IsImpersonated == isImpersonated && (!isImpersonated || o.ComputerKey == computerKey)).ToList();

                foreach(T activeLogin in activeLogins)
                {
                    activeLogin.Delete(ActiveLoginDeleteKey.MultiLoggedOut);
                    ActiveLogins.Remove(activeLogin);
                }
            }
        }

        

        protected virtual T LoadActiveLogin(DataRow row) { return null; }

        protected virtual void ActiveLoginsLoaded() { }

        /// <summary>
        /// Kick out all users for a login
        /// </summary>
        public int KickOutLogin(int loginID)
        {
            int kickCount = 0;

            List<T> forLogin = ActiveLogins.Where(o => o.LoginID == loginID).ToList();

            foreach (T login in forLogin)
            {
                login.Delete(ActiveLoginDeleteKey.KickedOut);
                lock (_activeLoginsLock)
                {
                    ActiveLogins.Remove(login);
                }

                kickCount++;
            }

            return kickCount;
        }

        /// <summary>
        /// Kick out a site user
        /// </summary>
        public int KickOutSiteUser(int siteUserID)
        {
            int kickCount = 0;

            List<T> users = ActiveLogins.Where(o => o.SiteUserID == siteUserID).ToList();

            foreach (T login in users)
            {
                login.Delete(ActiveLoginDeleteKey.KickedOut);
                lock (_activeLoginsLock)
                {
                    ActiveLogins.Remove(login);
                }

                kickCount++;
            }

            return kickCount;
        }

        public void RefreshCache()
        {
            StringBuilder builder = new StringBuilder();
            bool writeLog = false;

            try
            {
                if (ActiveLogins != null)
                {
                    DateTime cuttoff = DateTime.Now.AddDays(-7);

                    int count = 0;

                    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                    stopwatch.Start();

                    foreach (T activeLogin in ActiveLogins.ToList())
                    {
                        if (activeLogin.LastActivity < cuttoff)
                        {
                            activeLogin.Delete(ActiveLoginDeleteKey.TimeOut);
                            ActiveLogins.Remove(activeLogin);

                            count++;
                            writeLog = true;
                        }
                    }

                    builder.AppendLine("Deleted " + count + " active logins in " + stopwatch.ElapsedMilliseconds + " milliseconds.");
                }
            }
            catch (Exception ex)
            {
                builder.AppendLine("ERROR: " + ex.Message);
                writeLog = true;
            }

            if (writeLog)
            {
                ErrorLogger.LogError(builder.ToString(), "LoginManager RefreshCache");
            }
        }

        public int GetNumberOfLogins(int loginID)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("LoginID: " + loginID);

            int logins = 1;

            Contract activeContract = Contract.GetActive(loginID);

            if (activeContract != null)
            {
                builder.Append(" ContractID: " + activeContract.ID);

                try
                {
                    List<ContractAddOn> muAddOns = ContractAddOn.GetForContract(activeContract.ID).Where(o => o.AddOnType.ID == 8 && o.Active && o.HasPayment).ToList();
                    muAddOns.ForEach(o => { logins += o.Quantity; });
                    builder.Append(" + " + (logins - 1).ToString() + " add ons.");

                    List<ContractAddOnTrial> muTrialAddOns = ContractAddOnTrial.GetForContract(activeContract.ID).Where(o => o.AddOnType.ID == 8 && o.StartDate < DateTime.Now && o.EndDate > DateTime.Now).ToList();
                    logins += muTrialAddOns.Count;
                    builder.Append(" + " + muTrialAddOns.Count.ToString() + " trials.");
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, loginID, "GetNumberOfLogins Error");
                }
            }

            builder.Append(" Total: " + logins);

            // ErrorLogger.LogError(builder.ToString(), "GetNumberOfLogins");

            return logins;
        }        

        public LogInFunctionResult AuthenticateUser(string emailAddress, string password, string computerKey = "", string ipAddress = "", bool confirmLogOutOthers = false, bool noOfLoginsExceeded = false)
        {
            emailAddress = string.IsNullOrEmpty(emailAddress) ? "" : emailAddress.Trim();
            password = string.IsNullOrEmpty(password) ? "" : password.Trim();

            List<SiteUser> usersWithEmailMatch = SiteUser.GetForEmailAddress(emailAddress).Where(o => !o.IsDeleted).ToList();

            SiteUser user = GetUserPasswordMatch(usersWithEmailMatch, password);
            if (user != null)
            {
                return ProcessUserMatch(user, computerKey, confirmLogOutOthers);
            }
            else
            {
                return ProcessNoMatch(usersWithEmailMatch, emailAddress, password, ipAddress);
            }            
        }

        public List<ActiveLogin> GetActiveLoginsForLoginID(int loginID)
        {
            List<ActiveLogin> accountLogins = ActiveLogins.Where(o => o.LoginID == loginID && !o.IsImpersonated).ToList<ActiveLogin>();
            return accountLogins;
        }

        public ActiveLogin GetImpersonatedActiveLoginForLoginID(int loginID)
        {
            ActiveLogin activeLoginIsImpersonation = ActiveLogins.Where(o => o.LoginID == loginID && o.IsImpersonated).OrderByDescending(o=> o.LastActivity).FirstOrDefault();
            return activeLoginIsImpersonation;
        }

        public List<ActiveLogin> GetAllImpersonatedActiveLoginForLoginID(int loginID)
        {
            List<ActiveLogin> allImpersonatedActiveLogins = ActiveLogins.Where(o => o.LoginID == loginID && o.IsImpersonated).ToList<ActiveLogin>();
            return allImpersonatedActiveLogins;
        }

        public void LogoutImpersonationActiveLoginForLoginID(int loginID)
        {
            IEnumerable<ActiveLogin> impersonationAccountLogins = ActiveLogins.Where(o => o.LoginID == loginID && o.AdminIsImpersonating); // don't convert it to list otherwise it doesn't work
            foreach (ActiveLogin eachActiveLogin in impersonationAccountLogins)
            {
                eachActiveLogin.AdminIsImpersonating = false;
                eachActiveLogin.AutoSaveTurnedOnTechSupport = false;
                eachActiveLogin.AutoSaveTurnedOnSiteUser = true;
            }
            List<ActiveLogin> allImpersonatedActiveLogins = GetAllImpersonatedActiveLoginForLoginID(loginID);
            foreach (ActiveLogin eachImpersonatedActiveLogin in allImpersonatedActiveLogins)
            {
                if (eachImpersonatedActiveLogin != null)
                {
                    eachImpersonatedActiveLogin.IsImpersonated = false;
                    eachImpersonatedActiveLogin.Save();
                }
            }
        }

        private LogInFunctionResult ProcessUserMatch(SiteUser user, string computerKey, bool confirmLogOutOthers)
        {
            // Don't allow in disabled accounts
            LoginInfo loginInfo = LoginInfo.GetByID(user.LoginID);

            if (loginInfo.Disabled)
            {
                return new LogInFunctionResult(ProEstimator.Business.Resources.ProEstBusiness.Login_Error_AccountDisabled);
            }

            // See if the same user is logged into another computer
            LogInFunctionResult otherComputerResult = CheckSameUserOnOtherComputers(user, computerKey, confirmLogOutOthers);
            if (otherComputerResult != null)
            {
                return otherComputerResult;
            }

            // See if other users in the same account are logged in
            LogInFunctionResult otherUsersResult = CheckOtherUsersForLogin(user, confirmLogOutOthers);
            if (otherUsersResult != null)
            {
                return otherUsersResult;
            }

            return new LogInFunctionResult(user);
        }

        /// <summary>
        /// If the user is logged in on another computer they are first asked if they want to log the other out, and then when they submit again they log the other out ang log in.
        /// Pass ConfirmLogOutOthers = true the second time to log them out.
        /// </summary>
        /// <returns></returns>
        private LogInFunctionResult CheckSameUserOnOtherComputers(SiteUser user, string computerKey, bool confirmLogOutOthers)
        {
            List<T> sameUserDifferentComputerList = ActiveLogins.Where(o => o.SiteUserID == user.ID && o.ComputerKey != computerKey && !o.IsImpersonated).ToList();

            // Before checking the other logins, delete any that haven't been touched in a few hours
            foreach (T activeLogin in sameUserDifferentComputerList.ToList())
            {
                if (activeLogin.LastActivity < DateTime.Now.AddHours(-6))
                {
                    DeactivateSiteUser(activeLogin.SiteUserID, activeLogin.ComputerKey, ActiveLoginDeleteKey.MultiLoggedOut);
                    sameUserDifferentComputerList.Remove(activeLogin);
                }
            }

            if (sameUserDifferentComputerList.Count > 0)
            {
                // If the user has already confirmed that they want to log out their other sessions, do it now.
                if (confirmLogOutOthers)
                {
                    foreach (T activeLogin in sameUserDifferentComputerList)
                    {
                        activeLogin.Delete(ActiveLoginDeleteKey.MultiLoggedOut);
                        ActiveLogins.Remove(activeLogin);
                    }
                }
                else
                {
                    // Return the user and the number of other matches.  The user will be asked to confirm logging out the other users.
                    return new LogInFunctionResult(user, sameUserDifferentComputerList.Count);
                }
            }

            return null;
        }

        private LogInFunctionResult CheckOtherUsersForLogin(SiteUser user, bool confirmLogOut)
        {
            List<T> otherUsers = ActiveLogins.Where(o => o.LoginID == user.LoginID).ToList();

            otherUsers = otherUsers.Where(o => o.SiteUserID != user.ID && !o.IsImpersonated).OrderBy(o => o.LastActivity).ToList();

            int allowedLogins = GetNumberOfLogins(user.LoginID);

            if (otherUsers.Count >= allowedLogins)
            {
                if (confirmLogOut)
                {
                    while (otherUsers.Count >= allowedLogins)
                    {
                        T toRemove = otherUsers.First();

                        toRemove.Delete(ActiveLoginDeleteKey.MultiLoggedOut);
                        ActiveLogins.Remove(toRemove);
                        otherUsers.Remove(toRemove);  
                    }
                }
                else
                {
                    return new LogInFunctionResult(otherUsers.ToList<ActiveLogin>(), user);
                }
            }

            return null;
        }

        private LogInFunctionResult ProcessNoMatch(List<SiteUser> users, string emailAddress, string password, string ipAddress)
        {
            AddLoginFailure(emailAddress, "", password, ipAddress);

            if (users.Count == 0)
            {
                return new LogInFunctionResult("Invalid login credentials.");
            }

            int loginID = 0;
            DateTime lastLoginSuccess = new DateTime();

            foreach (SiteUser user in users)
            {
                ActiveLogin lastLogin = ActiveLogin.GetLastLoginActivityByUser(user.ID);
                if (lastLogin != null)
                {
                    if (lastLogin.LastActivity > lastLoginSuccess)//can use userActiveLogin.StartTime
                    {
                        loginID = user.LoginID;
                        lastLoginSuccess = lastLogin.LastActivity;
                    }
                }
            }

            int nFail = ProEstimatorData.DataModel.Admin.LoginFailure.GetLoginFailures(emailAddress, "", lastLoginSuccess.ToString(), "").Count;
            LogInFunctionResult loginResult = new LogInFunctionResult(nFail);
            if (nFail == 3)
            {
                loginResult.AddFollowUpMessage();
            }

            return loginResult;
        }

        private SiteUser GetUserPasswordMatch(List<SiteUser> users, string password)
        {
            if (users != null && users.Count > 0)
            {
                return users.FirstOrDefault(o => o.Password == password);
            }

            return null;
        }

        public FunctionResult ValidatePassword(string password, string repeatPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(repeatPassword))
            {
                return new FunctionResult("Please enter and re-enter the password for confirmation.");
            }

            if (password != repeatPassword)
            {
                return new FunctionResult("Passwords do not match.");
            }

            if (password.Length < 6)
            {
                return new FunctionResult("Password must be at least 6 characters.");
            }

            // TODO - etc.

            return new FunctionResult();
        }

        public static FunctionResultInt CreateUser(int loginID, string emailAddress, string password, bool isAdmin = false)
        {
            List<SiteUser> existingUsers = SiteUser.GetForEmailAddress(emailAddress);

            if (existingUsers != null && existingUsers.Count > 0)
            {
                return new FunctionResultInt("That user name is already used.");
            }

            LoginInfo loginInfo = LoginInfo.GetByID(loginID);
            if (loginInfo == null)
            {
                return new FunctionResultInt("Invalid login ID, account info not found for account " + loginID + ".");
            }

            SiteUser newUser = new SiteUser();
            newUser.LoginID = loginID;
            newUser.EmailAddress = emailAddress;

            newUser.Password = password;

            SaveResult result = newUser.Save();
            if (result.Success)
            {
                // If the new user is an admin, give them the admin permission
                if (isAdmin)
                {
                    SitePermissionsManager permissionsManager = new SitePermissionsManager(newUser.ID);
                    permissionsManager.SavePermission("Admin", true);
                }

                return new FunctionResultInt(newUser.ID);
            }
            else
            {
                return new FunctionResultInt(result.ErrorMessage);
            }
        }

        public void AddLoginFailure(string loginName, string organization, string password, string ipAddress)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginName", loginName));
            parameters.Add(new SqlParameter("Organization", organization));
            parameters.Add(new SqlParameter("Password", password));
            parameters.Add(new SqlParameter("USER_ADDR", ipAddress));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("sp_AddLoginFailure", parameters);
        }

        public FunctionResult ChangeUserPassword(int activeLoginID, int userID, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 4)
            {
                return new FunctionResult(ProEstimator.Business.Resources.ProEstBusiness.Message_PasswordMinLength);
            }

            SiteUser siteUser = SiteUser.Get(userID);

            if (siteUser.Password != newPassword)
            {
                siteUser.Password = newPassword;
                SaveResult result = siteUser.Save(activeLoginID);

                return new FunctionResult(result.ErrorMessage);
            }

            return new FunctionResult();
        }

        public FunctionResult ChangeUserName(int activeLoginID, int userID, string newEmailAddress)
        {
            // Cancel if the user name is already taken
            List<SiteUser> existingUsers = SiteUser.GetForEmailAddress(newEmailAddress);
            if (existingUsers != null && existingUsers.Count > 0)
            {
                return new FunctionResult("That user name is already used.");
            }

            // Change the user name
            SiteUser siteUser = SiteUser.Get(userID);
            siteUser.EmailAddress = newEmailAddress;

            SaveResult saveResult = siteUser.Save(activeLoginID);

            if (saveResult.Success)
            {
                return new FunctionResult();
            }
            else
            {
                return new FunctionResult(saveResult.ErrorMessage);
            }
        }

        public static OldLoginFunctionResult AuthenticateUser(string userName, string organization, string password, string computerKey = "", System.Web.HttpRequestBase request = null)
        {
            StringBuilder errorMessage = new StringBuilder();

            userName = userName.Trim();
            organization = organization.Trim();
            password = password.Trim();

            // Get the LoginInfo record for the passed credentials
            List<LoginInfo> loginInfoList = LoginInfo.GetByCredentials(userName, organization, password);
            if (loginInfoList == null)
            {
                return new OldLoginFunctionResult(ProEstimator.Business.Resources.ProEstBusiness.Login_Error_NotFound);
            }

            LoginInfo loginInfo = loginInfoList[0];

            if (loginInfo.Disabled)
            {
                return new OldLoginFunctionResult(ProEstimator.Business.Resources.ProEstBusiness.Login_Error_AccountDisabled);
            }

            // If we got this far, the login credentials check out
            return new OldLoginFunctionResult(loginInfo);
        }

        public FunctionResult SendForgotPasswordLink(string emailAddress)
        {
            List<SiteUser> users = SiteUser.GetForEmailAddress(emailAddress).Where(o => !o.IsDeleted).ToList();

            if (users.Count == 0)
            {
                return new FunctionResult("We couldn't find a user with that e-mail address, please contact support for help logging in.");
            }
            else if (users.Count > 1)
            {
                return new FunctionResult("We found multiple user accounts with that e-mail address, please contact support for help logging in.");
            }
            else
            {
                PasswordResetLink link = new PasswordResetLink();
                link.Code = Guid.NewGuid().ToString().ToUpper().Replace("-", "").Substring(0, 20);
                link.SiteUserID = users[0].ID;
                SaveResult saveResult = link.Save();

                if (saveResult.Success)
                {
                    string linkUrl = ConfigurationManager.AppSettings["BaseURL"] + "password-reset/" + link.Code;

                    EmailManager.SendPasswordResetLink(emailAddress, linkUrl);

                    return new FunctionResult();
                }
                else
                {
                    return new FunctionResult(saveResult.ErrorMessage);
                }
            }
        }

        public FunctionResult SendForgotPasswordEmail(string emailAddress)
        {
            List<SiteUser> users = SiteUser.GetForEmailAddress(emailAddress).Where(o => !o.IsDeleted).ToList();

            if (users.Count == 0)
            {
                return new FunctionResult("We couldn't find a user with that e-mail address, please contact support for help logging in.");
            }
            else if (users.Count > 1)
            {
                return new FunctionResult("We found multiple user accounts with that e-mail address, please contact support for help logging in.");
            }
            else
            {
                SiteUser siteUser = users.FirstOrDefault();

                EmailManager.SendForgotPasswordEmail(emailAddress, siteUser);

                return new FunctionResult();
            }
        }

    }

    public class LogInFunctionResult : FunctionResult
    {
        public SiteUser SiteUser { get; private set; }
        public bool MultipleMatches { get; private set; }
        public int OtherLoginsCount { get; private set; }
        public int NumFails { get; private set; }
        public bool NoOfLoginsExceeded { get; set; }

        public List<OtherUserInfo> OtherLogins { get; private set; }

        public LogInFunctionResult(string errorMessage)
            : base(errorMessage)
        {
            
        }

        public LogInFunctionResult(bool multipleMatches)
            : base("There are multiple matches.")
        {
            MultipleMatches = multipleMatches;
        }

        public LogInFunctionResult(SiteUser siteUser)
            : base(true, "")
        {
            SiteUser = siteUser;
        }

        public LogInFunctionResult(SiteUser siteUser, int otherLoginsCount)
            : base(false, "This user is already logged in on another system.  Log in again to end the other session and continue.")
        {
            SiteUser = siteUser;
            OtherLoginsCount = otherLoginsCount;
        }

        public LogInFunctionResult(SiteUser siteUser, int otherLoginsCount, Boolean noOfLoginsExceeded, string errorMessage)
            : base(false, errorMessage)
        {
            SiteUser = siteUser;
            OtherLoginsCount = otherLoginsCount;
            NoOfLoginsExceeded = noOfLoginsExceeded;
        }

        public LogInFunctionResult(int numFails)
            : base("Wrong password.")
        {
            NumFails = numFails;
        }

        public LogInFunctionResult(List<ActiveLogin> otherUsers, SiteUser siteUser)
            : base(false, "The maximum number of users are already logged in.  Log in again to end the oldest session and continue.")
        {
            OtherLogins = new List<OtherUserInfo>();

            foreach(ActiveLogin activeLogin in otherUsers)
            {
                SiteUser user = SiteUser.Get(activeLogin.SiteUserID);
                OtherLogins.Add(new OtherUserInfo() { UserID = user.ID, UserName = user.Name, LastActivity = activeLogin.LastActivity });
            }

            SiteUser = siteUser;
            NoOfLoginsExceeded = true;
            OtherLoginsCount = otherUsers.Count;
        }

        public void AddFollowUpMessage()
        {
            ErrorMessage += " If you are having trouble logging in please contact support for assistance.";
        }
    }

    public class OtherUserInfo
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public DateTime LastActivity { get; set; }

    }

    public class OldLoginFunctionResult : FunctionResult
    {
        public LoginInfo LoginInfo { get; private set; }

        public OldLoginFunctionResult(string errorMessage)
            : base(errorMessage)
        {

        }

        public OldLoginFunctionResult(LoginInfo loginInfo)
            : base(true, "")
        {
            LoginInfo = loginInfo;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using ProEstimatorData;
using ProEstimatorData.DataModel;

namespace ProEstimator.Business.Logic.Admin
{
    public class NewUserSignupService
    {
        public async Task<NewUserSignupServiceResult> SendNewCustomerSMSCustomMessage(int salesRepID, int loginID, string phoneNumber, string message)
        {
            return await ExecuteSMS(salesRepID, loginID, phoneNumber, message);
        }

        public async Task<NewUserSignupServiceResult> SendNewCustomerSMS(int salesRepID, int loginID, string phoneNumber)
        {
            return await ExecuteSMS(salesRepID, loginID, phoneNumber);
        }

        public async Task<NewUserSignupServiceResult> ExecuteSMS(int salesRepID, int loginID, string phoneNumber, string message = null)
        {
            LoginInfo loginInfo = LoginInfo.GetByID(loginID);
            if (loginInfo == null)
            {
                return new NewUserSignupServiceResult(false, "Account information not found for ID " + loginID.ToString());
            }

            string sql = "SELECT PhoneNumber, PhoneExtension FROM SalesRep WHERE SalesRepID = " + salesRepID.ToString();
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTableForQuery(sql);
            if (tableResult.Success)
            {
                string userName = ConfigurationManager.AppSettings["RingCentralUserName"]; // "16169304224";
                string extension = ConfigurationManager.AppSettings["RingCentralExtension"]; // tableResult.DataTable.Rows[0]["PhoneExtension"].ToString();
                string password = ConfigurationManager.AppSettings["RingCentralPassword"]; // "F00tba11!";
                string fromNumber = "1" + tableResult.DataTable.Rows[0]["PhoneNumber"].ToString();

                string encryptedLoginID = MakeCodeForLogin(loginInfo);
                string url = "https://proestimator.web-est.com/login/code/" + encryptedLoginID;

                string body = string.IsNullOrEmpty(message) ? "Welcome to Pro Estimator.  Please use the following link to log into your new account: " + url : message;

                RingCentralSMSResult result = await RingCentralSMS.SendSMS(userName, extension, password, fromNumber, phoneNumber, body);
                if (result.Success)
                {
                    return new NewUserSignupServiceResult(true, "");
                }
                else
                {
                    throw new Exception(result.ErrorMessage);
                    return new NewUserSignupServiceResult(false, result.ErrorMessage);
                }
            }

            return new NewUserSignupServiceResult(false, "Sales rep info not found.");
        }

        public string MakeCodeForLogin(LoginInfo loginInfo)
        {
            return NumbersToLetters(loginInfo.ID);
        }

        public LoginInfo GetLoginInfoForCode(string code)
        {
            int loginID = LettersToNumber(code);

            LoginInfo loginInfo = LoginInfo.GetByID(loginID);
            if (loginInfo != null)
            {
                return loginInfo;
            }

            return null;
        }

        private string NumbersToLetters(int numbers)
        {
            StringBuilder builder = new StringBuilder();
            string numbersString = numbers.ToString();

            for (int i = 0; i < numbersString.Length; i++)
            {
                int number = InputHelper.GetInteger(numbersString[i].ToString());
                builder.Append(_characters[number]);
            }

            return builder.ToString();
        }

        private int LettersToNumber(string letters)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < letters.Length; i++)
            {
                int number = _characters.IndexOf(letters[i]);
                builder.Append(number);
            }

            return InputHelper.GetInteger(builder.ToString());
        }

        private static List<char> _characters = new List<char>() { 'F', 'A', 'C', 'U', 'E', 'R', 'O', 'T', 'M', 'P' };
    }

    public class NewUserSignupServiceResult
    {
        public bool Success { get; private set; }
        public string ErrorMessage { get; private set; }

        public NewUserSignupServiceResult(bool success, string errorMessage)
        {
            Success = success;
            ErrorMessage = errorMessage;
        }
    }
}

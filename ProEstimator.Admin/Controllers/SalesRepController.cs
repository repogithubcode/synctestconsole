using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Configuration;
using System.Text;

using ProEstimatorData;
using ProEstimatorData.DataModel;

using ProEstimator.Business.Logic;
using ProEstimator.Business.Logic.Admin;

namespace ProEstimator.Admin.Controllers
{
    public class SalesRepController : Controller
    {

        public JsonResult ImportWELogin(string id)
        {
            try
            {
                ErrorLogger.LogError("Starting login for " + id, "ImportWELogin Start");

                AdminService adminService = new AdminService();
                var model = adminService.ImportLogin(id);

                // Send an email to Mike
                List<string> emailAddresses = new List<string>();
                List<string> salesRepEmailAddresses = SalesRepPermissionManager.GetSalesRepEmailAddressForPermissionTag("IsEmailSentImportWELogin");

                foreach (string eachSalesRepStaffEmailSent in salesRepEmailAddresses)
                {
                    emailAddresses.Add(eachSalesRepStaffEmailSent);
                }

                StringBuilder builder = new StringBuilder();

                builder.AppendLine("Account " + model.LoginID + " initiated a self import into PE.");
                builder.AppendLine("Success: " + model.LoginImported);
                builder.AppendLine("Message: " + model.Message);

                Email email = new Email();
                email.LoginID = InputHelper.GetInteger(id);
                emailAddresses.ForEach(o => email.AddToAddress(o));
                email.Subject = "ProEstimator Self Import";
                email.Body = builder.ToString().Replace(Environment.NewLine, "<br />");
                email.Save(0);
                EmailSender.SendEmail(email);

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ProEstimatorData.ErrorLogger.LogError(ex, InputHelper.GetInteger(id), "ImportWELogin Error");
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }

    }
}
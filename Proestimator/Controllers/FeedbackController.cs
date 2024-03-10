using Proestimator.ViewModel;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using ProEstimator.Business.Logic;
using System.IO;
using System.Configuration;
using ProEstimatorData.DataModel;
using ProEstimatorData;

namespace Proestimator.Controllers
{
    public class FeedbackController : SiteController
    {
        [HttpGet]
        [Route("{userID}/feedback")]
        public ActionResult GetFeedback(int userID)
        {
            SuccessBoxFeatureLog.LogFeature(ActiveLogin.LoginID, SuccessBoxModule.Search, "Feedback page visited", ActiveLogin.ID);
            return View(new UserFeedbackVM { UserID = userID });
        }

        [HttpPost]
        [Route("{userID}/feedback")]
        public ActionResult SubmitFeedback(UserFeedbackVM vm)
        {
            var fileNames = new List<string>();
            string imageFolder = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), "UserFeedBackImages", ActiveLogin.LoginID.ToString());

            if (Request.Files.Count > 0)
            {
                Directory.CreateDirectory(imageFolder);
            }

            for (int i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];
                if (!string.IsNullOrEmpty(file?.FileName))
                {
                    var fileName = $"{Guid.NewGuid().ToString("N").ToUpper()}.{Path.GetExtension(file.FileName).Replace(".", "")}";
                    file.SaveAs(Path.Combine(imageFolder, fileName));
                    fileNames.Add(fileName);
                }
            }

            var objBrwInfo = Request.Browser;

            var browser = new
            {
                BrowserName = objBrwInfo.Browser,
                BrowserVersion = objBrwInfo.Version,
                BrowserType = objBrwInfo.Type,
                objBrwInfo.MajorVersion,
                objBrwInfo.MinorVersion
            };

            var platform = new
            {
                operatingSystem = objBrwInfo.Platform,
                IsMobilebrowser = objBrwInfo.IsMobileDevice
            };

            var userFeedbackService = new UserFeedbackService();
            FunctionResult result = userFeedbackService.InsertFeedBack(new UserFeedback
            {
                ActiveLoginID = ActiveLogin.LoginID,
                FeedbackText = vm.FeedbackText,
                CreatedDate = DateTime.Now,
                BrowserDetails = Newtonsoft.Json.JsonConvert.SerializeObject(browser),
                DevicePlatform = Newtonsoft.Json.JsonConvert.SerializeObject(platform),
                ImagesPath = fileNames
            });

            if (result.Success)
            {
                SuccessBoxFeatureLog.LogFeature(ActiveLogin.LoginID, SuccessBoxModule.Search, "Feedback submitted", ActiveLogin.ID);
            }
            else
            {
                string s = result.ErrorMessage + vm.FeedbackText;
                ErrorLogger.LogError(s.Substring(0, Math.Min(s.Length, 2000)), ActiveLogin.LoginID, 0, "SubmitFeedback");
            }

            TempData["FeedbackSubmitted"] = true;

            return RedirectToAction("GetFeedback");
        }
    }
}
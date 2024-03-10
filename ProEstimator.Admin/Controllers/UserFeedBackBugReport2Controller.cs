using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using ProEstimator.Admin.ViewModel.SalesBoard;
using ProEstimator.Admin.ViewModel.UserFeedbackBug;
using ProEstimatorData;
using ProEstimatorData.DataModel.Admin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace ProEstimator.Admin.Controllers
{
    public class UserFeedBackBugReportController : AdminController
    {
        [Route("UserFeedBackBugReport/List")]
        public ActionResult Index()
        {
            UserFeedbackBugVM userFeedbackBugVM = new UserFeedbackBugVM();

            return View(userFeedbackBugVM);
        }

        public ActionResult GetUserFeedBackBugReport([DataSourceRequest] DataSourceRequest request, string reporterNameFilter,
                                                string rangeStartFilter, string rangeEndFilter)
        {
            List<UserFeedbackBugVM> userFeedbackBugs = new List<UserFeedbackBugVM>();

            List<UserFeedBack> userFeedBacks = UserFeedBack.GetForFilter(reporterNameFilter, rangeStartFilter, rangeEndFilter);

            foreach (UserFeedBack userFeedBack in userFeedBacks)
            {
                userFeedbackBugs.Add(new UserFeedbackBugVM(userFeedBack));
            }

            return Json(userFeedbackBugs.ToDataSourceResult(request));
        }

        // Post api/errorLogReport/GetSalesBoardReport
        public JsonResult GetReportImages(string imageString, int loginId)
        {
            UserFeedbackJsonResult userFeedbackJsonResult = new UserFeedbackJsonResult();
            string imageFolder = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), "UserFeedBackImages");
            var imageStrs = imageString.Split('|').Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => x = Path.Combine(imageFolder, loginId.ToString(), x.Trim())).ToList();

            var imageDataList = new List<string>();
            foreach (var path in imageStrs)
            {
                imageDataList.Add(GetBase64StringFromImgPath(path));
            }

            userFeedbackJsonResult.Success = true;
            userFeedbackJsonResult.ImageDataList = imageDataList;

            var jsonResult = Json(userFeedbackJsonResult, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;

            return jsonResult;
        }

        private string GetBase64StringFromImgPath(string path)
        {
            using (Image image = Image.FromFile(path))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }
        }

        [HttpPost]
        public JsonResult SaveActionNote(UserFeedbackBugVM userFeedbackBugVM)
        {
            List<UserFeedBack> userFeedBacks = UserFeedBack.GetForFilter(null, null, null, userFeedbackBugVM.UserFeedbackID);

            userFeedBacks[0].ActionNote = userFeedbackBugVM.ActionNote;

            FunctionResult functionResult = UserFeedBack.SaveUserFeedBackBug(userFeedBacks[0]);

            return Json(new { Success = functionResult.Success }, JsonRequestBehavior.AllowGet);
        }

    }

    public class UserFeedbackJsonResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> ImageDataList { get; set; }
    }
}

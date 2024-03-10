using ProEstimator.Business.Model;
using ProEstimator.Business.Model.Admin.Reports;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ProEstimator.Admin.Controllers
{
    public class UserFeedBackBugReportController : BaseApiController
    {
        [HttpPost]
        // Post api/errorLogReport/GetSalesBoardReport
        public HttpResponseMessage GetReport(UserFeedBackVM requestObj)
        {
            var model = AdminRepoServices.FeedBackBugReportService.GetUserFeedBackBugReport(requestObj);
            return Request.CreateResponse(HttpStatusCode.OK, model);
        }
        //string imageFolder = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), "UserFeedBackImages", CurrentSession.LoginID.ToString());
        [HttpPost]
        // Post api/errorLogReport/GetSalesBoardReport
        public HttpResponseMessage GetReportImages(string imageString,int loginId)
        {
            string imageFolder = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), "UserFeedBackImages");
            var imageStrs = imageString.Split('|').Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => x = Path.Combine(imageFolder, loginId.ToString(),x.Trim())).ToList();

            var imageDataList = new List<string>();
            foreach (var path in imageStrs)
            {
                imageDataList.Add(GetBase64StringFromImgPath(path));
            }

            return Request.CreateResponse(HttpStatusCode.OK, imageDataList);
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
    }
}

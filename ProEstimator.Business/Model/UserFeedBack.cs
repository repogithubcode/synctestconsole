using ProEstimator.Business.ILogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using ProEstimator.Business.Extension;

namespace ProEstimator.Business.Model
{
   
    public class UserFeedBackVM : IModelMap<UserFeedBackVM>
    {
        public string Name { get; set; }
        public string Subject { get; set; }
        public int UserID { get; set; }
        public int LoginID { get; set; }
        public string Contact { get; set; }
        public string EmailID { get; set; }
        public string Description { get; set; }
        public string MainModule { get; set; }
        public string SubModule { get; set; }
        public string CreatedDate { get; set; }
        public string IsActive { get; set; }
        public string BrowserDetails { get; set; }
        public string DevicePlatform { get; set; }
        public string URL { get; set; }
        public string FeedbackId { get; set; }
        
        public string DateStart { get; set; }
        public string DateEnd { get; set; }

        public List<string> ImagesPath { get; set; }
        public string ImagePaths { get; set; }

        public UserFeedBackVM ToModel(DataRow row)
        {
           
            var model = new UserFeedBackVM();
            model.Name =  row["Name"].SafeString();
            model.Subject = row["Subject"].SafeString();
            model.UserID = row["UserID"].SafeIntReturnInt();
            model.LoginID = row["LoginID"].SafeIntReturnInt();
            model.Contact = row["ContactNo"].SafeString();
            model.EmailID = row["Email"].SafeString();
            model.Description = row["Description"].SafeString();
            model.MainModule = row["MainModule"].SafeString();
            model.SubModule = row["SubModule"].SafeString();
            model.CreatedDate = row["CreatedDate"].SafeDate();
            model.IsActive = row["IsActive"].SafeBool().GetValueOrDefault().ToString();
            model.BrowserDetails = row["BrowserDetails"].SafeString();
            model.DevicePlatform = row["DevicePlatform"].SafeString();
            model.URL = row["URL"].SafeString();
            model.FeedbackId = row["FeedbackId"].SafeIntReturnInt().ToString();
            model.ImagesPath = row["ImagesPaths"].SafeString().Split('|').ToList();
            model.ImagePaths = row["ImagesPaths"].SafeString();
            return model;
        }
    }
   

}

using ProEstimatorData;
using ProEstimatorData.DataModel.Admin;

namespace ProEstimator.Admin.ViewModel.UserFeedbackBug
{
    public class UserFeedbackBugVM
    {
        public int UserFeedbackID { get; set; }
        public string LoginID { get; set; }
        public string ReporterName { get; set; }
        public string Phone { get; set; }
        public string CompanyName { get; set; }
        public string CreatedDate { get; set; }
        public string FeedbackText { get; set; }
        public string ImagePaths { get; set; }
        public string ActionNote { get; set; }

        public UserFeedbackBugVM() { }

        public UserFeedbackBugVM(UserFeedBack userFeedBack)
        {
            UserFeedbackID = InputHelper.GetInteger(userFeedBack.UserFeedbackID.ToString());
            LoginID = InputHelper.GetString(userFeedBack.LoginID.ToString());
            ReporterName = $"{InputHelper.GetString(userFeedBack.FirstName.ToString())} {InputHelper.GetString(userFeedBack.LastName.ToString())}";
            Phone = InputHelper.GetString(userFeedBack.Phone.ToString());
            CompanyName = InputHelper.GetString(userFeedBack.CompanyName.ToString());
            CreatedDate = InputHelper.GetString(userFeedBack.CreatedDate.ToString());
            FeedbackText = InputHelper.GetString(userFeedBack.FeedbackText.ToString());

            if (userFeedBack.ImagePaths != null)
            {
                ImagePaths = InputHelper.GetString(userFeedBack.ImagePaths.ToString());
            }

            ActionNote = InputHelper.GetString(userFeedBack.ActionNote.ToString());
        }
    }
}
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Admin;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ProEstimator.Business.Logic
{
    public class UserFeedbackService
    {
        public FunctionResult InsertFeedBack(UserFeedback model)
        {
            DBAccess db = new DBAccess();
            List<SqlParameter> parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("ActiveLoginID ", model.ActiveLoginID));
            parameters.Add(new SqlParameter("FeedbackText", model.FeedbackText));
            parameters.Add(new SqlParameter("CreatedDate", model.CreatedDate));
            parameters.Add(new SqlParameter("BrowserDetails", model.BrowserDetails));
            parameters.Add(new SqlParameter("DevicePlatform", model.DevicePlatform));
            parameters.Add(new SqlParameter("ImagePaths", string.Join("|", model.ImagesPath.ToArray())));

            return db.ExecuteNonQuery("InsertUserFeedBack", parameters);
        }
    }
}
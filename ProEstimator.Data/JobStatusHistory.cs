using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class JobStatusHistory
    {

        public int ID { get; private set; }
        public int EstimateID { get; set; }
        public JobStatus JobStatus { get; set; }
        public DateTime ActionDate { get; set; }

        public JobStatusHistory()
        {
            ActionDate = DateTime.Now;
        }

        public JobStatusHistory(DataRow row)
        {
            ID = InputHelper.GetInteger(row["id"].ToString());
            EstimateID = InputHelper.GetInteger(row["AdminInfoID"].ToString());
            ActionDate = InputHelper.GetDateTime(row["ActionDate"].ToString());

            int jobStatusID = InputHelper.GetInteger(row["JobStatusID"].ToString());
            JobStatus = JobStatus.GetByID(jobStatusID);
        }

        public SaveResult Save()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("AdminInfoID", EstimateID));
            parameters.Add(new SqlParameter("JobStatusID", JobStatus == null ? 0 : JobStatus.ID));
            parameters.Add(new SqlParameter("ActionDate", ActionDate));

            DBAccess db = new DBAccess();
            DBAccessIntResult intResult = db.ExecuteWithIntReturn("JobStatusHistory_Insert", parameters);
            
            if (intResult.Success)
            {
                ID = intResult.Value;
                return new SaveResult();
            }
            else
            {
                return new SaveResult(intResult.ErrorMessage);
            }
        }

        public static List<JobStatusHistory> GetForEstimate(int estimateID)
        {
            List<JobStatusHistory> history = new List<JobStatusHistory>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("JobStatusHistory_GetForEstimate", new SqlParameter("AdminInfoID", estimateID));

            foreach(DataRow row in tableResult.DataTable.Rows)
            {
                history.Add(new JobStatusHistory(row));
            }

            return history;
        }
    }
}

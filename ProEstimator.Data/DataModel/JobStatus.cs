using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ProEstimatorData.DataModel
{
    public class JobStatus
    {

        public int ID { get; private set; }
        public string Description { get; private set; }

        public JobStatus(DataRow row)
        {
            ID = InputHelper.GetInteger(row["id"].ToString());
            Description = row["Description"].ToString();
        }

        public static List<JobStatus> GetAll()
        {
            lock(_loadLock)
            {
                if (_jobStatuses == null || _jobStatuses.Count == 0)
                {
                    DBAccess db = new DBAccess();
                    DBAccessTableResult tableResult = db.ExecuteWithTable("JobStatus_GetAll");
                    if (tableResult.Success)
                    {
                        _jobStatuses = new List<JobStatus>();

                        foreach (DataRow row in tableResult.DataTable.Rows)
                        {
                            _jobStatuses.Add(new JobStatus(row));
                        }
                    }
                }
            }

            return _jobStatuses;
        }

        public static JobStatus GetByID(int id)
        {
            return GetAll().FirstOrDefault(o => o.ID == id);
        }

        /// <summary>
        /// For the passed current status, return the list of statuses that the estimate can be changed to.
        /// </summary>
        public static List<JobStatus> GetForStatus(int currentStatus)
        {
            List<JobStatus> returnList = new List<JobStatus>();

            List<JobStatus> all = GetAll();

            // New Estimate
            if (currentStatus == 1 || currentStatus == 0)
            {
                returnList.Add(all.FirstOrDefault(o => o.ID == 2));     // Repair Order
                returnList.Add(all.FirstOrDefault(o => o.ID == 4));     // Canceled Job
            }

            // Repair Order
            else if (currentStatus == 2)
            {
                returnList.Add(all.FirstOrDefault(o => o.ID == 3));     // Closed Repair Order
                returnList.Add(all.FirstOrDefault(o => o.ID == 4));     // Canceled Job
            }

            // Closed Repair Order
            else if(currentStatus == 3)
            {
                returnList.Add(all.FirstOrDefault(o => o.ID == 5));     // Reopened
            }

            // Canceled Job
            else if (currentStatus == 4)
            {
                returnList.Add(all.FirstOrDefault(o => o.ID == 5));     // Reopened
            }

            // Reopened
            else if (currentStatus == 5)
            {
                returnList.Add(all.FirstOrDefault(o => o.ID == 2));     // Repair Order
                returnList.Add(all.FirstOrDefault(o => o.ID == 4));     // Canceled Job
            }

            return returnList;
        }

        private static object _loadLock = new object();
        private static List<JobStatus> _jobStatuses = null;

    }
}

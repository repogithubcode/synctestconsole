using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

using ProEstimatorData;

namespace ProEstimator.TimedEvents
{
    public class TimedEvent
    {

        public virtual TimeSpan TimeSpan { get { return new TimeSpan(1, 0, 0); } }
        public DateTime LastExecution { get; set; }
        public bool IsRunning { get; private set; }

        public TimedEvent()
        {
            LoadData();
        }

        protected virtual void LoadData()
        {
            //DBAccess dbAccess = new DBAccess();
            //DBAccessTableResult result = dbAccess.ExecuteWithTable("TimedEventData_Get", new SqlParameter("EventName", this.GetType().ToString()));
            //if (result.Success)
            //{
            //    LastExecution = InputHelper.GetDateTime(result.DataTable.Rows[0]["LastExecution"].ToString());
            //}
        }

        public bool ShouldExecute()
        {
            TimeSpan sinceLastExecute = DateTime.Now - LastExecution;
            if (!IsRunning && sinceLastExecute > TimeSpan)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Timed even must call this when its work is done so it will be scheduled to run again.
        /// </summary>
        protected void ExecutionFinished()
        {
            LastExecution = DateTime.Now;
            IsRunning = false;
        }

        public virtual void DoWork(System.Text.StringBuilder messageBuilder)
        {
            IsRunning = true;

            //List<SqlParameter> parameters = new List<SqlParameter>();
            //parameters.Add(new SqlParameter("EventName", this.GetType().ToString()));
            //parameters.Add(new SqlParameter("LastExecution", LastExecution));

            //DBAccess dbAccess = new DBAccess();
            //dbAccess.ExecuteNonQuery("TimedEventData_Save", parameters);
        }

        public virtual void Cancel() { }

    }
}
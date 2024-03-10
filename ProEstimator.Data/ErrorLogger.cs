using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.DataModel;

namespace ProEstimatorData
{
    public static class ErrorLogger
    {

        public static void LogError(string message, string messageTag)
        {
            LogError(message, 0, 0, messageTag);
        }

        /// <summary>
        /// Log an exception to the database.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="loginID">The logged in user.</param>
        /// <param name="estimateID">If available, the estimate being edited when the error occured.</param>
        /// <param name="errorTag">A tag to associate with this error.</param>
        public static void LogError(string message, int loginID, int estimateID, string errorTag)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("LoginID", loginID));
                parameters.Add(new SqlParameter("AdminInfoID", estimateID));
                parameters.Add(new SqlParameter("ErrorText", message));
                parameters.Add(new SqlParameter("ErrorTag", errorTag));

                using (SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.AppSettings["ProConnectionString"]))
                {
                    using (SqlCommand command = new SqlCommand("Error_Log", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        if (parameters != null && parameters.Count > 0)
                        {
                            command.Parameters.AddRange(parameters.ToArray());
                        }

                        connection.Open();

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Log an exception to the database.
        /// </summary>
        /// <param name="ex">The exception.  The message, stack trace, and inner exception will be logged if available.</param>
        /// <param name="loginID">The logged in user.</param>
        /// <param name="errorTag">A tag to associate with this error.</param>
        public static void LogError(Exception ex, int loginID, string errorTag)
        {
            LogError(ex, loginID, 0, errorTag);
        }

        /// <summary>
        /// Log an exception to the database.
        /// </summary>
        /// <param name="ex">The exception.  The message, stack trace, and inner exception will be logged if available.</param>
        /// <param name="loginID">The logged in user.</param>
        /// <param name="estimateID">If available, the estimate being edited when the error occured.</param>
        /// <param name="errorTag">A tag to associate with this error.</param>
        public static void LogError(Exception ex, int loginID, int estimateID, string errorTag)
        {
            string message = ex.Message + (ex.StackTrace != null ? Environment.NewLine + "Stack Trace: " + ex.StackTrace : "") + (ex.InnerException != null ? Environment.NewLine + "Inner Exception: " + ex.InnerException.Message : "");
            LogError(message, loginID, estimateID, errorTag);
        }

        /// <summary>
        /// Get a list of logged errors.  All parametes are optional.
        /// </summary>
        public static List<Error> GetErrors(int loginID, int estimateID, string errorTag, DateTime? rangeStart, DateTime? rangeEnd)
        {
            if (!rangeStart.HasValue)
            {
                rangeStart = new DateTime(2000, 1, 1);
            }

            if (!rangeEnd.HasValue)
            {
                rangeEnd = new DateTime(2030, 1, 1);
            }

            List<Error> results = new List<Error>();

            List<SqlParameter> parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("AdminInfoID", estimateID));
            parameters.Add(new SqlParameter("ErrorTag", errorTag));
            parameters.Add(new SqlParameter("RangeStart", rangeStart.Value));
            parameters.Add(new SqlParameter("RangeEnd", rangeEnd.Value));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("Error_GetList", parameters);
            if (result.Success && result.DataTable != null)
            {
                foreach(DataRow row in result.DataTable.Rows)
                {
                    results.Add(new Error(row));
                }
            }

            return results;
        }
        
        /// <summary>
        /// Write a line to log.txt in the root directory
        /// </summary>
        public static void WriteToLogFile(string value)
        {
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
                System.IO.File.AppendAllText(path, Environment.NewLine + DateTime.Now.ToString() + ": " + value);
            }
            catch { }
        }

    }
}
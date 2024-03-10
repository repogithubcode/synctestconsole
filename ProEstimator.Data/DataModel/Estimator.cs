using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace ProEstimatorData.DataModel
{
    public class Estimator : ProEstEntity 
    {
        public int ID { get; private set; }
        public int EstimateID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int LoginID { get; set; }
        public int OrderNumber { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }

        public bool DefaultEstimator { get; set; }

        public Estimator()
        {
            ID = 0;
            EstimateID = 0;
            FirstName = "";
            LastName = "";
            LoginID = 0;
            OrderNumber = 0;
            Email = "";
            Phone = "";
            DefaultEstimator = false;
        }

        public Estimator(DataRow row, int defaultEstimatorID)
        {
            ID = InputHelper.GetInteger(row["EstimatorID"].ToString());
            EstimateID = InputHelper.GetInteger(row["AdminInfoID"].ToString());
            FirstName = row["AuthorFirstName"].ToString();
            LastName = row["AuthorLastName"].ToString();
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            OrderNumber = InputHelper.GetInteger(row["OrderNo"].ToString());
            Email = row["Email"].ToString();
            Phone = row["Phone"].ToString();
            DefaultEstimator = ID == defaultEstimatorID;

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("EstimatorID", ID));
            parameters.Add(new SqlParameter("EstimateID", EstimateID));
            parameters.Add(new SqlParameter("AuthorFirstName", FirstName));
            parameters.Add(new SqlParameter("AuthorLastName", LastName));
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("OrderNo", OrderNumber));
            parameters.Add(new SqlParameter("Email", Email));
            parameters.Add(new SqlParameter("Phone", Phone));
            parameters.Add(new SqlParameter("ActiveLoginID", activeLoginID));
            parameters.Add(new SqlParameter("SetAsDefaultEstimator", DefaultEstimator));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("Estimator_Save", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "Estimator", ID, LoginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public static List<Estimator> GetByLogin(int loginID, int activeLoginID = 0)
        {
            List<Estimator> estimators = new List<Estimator>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Estimator_GetByLogin", new System.Data.SqlClient.SqlParameter("LoginID", loginID));

            if (tableResult.Success)
            {
                var defaultEstimatorID = GetDefaultEstimatorID(activeLoginID);
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    estimators.Add(new Estimator(row, defaultEstimatorID));
                }
            }

            return estimators;
        }

        public static Estimator Get(int id, int activeLoginID = 0)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Estimator_Get", new System.Data.SqlClient.SqlParameter("ID", id));

            if (tableResult.Success)
            {
                return new Estimator(tableResult.DataTable.Rows[0], GetDefaultEstimatorID(activeLoginID));
            }

            return null;
        }

        private static int GetDefaultEstimatorID(int activeLoginID)
        {
            var defaultEstimatorID = 0;
            if (activeLoginID == 0)
            {
                activeLoginID = InputHelper.GetInteger(HttpContext.Current.Items["ActiveLoginID"]?.ToString(), 0);
            }
            if (activeLoginID > 0)
            {
                DBAccess db = new DBAccess();
                var dbResult = db.ExecuteWithIntReturn("Estimator_GetDefaultByActiveLoginID", new SqlParameter("ActiveLoginID", activeLoginID));
                if (dbResult.Success)
                {
                    defaultEstimatorID = dbResult.Value;
                }
            }
            return defaultEstimatorID;
        }
    }
}

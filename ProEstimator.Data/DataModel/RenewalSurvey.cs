using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;
using ProEstimatorData;

namespace ProEstimatorData.DataModel
{
    public class RenewalSurvey : ProEstEntity 
    {
        public int ID { get; private set; }
        public int LoginID { get; set; }
        public int ContractID { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Reason { get; set; }
        public string Comments { get; set; }

        public RenewalSurvey()
        {

        }

        public static RenewalSurvey GetForContract(int contractID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult table = db.ExecuteWithTable("RenewalSurvey_GetByContract", new SqlParameter("ContractID", contractID));

            if (table.Success)
            {
                RenewalSurvey survey = new RenewalSurvey();
                survey.LoadData(table.DataTable.Rows[0]);
                return survey;
            }

            return null;
        }

        private void LoadData(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            ContractID = InputHelper.GetInteger(row["ContractID"].ToString());
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
            Reason = row["Reason"].ToString();
            Comments = row["Comments"].ToString();
        }

        public FunctionResult Insert()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("ContractID", ContractID));
            parameters.Add(new SqlParameter("TimeStamp", TimeStamp));
            parameters.Add(new SqlParameter("Reason", Reason));
            parameters.Add(new SqlParameter("Comments", Comments));

            DBAccess db = new DBAccess();
            return db.ExecuteNonQuery("RenewalSurvey_Insert", parameters);
        }
    }
}

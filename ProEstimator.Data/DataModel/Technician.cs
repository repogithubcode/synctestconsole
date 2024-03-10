using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class Technician : ProEstEntity 
    {
        public int ID { get; set; }
        public int EstimateID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int LoginID { get; set; }
        public int OrderNumber { get; set; }

        public int LaborTypeID { get; set; }
        public string LaborTypeText { get; set; }

        public DateTime? TimeStamp { get; set; }
        public Boolean IsDeleted { get; set; }

        public Technician()
        {
            ID = 0;
            EstimateID = 0;
            FirstName = "";
            LastName = "";
            LoginID = 0;
            OrderNumber = 0;
            LaborTypeID = 0;
            LaborTypeText = "";
        }

        public Technician(DataRow row)
        {
            ID = InputHelper.GetInteger(row["TechnicianID"].ToString());
            EstimateID = InputHelper.GetInteger(row["AdminInfoID"].ToString());
            FirstName = row["AuthorFirstName"].ToString();
            LastName = row["AuthorLastName"].ToString();
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            OrderNumber = InputHelper.GetInteger(row["OrderNo"].ToString());
            LaborTypeID = InputHelper.GetInteger(row["LaborTypeID"].ToString());
            LaborTypeText = row["LaborTypeText"].ToString();
            IsDeleted = InputHelper.GetBoolean(row["IsDeleted"].ToString());

            if (!string.IsNullOrEmpty(row["TimeStamp"].ToString()))
                TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("TechnicianID", ID));
            parameters.Add(new SqlParameter("EstimateID", EstimateID));
            parameters.Add(new SqlParameter("FirstName", FirstName));
            parameters.Add(new SqlParameter("LastName", LastName));
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("OrderNo", OrderNumber));
            parameters.Add(new SqlParameter("LaborTypeID", LaborTypeID));
            parameters.Add(new SqlParameter("LaborTypeText", LaborTypeText));
            parameters.Add(new SqlParameter("DeleteStamp", TimeStamp));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("Technician_Save", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "Technician", ID, LoginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public static List<Technician> GetByLogin(int loginID, string showDeletedTechnicians = "0")
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("IsDeleted", InputHelper.GetBoolean(showDeletedTechnicians)));

            List<Technician> Technicians = new List<Technician>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Technician_GetByLogin", parameters);

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    Technicians.Add(new Technician(row));
                }
            }

            return Technicians;
        }

        public static Technician Get(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Technician_Get", new System.Data.SqlClient.SqlParameter("ID", id));

            if (tableResult.Success)
            {
                return new Technician(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public void Delete(int activeLoginID = 0)
        {
            TimeStamp = DateTime.Now;
            Save(activeLoginID);
        }

        public void Restore(int activeLoginID = 0)
        {
            TimeStamp = null;
            Save(activeLoginID);
        }

        public SaveResult SavePrintTechniciansMapping(int activeLoginID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("EstimateID", EstimateID));
            parameters.Add(new SqlParameter("LaborTypeID", LaborTypeID));
            parameters.Add(new SqlParameter("TechnicianID", ID));
            
            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("PrintTechniciansMapping_Update", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "SavePrintTechniciansMapping", ID, LoginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public static List<Technician> GetPrintTechniciansMapping(int? estimateID = null, int? laborTypeID = null, int? id = null)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("EstimateID", estimateID));
            parameters.Add(new SqlParameter("LaborTypeID", laborTypeID));
            parameters.Add(new SqlParameter("TechnicianID", id));

            List<Technician> Technicians = new List<Technician>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("PrintTechniciansMapping_Get", parameters);

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    Technician technician = new Technician();

                    technician.ID = InputHelper.GetInteger(row["TechnicianID"].ToString());
                    technician.EstimateID = InputHelper.GetInteger(row["EstimateID"].ToString());
                    technician.LaborTypeID = InputHelper.GetInteger(row["LaborTypeID"].ToString());

                    Technicians.Add(technician);
                }
            }

            return Technicians;
        }
    }
}

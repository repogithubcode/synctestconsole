using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class InsuranceCompany : ProEstEntity
    {

        public int ID { get; private set; }
        public int LoginID { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }

        public InsuranceCompany()
        {

        }

        public InsuranceCompany(int insuranceCompanyID)
        {
            this.ID = insuranceCompanyID;
        }

        public InsuranceCompany(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            Name = InputHelper.GetString(row["Name"].ToString());
            IsDeleted = InputHelper.GetBoolean(row["IsDeleted"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("Name", Name));
            parameters.Add(new SqlParameter("IsDeleted", IsDeleted));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("InsuranceCompany_Update", parameters);
            if (result.Success)
            {
                ID = result.Value;
                
                ChangeLogManager.LogChange(activeLoginID, "InsuranceCompany", ID, LoginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public static InsuranceCompany Get(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("InsuranceCompany_Get", new SqlParameter("ID", id));

            if (tableResult.Success)
            {
                return new InsuranceCompany(tableResult.DataTable.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        public static List<InsuranceCompany> GetForLogin(int loginID, bool includeDeleted = false)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("ShowDeleted", includeDeleted));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("InsuranceCompany_GetAll", parameters);

            List<InsuranceCompany> companies = new List<InsuranceCompany>();

            if (tableResult.Success)
            {
                foreach(DataRow row in tableResult.DataTable.Rows)
                {
                    companies.Add(new InsuranceCompany(row));
                }
            }

            return companies;
        }

    }
}

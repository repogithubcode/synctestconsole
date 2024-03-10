using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class InsuranceCompanyEmployee : ProEstEntity
    {

        public int ID { get; private set; }
        public int LoginID { get; set; }
        public int InsuranceCompanyID { get; set; }
        public InsuranceCompanyEmployeeType EmployeeType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Extension { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }

        public bool IsDeleted { get; set; }

        public InsuranceCompanyEmployee()
        {

        }

        public InsuranceCompanyEmployee(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            InsuranceCompanyID = InputHelper.GetInteger(row["InsuranceCompanyID"].ToString());
            EmployeeType = (InsuranceCompanyEmployeeType)Enum.Parse(typeof(InsuranceCompanyEmployeeType), row["EmployeeType"].ToString());
            FirstName = InputHelper.GetString(row["FirstName"].ToString());
            LastName = InputHelper.GetString(row["LastName"].ToString());
            Phone = InputHelper.GetString(row["Phone"].ToString());
            Extension = InputHelper.GetString(row["Extension"].ToString());
            Fax = InputHelper.GetString(row["Fax"].ToString());
            Email = InputHelper.GetString(row["Email"].ToString());
            IsDeleted = InputHelper.GetBoolean(row["IsDeleted"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("InsuranceCompanyID", InsuranceCompanyID));
            parameters.Add(new SqlParameter("EmployeeType", (int)EmployeeType));
            parameters.Add(new SqlParameter("FirstName", FirstName));
            parameters.Add(new SqlParameter("LastName", LastName));
            parameters.Add(new SqlParameter("Phone", Phone));
            parameters.Add(new SqlParameter("Extension", Extension));
            parameters.Add(new SqlParameter("Fax", Fax));
            parameters.Add(new SqlParameter("Email", Email));
            parameters.Add(new SqlParameter("IsDeleted", IsDeleted));

            DBAccess db = new DBAccess();
            DBAccessIntResult saveResult = db.ExecuteWithIntReturn("InsuranceCompanyEmployee_Update", parameters);
            if (saveResult.Success)
            {
                ID = saveResult.Value;
                ChangeLogManager.LogChange(activeLoginID, "InsCompanyEmployee", ID, LoginID, parameters, RowAsLoaded);
            }

            return new SaveResult(saveResult);
        }

        public static InsuranceCompanyEmployee Get(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("InsuranceCompanyEmployee_Get", new SqlParameter("ID", id));
            if (tableResult.Success)
            {
                return new InsuranceCompanyEmployee(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public static List<InsuranceCompanyEmployee> GetForCompany(int companyID, InsuranceCompanyEmployeeType employeeType, bool includeDeleted = false)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("CompanyID", companyID));
            parameters.Add(new SqlParameter("EmployeeType", (int)employeeType));
            parameters.Add(new SqlParameter("IsDeleted", includeDeleted));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("InsuranceCompanyEmployee_GetForCompany", parameters);

            List<InsuranceCompanyEmployee> employees = new List<InsuranceCompanyEmployee>();

            if (tableResult.Success)
            {
                foreach(DataRow row in tableResult.DataTable.Rows)
                {
                    employees.Add(new InsuranceCompanyEmployee(row));
                }
            }

            return employees;
        }
    }

    public enum InsuranceCompanyEmployeeType
    {
          Adjuster = 1
        , ClaimRep = 2
    }
}

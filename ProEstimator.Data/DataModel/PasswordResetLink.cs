using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace ProEstimatorData.DataModel
{
    public class PasswordResetLink : ProEstEntity
    {
        public int ID { get; private set; }
        public string Code { get; set; }
        public int SiteUserID { get; set; }
        public DateTime TimeStamp { get; private set; }
        public bool Used { get; set; }

        public PasswordResetLink()
        {
            TimeStamp = DateTime.Now;
        }

        public PasswordResetLink(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            Code = InputHelper.GetString(row["Code"].ToString());
            SiteUserID = InputHelper.GetInteger(row["SiteUserID"].ToString());
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
            Used = InputHelper.GetBoolean(row["Used"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("Code", Code));
            parameters.Add(new SqlParameter("SiteUserID", SiteUserID));
            parameters.Add(new SqlParameter("TimeStamp", TimeStamp));
            parameters.Add(new SqlParameter("Used", Used));

            DBAccess db = new DBAccess();
            DBAccessIntResult intResult = db.ExecuteWithIntReturn("PasswordResetLink_Update", parameters);

            if (intResult.Success)
            {
                ID = intResult.Value;
                ChangeLogManager.LogChange(activeLoginID, "PasswordResetLink", ID, 0, parameters, RowAsLoaded);
            }

            return new SaveResult(intResult);
        }

        public static PasswordResetLink GetByCode(string code)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("PasswordResetLink_GetByCode", new SqlParameter("Code", code));
            
            if (tableResult.Success)
            {
                return new PasswordResetLink(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

    }
}

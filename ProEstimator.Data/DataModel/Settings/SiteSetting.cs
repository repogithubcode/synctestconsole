using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Settings
{
    public class SiteSetting
    {
        public int ID { get; private set; }
        public int LoginID { get; set; }
        public string TagGroup { get; set; }
        public string Tag { get; set; }
        public string ValueString { get; set; }

        private string _valueStringLoaded = "";

        public SiteSetting()
        {
            Tag = "";
            ValueString = "";
        }

        public SiteSetting(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            TagGroup = InputHelper.GetString(row["TagGroup"].ToString());
            Tag = InputHelper.GetString(row["Tag"].ToString());
            ValueString = InputHelper.GetString(row["Value"].ToString());

            _valueStringLoaded = ValueString;
        }

        public SaveResult Save(int activeLoginID)
        {
            if (_valueStringLoaded != ValueString)
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("ID", ID));
                parameters.Add(new SqlParameter("LoginID", LoginID));
                parameters.Add(new SqlParameter("TagGroup", TagGroup));
                parameters.Add(new SqlParameter("Tag", Tag));
                parameters.Add(new SqlParameter("Value", ValueString));

                DBAccess db = new DBAccess();
                DBAccessIntResult result = db.ExecuteWithIntReturn("SiteSetting_Save", parameters);
                if (result.Success)
                {
                    ID = result.Value;

                    if (activeLoginID > 0)
                    {
                        ChangeLogManager.LogChange(activeLoginID, "SiteSetting", ID, LoginID, _valueStringLoaded, ValueString);
                    }
                    
                    _valueStringLoaded = ValueString;
                }

                return new SaveResult(result);
            }

            return new SaveResult();
        }

        public static SiteSetting GetByTag(int loginID, string tag)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("Tag", tag));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("SiteSetting_GetByTag", parameters);
            if (tableResult.Success)
            {
                return new SiteSetting(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

    }
}
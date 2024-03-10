using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimatorData.DataModel
{
    public enum OptOutType
    {
        AutoPayFail = 1
    }

    public class OptOutLog
    {

        public int ID { get; private set; }
        public int LoginID { get; set; }
        public OptOutType OptOutType { get; set; }
        public int DetailID { get; set; }
        public DateTime CreateStamp { get; set; }
        public DateTime? DeleteStamp { get; set; }
        public bool IsDeleted { get { return DeleteStamp != null; } }

        public OptOutLog()
        {

        }

        public OptOutLog(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            OptOutType = (OptOutType)InputHelper.GetInteger(row["TypeID"].ToString());
            DetailID = InputHelper.GetInteger(row["DetailID"].ToString());
            CreateStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());

            DateTime? loadedDeleteDate = InputHelper.GetNullableDateTime(row["DeleteStamp"].ToString());
            if (loadedDeleteDate.HasValue)
            {
                DeleteStamp = loadedDeleteDate.Value;
            }
        }

        public static void Insert(int loginID, OptOutType optOutType, int detailID = 0)
        {
            List<SqlParameter> paramters = new List<SqlParameter>();
            paramters.Add(new SqlParameter("LoginID", loginID));
            paramters.Add(new SqlParameter("TypeID", optOutType));
            paramters.Add(new SqlParameter("DetailID", detailID));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("OptOut_Insert", paramters);
        }

        public static List<OptOutLog> Get(int loginID, OptOutType optOutType, int detailID = 0)
        {
            List<SqlParameter> paramters = new List<SqlParameter>();
            paramters.Add(new SqlParameter("LoginID", loginID));
            paramters.Add(new SqlParameter("TypeID", optOutType));
            paramters.Add(new SqlParameter("DetailID", detailID));

            DBAccess db = new DBAccess();
            DBAccessTableResult dataResult = db.ExecuteWithTable("OptOut_Get", paramters);

            List<OptOutLog> returnList = new List<OptOutLog>();

            foreach(DataRow row in dataResult.DataTable.Rows)
            {
                returnList.Add(new OptOutLog(row));
            }

            return returnList;
        }

    }
}
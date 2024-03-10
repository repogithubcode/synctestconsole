using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Admin
{
    /// <summary>
    /// A customer is made up of a Contact and an Address. 
    /// A customer can have multiple estimates assoticated with it.
    /// </summary>
    public class SuccessBox
    {

        public int ID { get; private set; }
        public int LoginID { get; set; }
        public DateTime TimeStamp { get; set; }

        public SuccessBox()
        {
            ID = 0;
            LoginID = 0;
            TimeStamp = DateTime.MinValue;
        }

        public static FunctionResult CheckSuccessBoxByLogin(int loginID)
        {
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));

            DBAccessIntResult result = db.ExecuteWithIntReturn("CheckSuccessBoxByLogin", parameters, CommandType.StoredProcedure);
            
            if (result.Value == 0)
            {
               return new FunctionResult();
            }
            else
            {
                return new FunctionResult(false, "LoginID already exists.");
            }
        }

        public static FunctionResult Save(int loginID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("TimeStamp", DateTime.Now));

            DBAccess db = new DBAccess();
            return db.ExecuteNonQuery("InsertSuccessBoxSync", parameters,true);
        }
    }
}

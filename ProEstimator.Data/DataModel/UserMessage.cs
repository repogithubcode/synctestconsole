using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class UserMessage : ProEstEntity
    {

        public int ID { get; private set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedStamp { get; set; }
        public bool IsPermanent { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public bool SeenByUser { get; private set; }

        public UserMessage()
        {

        }

        public UserMessage(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            Title = InputHelper.GetString(row["Title"].ToString());
            Message = InputHelper.GetString(row["Message"].ToString());
            StartDate = InputHelper.GetDateTime(row["StartDate"].ToString()).Date;
            EndDate = InputHelper.GetDateTime(row["EndDate"].ToString()).Date;
            CreatedStamp = InputHelper.GetDateTime(row["CreatedDate"].ToString());
            IsPermanent = InputHelper.GetBoolean(row["IsPermanent"].ToString());
            IsActive = InputHelper.GetBoolean(row["IsActive"].ToString());
            IsDeleted = InputHelper.GetBoolean(row["IsDeleted"].ToString());

            try
            {
                SeenByUser = InputHelper.GetBoolean(row["SeenByUser"].ToString());
            }
            catch { }

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("Title", Title));
            parameters.Add(new SqlParameter("Message", Message));
            parameters.Add(new SqlParameter("StartDate", StartDate));
            parameters.Add(new SqlParameter("EndDate", EndDate));
            parameters.Add(new SqlParameter("CreatedDate", CreatedStamp));
            parameters.Add(new SqlParameter("IsPermanent", IsPermanent));
            parameters.Add(new SqlParameter("IsActive", IsActive));
            parameters.Add(new SqlParameter("IsDeleted", IsDeleted));

            DBAccess db = new DBAccess();
            DBAccessIntResult saveResult = db.ExecuteWithIntReturn("UserMessages_Update", parameters);
            if (saveResult.Success)
            {
                ID = saveResult.Value;
                ChangeLogManager.LogChange(activeLoginID, "UserMessage", ID, 0, parameters, RowAsLoaded);
                return new SaveResult();
            }
            else
            {
                return new SaveResult(saveResult.ErrorMessage);
            }
        }

        public static void MarkSeen(int messageID, int userID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("MessageID", messageID));
            parameters.Add(new SqlParameter("UserID", userID));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("UserMessages_MarkSeen", parameters);
        }

        public static UserMessage Get(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("UserMessages_Get", new SqlParameter("ID", id));
            if (tableResult.Success)
            {
                return new UserMessage(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public static List<UserMessage> GetAll()
        {
            List<UserMessage> messages = new List<UserMessage>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("UserMessages_GetAll");
            if (tableResult.Success)
            {
                foreach(DataRow row in tableResult.DataTable.Rows)
                {
                    messages.Add(new UserMessage(row));
                }
            }

            return messages;
        }

        public static List<UserMessage> GetForUser(int userID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("UserMessages_GetForUser", new SqlParameter("UserID", userID));

            List<UserMessage> messages = new List<UserMessage>();

            foreach(DataRow row in tableResult.DataTable.Rows)
            {
                messages.Add(new UserMessage(row));
            }

            return messages;
        }

        public static int GetUnseenCount(int userID)
        {
            DBAccess db = new DBAccess();
            DBAccessIntResult intResult = db.ExecuteWithIntReturn("UserMessages_GetUnseenCountForUser", new SqlParameter("UserID", userID));
            return intResult.Value;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace ProEstimatorData.DataModel
{
    public class SiteUser : ProEstEntity
    {

        public int ID { get; private set; }
        public int LoginID { get; set; }
        public string EmailAddress { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }

        private string _passwordEncrypted = "";

        public string Password
        {
            get { return ProEstimatorData.Encryptor.Decrypt(_passwordEncrypted); }
            set { _passwordEncrypted = ProEstimatorData.Encryptor.Encrypt(value); }
        }

        public SiteUser()
        {
        }

        public SiteUser(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            EmailAddress = InputHelper.GetString(row["EmailAddress"].ToString());
            Name = InputHelper.GetString(row["Name"].ToString());
            _passwordEncrypted = InputHelper.GetString(row["Password"].ToString());
            IsDeleted = InputHelper.GetBoolean(row["IsDeleted"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            // Validate
            if (string.IsNullOrEmpty(EmailAddress) || EmailAddress.Length < 6)
            {
                return new SaveResult("Email Address must be at least 6 characters.");   
            }

            if (IsEmailAddressTaken(EmailAddress, ID))
            {
                return new SaveResult("That Email Address is already taken.");
            }

            // Save
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("EmailAddress", InputHelper.GetString(EmailAddress)));
            parameters.Add(new SqlParameter("Name", InputHelper.GetString(Name)));
            parameters.Add(new SqlParameter("Password", _passwordEncrypted));
            parameters.Add(new SqlParameter("IsDeleted", IsDeleted));

            DBAccess db = new DBAccess();
            DBAccessIntResult intResult = db.ExecuteWithIntReturn("SiteUsers_Update", parameters);

            if (intResult.Success)
            {
                ID = intResult.Value;

                ChangeLogManager.LogChange(activeLoginID, "SiteUser", ID, LoginID, parameters, RowAsLoaded);
            }

            return new SaveResult(intResult);
        }

        public static bool IsEmailAddressTaken(string emailAddress, int ignoreID = 0)
        {
            List<SiteUser> usersWithName = SiteUser.GetForEmailAddress(emailAddress);
            foreach(SiteUser user in usersWithName)
            {
                if (user.ID != ignoreID && !user.IsDeleted)
                {
                    return true;
                }
            }

            return false;
        }

        public static SiteUser Get(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("SiteUsers_GetByID", new SqlParameter("ID", id));

            if (tableResult.Success)
            {
                return new SiteUser(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public static List<SiteUser> GetForEmailAddress(string emailAddress)
        {
            List<SiteUser> users = new List<SiteUser>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("SiteUsers_GetByEmailAddress", new SqlParameter("EmailAddress", emailAddress));

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                users.Add(new SiteUser(row));
            }

            return users;
        }

        public static SiteUser GetForCredentials(string emailAddress, string password)
        {
            string hashed = HashPassword(password);

            List<SiteUser> users = GetForEmailAddress(emailAddress);

            foreach(SiteUser user in users)
            {
                if (!user.IsDeleted && user.Password == hashed)
                {
                    return user;
                }
            }

            return null;
        }

        public static List<SiteUser> GetForLogin(int loginID, bool includeDeleted = false)
        {
            List<SiteUser> users = new List<SiteUser>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("SiteUsers_GetByLoginID", new SqlParameter("LoginID", loginID));

            foreach(DataRow row in tableResult.DataTable.Rows)
            {
                SiteUser siteUser = new SiteUser(row);
                if (!siteUser.IsDeleted || includeDeleted)
                {
                    users.Add(siteUser);
                }
            }

            return users;
        }

        public static List<SiteUser> GetForSearch(string search, bool includeDeleted = false)
        {
            List<SiteUser> users = new List<SiteUser>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("Search", search));
            parameters.Add(new SqlParameter("IncludeDeleted", includeDeleted));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("SiteUsers_GetBySearch", parameters);

            foreach(DataRow row in tableResult.DataTable.Rows)
            {
                users.Add(new SiteUser(row));
            }

            return users;
        }

        public static string HashPassword(string password)
        {
            // Add some random text to all passwords to make it harder to match a hash.
            password = "P$#E" + password;

            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString().Substring(0, 20);
            }
        }  
    }
}

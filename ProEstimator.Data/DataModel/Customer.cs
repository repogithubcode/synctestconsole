using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    /// <summary>
    /// A customer is made up of a Contact and an Address. 
    /// A customer can have multiple estimates assoticated with it.
    /// </summary>
    public class Customer : ProEstEntity
    {

        public int ID { get; private set; }
        public int LoginID { get; set; }
        public bool IsDeleted { get; set; }

        public Contact Contact { get; private set; }
        public Address Address { get; private set; }

        public Customer()
        {
            ID = 0;
            LoginID = 0;
            IsDeleted = false;

            Contact = new Contact();
            Address = new Address();
        }

        public static Customer Get(int customerID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("CustomerGet", new SqlParameter("ID", customerID));

            if (result.Success)
            {
                return new Customer(result.DataTable.Rows[0]);
            }

            return null;
        }

        public static List<Customer> GetForLogin(int loginID)
        {
            List<Customer> customerList = new List<Customer>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("CustomerGetByLogin", new SqlParameter("LoginID", loginID));

            if (tableResult.Success)
            {
                foreach(DataRow row in tableResult.DataTable.Rows)
                {
                    customerList.Add(new Customer(row));
                }
            }

            return customerList;
        }

        public static List<Customer> GetForDate(int loginID, DateTime startDate, DateTime endDate, bool closedOnly)
        {
            List<Customer> customerList = new List<Customer>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("StartDate", startDate));
            parameters.Add(new SqlParameter("EndDate", endDate));
            parameters.Add(new SqlParameter("ClosedOnly", closedOnly));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("CustomerGetByDate", parameters);
            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    customerList.Add(new Customer(row));
                }
            }
            return customerList;
        }

        public Customer(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            IsDeleted = InputHelper.GetBoolean(row["IsDeleted"].ToString());

            Contact = new Contact(row);
            Address = new Address(row);

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            SaveResult contactSave = Contact.Save(activeLoginID, LoginID);
            if (!contactSave.Success)
            {
                return contactSave;
            }

            SaveResult addressSave = Address.Save(activeLoginID, LoginID);
            if (!addressSave.Success)
            {
                return addressSave;
            }

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("IsDeleted", IsDeleted));
            parameters.Add(new SqlParameter("ContactID", Contact.ContactID));
            parameters.Add(new SqlParameter("AddressID", Address.AddressID));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("CustomerUpdate", parameters);

            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "Customer", ID, LoginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result.ErrorMessage);
        }
    }
}

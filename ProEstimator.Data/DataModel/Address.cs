using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel
{
    public class Address : ProEstEntity 
    {
        public int AddressID { get; private set; }
        public int ContactID { get; set; }
        public int EstimateID { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string TimeZone { get; set; }

        public Address() { }

        public static Address GetForContact(int contactID)
        {
            if (contactID > 0)
            {
                DBAccess db = new DBAccess();
                DBAccessTableResult result = db.ExecuteWithTable("sp_GetTblAddress", new System.Data.SqlClient.SqlParameter("ContactsID", SqlDbType.Int) { Value = contactID });
                if (result.Success)
                {
                    return new Address(result.DataTable.Rows[0]);
                }
            }
            
            // No address in the database, return an empty address record
            Address emptyAddress = new Address();
            emptyAddress.ContactID = contactID;
            return emptyAddress;
        }

        public static Address GetForLoginID(int loginID)
        {
            Contact contact = Contact.GetContactForLogins(loginID);
            Address address = Address.GetForContact(contact.ContactID);
            return address;
        }

        public Address(DataRow row)
        {
            AddressID = InputHelper.GetInteger(row["AddressID"].ToString());
            ContactID = InputHelper.GetInteger(row["ContactsID"].ToString());
            EstimateID = InputHelper.GetInteger(row["AdminInfoID"].ToString());
            Line1 = row["Address1"].ToString();
            Line2 = row["Address2"].ToString();
            City = row["City"].ToString();
            State = row["State"].ToString();
            Zip = row["zip"].ToString();
            TimeZone = row["TimeZone"].ToString();

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("AddressID", AddressID));
            parameters.Add(new SqlParameter("AdmininfoID", EstimateID));
            parameters.Add(new SqlParameter("ContactsID", ContactID));
            parameters.Add(new SqlParameter("Address1", GetString(Line1)));
            parameters.Add(new SqlParameter("Address2", GetString(Line2)));
            parameters.Add(new SqlParameter("City", GetString(City)));
            parameters.Add(new SqlParameter("State", GetString(State)));
            parameters.Add(new SqlParameter("Country", ""));
            parameters.Add(new SqlParameter("zip", GetString(Zip)));
            parameters.Add(new SqlParameter("TimeZone", GetString(TimeZone)));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddressUpdate", parameters);
            if (result.Success)
            {
                AddressID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "Address", AddressID, loginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public bool NotValid()
        {
            if (string.IsNullOrEmpty(Line1) && string.IsNullOrEmpty(Line2) && string.IsNullOrEmpty(City) && string.IsNullOrEmpty(State) && string.IsNullOrEmpty(Zip))
            {
                return true;
            }
            return false;
        }
    }
}
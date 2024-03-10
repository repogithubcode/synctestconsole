using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel
{
    public class Contact : ProEstEntity 
    {
        public int ContactID { get; set; }
        public int EstimateID { get; set; }
        public string Email { get; set; }
        public string SecondaryEmail { get; set; }
        public string Fax { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Phone2 { get; set; }
        public string Phone3 { get; set; }
        public string PhoneNumberType1 { get; set; }
        public string PhoneNumberType2 { get; set; }
        public string PhoneNumberType3 { get; set; } 
        public string Extension1 { get; set; }
        public string Extension2 { get; set; }
        public string Extension3 { get; set; }
        public string BusinessName { get; set; }
        public string Notes { get; set; }
        public bool CustomerSaved { get; set; }
        public ContactType ContactType { get; set; }
        public ContactSubType ContactSubType { get; set; }
        public string Title { get; set; }

        private bool _exists = false;

        public Contact() { }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ContactID", ContactID));
            parameters.Add(new SqlParameter("AdmininfoID", EstimateID));
            parameters.Add(new SqlParameter("FirstName", GetString(FirstName)));
            parameters.Add(new SqlParameter("MiddleName", ""));
            parameters.Add(new SqlParameter("LastName", GetString(LastName)));
            parameters.Add(new SqlParameter("Email", GetString(Email)));
            parameters.Add(new SqlParameter("SecondaryEmail", GetString(SecondaryEmail)));
            parameters.Add(new SqlParameter("Phone1", GetString(Phone)));
            parameters.Add(new SqlParameter("Extension1", GetString(Extension1)));
            parameters.Add(new SqlParameter("Phone2", GetString(Phone2)));
            parameters.Add(new SqlParameter("Phone3", GetString(Phone3)));
            parameters.Add(new SqlParameter("PhoneNumberType1", GetString(PhoneNumberType1)));
            parameters.Add(new SqlParameter("PhoneNumberType2", GetString(PhoneNumberType2)));
            parameters.Add(new SqlParameter("PhoneNumberType3", GetString(PhoneNumberType3)));
            parameters.Add(new SqlParameter("Extension2", GetString(Extension2)));
            parameters.Add(new SqlParameter("Extension3", GetString(Extension3)));
            parameters.Add(new SqlParameter("FaxNumber", GetString(Fax)));
            parameters.Add(new SqlParameter("BusinessName", GetString(BusinessName)));
            parameters.Add(new SqlParameter("Notes", GetString(Notes)));
            parameters.Add(new SqlParameter("Title", GetString(Title)));
            parameters.Add(new SqlParameter("SaveCustomer", CustomerSaved));
            parameters.Add(new SqlParameter("ContactTypeID", (byte)ContactType));
            parameters.Add(new SqlParameter("ContactSubTypeID", (short)ContactSubType));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddOrUpdateContact", parameters);
            if (result.Success)
            {
                ContactID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "Contact", ContactID, loginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        } 

        #region Static Get Functions

        /// <summary>
        /// Get a Contact by their database ID
        /// </summary>
        public static Contact GetContact(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("Contact_GetByID", new SqlParameter("ID", id));
            
            if (result.Success)
            {
                return new Contact(result.DataTable.Rows[0]);
            }

            return null;
        }

        /// <summary>
        /// Get a Contact associated with an Estimate.  Pass just the sub type, the Contact Type is set to Person.
        /// </summary>
        public static Contact GetContact(int estimateID, ContactSubType contactType)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("estimateID", estimateID));
            parameters.Add(new SqlParameter("contactSubTypeID", (short)contactType));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("Contact_GetByEstimateAndSubType", parameters);

            if (result.Success)
            {
                return new Contact(result.DataTable.Rows[0]);
            }
            else
            {
                Contact contact = new Contact();

                contact.ContactID = 0;
                contact.EstimateID = estimateID;
                contact.ContactType = ContactType.Person;
                contact.ContactSubType = contactType;

                return contact;
            }
        }

        /// <summary>
        /// Get the Contact associated with the passed Logins ID, this is the information for the user logged into the site.  
        /// This creates and associates the contact with the login if it doesn't already exist, so it will always return a good Contact instance.
        /// </summary>
        public static Contact GetContactForLogins(int loginsID)
        {
            LoginInfo loginInfo = LoginInfo.GetByID(loginsID);
            if (loginInfo != null)
            {
                return Contact.GetContact(loginInfo.ContactID);
            }

            return null;
        }

        public Contact(DataRow row)
        {
            ContactID = InputHelper.GetInteger(row["ContactID"].ToString());
            EstimateID = InputHelper.GetInteger(row["AdminInfoID"] == null ? "0" : row["AdminInfoID"].ToString());
            Email = row["Email"].ToString();
            SecondaryEmail = row["SecondaryEmail"].ToString();
            Fax = row["FaxNumber"].ToString();
            FirstName = row["FirstName"].ToString();
            LastName = row["LastName"].ToString();
            Phone = InputHelper.GetNumbersOnly(row["Phone1"].ToString());
            Phone2 = InputHelper.GetNumbersOnly(row["Phone2"].ToString());
            Phone3 = InputHelper.GetNumbersOnly(row["Phone3"].ToString());
            PhoneNumberType1 = InputHelper.GetString(row["PhoneNumberType1"].ToString()).Trim();
            PhoneNumberType2 = InputHelper.GetString(row["PhoneNumberType2"].ToString()).Trim();
            PhoneNumberType3 = InputHelper.GetString(row["PhoneNumberType3"].ToString()).Trim();
            Extension1 = row["Extension1"].ToString();
            Extension2 = row["Extension2"].ToString();
            Extension3 = row["Extension3"].ToString();
            BusinessName = row["BusinessName"].ToString();
            Notes = row["Notes"].ToString();
            CustomerSaved = InputHelper.GetBoolean(row["SaveCustomer"].ToString());
            ContactType = (ContactType)InputHelper.GetInteger(row["ContactTypeID"].ToString(), 1);
            ContactSubType = (ContactSubType)InputHelper.GetInteger(row["ContactSubTypeID"].ToString());
            Title = row["Title"].ToString();

            RowAsLoaded = row;

            _exists = true;
        }

       

        #endregion               

        /// <summary>
        /// Delete Customer 
        /// </summary>
        /// <param name="ContactID"></param>
        /// <returns></returns>
        public static bool Delete(int ContactID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ContactID", ContactID));

            DBAccess db = new DBAccess();
            var sql = "delete from tbl_ContactPerson where ContactID=@ContactID;delete from tbl_Address where ContactsID=@ContactID";  
            var result = db.ExecuteNonQuery(sql, parameters, false);
            return result.Success;
        }

        public bool NotValid()
        {
            if(string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName) && string.IsNullOrEmpty(Email) && string.IsNullOrEmpty(SecondaryEmail)
                && string.IsNullOrEmpty(Phone) && string.IsNullOrEmpty(Extension1) && string.IsNullOrEmpty(PhoneNumberType1)
                && string.IsNullOrEmpty(Phone2) && string.IsNullOrEmpty(Extension2) && string.IsNullOrEmpty(PhoneNumberType2)
                && string.IsNullOrEmpty(Phone3) && string.IsNullOrEmpty(Extension3) && string.IsNullOrEmpty(PhoneNumberType3)
                && string.IsNullOrEmpty(BusinessName) && string.IsNullOrEmpty(Notes))
            {
                return true;
            }
            return false;
        }
     
    }
}
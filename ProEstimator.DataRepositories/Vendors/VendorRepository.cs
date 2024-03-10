using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using ProEstimatorData;
using ProEstimatorData.DataModel;

namespace ProEstimator.DataRepositories.Vendors
{
    public class VendorRepository : IVendorRepository
    {
        public Vendor Get(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetVendorByID", new System.Data.SqlClient.SqlParameter("ID", SqlDbType.Int) { Value = id });
            if (result.Success)
            {
                Vendor vendor = InstantiateVendor(result.DataTable.Rows[0]);
                return vendor;
            }

            return null;
        }

        public List<Vendor> GetUniversalList(int loginID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));

            List<Vendor> vendors = new List<Vendor>();

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("Vendors_GetUniversal", parameters);
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    Vendor vendor = InstantiateVendor(row);
                    vendors.Add(vendor);
                }
            }

            return vendors;
        }

        public List<Vendor> GetPrivateList(int loginID, VendorType vendorType)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("TypeID", (int)vendorType));

            List<Vendor> vendors = new List<Vendor>();

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("Vendors_GetPrivate", parameters);
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    Vendor vendor = InstantiateVendor(row);
                    vendors.Add(vendor);
                }
            }

            return vendors;
        }

        private Vendor InstantiateVendor(DataRow row)
        {
            Vendor vendor = new Vendor();

            ((IIDSetter)vendor).ID = InputHelper.GetInteger(row["ID"].ToString());
            vendor.LoginsID = InputHelper.GetInteger(row["LoginsID"].ToString());
            vendor.CompanyIDCode = InputHelper.GetInteger(row["CompanyIDCode"].ToString());
            vendor.CompanyName = row["CompanyName"].ToString();
            vendor.FirstName = row["FirstName"].ToString();
            vendor.LastName = row["LastName"].ToString();
            vendor.Email = row["Email"].ToString();
            vendor.MobilePhone = row["MobileNumber"].ToString();
            vendor.WorkPhone = row["WorkNumber"].ToString();
            vendor.Address1 = row["Address1"].ToString();
            vendor.Address2 = row["Address2"].ToString();
            vendor.City = row["City"].ToString();
            vendor.State = InputHelper.GetString(row["State"].ToString()).Trim();
            vendor.Zip = row["Zip"].ToString();
            vendor.TimeZone = row["TimeZone"].ToString();
            vendor.Universal = InputHelper.GetBoolean(row["Universal"].ToString());
            vendor.Type = (VendorType)InputHelper.GetInteger(row["TypeID"].ToString());
            vendor.FaxNumber = row["FaxNumber"].ToString();
            vendor.FileName = row["FileName"].ToString();
            vendor.Deleted = InputHelper.GetBoolean(row["Deleted"].ToString());
            vendor.Extension = row["Extension"].ToString();

            vendor.FederalTaxID = row["FederalTaxID"].ToString();
            vendor.LicenseNumber = row["LicenseNumber"].ToString();
            vendor.BarNumber = row["BarNumber"].ToString();
            vendor.RegistrationNumber = row["RegistrationNumber"].ToString();

            if (row.Table.Columns.Contains("Selected"))
            {
                vendor.IsSelected = InputHelper.GetBoolean(row["Selected"].ToString());
            }

            vendor.RowAsLoaded = row;

            return vendor;
        }

        public List<Vendor> GetAllForType(int loginID, VendorType vendorType)
        {
            List<Vendor> vendors = new List<Vendor>();
            vendors.AddRange(GetPrivateList(loginID, vendorType));

            if (vendorType == VendorType.AfterMarket)
            {
                vendors.AddRange(GetUniversalList(loginID).Where(o => o.IsSelected).ToList());
            }

            return vendors.OrderBy(o => o.CompanyName).ToList();
        }

        public SaveResult Save(Vendor vendor, int activeLoginID)
        {
            bool isNew = vendor.ID == 0;

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", vendor.ID));
            parameters.Add(new SqlParameter("LoginsID", vendor.LoginsID));
            parameters.Add(new SqlParameter("CompanyName", InputHelper.GetString(vendor.CompanyName)));
            parameters.Add(new SqlParameter("FirstName", InputHelper.GetString(vendor.FirstName)));
            parameters.Add(new SqlParameter("LastName", InputHelper.GetString(vendor.LastName)));
            parameters.Add(new SqlParameter("Email", InputHelper.GetString(vendor.Email)));
            parameters.Add(new SqlParameter("MobileNumber", InputHelper.GetString(vendor.MobilePhone)));
            parameters.Add(new SqlParameter("WorkNumber", InputHelper.GetString(vendor.WorkPhone)));
            parameters.Add(new SqlParameter("Address1", InputHelper.GetString(vendor.Address1)));
            parameters.Add(new SqlParameter("Address2", InputHelper.GetString(vendor.Address2)));
            parameters.Add(new SqlParameter("City", InputHelper.GetString(vendor.City)));
            parameters.Add(new SqlParameter("State", InputHelper.GetString(vendor.State)));
            parameters.Add(new SqlParameter("Zip", InputHelper.GetString(vendor.Zip)));
            parameters.Add(new SqlParameter("TimeZone", InputHelper.GetString(vendor.TimeZone)));
            parameters.Add(new SqlParameter("Universal", vendor.Universal));
            parameters.Add(new SqlParameter("TypeID", ((int)vendor.Type).ToString()));
            parameters.Add(new SqlParameter("FaxNumber", InputHelper.GetString(vendor.FaxNumber)));
            parameters.Add(new SqlParameter("FileName", InputHelper.GetString(vendor.FileName)));
            parameters.Add(new SqlParameter("Deleted", vendor.Deleted));

            parameters.Add(new SqlParameter("MiddleName", ""));
            parameters.Add(new SqlParameter("HomeNumber", ""));

            parameters.Add(new SqlParameter("Extension", InputHelper.GetString(vendor.Extension)));

            parameters.Add(new SqlParameter("FederalTaxID", InputHelper.GetString(vendor.FederalTaxID)));
            parameters.Add(new SqlParameter("LicenseNumber", InputHelper.GetString(vendor.LicenseNumber)));
            parameters.Add(new SqlParameter("BarNumber", InputHelper.GetString(vendor.BarNumber)));
            parameters.Add(new SqlParameter("RegistrationNumber", InputHelper.GetString(vendor.RegistrationNumber)));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddOrUpdateVendor", parameters);
            if (result.Success)
            {
                ((IIDSetter)vendor).ID = result.Value;

                if (isNew)
                {
                    ToggleSelection(vendor.LoginsID, vendor.ID, activeLoginID);
                }

                ChangeLogManager.LogChange(activeLoginID, "Vendor", vendor.ID, vendor.LoginsID, parameters, vendor.RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public SaveResult Delete(int vendorID, int activeLoginID)
        {
            Vendor vendor = Get(vendorID);
            if (vendor != null)
            {
                vendor.Deleted = true;
                return Save(vendor, activeLoginID);
            }

            return new SaveResult("Error deleting vendor.");
        }

        public SaveResult ToggleSelection(int loginID, int vendorID, int activeLoginID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("VendorID", vendorID));

            DBAccess db = new DBAccess();
            FunctionResult saveResult = db.ExecuteNonQuery("ToggleSelectedVendor", parameters);

            if (saveResult.Success)
            {
                return new SaveResult();
            }
            else
            {
                return new SaveResult(saveResult.ErrorMessage);
            }
        }

        // NOTE - This is only needed to fix the Vendors table when updating the site.  Delete this after.
        public List<Vendor> GetAllPrivateVendors()
        {
            List<Vendor> vendors = new List<Vendor>();

            DBAccess db = new DBAccess();
            DBAccessTableResult table = db.ExecuteWithTableForQuery("SELECT * FROM Vendor WHERE ISNULL(Deleted, 0) = 0 AND LoginsID IS NOT NULL");

            foreach (DataRow row in table.DataTable.Rows)
            {
                vendors.Add(InstantiateVendor(row));
            }

            return vendors;
        }
    }
}

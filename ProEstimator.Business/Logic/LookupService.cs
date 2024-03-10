using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Web.Mvc;
using ProEstimator.Business.Extension;
using ProEstimator.Business.Model.Admin;
using ProEstimatorData;
using ProEstimatorData.Models;

namespace ProEstimator.Business.Logic
{
    public static class LookupService
    {
        private static readonly DBAccess _data = new DBAccess();
        public static IEnumerable<SelectListItem> VehicleSubModels(int year, int makeid, int modelid)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("Year", year));
            parameters.Add(new SqlParameter("MakeID", makeid));
            parameters.Add(new SqlParameter("ModelID", modelid));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("SP_GetGetSubModels", parameters);

            List<SelectListItem> returnList = new List<SelectListItem>();
            returnList.Add(new SelectListItem() { Text = "-----Select Trim-----", Value = "0" });

            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    string textValue = row["Submodel"].ToString();
                    if (!string.IsNullOrEmpty(textValue))
                    {
                        returnList.Add(new SelectListItem() { Text = textValue, Value = row["id"].ToString() });
                    }
                }
            }

            return returnList;
        }

        public static IEnumerable<SelectListItem> GetMonthSelectList()
        {
            List<SelectListItem> monthItems = new List<SelectListItem>();
            var monthList = new List<string> { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
            foreach (var item in monthList)
            {
                monthItems.Add(new SelectListItem { Text = item, Value = item.Substring(0, 3).ToUpper() });
            }
            return monthItems;
        }


        public static IEnumerable<SelectListItem> VehicleBodyTypes(int year, int makeID, int modelID, int trimLevelID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("Year", year));
            parameters.Add(new SqlParameter("MakeID", makeID));
            parameters.Add(new SqlParameter("ModelID", modelID));
            parameters.Add(new SqlParameter("TrimLevelID", trimLevelID));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("sp_GetBodyTypes", parameters);

            List<SelectListItem> returnList = new List<SelectListItem>();
            returnList.Add(new SelectListItem() { Text = "-----Select Body Type-----", Value = "0" });

            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    returnList.Add(new SelectListItem() { Text = row["Body"].ToString(), Value = row["BodyID"].ToString() });
                }
            }

            return returnList;
        }

        public static IEnumerable<SelectListItem> VehicleDriveTypes(int vehicleID, int estimateID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("VehicleID", vehicleID));
            parameters.Add(new SqlParameter("AdminInfoID", estimateID));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("SP_GetDriveTypes", parameters);

            List<SelectListItem> returnList = new List<SelectListItem>();
            returnList.Add(new SelectListItem() { Text = "-----Select Drive Type-----", Value = "0" });

            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    string textValue = row["DriveType"].ToString();
                    if (!string.IsNullOrEmpty(textValue))
                    {
                        returnList.Add(new SelectListItem() { Text = textValue, Value = row["DriveTypeID"].ToString() });
                    }
                }
            }

            return returnList;
        }

        public static IEnumerable<SelectListItem> VehicleEngineTypes(int bodyID, int vehicleID, int estimateID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("VehicleID", vehicleID));
            parameters.Add(new SqlParameter("AdminInfoID", estimateID));
            parameters.Add(new SqlParameter("BodyID", bodyID));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("SP_GetEngineTypes", parameters);

            List<SelectListItem> returnList = new List<SelectListItem>();
            returnList.Add(new SelectListItem() { Text = "-----Select Engine Type-----", Value = "0" });

            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    string textValue = row["Engine"].ToString();
                    if (!string.IsNullOrEmpty(textValue))
                    {
                        returnList.Add(new SelectListItem() { Text = textValue, Value = row["EngineID"].ToString() });
                    }
                }
            }

            return returnList;
        }

        public static IEnumerable<SelectListItem> VehicleTransmissionTypes(int bodyID, int vehicleID, int estimateID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("VehicleID", vehicleID));
            parameters.Add(new SqlParameter("AdminInfoID", estimateID));
            parameters.Add(new SqlParameter("BodyID", bodyID));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("SP_GetTransmissionTypes", parameters);

            List<SelectListItem> returnList = new List<SelectListItem>();
            returnList.Add(new SelectListItem() { Text = "-----Select Transmission Type-----", Value = "0" });

            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    string textValue = row["Transmission"].ToString();
                    if (!string.IsNullOrEmpty(textValue))
                    {
                        returnList.Add(new SelectListItem() { Text = textValue, Value = row["TransmissionID"].ToString() });
                    }
                }
            }

            return returnList;
        }


        public static IEnumerable<SelectListItem> VehiclePaintCodes(int year, int makeID, int modelID, int bodyID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("year", year));
            parameters.Add(new SqlParameter("makeID", makeID));
            parameters.Add(new SqlParameter("modelID", modelID));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetPaintCodes", parameters);

            List<SelectListItem> returnList = new List<SelectListItem>();
            returnList.Add(new SelectListItem() { Text = "-----Select Paint-----", Value = "0" });

            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    string textValue = row["Color"].ToString();
                    if (!string.IsNullOrEmpty(textValue))
                    {
                        returnList.Add(new SelectListItem() { Text = textValue, Value = row["PaintCode"].ToString() });
                    }
                }
            }

            return returnList;
        }

        public static IEnumerable<SelectListItem> PaintTypes()
        {
            List<SelectListItem> painttypes = new List<SelectListItem>();
            painttypes.Add(new SelectListItem() { Text = "-----Select Paint Type-----", Value = "0" });
            painttypes.Add(new SelectListItem() { Text = "Single Stage", Value = "16" });
            painttypes.Add(new SelectListItem() { Text = "2 Stage", Value = "19" });
            painttypes.Add(new SelectListItem() { Text = "3 Stage", Value = "18" });
            painttypes.Add(new SelectListItem() { Text = "2 Tone", Value = "29" });

            return painttypes;
        }

        public static List<PromosLookup> GetPromosOptionList(int priceLevelId)
        {
            var result = new List<PromosLookup>();
            //CONTRACTTODO
            //var parameters = new List<SqlParameter>();
            //parameters.Add(new SqlParameter("ContractPriceLevelID", priceLevelId));

            //var item = _data.ExecuteWithTable(AdminConstants.GET_PROMOS, parameters);
            //if (item != null && item.DataTable != null)
            //{
            //    var ds = new DataSet();
            //    ds.Tables.Add(item.DataTable);
            //    result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<PromosLookup>()).ToList();
            //}

            return result;
        }

        public static List<PhoneLookup> GetPhoneTypeOptionList()
        {
            var result = new List<PhoneLookup>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("GroupNumber", AdminConstants.PHONE_GROUP));
            parameters.Add(new SqlParameter("EntryType", AdminConstants.PHONE_ENTRY));

            var item = _data.ExecuteWithTable(AdminConstants.GET_CODE_LIST, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<PhoneLookup>()).ToList();
                result.RemoveAll(x => x.Type == "FX");
                result.RemoveAll(x => string.IsNullOrEmpty(x.Type));
            }

            return result;
        }

        public static List<PhoneLookup> GetFaxTypeOptionList()
        {
            var result = new List<PhoneLookup>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("GroupNumber", AdminConstants.PHONE_GROUP));
            parameters.Add(new SqlParameter("EntryType", AdminConstants.PHONE_ENTRY));

            var item = _data.ExecuteWithTable(AdminConstants.GET_CODE_LIST, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<PhoneLookup>()).ToList();
                result.RemoveAll(x => x.Type != "FX");
                result.RemoveAll(x => string.IsNullOrEmpty(x.Type));
            }

            return result;
        }
        public static List<PhoneLookup> GetStatesOptionList()
        {
            var result = new List<PhoneLookup>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("GroupNumber", AdminConstants.STATE_GROUP));
            parameters.Add(new SqlParameter("EntryType", AdminConstants.STATE_ENTRY));

            var item = _data.ExecuteWithTable(AdminConstants.GET_CODE_LIST, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<PhoneLookup>()).ToList();
            }

            //remove empty from list
            var empty = result.FirstOrDefault(x => string.IsNullOrEmpty(x.Type));
            if (empty != null)
            {
                result.Remove(empty);
            }

            return result;
        }

        public static List<SalesRepLookup> GetSalesRepOptionList()
        {
            var result = new List<SalesRepLookup>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("SalesRepID", null));

            var item = _data.ExecuteWithTable(AdminConstants.GetSalesReps, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<SalesRepLookup>()).ToList();
            }

            return result;
        }

        public static List<SalesRepPermissionLookup> GetSalesRepPermissionOptionList(int? salesRepID = null)
        {
            var result = new List<SalesRepPermissionLookup>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("SalesRepID", salesRepID));

            var item = _data.ExecuteWithTable(AdminConstants.GetSalesReps, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<SalesRepPermissionLookup>()).ToList();
                result = result.Where(x => x.Type != "1" && !x.Deleted).OrderBy(o => o.Description).ToList();
            }

            return result;
        }

        public static List<ResellerLookup> GetResellerOptionList()
        {
            var result = new List<ResellerLookup>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("SalesRepID", null));

            var item = _data.ExecuteWithTable(AdminConstants.GetSalesReps, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<ResellerLookup>()).ToList();
            }

            return result;
        }

        public static List<LevelTermsLookup> GetLevelTermsOptionList(int contractId)
        {
            var result = new List<LevelTermsLookup>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ContractTypeID", 1));
            parameters.Add(new SqlParameter("PriceLevelActive", true));
            parameters.Add(new SqlParameter("ContractID", contractId));
            parameters.Add(new SqlParameter("IncludeFreeTerms", true));

            var item = _data.ExecuteWithTable(AdminConstants.GET_LEVEL_TERMS, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<LevelTermsLookup>()).ToList();
            }

            return result;
        }

        public static List<InvoicesLookup> GetInvoicesOptionList(int contractId)
        {
            var result = new List<InvoicesLookup>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ContractID", contractId.ToString()));

            // CONTRACTTODO
            //var item = _data.ExecuteWithTable(AdminConstants.GetInvoices, parameters);
            //if (item != null && item.DataTable != null)
            //{
            //    var ds = new DataSet();
            //    ds.Tables.Add(item.DataTable);
            //    result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<InvoicesLookup>()).ToList();
            //}

            return result;
        }

        public static List<SubscriptionLookup> GetSubscriptionOptionList(int loginId)
        {
            var result = new List<SubscriptionLookup>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginId.ToString()));

            // CONTRACTTODO
            //var item = _data.ExecuteWithTable(AdminConstants.GetContracts, parameters);
            //if (item != null && item.DataTable != null)
            //{
            //    var ds = new DataSet();
            //    ds.Tables.Add(item.DataTable);
            //    result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<SubscriptionLookup>()).ToList();
            //}

            return result;
        }

        public static List<InvoiceTypeLookup> GetInvoiceTypeOptionList()
        {
            var result = new List<InvoiceTypeLookup>();
            var parameters = new List<SqlParameter>();

            var item = _data.ExecuteWithTable(AdminConstants.GetInvoiceTypes, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<InvoiceTypeLookup>()).ToList();
            }

            return result;
        }
    }
}

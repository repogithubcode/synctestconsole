using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel.Admin
{
    public class ImportLineItem
    {
        public int LoginId { get; set; }
        public string LoginMoved { get; set; }
        public string CompanyName { get; set; }
        public string MovedBy { get; set; }
        public string SalesRep { get; set; }
        public string SelfImport { get; set; }
        public string ConversionComplete { get; set; }
        public bool Trial { get; set; }
        public bool ActiveTrial { get; set; }
        public string EffectiveDate { get; set; }
        public string ExpirationDate { get; set; }

        public ImportLineItem()
        {

        }

        public ImportLineItem(DataRow row)
        {
            LoginId = InputHelper.GetInteger(row["LoginID"].ToString());
            LoginMoved = InputHelper.GetString(row["LoginMoved"].ToString());
            CompanyName = InputHelper.GetString(row["companyName"].ToString());
            MovedBy = InputHelper.GetString(row["Moved_By"].ToString());
            SalesRep = InputHelper.GetString(row["Sales_Rep"].ToString());
            SelfImport = InputHelper.GetString(row["SelfImport"].ToString());
            ConversionComplete = InputHelper.GetString(row["Conversion_complete"].ToString());
            Trial = InputHelper.GetBoolean(row["Trial"].ToString());
            ActiveTrial = InputHelper.GetBoolean(row["ActiveTrial"].ToString());
            EffectiveDate = InputHelper.GetString(row["effectiveDate"].ToString());
            ExpirationDate = InputHelper.GetString(row["expirationDate"].ToString());
        }

        public static List<ImportLineItem> GetForFilter(string startDate, string endDate)
        {
            List<ImportLineItem> importLineItems = new List<ImportLineItem>();
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@startDate", startDate));
            parameters.Add(new SqlParameter("@endDate", endDate));

            DBAccessTableResult tableResult = db.ExecuteWithTable("[Admin].[Get_WebestImports]", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                importLineItems.Add(new ImportLineItem(row));
            }

            return importLineItems;
        }
    }
}
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using ProEstimator.Business.Model;
using ProEstimator.Business.Model.Admin;
using ProEstimatorData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Extension;
using ProEstimatorData.Models.EditorTemplateModel;
using ProEstimator.Business.Type;
using System.Web.Script.Serialization;
using System.Configuration;

namespace ProEstimator.Business.Logic
{
    public class PartsUploadService
    {
        public PartsUploadService()
        {

        }

        public static List<KeyValuePair<string, string>> GetPartsCompany()
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("SP_GetPartsCompany");

            List<KeyValuePair<string, string>> results = new List<KeyValuePair<string, string>>();

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    results.Add(new KeyValuePair<string, string>(row["PartsCompanyId"].ToString(), row["PartsCompanyName"].ToString()));
                }
            }

            return results;
        }

        public static Boolean UploadPartsCompanyData(DataTable partsCompanyDataTable, string partsCompanyName, List<string> sourceIDCollection)
        {
            Boolean success = PartsCompanyUpload.UploadPartsCompanyData(partsCompanyDataTable, partsCompanyName, sourceIDCollection);

            return success;
        }

    }
}

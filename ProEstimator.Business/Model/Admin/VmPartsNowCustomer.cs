using ProEstimator.Business.ILogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Extension;

namespace ProEstimator.Business.Model.Admin
{
    public class VmPartsNowCustomer : IModelMap<VmPartsNowCustomer>, IDataTableMap<VmPartsNowCustomer>
    {
        public int Id { get; set; }
        public string companyname { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string phone1 { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public bool PartsNow { get; set; }

        public int DT_RowId;

        public VmPartsNowCustomer ToModel(System.Data.DataRow row)
        {
            var result = new VmPartsNowCustomer();
            result.Id = (int)row["id"];
            result.DT_RowId = (int)row["id"];
            result.companyname = (string)row["companyname"].SafeString();
            result.address1 = (string)row["address1"].SafeString();
            result.address2 = (string)row["address2"].SafeString();
            result.city = (string)row["city"].SafeString();
            result.state = (string)row["state"].SafeString();
            result.zip = (string)row["zip"].SafeString();
            result.phone1 = (string)row["phone1"].SafeString();
            result.firstname = (string)row["firstname"].SafeString();
            result.lastname = (string)row["lastname"].SafeString();
            result.PartsNow = (bool)row["PartsNow"].SafeBool();

            return result;
        }

        public VmPartsNowCustomer MapFromDataTableRow(Dictionary<string, Dictionary<string, string>> row)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, Dictionary<string, string>> MapToDataTableRow()
        {
            throw new NotImplementedException();
        }
    }
}

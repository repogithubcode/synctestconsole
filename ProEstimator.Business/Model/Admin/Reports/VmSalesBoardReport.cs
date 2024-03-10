using ProEstimator.Business.ILogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Extension;
using System.Data;
using Newtonsoft.Json;

namespace ProEstimator.Business.Model.Admin
{
    public class VmSalesBoardReport : IModelMap<VmSalesBoardReport>
    {
        [JsonProperty("year")]
        public int Year { get; set; }
        [JsonProperty("jan")]
        public int Jan { get; set; }
        [JsonProperty("feb")]
        public int Feb { get; set; }
        [JsonProperty("mar")]
        public int Mar { get; set; }
        [JsonProperty("apr")]
        public int Apr { get; set; }
        [JsonProperty("may")]
        public int May { get; set; }
        [JsonProperty("jun")]
        public int Jun { get; set; }
        [JsonProperty("jul")]
        public int Jul { get; set; }
        [JsonProperty("aug")]
        public int Aug { get; set; }
        [JsonProperty("sept")]
        public int Sept { get; set; }
        [JsonProperty("oct")]
        public int Oct { get; set; }
        [JsonProperty("nov")]
        public int Nov { get; set; }
        [JsonProperty("dec")]
        public int Dec { get; set; }
        [JsonProperty("total")]
        public int? Total { get; set; }


        public VmSalesBoardReport ToModel(DataRow row)
        {
            var model = new VmSalesBoardReport();
            model.Year = row["Year"].ByteToInt();
            model.Jan = row["Jan"].ByteToInt();
            model.Feb = row["Feb"].ByteToInt();
            model.Mar = row["Mar"].ByteToInt();
            model.Apr = row["Apr"].ByteToInt();
            model.May = row["May"].ByteToInt();
            model.Jun = row["Jun"].ByteToInt();
            model.Jul = row["Jul"].ByteToInt();
            model.Aug = row["Aug"].ByteToInt();
            model.Sept = row["Sep"].ByteToInt();
            model.Oct = row["Oct"].ByteToInt();
            model.Nov = row["Nov"].ByteToInt();
            model.Dec = row["Dec"].ByteToInt();
            model.Total = row["total"].ByteToInt();
            return model;
        }

    }

    public class VmSalesBoardAll : IModelMap<VmSalesBoardAll>
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("salesBoardID")]
        public int? SalesBoardID { get; set; }
        [JsonProperty("loginID")]
        public int? LoginID { get; set; }
        [JsonProperty("numberSold")]
        public int? NumberSold { get; set; }
        [JsonProperty("dateSold")]
        public string DateSold { get; set; }
        [JsonProperty("frame")]
        public int? Frame { get; set; }
        [JsonProperty("ems")]
        public int? Ems { get; set; }
        [JsonProperty("companyName")]
        public string CompanyName { get; set; }

        public VmSalesBoardAll ToModel(DataRow row)
        {
           
            var model = new VmSalesBoardAll();
            model.Name = row["Name"].SafeString();
            model.SalesBoardID = row["salesBoardID"].SafeInt();
            model.LoginID = row["loginID"].SafeInt();
            model.NumberSold = row["numberSold"].SafeInt();
            model.DateSold = row["dateSold"].SafeDate();
            model.Frame = row["frame"].SafeInt();
            model.Ems = row["ems"].SafeInt();
            return model;
        }
    }

    public class VmSalesBoardDetail : IModelMap<VmSalesBoardDetail>
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("count")]
        public int? Count { get; set; }
        [JsonProperty("frame")]
        public int? Frame { get; set; }
        [JsonProperty("ems")]
        public int? Ems { get; set; }
        [JsonProperty("multiUser")]
        public bool? MultiUser { get; set; }
        [JsonProperty("salesRepID")]
        public string SalesRepID { get; set; }
        [JsonProperty("companyName")]
        public string CompanyName { get; set; }

        public VmSalesBoardDetail ToModel(DataRow row)
        {
            var model = new VmSalesBoardDetail();
            model.Name = row["Name"].SafeString();
            model.Count = row["Count"].SafeInt();
            model.Frame = row["Frame"].SafeInt();
            model.Ems = row["EMS"].SafeInt();
            model.SalesRepID = row["SalesRepID"].ByteToString();
            return model;
        }
    }

    public class VmSalesBoardUser : IModelMap<VmSalesBoardUser>
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("salesBoardID")]
        public int? SalesBoardId { get; set; }
        [JsonProperty("loginID")]
        public int? LoginId { get; set; }
        [JsonProperty("numberSold")]
        public int? NumberSold { get; set; }
        [JsonProperty("frame")]
        public int? Frame { get; set; }
        [JsonProperty("ems")]
        public int? Ems { get; set; }
        [JsonProperty("companyName")]
        public string CompanyName { get; set; }

        public VmSalesBoardUser ToModel(DataRow row)
        {
            var model = new VmSalesBoardUser();
            model.Name = row["Name"].SafeString();
            model.SalesBoardId = row["salesBoardID"].SafeInt();
            model.LoginId = row["loginID"].SafeInt();
            model.NumberSold = row["numberSold"].SafeInt();
            model.Frame = row["Frame"].SafeInt();
            model.Ems = row["EMS"].SafeInt();
            return model;
        }
    }

    public class VmSalesBoardInsert
    {
        public string LoginID { get; set; }
        public string Est { get; set; }
        public string Frame { get; set; }
        public string EMS { get; set; }
        public string SalesRep { get; set; }
        public string SoldMonth { get; set; }
        public string SoldYear { get; set; }
        public string AddUser { get; set; }
        public string ProEstimator { get; set; }
        public string DateSold { set; get; }
    }
}



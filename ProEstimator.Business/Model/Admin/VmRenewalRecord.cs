using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;
using System.Globalization;

namespace ProEstimator.Business.Model.Admin
{
    public class VmRenewalRecord : IModelMap<VmRenewalRecord>, IDataTableMap<VmRenewalRecord>
    {
        public int Id { get; set; }
        public int LoginID { get; set; }
        public int ContractID { get; set; }
        public string CompanyName { get; set; }
        public string FrameData { get; set; }
        public string EMS { get; set; }
        public string Multi { get; set; }
        public string PDR { get; set; }
        public string Contact { get; set; }
        public string Phone { get; set; }
        public string State { get; set; }
        public string SalesRep { get; set; }
        public int SalesRepId { get; set; }
        public int PriceLevel { get; set; }
        public string RenewalDate { get; set; }
        public int EstimateCountTotal { get; set; }
        public int EstimateCountCurrent { get; set; }
        public int YearsWithWebEst { get; set; }
        public string Notes { get; set; }
        public decimal RenewalAmount { get; set; }
        public bool WillRenew { get; set; }
        public bool WillNotRenew { get; set; }
        public bool HasRenewed { get; set; }
        public decimal PastDue { get; set; }
        public bool OneYrRenewal { get; set; }
        public bool TwoYrRenewalOrGreater { get; set; }
        public string Platform { get; set; }

        public int DT_RowId;

        public VmRenewalRecord MapFromDataTableRow(Dictionary<string, Dictionary<string, string>> row)
        {
            var model = new VmRenewalRecord();
            int myKey;
            if (int.TryParse(row.FirstOrDefault().Key, out myKey))
            {
                model.Id = myKey;
            }
            model.WillRenew = string.IsNullOrEmpty(row.FirstOrDefault().Value["WillRenew"]) ? false : row.FirstOrDefault().Value["WillRenew"] == "0" ? false : true;
            model.WillNotRenew = string.IsNullOrEmpty(row.FirstOrDefault().Value["WillNotRenew"]) ? false : row.FirstOrDefault().Value["WillNotRenew"] == "0" ? false : true;
            model.Notes = row.FirstOrDefault().Value["Notes"];

            return model;
        }

        public Dictionary<string, Dictionary<string, string>> MapToDataTableRow()
        {
            var model = this;
            var row = new Dictionary<string, Dictionary<string, string>>();
            row[model.Id.ToString()] = new Dictionary<string, string>()
            {
                {"Id", model.Id.ToString(CultureInfo.InvariantCulture)},
                {"LoginID", model.LoginID.ToString(CultureInfo.InvariantCulture)},
                {"SalesRep", model.SalesRep.ToString(CultureInfo.InvariantCulture)},
                {"CompanyName", model.CompanyName.ToString(CultureInfo.InvariantCulture)},
                {"Contact", model.Contact.ToString(CultureInfo.InvariantCulture)},
                {"Phone", model.Phone.ToString(CultureInfo.InvariantCulture)},
                {"State", model.State.ToString(CultureInfo.InvariantCulture)},
                {"FrameData", model.FrameData.ToString(CultureInfo.InvariantCulture)},
                {"EMS", model.EMS.ToString(CultureInfo.InvariantCulture)},
                {"Multi", model.Multi.ToString(CultureInfo.InvariantCulture)},
                {"PDR", model.PDR.ToString(CultureInfo.InvariantCulture)},
                {"RenewalAmount", model.RenewalAmount.ToString(CultureInfo.InvariantCulture)},
                {"EstimateCountTotal", model.EstimateCountTotal.ToString(CultureInfo.InvariantCulture)},
                {"EstimateCountCurrent", model.EstimateCountCurrent.ToString(CultureInfo.InvariantCulture)},
                {"YearsWithWebEst", model.YearsWithWebEst.ToString(CultureInfo.InvariantCulture)},
                {"RenewalDate", model.RenewalDate.ToString(CultureInfo.InvariantCulture)},
                {"WillRenew", model.WillRenew.ToString(CultureInfo.InvariantCulture)},
                {"WillNotRenew", model.WillNotRenew.ToString(CultureInfo.InvariantCulture)},
                {"HasRenewed", model.HasRenewed.ToString(CultureInfo.InvariantCulture)},
                {"PastDue", model.PastDue.ToString(CultureInfo.InvariantCulture)},
                {"Notes", model.Notes},
                {"DT_RowId", model.DT_RowId.ToString()}
            };

            return row;
        }

        public VmRenewalRecord ToModel(DataRow row)
        {
            var model = new VmRenewalRecord();
            model.Id = (int)row["Id"];
            model.LoginID = (int)row["LoginID"];
            model.DT_RowId = (int)row["Id"];
            model.SalesRepId = (int)row["salesrepid"];
            model.SalesRep = row["SalesRep"].SafeString();
            model.CompanyName = row["CompanyName"].SafeString();
            model.Contact = row["Contact"].SafeString();
            model.Phone = row["phone"].SafeString();
            model.FrameData = row["FrameData"].SafeBool() ?? false ? "Y" : "N";
            model.EMS = row["EMS"].SafeBool() ?? false ? "Y" : "N";
            model.Multi = row["Multi"].SafeBool() ?? false ? "Y" : "N";
            model.PDR = row["pdr"].SafeBool() ?? false ? "Y" : "N";
            model.RenewalAmount = row["RenewalAmount"].SafeDecimal();
            model.RenewalDate = DateTime.Parse(row["RenewalDate"].SafeDate()).ToString("MM/dd/yy");
            model.EstimateCountTotal = (int)row["estcounttotal"];
            model.EstimateCountCurrent = (int)row["estcountcur"];
            model.YearsWithWebEst = (int)row["yearswe"];
            model.WillRenew = row["WillRenew"].SafeBool() ?? false;
            model.WillNotRenew = row["willnotrenew"].SafeBool() ?? false;
            model.HasRenewed = row["HasRenewed"].SafeBool() ?? false;
            model.PastDue = row["PastDue"].SafeDecimal();
            model.Notes = row["Notes"].SafeString();
            model.Platform = row["Platform"].SafeString();


            model.State = row["state"].SafeString();
            var abbr = model.State.GetStateByName();
            if(!string.IsNullOrEmpty(abbr))
            {
                model.State = abbr;
            }

            model.OneYrRenewal = model.YearsWithWebEst == 1;
            model.TwoYrRenewalOrGreater = model.YearsWithWebEst > 1;

            return model;
        }
    }
}

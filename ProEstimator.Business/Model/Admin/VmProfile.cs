using System.Collections.Generic;
using System.Data;
using System.Linq;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmProfile : IModelMap<VmProfile>, IDataTableMap<VmProfile>
    {
        public int ID;
        public int DT_RowId;

        public int? CustomerProfilesID { get; set; }
        public int? RateType { get; set; }
        public float? Rate { get; set; }
        public int? CapType { get; set; }
        public float? Cap { get; set; }
        public bool? Taxable { get; set; }
        public float? DiscountMarkup { get; set; }
        public int? IncludeIn { get; set; }



        public VmProfile ToModel(DataRow row)
        {
            byte myCapType;
            byte myIncludeIn;

            var model = new VmProfile();
            model.ID = (int)row["ID"];
            model.DT_RowId = (int)row["ID"];
            model.CustomerProfilesID = (int?)row["CustomerProfilesID"];
            model.RateType = (int?)(byte?)row["RateType"];
            model.Rate = (float?)row["Rate"];
            model.CapType = byte.TryParse(row["CapType"].ToString(), out myCapType) ? (byte?) myCapType : null;
            model.Cap = (float?)row["Cap"];
            model.Taxable = (bool?)row["Taxable"];
            model.DiscountMarkup = (float?)row["DiscountMarkup"];
            model.IncludeIn = byte.TryParse(row["IncludeIn"].ToString(), out myIncludeIn) ? (byte?)myIncludeIn : null;


            return model;
        }

        public VmProfile MapFromDataTableRow(Dictionary<string, Dictionary<string, string>> row)
        {
            var model = new VmProfile();
            int myKey;
            if (int.TryParse(row.FirstOrDefault().Key, out myKey))
            {
                model.ID = myKey;
            }

            int myIncludeIn;
            float myDiscountMarkup;
            bool myTaxable;
            float myCap;
            int myCapType;
            float myRate;
            int myRateType;
            int myCustomerProfilesID;

            model.CustomerProfilesID = int.TryParse( row.FirstOrDefault().Value["CustomerProfilesID"], out myCustomerProfilesID) ? (int?) CustomerProfilesID : null;
            model.RateType = int.TryParse( row.FirstOrDefault().Value["RateType"], out myRateType) ? (int?) myRateType : null;
            model.Rate = float.TryParse( row.FirstOrDefault().Value["Rate"], out myRate) ? (float?) myRate : null;
            model.CapType = int.TryParse( row.FirstOrDefault().Value["CapType"], out  myCapType) ? (int?) myCapType : null;
            model.Cap = float.TryParse(row.FirstOrDefault().Value["Cap"], out myCap) ? (float?)myCap : null;
            model.Taxable = bool.TryParse( row.FirstOrDefault().Value["Taxable"], out myTaxable) ? (bool?) myTaxable : null;
            model.DiscountMarkup = float.TryParse(row.FirstOrDefault().Value["DiscountMarkup"], out myDiscountMarkup) ? (float?)myDiscountMarkup : null;
            model.IncludeIn = int.TryParse( row.FirstOrDefault().Value["IncludeIn"], out myIncludeIn) ? (int?) myIncludeIn : null;

            return model;
        }

        public Dictionary<string, Dictionary<string, string>> MapToDataTableRow()
        {
            var model = this;
            var row = new Dictionary<string, Dictionary<string, string>>();
            row[model.ID.ToString()] = new Dictionary<string, string>()
            {
                {"CustomerProfilesID", model.CustomerProfilesID.ToString()},
                {"RateType", model.RateType.ToString()},
                {"Rate", model.Rate.ToString()},
                {"CapType", model.CapType.ToString()},
                {"Cap", model.Cap.ToString()},
                {"Taxable", model.Taxable.ToString()},
                {"DiscountMarkup", model.DiscountMarkup.ToString()},
                {"IncludeIn", model.IncludeIn.ToString()},
                {"DT_RowId", model.DT_RowId.ToString()},
                {"ID", model.ID.ToString()}
            };

            return row;
        }
    }
}

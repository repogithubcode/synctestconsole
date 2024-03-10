using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;
using System;

namespace ProEstimator.Business.Model.Admin.Reports
{
    public class VmStripePayment : IModelMap<VmStripePayment>
    {
        public int LoginID { get; set; }
        public string Organization { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Date { get; set; }
        public string Amount { get; set; }

        public VmStripePayment ToModel(System.Data.DataRow row)
        {
            var model = new VmStripePayment();
            model.LoginID = (int)row["LoginID"];
            model.Organization = row["Organization"].SafeString();
            model.Address = row["Address1"].SafeString();
            model.City = row["City"].SafeString();
            model.State = row["State"].SafeString();
            model.Zip = row["Zip"].SafeString();
            model.Date = row["TimeStamp"].ToString();
            model.Amount = Math.Round(ProEstimatorData.InputHelper.GetDouble(row["Total"].ToString()), 2).ToString("0.00");
            
            return model;
        }
    }
}
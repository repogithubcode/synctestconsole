using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.PDR;

namespace Proestimator.ViewModel.PDR
{
    public class OversizedDentsVM
    {

        public int LoginID { get; set; }
        public int ProfileID { get; set; }
        public bool GoodData { get; set; }

        public string Message { get; set; }

        public List<OversizedDentSizeRowVM> Rows { get; private set; }

        public double FlatRate { get; set; }

        public OversizedDentsVM()
        {
            Rows = new List<OversizedDentSizeRowVM>();
        }

        public void LoadData(PDR_RateProfile profile, List<PDR_Rate> rates)
        {
            LoginID = profile.LoginID;
            ProfileID = profile.ID;

            for (int sizeID = 1; sizeID < 6; sizeID++)
            {
                Rows.Add(new OversizedDentSizeRowVM(rates, sizeID));
            }

            PDR_Rate pdrRate = rates.FirstOrDefault(o => o.Size == PDR_Size.Oversized && o.Panel.ID == 1);
            if (pdrRate != null)
            {
                FlatRate = (double)pdrRate.Amount;
            }

            //rates.FirstOrDefault(o => o.Size == PDR_Size.Oversized)
        }

    }

    public class OversizedDentSizeRowVM
    {
        public int SizeID { get; set; }
        public string SizeName { get; set; }

        public List<OversizedDentRateVM> Rates { get; private set; }

        public OversizedDentSizeRowVM()
        {
            Rates = new List<OversizedDentRateVM>();
        }

        public OversizedDentSizeRowVM(List<PDR_Rate> rates, int sizeID)
        {
            SizeID = sizeID;
            SizeName = sizeID.ToString() + " in.";

            Rates = new List<OversizedDentRateVM>();
            Rates.Add(new OversizedDentRateVM(rates.FirstOrDefault(o => o.Panel.ID == 1 && (int)o.Size == SizeID && o.Depth == PDR_Depth.Shallow)));
            Rates.Add(new OversizedDentRateVM(rates.FirstOrDefault(o => o.Panel.ID == 1 && (int)o.Size == SizeID && o.Depth == PDR_Depth.Medium)));
            Rates.Add(new OversizedDentRateVM(rates.FirstOrDefault(o => o.Panel.ID == 1 && (int)o.Size == SizeID && o.Depth == PDR_Depth.Deep)));
        }
    }

    public class OversizedDentRateVM
    {
        public int ID { get; set; }
        public int SizeID { get; set; }
        public int DepthID { get; set; }
        public string Amount { get; set; }

        public OversizedDentRateVM()
        {

        }

        public OversizedDentRateVM(PDR_Rate rate)
        {
            if (rate != null)
            {
                ID = rate.ID;
                SizeID = (int)rate.Size;
                DepthID = (int)rate.Depth;
                Amount = Math.Round((double)rate.Amount, 2).ToString().Replace(".00", "");
            }
        }
    }
}
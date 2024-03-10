using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.PDR;

namespace Proestimator.ViewModel.PDR
{
    public class RateMatrixVM
    {

        public int LoginID { get; set; }
        public bool GoodData { get; set; }
        public int ProfileID { get; set; }
        public string ProfileName { get; set; }
        public string ProfileType { get; set; }
        public bool IsDefault { get; set; }
        public bool Taxable { get; set; }
        public string Message { get; set; }

        public List<RateMatrixPanelVM> Panels { get; set; }

        public RateMatrixVM()
        {
            Panels = new List<RateMatrixPanelVM>();
        }

        public void LoadData(PDR_RateProfile profile, List<PDR_Rate> rates)
        {
            LoginID = profile.LoginID;
            ProfileID = profile.ID;
            ProfileName = profile.ProfileName;
            ProfileType = profile.ProfileType.ToString();
            IsDefault = profile.IsDefault;
            Taxable = profile.Taxable;

            Panels = new List<RateMatrixPanelVM>();

            List<PDR_Panel> panels = PDR_Panel.GetAll().OrderBy(o => o.SortOrder).ToList();
            List<PDR_Quantity> quantities = PDR_Quantity.GetAll().OrderBy(o => o.ID).ToList();

            foreach (PDR_Panel panel in panels)
            {
                Panels.Add(new RateMatrixPanelVM(panel, rates.Where(o => o.Panel.ID == panel.ID).ToList(), quantities));
            }
        }

    }

    public class RateMatrixPanelVM
    {
        public int PanelID { get; set; }
        public string PanelName { get; set; }

        public List<RateMatrixQuantityWrapperVM> QuantityWrappers { get; set; }

        public RateMatrixPanelVM()
        {
            QuantityWrappers = new List<RateMatrixQuantityWrapperVM>();
        }

        public RateMatrixPanelVM(PDR_Panel panel, List<PDR_Rate> rates, List<PDR_Quantity> quantities)
        {
            PanelID = panel.ID;
            PanelName = panel.PanelName;

            QuantityWrappers = new List<RateMatrixQuantityWrapperVM>();

            foreach(PDR_Quantity quantity in quantities)
            {
                QuantityWrappers.Add(new RateMatrixQuantityWrapperVM(rates.Where(o => o.Quantity?.ID == quantity.ID).ToList(), quantity));
            }
        }
    }

    public class RateMatrixQuantityWrapperVM
    {
        public int QuantityID { get; set; }

        public List<RateMatrixRateVM> Rates { get; set; }

        public RateMatrixQuantityWrapperVM()
        {
            Rates = new List<RateMatrixRateVM>();
        }

        public RateMatrixQuantityWrapperVM(List<PDR_Rate> rates, PDR_Quantity quantity)
        {
            QuantityID = quantity.ID;

            Rates = new List<RateMatrixRateVM>();
            Rates.Add(new RateMatrixRateVM(rates.FirstOrDefault(o => o.Size == PDR_Size.Dime), PDR_Size.Dime));
            Rates.Add(new RateMatrixRateVM(rates.FirstOrDefault(o => o.Size == PDR_Size.Nickel), PDR_Size.Nickel));
            Rates.Add(new RateMatrixRateVM(rates.FirstOrDefault(o => o.Size == PDR_Size.Quarter), PDR_Size.Quarter));
            Rates.Add(new RateMatrixRateVM(rates.FirstOrDefault(o => o.Size == PDR_Size.Half), PDR_Size.Half));
        }
    }

    public class RateMatrixRateVM
    {
        public int ID { get; set; }
        public int SizeID { get; set; }
        public string Amount { get; set; }

        public RateMatrixRateVM()
        {

        }

        public RateMatrixRateVM(PDR_Rate rate, PDR_Size size)
        {
            SizeID = (int)size;

            if (rate != null)
            {
                ID = rate.ID;
                Amount = Math.Round((double)rate.Amount, 2).ToString().Replace(".00", "");
            }
            else
            {
                ID = 0;
                Amount = "";
            }
        }

    }
}
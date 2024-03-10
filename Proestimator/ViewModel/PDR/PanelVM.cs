using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.PDR;
using ProEstimatorData.Models.SubModel;

namespace Proestimator.ViewModel.PDR
{
    public class PanelVM
    {
        public int ID { get; set; }
        public string PanelName { get; set; }
        public int PanelID { get; set; }
        public string SelectedID { get; set; }
        public string PanelWrapperName { get { return "panel" + PanelID; } }

        public List<DentSizeSelector> DentSizeSelectors { get; set; }
        public List<OversizedDentVM> OversizedDents { get; set; }
        
        public string SelectedOversizedDentQuantity { get; set; }
        public string SelectedMultiplier { get; set; }

        public string CustomCharge { get; set; }
        public string Description { get; set; }
        public bool IsExpanded { get; set; }
        public bool HasAddOnLink { get; set; }

        public string LineSummary { get; set; }

        public bool HasAnyPrice(int layer)
        {
            int min = 1;
            int max = 4;

            if (layer == 2)
            {
                min = 5;
                max = 9;
            }

            foreach (DentSizeSelector selector in DentSizeSelectors.Where(o => o.QuantityID >= min && o.QuantityID <= max))
            {
                if (selector.HasAnyPrice())
                {
                    return true;
                }
            }

            return false;
        }

        public PanelVM()
        {
            ID = 0;
            PanelName = "";
            PanelID = 0;
            SelectedID = "";
            DentSizeSelectors = new List<DentSizeSelector>();
            OversizedDents = new List<OversizedDentVM>();
            SelectedOversizedDentQuantity = "0";
            SelectedMultiplier = "0";
            Description = "";
            CustomCharge = "";
            IsExpanded = false;
            LineSummary = "";
        }
    }

    public class OversizedDentVM
    {
        public int ID { get; set; }
        public string Display { get; set; }
        public string Amount { get; set; }

        public OversizedDentVM(PDR_EstimateDataPanelOversize oversize)
        {
            ID = oversize.ID;
            Display = oversize.Size.ToString().Replace("_", "").Replace("in", " in") + " " + oversize.Depth.ToString();
            Amount = oversize.Amount.ToString("C");
        }
    }

    public class DentSizeSelector
    {
        public string QuantityDisplay { get; private set; }
        public int QuantityID { get; private set; }
        public string DimePrice { get; private set; }
        public string NickelPrice { get; private set; }
        public string QuarterPrice { get; private set; }
        public string HalfDollarPrice { get; private set; }
        public int SelectedSizeID { get; private set; }

        public DentSizeSelector(PDR_Quantity quantity, List<PDR_Rate> rates)
        {
            QuantityDisplay = quantity.Min.ToString() + " - " + quantity.Max.ToString();
            QuantityID = quantity.ID;

            DimePrice = GetDisplayPrice(rates, PDR_Size.Dime);
            NickelPrice = GetDisplayPrice(rates, PDR_Size.Nickel);
            QuarterPrice = GetDisplayPrice(rates, PDR_Size.Quarter);
            HalfDollarPrice = GetDisplayPrice(rates, PDR_Size.Half);
        }

        private string GetDisplayPrice(List<PDR_Rate> rates, PDR_Size size)
        {
            PDR_Rate rate = rates.FirstOrDefault(o => o.Size == size);
            if (rate != null)
            {
                int priceInt = (int)rate.Amount;
                if (priceInt == 0)
                {
                    return "";
                }

                return priceInt.ToString();
            }

            return "";            
        }

        public string GetExtraHeaderClass(int index)
        {
            if (index == 1 || index == 2)
            {
                return "hide-mobile";
            }
            else
            {
                return "";
            }
        }

        public string GetExtraDentCostContainerWrapper()
        {
            if (DimePrice + NickelPrice + QuarterPrice + HalfDollarPrice == "")
            {
                return "hide-mobile";
            }

            return "";
        }

        public bool HasAnyPrice()
        {
            return (!string.IsNullOrEmpty(DimePrice) || !string.IsNullOrEmpty(NickelPrice) || !string.IsNullOrEmpty(QuarterPrice) || !string.IsNullOrEmpty(HalfDollarPrice));
        }
    }
}
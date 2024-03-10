using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel.PDR;

namespace Proestimator.ViewModel
{
    public class PDRRatesVM
    {
        public int LoginID { get; set; }

        public bool GoodData { get; set; }

        public int ProfileID { get; set; }
        public string ProfileName { get; set; }
        public string ProfileType { get; set; }
        public bool IsDefault { get; set; }
        public bool HideDentCounts { get; set; }
        public bool Taxable { get; set; }
        public string OversizedPrice { get; set; }

        public string Message { get; set; }

        public RateSectionVM DimeRates { get; set; }
        public RateSectionVM NickelRates { get; set; }
        public RateSectionVM QuarterRates { get; set; }
        public RateSectionVM HalfRates { get; set; }

        public List<OversizeddRateVM> OversizedRates { get; set; }

        public SelectList PanelsList { get; set; }
        public SelectList CopyPanelsList { get; set; }
        public int SelectedPanel { get; set; }
        public int CopyPanel { get; set; }

        public PDRRatesVM()
        {
            // Get the repair facilities list from the Vendors table
            List<SelectListItem> panelsSelectList = new List<SelectListItem>();

            List<PDR_Panel> allPanels = PDR_Panel.GetAll();
            foreach(PDR_Panel panel in allPanels)
            {
                panelsSelectList.Add(new SelectListItem() { Text = panel.PanelName, Value = panel.ID.ToString() });
            }

            PanelsList = new SelectList(panelsSelectList.ToList(), "Value", "Text");

            panelsSelectList.Insert(0, new SelectListItem() { Text = "--Select Panel--", Value = "0" });
            CopyPanelsList = new SelectList(panelsSelectList, "Value", "Text");
        }

        public void LoadRateProfile(PDR_RateProfile rateProfile)
        {
            ProfileID = rateProfile.ID;
            ProfileName = rateProfile.ProfileName;
            ProfileType = rateProfile.ProfileType.ToString();
            IsDefault = rateProfile.IsDefault;
            HideDentCounts = rateProfile.HideDentCounts;
            Taxable = rateProfile.Taxable;

            List<PDR_Rate> rates = rateProfile.GetAllRates().Where(o => o.Panel != null && o.Panel.ID == SelectedPanel).ToList();
            DimeRates = new RateSectionVM(rates, PDR_Size.Dime);
            NickelRates = new RateSectionVM(rates, PDR_Size.Nickel);
            QuarterRates = new RateSectionVM(rates, PDR_Size.Quarter);
            HalfRates = new RateSectionVM(rates, PDR_Size.Half);

            OversizedRates = new List<OversizeddRateVM>();
            foreach(PDR_Rate rate in rates.Where(o => ((int)o.Size < 6 || o.Size == PDR_Size.Oversized) && o.Depth != PDR_Depth.None).OrderBy(o => o.Size))
            {
                OversizedRates.Add(new OversizeddRateVM(rate));
            }

            PDR_Rate oversizedRate = rates.FirstOrDefault(o => o.Size == PDR_Size.Oversized);
            if (oversizedRate != null)
            {
                OversizedPrice = Math.Round(oversizedRate.Amount, 2).ToString();
            }
            else
            {
                OversizedPrice = "0";
            }
        }
    }

    public class RateSectionVM
    {
        public PDR_Size Size { get; set; }
        public string Range_1_5 { get; set; }
        public string Range_6_15 { get; set; }
        public string Range_16_30 { get; set; }
        public string Range_31_50 { get; set; }
        public string Range_51_75 { get; set; }
        public string Range_76_100 { get; set; }
        public string Range_101_150 { get; set; }
        public string Range_151_200 { get; set; }
        public string Range_201_300 { get; set; }

        public RateSectionVM(List<PDR_Rate> allRates, PDR_Size size)
        {
            Size = size;
            List<PDR_Rate> rates = allRates.Where(o => o.Size == size).ToList();

            Range_1_5 = Math.Round(rates.FirstOrDefault(o => o.Quantity.ID == 1).Amount, 2).ToString();
            Range_6_15 = Math.Round(rates.FirstOrDefault(o => o.Quantity.ID == 2).Amount, 2).ToString();
            Range_16_30 = Math.Round(rates.FirstOrDefault(o => o.Quantity.ID == 3).Amount, 2).ToString();
            Range_31_50 = Math.Round(rates.FirstOrDefault(o => o.Quantity.ID == 4).Amount, 2).ToString();
            Range_51_75 = Math.Round(rates.FirstOrDefault(o => o.Quantity.ID == 5).Amount, 2).ToString();
            Range_76_100 = Math.Round(rates.FirstOrDefault(o => o.Quantity.ID == 6).Amount, 2).ToString();
            Range_101_150 = Math.Round(rates.FirstOrDefault(o => o.Quantity.ID == 7).Amount, 2).ToString();
            Range_151_200 = Math.Round(rates.FirstOrDefault(o => o.Quantity.ID == 8).Amount, 2).ToString();
            Range_201_300 = Math.Round(rates.FirstOrDefault(o => o.Quantity.ID == 9).Amount, 2).ToString();
        }

        public RateSectionVM()
        {

        }
    }

    public class OversizeddRateVM
    {
        public int RateID { get; set; }
        public PDR_Size Size { get; set; }
        public PDR_Depth Depth { get; set; }
        public decimal Amount { get; set; }

        public OversizeddRateVM()
        {

        }

        public OversizeddRateVM(PDR_Rate rate)
        {
            RateID = rate.ID;
            Size = rate.Size;
            Depth = rate.Depth;
            Amount = Math.Round(rate.Amount, 2);
        }
    }
}

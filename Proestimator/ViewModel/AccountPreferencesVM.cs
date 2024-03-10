using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData;
using ProEstimatorData.Models;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Settings;

namespace Proestimator.ViewModel
{
    public class AccountPreferencesVM
    {

        public int LoginID { get; set; }
        public int UserID { get; set; }
        public bool ShowAppraiser { get; set; }
        public bool ShowLaborTimes { get; set; }
        public bool AllowEstimateVehicleModsWithLineItems { get; set; }

        public bool VehicleOptionsInFooter { get; set; }
        public bool PrintHeaderInfoOnEstimateReport { get; set; }

        public FontSize FontSizeHeader { get; set; }
        public FontSize FontSizeDetails { get; set; }
        public FontSize FontSizeLines { get; set; }
        public FontSize FontSizeTotals { get; set; }
        public FontSize FontSizeHeaders { get; set; }
        public FontSize FontSizeTableHeaders { get; set; }

        public SelectList TimeZones { get; set; }
        public int SelectedTimeZone { get; set; }

        public bool EmailAutoCcOn { get; set; }
        public bool IncludePartNotesOnPdfLineEntry { get; set; }

        public bool AddPartsShowAllYears { get; set; }
        public bool ProAdvisorEnabled { get; set; }
        public bool HasProAdvisorContract { get; set; }

        public bool PrintShopInfo { get; set; }
        public bool PrintInspectionDate { get; set; }

        public bool PrintRepairDays { get; set; }
        public PDFDownloadSetting DownloadPDF { get; set; }
        public bool PrintEmailAddressInHeader { get; set; }

        public bool ManualEstimatePrintSortAscendingOrder { get; set; }

        public bool UseLegacyPartsSectionDropdown { get; set; }

        //public bool DisableProAdvisor { get; set; }

        public string Culture { get; set; }
        public bool ShowChatIconDesktop { get; set; }
        public bool ShowChatIconMobile { get; set; }

        public int PartsSectionTreeFontSize { get; set; }

        public AccountPreferencesVM()
        {
            List<SelectListItem> timeZones = new List<SelectListItem>();
            timeZones.Add(new SelectListItem() { Text = "Eastern", Value = "0" });
            timeZones.Add(new SelectListItem() { Text = "Central", Value = "-1" });
            timeZones.Add(new SelectListItem() { Text = "Mountain", Value = "-2" });
            timeZones.Add(new SelectListItem() { Text = "Pacific", Value = "-3" });
            timeZones.Add(new SelectListItem() { Text = "Alaska", Value = "-4" });
            timeZones.Add(new SelectListItem() { Text = "Hawaii", Value = "-5" });

            TimeZones = new SelectList(timeZones, "Value", "Text");

            SelectedTimeZone = 0;            
        }
    }    
}
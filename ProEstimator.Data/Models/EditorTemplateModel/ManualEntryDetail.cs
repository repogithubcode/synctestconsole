using System.Collections.Generic;
using Kendo.Mvc.UI;
using ProEstimatorData.DataModel;
using ProEstimatorData.Models.SubModel;

namespace ProEstimatorData.Models.EditorTemplateModel
{
    public class ManualEntryDetail
    {
        /// <summary>
        /// Empty = none, "P" = percent, "D" = dollar
        /// </summary>
        public string BettermentType { get; set; }
        public List<SimpleListItem> BettermentTypeList { get; set; }

        public string MEMode { get; set; }
        public int EstimateID { get; set; }
        public bool EstimateIsLocked { get; set; }

        public bool Sublet { get; set; }
        public List<string> OperationTypes { get; set; }
        public string OperationType { get; set; }
        public string OperationDescription { get; set; }
        public string PartNumber { get; set; }
        public string SourcePartNumber { get; set; }
        public List<SimpleListItem> PartsList { get; set; }
        public string SelectedPart {get;set;}
        public string PartDescription { get; set; }
        public string PartSource { get; set; }
        public List<SimpleListItem> PartSourceList { get; set; }
        public List<SimpleListItem> VendorList { get; set; }
        public int SelectedVendor { get; set; }
        public string PartPrice { get; set; }
        public int Quantity { get; set; }
        public bool Overhaul { get; set; }
        public List<SimpleListItem> SectionList { get; set; }

        public List<DropDownTreeItemModel> SectionListDropDownTreeItemModel { get; set; }
        public List<TreeViewItemModel> SectionListTreeViewItemModel { get; set; }
        public string SelectedSection { get; set; }
        public int LaborType { get; set; }
        public List<SimpleListItem> LaborTypeList { get; set; }
        public decimal LaborHours { get; set; }
        public string OtherCharge { get; set; }
        public int OtherChargeType { get; set; }
        public List<SimpleListItem> OtherChargeTypeList { get; set; }
        public int PaintType { get; set; }
        public List<SimpleListItem> PaintTypeList { get; set; }
        public decimal PaintHours { get; set; }
        public string PanelType { get; set; }
        public List<SimpleListItem> PanelTypeList { get; set; }
        public bool LockPanelType { get; set; }
        public string BettermentValue { get; set; }
        public bool BettermentParts { get; set; }
        public bool BettermentMaterials { get; set; }
        public bool BettermentPaint { get; set; }
        public bool BettermentLabor { get; set; }
        public decimal AllowanceHours { get; set; }
        public decimal ClearcoatHours { get; set; }
        public decimal BlendHours { get; set; }
        public decimal EdgingHours { get; set; }
        public decimal UndersideHours { get; set; }
        public bool IncludeAllowance { get; set; }
        public bool IncludeClearcoat { get; set; }
        public bool IncludeBlend { get; set; }
        public bool IncludeEdging { get; set; }
        public bool IncludeUnderside { get; set; }
        public string InternalNotes { get; set; }
        public string ExternalNotes { get; set; }
        public int StepID { get; set; }
        public string Barcode { get; set; }
        public int LineID { get; set; }
        public List<SimpleListItem> PresetList { get; set; }
        public int TheID { get; set; } //CustomerProfileID for Presets - AdminInfoID for LineItem entries
        public string Action { get; set; }
        public bool LockAllowance { get; set; }
        public bool LockClearcoat { get; set; }
        public bool LockBlend { get; set; }
        public bool LockEdging { get; set; }
        public bool LockUnderside { get; set; }

        public bool UpdateBaseRecord { get; set; }

        public PaintGainValues PaintGainValues { get; set; }

        public string VehicleProuctionDate { get; set; }

        public bool OverlapDetails { get; set; }

        public bool IsPartsQuantity { get; set; }
        public bool IsLaborQuantity { get; set; }
        public bool IsPaintQuantity { get; set; }
        public bool IsOtherCharges { get; set; }

        public bool IsPALine { get; set; }
        public bool LaborIncluded { get; set; }

        public ManualEntryDetail(bool useAluminum)
        {
            PaintGainValues = new EditorTemplateModel.PaintGainValues();

            // Fill the Operation Types selection list
            List<string> operationTypes = new List<string>();
            operationTypes.Add("Replace");
            operationTypes.Add("Repair");
            operationTypes.Add("Refinish");
            operationTypes.Add("Blend");
            operationTypes.Add("R&I");
            operationTypes.Add("Overhaul");
            operationTypes.Add("Align");
            operationTypes.Add("Other");
            operationTypes.Add("PDR");
            OperationTypes = operationTypes;

            // Fill the Part Source List
            PartSourceList = new List<SimpleListItem>();
            PartSourceList.Add(new SimpleListItem("OEM", "OEM"));
            PartSourceList.Add(new SimpleListItem("LKQ", "LKQ"));
            PartSourceList.Add(new SimpleListItem("Aftermarket", "After"));
            PartSourceList.Add(new SimpleListItem("Remanufactured","Reman"));
            PartSourceList.Add(new SimpleListItem("Other", "Other"));

            // Fill the Labor Type List
            LaborTypeList = new List<SimpleListItem>();
            LaborTypeList.Add(new SimpleListItem("", "-1"));
            LaborTypeList.Add(new SimpleListItem("Body", "1"));
            LaborTypeList.Add(new SimpleListItem("Frame", "2"));
            LaborTypeList.Add(new SimpleListItem("Structure", "3"));
            LaborTypeList.Add(new SimpleListItem("Mechanical", "4"));
            LaborTypeList.Add(new SimpleListItem("Electrical", "24"));
            LaborTypeList.Add(new SimpleListItem("Glass", "25"));
            LaborTypeList.Add(new SimpleListItem(useAluminum ? "Aluminum" : "Detail", "5"));
            LaborTypeList.Add(new SimpleListItem("Cleanup", "6"));
            LaborTypeList.Add(new SimpleListItem("Other", "8"));

            // Fill the Other Charge Type List
            OtherChargeTypeList = new List<SimpleListItem>();
            OtherChargeTypeList.Add(new SimpleListItem("", "-1"));
            OtherChargeTypeList.Add(new SimpleListItem("Nontaxed", "13"));
            OtherChargeTypeList.Add(new SimpleListItem("Taxed", "14"));
            OtherChargeTypeList.Add(new SimpleListItem("Sublet", "15"));
            OtherChargeTypeList.Add(new SimpleListItem("Towing", "30"));
            OtherChargeTypeList.Add(new SimpleListItem("Storage", "31"));

            // Fill the Pain Type list
            PaintTypeList = new List<SimpleListItem>();
            PaintTypeList.Add(new SimpleListItem("No Paint", "0"));
            // paintTypeList.Add(new SimpleListItem("Clearcoat", "9"));  // Ezra - 10/17/2016 - Per discussion with Mike and Ryan "Clearcoat" is removed becuase it is the same as "2 stage"
            PaintTypeList.Add(new SimpleListItem("Single Stage", "16"));
            PaintTypeList.Add(new SimpleListItem("2 Stage", "19"));
            PaintTypeList.Add(new SimpleListItem("3 Stage", "18"));
            PaintTypeList.Add(new SimpleListItem("2 Tone", "29"));

            // Fill the Panel Type list
            PanelTypeList = new List<SimpleListItem>();
            PanelTypeList.Add(new SimpleListItem("", ""));
            PanelTypeList.Add(new SimpleListItem("First Panel", "First"));
            PanelTypeList.Add(new SimpleListItem("Adjacent Panel", "Adjacent"));
            PanelTypeList.Add(new SimpleListItem("Non-Adjacent Panel", "NonAdjacent"));

            // Fill the bettermenty type list
            BettermentTypeList = new List<SimpleListItem>();
            BettermentTypeList.Add(new SimpleListItem("None", ""));
            BettermentTypeList.Add(new SimpleListItem("%", "P"));
            BettermentTypeList.Add(new SimpleListItem("$", "D"));

            SelectedVendor = 0;
            VendorList = new List<SimpleListItem>();
            
        }

        public static bool UseAluminum(int estimateID)
        {
            SiteGlobals siteGlobals = SiteGlobals.Get();
            return estimateID > siteGlobals.AluminumEstimateID || estimateID == 0;
        }
    }
}
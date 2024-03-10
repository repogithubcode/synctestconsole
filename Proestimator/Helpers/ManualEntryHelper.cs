using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;

using ProEstimatorData;
using ProEstimatorData.Models.EditorTemplateModel;
using ProEstimatorData.Models.SubModel;
using ProEstimatorData.DataModel.Profiles;
using ProEstimatorData.DataModel;

using ProEstimator.DataRepositories.Vendors;
using System.Text.RegularExpressions;

namespace Proestimator.Helpers
{
    public static class ManualEntryHelper
    {
        public static List<ManualEntryListItem> getManualEntryList(string Mode, int estimateID)
        {
            //ID is AdminInfoID or CustomerProfileID depending on the MODE

            List<ManualEntryListItem> list = new List<ManualEntryListItem>();
            if (Mode != "Presets") // Line Items
            {

                DBAccess db = new DBAccess();
                DBAccessTableResult tableResult = db.ExecuteWithTable("Estimate_GetLineItemsPreview", new SqlParameter("AdminInfoID", estimateID));
                if (tableResult.Success)
                {
                    foreach (DataRow row in tableResult.DataTable.Rows)
                    {
                        ManualEntryListItem listitem = new ManualEntryListItem();
                        listitem.ID = InputHelper.GetInteger(row["LineID"].ToString());
                        listitem.Group = row["Step"].ToString();

                        listitem.LaborItems = row["LaborItems"].ToString().Trim();
                        if (listitem.LaborItems.StartsWith(", "))
                        {
                            listitem.LaborItems = listitem.LaborItems.Substring(2, listitem.LaborItems.Length - 2);
                        }
                        if (listitem.LaborItems.EndsWith(","))
                        {
                            listitem.LaborItems = listitem.LaborItems.Substring(0, listitem.LaborItems.Length - 1);
                        }

                        listitem.LineNumber = InputHelper.GetInteger(row["LineNumber"].ToString());
                        listitem.OP = row["ActionCode"].ToString();
                        listitem.OPDescription = row["ActionDescription"].ToString();
                        listitem.Overhaul = row["PartOfOverHaul"].ToString();
                        listitem.PartName = row["PartDescription"].ToString();
                        listitem.PartNumber = row["PartNumber"].ToString();
                        listitem.PartPrice = InputHelper.GetDouble(row["Price"].ToString());
                        listitem.Quantity = InputHelper.GetInteger(row["Quantity"].ToString());
                        listitem.PartSource = row["PartSource"].ToString();
                        listitem.Locked = row["Locked"].ToString() == "1";
                        listitem.Modified = InputHelper.GetInteger(row["Modified"].ToString());
                        listitem.EstimationDataSuppVer = InputHelper.GetInteger(row["EstimationDataSuppVer"].ToString());
                        listitem.ProcessedLineSuppVer = InputHelper.GetInteger(row["ProcessedLineSuppVer"].ToString());
                        listitem.HasManualNotes = StripHTMLAndCheckVisible(row["Notes"].ToString());
                        //listitem.LineNumberCalculated = InputHelper.GetInteger(row["LineNumberCalculated"].ToString());
                        list.Add(listitem);
                    }
                }
            }

            if (Mode == "Presets")
            {
                //if (estimateID == 0)
                //{
                //    estimateID = ProEstHelper.GetProfileID();
                //}

                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("Function", "PresetLineItems"));
                parameters.Add(new SqlParameter("ProfileID", estimateID));

                DBAccess db = new DBAccess();
                DBAccessTableResult table = db.ExecuteWithTable("CustomerProfile_GetPresetsPreview", new SqlParameter("ProfileID", estimateID));

                if (table.Success)
                {
                    foreach(DataRow row in table.DataTable.Rows)
                    {
                        ManualEntryListItem listitem = new ManualEntryListItem();
                        listitem.ID = InputHelper.GetInteger(row["LineID"].ToString());
                        listitem.Group = row["Step"].ToString();
                        listitem.LaborItems = row["LaborItems"].ToString();
                        listitem.LineNumber = 0;
                        listitem.OP = row["ActionCode"].ToString();
                        listitem.OPDescription = row["ActionDescription"].ToString();
                        listitem.Overhaul = row["PartOfOverhaul"].ToString();
                        listitem.PartName = row["PartDescription"].ToString();
                        listitem.PartNumber = row["PartNumber"].ToString();
                        listitem.PartPrice = InputHelper.GetDouble(row["Price"].ToString());
                        listitem.PartSource = row["PartSource"].ToString();
                        list.Add(listitem);
                    }
                }
            }

            return list.OrderByDescending(o => o.LineNumber).ToList();
        }

        public static void DeleteLineItem(int lineID, string meMode, bool deleteChildLines)
        {
            if (lineID <= 0)
            {
                return;
            }

            try
            {
                DBAccess db = new DBAccess();

                if (meMode == "Preset")
                {
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter("Function", "DeletePresetLineItem"));
                    parameters.Add(new SqlParameter("PresetLineItemsID", lineID));

                    db.ExecuteNonQuery("CustomerProfileFunctions", parameters);
                }
                else
                {
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter("Function", "DeleteEstimateLineItem"));
                    parameters.Add(new SqlParameter("EstimationLineItemsID", lineID));

                    db.ExecuteNonQuery("App_Functions", parameters);

                    // Child line items (add ons) need to be deleted seperately
                    if (deleteChildLines)
                    {
                        DBAccessTableResult childIDsResult = db.ExecuteWithTable("Estimate_GetChildLineIDs", new SqlParameter("ParentLineID", lineID));
                        foreach (DataRow row in childIDsResult.DataTable.Rows)
                        {
                            int childlineID = InputHelper.GetInteger(row["ID"].ToString());

                            List<SqlParameter> childParams = new List<SqlParameter>();
                            childParams.Add(new SqlParameter("Function", "DeleteEstimateLineItem"));
                            childParams.Add(new SqlParameter("EstimationLineItemsID", childlineID));

                            db.ExecuteNonQuery("App_Functions", childParams);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ProEstimatorData.ErrorLogger.LogError(ex, 0, 0, "ManualEntryHelper DeleteLineItem");
            }
        }

        public static void SaveManualEntry(int estimateID, ManualEntryDetail Detail, string Mode, string Action, int ID, int activeLoginID = 0, int loginID = 0)
        {
            //ID is AdminInfoID (Add LineItem) or ProfileID (Add Preset) or LineItemID (Update LineItem or Preset) depending on the Action/MODE
            if (!(string.IsNullOrEmpty(Detail.PartNumber) && string.IsNullOrEmpty(Detail.PartDescription) && string.IsNullOrEmpty(Detail.OperationDescription) && Detail.LaborHours == 0 && Detail.PaintHours == 0 && string.IsNullOrEmpty(Detail.PartPrice) && string.IsNullOrEmpty(Detail.OtherCharge) && Detail.ClearcoatHours == 0 && Detail.BlendHours == 0 && Detail.EdgingHours == 0 && string.IsNullOrEmpty(Detail.InternalNotes) & string.IsNullOrEmpty(Detail.ExternalNotes)))
            {
                string[] PartInfoArr = null;
                int OtherPartsID = 0;
                int VendorContactsID = 0;
                if (!string.IsNullOrEmpty(Detail.SelectedPart)) { // Threw exception at Detail.SelectedPart.Split if .SelectedPart is null
                    try {
                        char[] separatingChars = {','};
		                PartInfoArr = Detail.SelectedPart.Split(separatingChars);
		                OtherPartsID = Convert.ToInt32(PartInfoArr[0]);
		                VendorContactsID = Convert.ToInt32(PartInfoArr[1]);
	                } catch {
		                OtherPartsID = 0;
		                VendorContactsID = 0;
	                }
                } else if (Detail.SelectedVendor == 0) {
	                try {
		                OtherPartsID = 0;
		                VendorContactsID = 0;
	                } catch {
		                VendorContactsID = 0;
	                }
                } else {
	                OtherPartsID = 0;
	                VendorContactsID = 0;
                }
                int AdjacentDeduction = 0;
                if (Detail.PanelType == "First")
                {
                    AdjacentDeduction = 0;
                }
                else if(Detail.PanelType == "Adjacent")
                {
                    AdjacentDeduction = 1;
                }
                else if (Detail.PanelType == "NonAdjacent")
                {
                    AdjacentDeduction = 2;
                }
                bool MajorPanel = AdjacentDeduction == 1;

                if (Mode == "Manual" || Mode == "Graphical") //Manual or Graphical
                {
                    int stepID = -1;
                    if (Action == "Add")
                    {
                        ID = estimateID;
                        stepID = 0;
                    }

                    int newLineID = AddUpdateEstimateLine(estimateID, Action, ID, Detail.PartNumber, Detail.PartDescription,
                        Detail.PartSource, Detail.OperationType,
                        Detail.OperationDescription, Detail.LaborHours, Detail.LaborType, Detail.LaborIncluded, Detail.PaintHours, Detail.PaintType, InputHelper.GetDecimal(Detail.PartPrice), InputHelper.GetDecimal(Detail.OtherCharge),
                        Detail.OtherChargeType, Detail.ClearcoatHours, Detail.BlendHours, 21, Detail.EdgingHours, Detail.InternalNotes,
                        Detail.ExternalNotes, Detail.Overhaul, Detail.Quantity, stepID/*stepid may need to come back and see what this is*/,
                        Detail.BettermentMaterials , Detail.Sublet,
                        Detail.BettermentParts, false, Detail.BettermentLabor, false, Detail.BettermentPaint, false,
                        Detail.SelectedVendor, Detail.AllowanceHours, Detail.UndersideHours, AdjacentDeduction,
                        MajorPanel, Detail.Barcode, InputHelper.GetDecimal(Detail.PartPrice), Detail.BettermentType, InputHelper.GetDecimal(Detail.BettermentValue), InputHelper.GetInteger(Detail.SelectedSection), "", Detail.SourcePartNumber,
                        Detail.LockPanelType, Detail.LockAllowance, Detail.LockClearcoat, Detail.LockBlend, Detail.LockEdging, Detail.LockUnderside,
                        Detail.IncludeAllowance, Detail.IncludeClearcoat, Detail.IncludeBlend, Detail.IncludeEdging, Detail.IncludeUnderside, Detail.UpdateBaseRecord,Detail.IsPartsQuantity, Detail.IsLaborQuantity, Detail.IsPaintQuantity, Detail.IsOtherCharges);

                    if (newLineID > ID)
                    {
                        CopyOverlapsForSupplementLine(ID, newLineID);
                    }
                }

                if (Mode == "Preset")
                {
                    if (Action == "Add")
                    {
                        // TODO Refactor ID = ProEstHelper.GetProfileID();
                    }
         
                    AddUpdatePresetLine(Action, ID, Detail.PartNumber, Detail.PartDescription,
                        Detail.PartSource, Detail.OperationType,
                        Detail.OperationDescription, Detail.LaborHours, Detail.LaborType, Detail.LaborIncluded, Detail.PaintHours, Detail.PaintType, InputHelper.GetDecimal(Detail.PartPrice), InputHelper.GetDecimal(Detail.OtherCharge),
                        Detail.OtherChargeType, Detail.ClearcoatHours, Detail.BlendHours, 21, Detail.EdgingHours, Detail.InternalNotes,
                        Detail.ExternalNotes, Detail.Overhaul, Detail.Quantity,
                        0, Detail.AllowanceHours, Detail.UndersideHours, AdjacentDeduction,
                        MajorPanel,
                        Convert.ToBoolean(HttpContext.Current.Session["CurrentProfileDefault"]), Convert.ToBoolean(HttpContext.Current.Session["CurrentPresetsDefault"]), Detail.Sublet,
                        HttpContext.Current.Session["CurrentProfileName"].ToString(), HttpContext.Current.Session["CurrentProfileDescription"].ToString(), Detail.BettermentType, InputHelper.GetDecimal(Detail.BettermentValue), InputHelper.GetInteger(Detail.SelectedSection), Detail.LockAllowance, Detail.LockClearcoat, Detail.LockBlend, Detail.LockEdging, Detail.LockUnderside,
                        Detail.IncludeAllowance, Detail.IncludeClearcoat, Detail.IncludeBlend, Detail.IncludeEdging, Detail.IncludeUnderside, activeLoginID, loginID);
                }

                ProEstimatorData.DataModel.Estimate.RefreshProcessedLines(estimateID);
            }
        }

        private static void CopyOverlapsForSupplementLine(int lineItemID, int newLineItemID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LineItemID", lineItemID));
            parameters.Add(new SqlParameter("NewLineID", newLineItemID));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("Overlaps_CopyForSupplement", parameters);
        }

        public static int AddUpdateEstimateLine(
            int estimateID,
            string AddOrUpdate, 
            int TheID, 
            string PartNumber, 
            string Description, 
            string DropdownlistPartSource, 
            string Operation, 
            string OpDesc, 
            decimal LaborTime, 
            int LaborType,
            bool LaborIncluded,
            decimal PaintTime,
            int PaintType, 
            decimal Price, 
            decimal OtherCharge, 
            int Other, 
            decimal ClearcoatTime, 
            decimal BlendTime, 
            int EdgingType, 
            decimal EdgingTime, 
            string InternalNotes, 
            string ExternalNotes,
            bool Overhaul, 
            int Qty, 
            int StepID, 
            bool MaterialsBetterment, 
            bool OperationSublet, 
            bool PartBetterment, 
            bool PartSublet, 
            bool LaborBetterment, 
            bool LaborSublet, 
            bool PaintBetterment,
            bool PaintSublet, 
            int VendorID, 
            decimal Allowance, 
            decimal UndersideTime, 
            int AdjacentDeduction, 
            bool MajorPanel, 
            string BarCode, 
            decimal CustomerPrice = -1, 
            string BettermentType = "",
            decimal BettermentValue = -1,
            int SectionID = -1, 
            string VehiclePosition = "", 
            string SourcePartNumber = "", 
            bool AdjacentDeductionLock = false, 
            bool LockAllowance = false, 
            bool LockClearcoat = false, 
            bool LockBlend = false,
            bool LockEdging = false,
            bool LockUnderside = false, 
            bool IncludeAllowance = false,
            bool IncludeClearcoat = false,
            bool IncludeBlend = false,
            bool IncludeEdging = false,
            bool IncludeUnderside = false,
            bool UpdateBaseRecord = false,
            bool IsPartsQuantity = false,
            bool IsLaborQuantity = false,
            bool IsPaintQuantity = false,
            bool IsOtherCharges = false)
        {
            if (LaborType < 0)
            {
                LaborType = 0;
            }

            DBAccess db = new DBAccess();

            // If the action code is different that what's saved we want to delete any child lines (add ons)
            //if (TheID > 0)
            //{
            //    List<SqlParameter> childParams = new List<SqlParameter>();
            //    childParams.Add(new SqlParameter("LineID", TheID));
            //    childParams.Add(new SqlParameter("NewActionCode", Operation));

            //    DBAccessTableResult childLineIDs = db.ExecuteWithTable("Estimate_GetChildLineIDsIfParentActionChange", childParams);
            //    foreach(DataRow row in childLineIDs.DataTable.Rows)
            //    {
            //        int lineID = InputHelper.GetInteger(row["ID"].ToString());

            //        if (lineID > 0)
            //        {
            //            List<SqlParameter> deleteLineParams = new List<SqlParameter>();
            //            deleteLineParams.Add(new SqlParameter("Function", "DeleteEstimateLineItem"));
            //            deleteLineParams.Add(new SqlParameter("EstimationLineItemsID", lineID));

            //            db.ExecuteNonQuery("App_Functions", deleteLineParams);
            //        }
            //    }
            //}

            List<SqlParameter> parameters = new List<SqlParameter>();

            //needed to get OUTPUT parameter values
            switch (AddOrUpdate)
            {
                case "Add":
                    parameters.Add(new SqlParameter("Function", "AddEstimateLineItem"));
                    parameters.Add(new SqlParameter("AdminInfoID", estimateID));
                    break;
                case "Update":
                    parameters.Add(new SqlParameter("Function", "UpdateEstimateLineItem"));
                    parameters.Add(new SqlParameter("EstimationLineItemsID", TheID));
                    break;
                case "Remove":
                    parameters.Add(new SqlParameter("Function", "RemoveEstimateLineItem"));
                    parameters.Add(new SqlParameter("EstimationLineItemsID", TheID));
                    break;
            }

            parameters.Add(new SqlParameter("StepID", StepID));
            parameters.Add(new SqlParameter("SectionID", SectionID));
            parameters.Add(new SqlParameter("PartNumber", PartNumber));
            parameters.Add(new SqlParameter("VehiclePosition", VehiclePosition));
            parameters.Add(new SqlParameter("Price", Price));
            parameters.Add(new SqlParameter("Description", Description));
            parameters.Add(new SqlParameter("PartSource", DropdownlistPartSource));
            parameters.Add(new SqlParameter("Operation", Operation));
            parameters.Add(new SqlParameter("OpDesc", OpDesc));

            if (LaborTime != 0)
                parameters.Add(new SqlParameter("LaborTime", Math.Round(LaborTime, 1)));
            if (LaborType != 0)
                parameters.Add(new SqlParameter("LaborType", LaborType));
            if (PaintTime != 0)
                parameters.Add(new SqlParameter("PaintTime", Math.Round(PaintTime, 1)));
            if (PaintType != 0)
                parameters.Add(new SqlParameter("PaintType", PaintType));
            if (OtherCharge != 0)
                parameters.Add(new SqlParameter("OtherCharge", OtherCharge));
            if (Other > 0)
                parameters.Add(new SqlParameter("Other", Other));
            if (ClearcoatTime != 0)
                parameters.Add(new SqlParameter("ClearcoatTime", Math.Round(ClearcoatTime, 1)));
            if (BlendTime != 0)
                parameters.Add(new SqlParameter("BlendTime", Math.Round(BlendTime, 1)));
            if (EdgingType != 0)
                parameters.Add(new SqlParameter("EdgingType", EdgingType));
            if (EdgingTime != 0)
                parameters.Add(new SqlParameter("EdgingTime", Math.Round(EdgingTime, 1)));

            parameters.Add(new SqlParameter("InternalNotes", InternalNotes));
            parameters.Add(new SqlParameter("ExternalNotes", ExternalNotes));
            parameters.Add(new SqlParameter("Overhaul", Overhaul));
            parameters.Add(new SqlParameter("Qty", Qty));
            parameters.Add(new SqlParameter("MaterialsBetterment", MaterialsBetterment));
            parameters.Add(new SqlParameter("OperationSublet", OperationSublet));
            parameters.Add(new SqlParameter("PartBetterment", PartBetterment));
            parameters.Add(new SqlParameter("PartSublet", PartSublet));
            parameters.Add(new SqlParameter("LaborBetterment", LaborBetterment));
            parameters.Add(new SqlParameter("LaborSublet", LaborSublet));
            parameters.Add(new SqlParameter("PaintBetterment", PaintBetterment));
            parameters.Add(new SqlParameter("PaintSublet", PaintSublet));
            
            if (VendorID != 0)
                parameters.Add(new SqlParameter("VendorID", VendorID));
            if ((SourcePartNumber != null))
                parameters.Add(new SqlParameter("SourcePartNumber", SourcePartNumber));
            if (Allowance != 0)
                parameters.Add(new SqlParameter("Allowance", Allowance));
            if (UndersideTime != 0)
                parameters.Add(new SqlParameter("UndersideTime", Math.Round(UndersideTime, 1)));

            if (AdjacentDeduction > -1)
            {
                parameters.Add(new SqlParameter("AdjacentDeduction", AdjacentDeduction));
            }
            
            parameters.Add(new SqlParameter("MajorPanel", MajorPanel));

            if (CustomerPrice > 0)
                parameters.Add(new SqlParameter("CustomerPrice", CustomerPrice));

            parameters.Add(new SqlParameter("BettermentValue", BettermentValue));
            parameters.Add(new SqlParameter("BettermentType", BettermentType));

            parameters.Add(new SqlParameter("BarCode", BarCode));
            
            parameters.Add(new SqlParameter("AdjacentDeductionLock", AdjacentDeductionLock));

            SqlParameter newIDParameter = new SqlParameter("Return", 0);
            newIDParameter.Direction = ParameterDirection.Output;
            parameters.Add(newIDParameter);

            parameters.Add(new SqlParameter("LockAllowance", LockAllowance));
            parameters.Add(new SqlParameter("LockClearcoat", LockClearcoat));
            parameters.Add(new SqlParameter("LockBlend", LockBlend));
            parameters.Add(new SqlParameter("LockEdging", LockEdging));
            parameters.Add(new SqlParameter("LockUnderside", LockUnderside));

            parameters.Add(new SqlParameter("IncludeAllowance", IncludeAllowance));
            parameters.Add(new SqlParameter("IncludeClearcoat", IncludeClearcoat));
            parameters.Add(new SqlParameter("IncludeBlend", IncludeBlend));
            parameters.Add(new SqlParameter("IncludeEdging", IncludeEdging));
            parameters.Add(new SqlParameter("IncludeUnderside", IncludeUnderside));

            parameters.Add(new SqlParameter("UpdateBaseRecord", UpdateBaseRecord));

            if (Qty > 1 && !IsPartsQuantity && !IsLaborQuantity && !IsPaintQuantity)
            {
                if(Price > 0)
                {
                    IsPartsQuantity = true;
                }
            }

            parameters.Add(new SqlParameter("IsPartsQuantity", IsPartsQuantity));
            parameters.Add(new SqlParameter("IsLaborQuantity", IsLaborQuantity));
            parameters.Add(new SqlParameter("IsPaintQuantity", IsPaintQuantity));
            parameters.Add(new SqlParameter("IsOtherCharges", IsOtherCharges));
            parameters.Add(new SqlParameter("LaborIncluded", LaborIncluded));

            db.ExecuteNonQuery("App_Functions", parameters);

            return InputHelper.GetInteger(newIDParameter.Value.ToString());
        }

        public static void AddUpdatePresetLine(
            string AddOrUpdate, 
            int TheID, 
            string PartNumber, 
            string Description,
            string DropdownlistPartSource,
            string Operation,
            string OpDesc,
            decimal LaborTime,
            int LaborType,
            bool LaborIncluded,
            decimal PaintTime,
            int PaintType, 
            decimal Price, 
            decimal OtherCharge, 
            int Other, 
            decimal ClearcoatTime, 
            decimal BlendTime, 
            int EdgingType, 
            decimal EdgingTime, 
            string InternalNotes, 
            string ExternalNotes,
            bool Overhaul, 
            int Qty, 
            int VendorID,
            decimal Allowance, 
            decimal UndersideTime, 
            int AdjacentDeduction, 
            bool MajorPanel, 
            bool DefaultProfile, 
            bool DefaultPreset,
            bool OperationSublet, 
            string ProfileName, 
            string ProfileDescription, 
            string BettermentType, 
            decimal BettermentValue, 
            int SectionID = -1,
            bool LockAllowance = false, 
            bool LockClearcoat = false, 
            bool LockBlend = false,
            bool LockEdging = false,
            bool LockUnderside = false, 
            bool IncludeAllowance = false,
            bool IncludeClearcoat = false,
            bool IncludeBlend = false,
            bool IncludeEdging = false,
            bool IncludeUnderside = false,
            int activeLoginID = 0,
            int loginID = 0)
        {
            if (LaborType < 0)
            {
                LaborType = 0;
            }

            DataRow row = null;
            List<SqlParameter> parameters = new List<SqlParameter>();

            //needed to get OUTPUT parameter values
            switch (AddOrUpdate)
            {
                case "Add":
                    parameters.Add(new SqlParameter("Function", "AddPresetLineItem"));
                    parameters.Add(new SqlParameter("ProfileID", TheID));
                    break;
                case "Update":
                    row = GetPresetData(TheID);
                    parameters.Add(new SqlParameter("Function", "UpdatePresetLineItem"));
                    parameters.Add(new SqlParameter("PresetLineItemsID", TheID));
                    break;
            }
           
            parameters.Add(new SqlParameter("PartNumber", PartNumber));
            parameters.Add(new SqlParameter("Price", Price));
            parameters.Add(new SqlParameter("Description", Description));
            parameters.Add(new SqlParameter("PartSource", DropdownlistPartSource));
            parameters.Add(new SqlParameter("Operation", Operation));
            parameters.Add(new SqlParameter("OpDesc", OpDesc));
            parameters.Add(new SqlParameter("LaborTime", Math.Round(LaborTime, 1)));
            parameters.Add(new SqlParameter("LaborType", LaborType));
            parameters.Add(new SqlParameter("PaintTime", Math.Round(PaintTime, 1)));
            parameters.Add(new SqlParameter("PaintType", PaintType));
            parameters.Add(new SqlParameter("OtherCharge", OtherCharge));
            parameters.Add(new SqlParameter("Other", Other));
            parameters.Add(new SqlParameter("ClearcoatTime", ClearcoatTime));
            parameters.Add(new SqlParameter("BlendTime", Math.Round(BlendTime, 1)));
            parameters.Add(new SqlParameter("EdgingType", EdgingType));
            parameters.Add(new SqlParameter("EdgingTime", Math.Round(EdgingTime, 1)));
            parameters.Add(new SqlParameter("InternalNotes", InternalNotes));
            parameters.Add(new SqlParameter("ExternalNotes", ExternalNotes));
            parameters.Add(new SqlParameter("Overhaul", (Overhaul ? 1 : 0)));
            parameters.Add(new SqlParameter("Qty", Qty));
            parameters.Add(new SqlParameter("VendorID", VendorID));
            parameters.Add(new SqlParameter("Allowance", Allowance));
            parameters.Add(new SqlParameter("UndersideTime", UndersideTime));
            parameters.Add(new SqlParameter("AdjacentDeduction", AdjacentDeduction));
            parameters.Add(new SqlParameter("MajorPanel", (MajorPanel ? 1 : 0)));
            parameters.Add(new SqlParameter("DefaultProfile", (DefaultProfile ? 1 : 0)));
            parameters.Add(new SqlParameter("DefaultPreset", (DefaultPreset ? 1 : 0)));
            parameters.Add(new SqlParameter("OperationSublet", (OperationSublet ? 1 : 0)));
            parameters.Add(new SqlParameter("ProfileName", ProfileName));
            parameters.Add(new SqlParameter("ProfileDescription", ProfileDescription));
            parameters.Add(new SqlParameter("SectionID", SectionID));

            parameters.Add(new SqlParameter("LockAllowance", LockAllowance));
            parameters.Add(new SqlParameter("LockClearcoat", LockClearcoat));
            parameters.Add(new SqlParameter("LockBlend", LockBlend));
            parameters.Add(new SqlParameter("LockEdging", LockEdging));
            parameters.Add(new SqlParameter("LockUnderside", LockUnderside));

            parameters.Add(new SqlParameter("IncludeAllowance", IncludeAllowance));
            parameters.Add(new SqlParameter("IncludeClearcoat", IncludeClearcoat));
            parameters.Add(new SqlParameter("IncludeBlend", IncludeBlend));
            parameters.Add(new SqlParameter("IncludeEdging", IncludeEdging));
            parameters.Add(new SqlParameter("IncludeUnderside", IncludeUnderside));

            parameters.Add(new SqlParameter("LaborIncluded", LaborIncluded));

            DBAccess db = new DBAccess();
            FunctionResult result = db.ExecuteNonQuery("CustomerProfileFunctions", parameters);
            if (result.Success && row != null)
            {
                ChangeLogManager.LogChange(activeLoginID, "PresetSettings", TheID, loginID, parameters, row, ProfileName + " " + Operation + " " + OpDesc);
            }
        }

        private static DataRow GetPresetData(int PresetID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("CustomerProfile_GetPreset", new SqlParameter("CustomerProfilePresetsID", PresetID));
            if (tableResult.Success)
            {
                return tableResult.DataTable.Rows[0];
            }
            return null;
        }

        public static ManualEntryDetail GetPresetLine(IVendorRepository vendorService, int loginID, int PresetID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("CustomerProfile_GetPreset", new SqlParameter("CustomerProfilePresetsID", PresetID));

            bool useAluminum = false;

            if (tableResult.Success)
            {
                int profileID = InputHelper.GetInteger(tableResult.DataTable.Rows[0]["CustomerProfilesID"].ToString());
                RateProfile rateProfile = RateProfile.Get(profileID);
                
                if (rateProfile.EstimateID == 0)
                {
                    useAluminum = true;
                }
                else
                {
                    useAluminum = ManualEntryDetail.UseAluminum(rateProfile.EstimateID);
                }

                return PopulateME(loginID, tableResult.DataTable.Rows[0], useAluminum, vendorService);
            }
        
            return new ManualEntryDetail(useAluminum);
        }

        public static ManualEntryDetail GetMELine(IVendorRepository vendorService, int estimateID, int lineID, string meMode)
        {
            ManualEntryDetail detail = new ManualEntryDetail(ManualEntryDetail.UseAluminum(estimateID));

            if (lineID > -1)
            {
                ProEstimatorData.DataModel.Estimate estimate = new ProEstimatorData.DataModel.Estimate(estimateID);

                if (meMode != "Preset")
                {

                    List<SqlParameter> paramters = new List<SqlParameter>();
                    paramters.Add(new SqlParameter("AdminInfoID", estimateID));
                    paramters.Add(new SqlParameter("EstimationLineItemsID", lineID));

                    DBAccess db = new DBAccess();
                    DBAccessTableResult tableResult = db.ExecuteWithTable("GetEstimateLine", paramters);

                    if (tableResult.Success)
                    {
                        detail = PopulateME(estimate.CreatedByLoginID, tableResult.DataTable.Rows[0], ManualEntryDetail.UseAluminum(estimateID), vendorService);
                    }
                }

                if (meMode == "Preset")
                {
                    detail = GetPresetLine(vendorService, estimate.CreatedByLoginID, lineID);
                }
            }
            
            return detail;
        }

        public static ManualEntryDetail PopulateME(int loginID, DataRow record, bool useAluminum, IVendorRepository vendorService)
        {
            ManualEntryDetail detail = new ManualEntryDetail(useAluminum);
            detail.StepID = Convert.ToInt32(IsNull(record["StepID"], -1));
            detail.PartNumber = IsNull(record["PartNumber"], "").ToString();
            detail.SourcePartNumber = IsNull(record["SourcePartNumber"], "").ToString();
            detail.PartDescription = IsNull(record["PartDescription"], "").ToString();
            detail.OperationDescription = IsNull(record["ActionDescription"], "").ToString();
            detail.InternalNotes = IsNull(record["InternalNotes"],"").ToString();
            detail.ExternalNotes = IsNull(record["ExternalNotes"], "").ToString();
            detail.Overhaul = Convert.ToBoolean(IsNull(record["PartOfOverHaul"], 0));
            detail.Barcode = IsNull(record["Barcode"], "").ToString();
            detail.Quantity = Convert.ToInt32(IsNull(record["Qty"], 0));
            detail.LineID = Convert.ToInt32(IsNull(record["ID"], -1));
            try
            {
                detail.BettermentValue = record["BettermentValue"].ToString();
            }
            catch
            {
                detail.BettermentValue = "0";
            }

            try
            {
                detail.BettermentType = record["BettermentType"].ToString().Trim();
            }
            catch
            {
                detail.BettermentType = "";
            }

            try
            {
                detail.SelectedSection = Convert.ToInt32(IsNull(record["SectionID"], -1)).ToString();
            }
            catch
            {
                detail.SelectedSection = "-1";
            }

            detail.LaborHours = Convert.ToDecimal(IsNull(record["LaborTime"],0));
            detail.PaintHours = Convert.ToDecimal(IsNull(record["PaintTime"], 0));
            detail.AllowanceHours = Convert.ToDecimal(IsNull(record["AllowanceTime"], 0));
            detail.IncludeAllowance = GetBool(record, "AllowanceInclude");

            detail.ClearcoatHours = Convert.ToDecimal(IsNull(record["ClearcoatTime"], 0));
            detail.IncludeClearcoat = GetBool(record, "ClearcoatInclude");

            detail.BlendHours = Convert.ToDecimal(IsNull(record["BlendTime"], 0));
            detail.IncludeBlend = GetBool(record, "BlendInclude");

            detail.PartPrice = record["Price"].ToString();
            detail.OtherCharge = record["OtherCost"].ToString();
            detail.EdgingHours = Convert.ToDecimal(IsNull(record["EdgingTime"], 0));
            detail.IncludeEdging = GetBool(record, "EdgingInclude");

            detail.UndersideHours = Convert.ToDecimal(IsNull(record["UndersideTime"], 0));
            detail.IncludeUnderside = GetBool(record, "UndersideInclude");
            
            detail.PartSource = InputHelper.GetString(record["PartSource"].ToString());

            detail.LaborType = Convert.ToInt32(IsNull(record["LaborType"], -1));

            detail.PanelType = "";

            try
            {
                List<string> typeList = new List<string>() { "First", "Adjacent", "NonAdjacent" };
                string adjacentDeductionFlag = record["AdjacentDeductionFlag"].ToString();
                bool majorPanel = InputHelper.GetBoolean(record["MajorPanel"].ToString());

                if (!string.IsNullOrEmpty(adjacentDeductionFlag))
                {
                    int panelTypeInt = Convert.ToInt32(IsNull(adjacentDeductionFlag, -1));

                    if (panelTypeInt > 0)
                    { 
                        detail.PanelType = typeList[panelTypeInt];
                    }
                    else if (panelTypeInt == 0)
                    {
                        if (majorPanel)
                        {
                            detail.PanelType = "First";
                        }
                    }
                }
            }
            catch
            {
                
            }

            detail.PaintType = Convert.ToInt32(IsNull(record["PaintType"], 0));
            // "Clear coat" (9) is no longer an options, as it's the same as "2 stage" (19).  For backwards compatablility, change clear coat to 2 stage
            if (detail.PaintType == 9)
            {
                detail.PaintType = 19;
            }

            detail.OperationType = IsNull(record["ActionCode"], "").ToString().Replace("+", "&");
            if(detail.OperationType == "Over")
            {
                detail.OperationType = "Overhaul";
            }
            detail.OtherChargeType = Convert.ToInt32(IsNull(record["OtherType"], 0));
            detail.LockPanelType = Convert.ToBoolean(IsNull(record["AdjacentDeductionLock"],0));

            List<SimpleListItem> vendorlist = new List<SimpleListItem>();

            string partSource = detail.PartSource;
            if (partSource == "After")
            {
                partSource = "AfterMarket";
            }

            if (!string.IsNullOrEmpty(partSource))
            {
                VendorType vendorType = (VendorType)Enum.Parse(typeof(VendorType), partSource);
                List<ProEstimatorData.DataModel.Vendor> vendors = vendorService.GetAllForType(loginID, vendorType);
                vendorlist.Add(new SimpleListItem("---" + Proestimator.Resources.ProStrings.SelectVendor + "---", "0"));
                foreach (ProEstimatorData.DataModel.Vendor vendor in vendors.Where(o => !string.IsNullOrEmpty(o.CompanyName)).OrderBy(o => o.CompanyName))
                {
                    SimpleListItem item = new SimpleListItem(vendor.CompanyName, vendor.ID.ToString());
                    vendorlist.Add(item);
                }
            }

            detail.VendorList = vendorlist;
            detail.SelectedVendor = Convert.ToInt32(IsNull(record["PartSourceVendorID"], 0));

            List<SimpleListItem> partslist = new List<SimpleListItem>();

            int SelectedIndex = 0;
            List<GetPartInfo> parts = GetAvailableParts(loginID, detail.PartNumber, ref SelectedIndex, IsNull(record["SourcePartNumber"], "").ToString(), Convert.ToInt32(IsNull(record["PartSourceVendorID"], -1)));
            foreach (GetPartInfo part in parts)
            {
                SimpleListItem item = new SimpleListItem(part.PartInfo, part.id);
                partslist.Add(item);
            }

            detail.PartsList = partslist;
            detail.Sublet = Convert.ToBoolean(IsNull(record["SubletOperationFlag"], 0));
            detail.LaborIncluded = Convert.ToBoolean(IsNull(record["LaborIncluded"], 0));

            try
            {
                detail.LockAllowance = Convert.ToBoolean(IsNull(record["AllowanceLock"], 0));
                detail.LockBlend = Convert.ToBoolean(IsNull(record["BlendLock"], 0));
                detail.LockClearcoat = Convert.ToBoolean(IsNull(record["ClearcoatLock"], 0));
                detail.LockEdging = Convert.ToBoolean(IsNull(record["EdgingLock"], 0));
                detail.LockUnderside = Convert.ToBoolean(IsNull(record["UndersideLock"], 0));
            }
            catch { }

            try
            {
                detail.BettermentLabor = InputHelper.GetBoolean(record["LaborBettermentFlag"].ToString());
                detail.BettermentMaterials = InputHelper.GetBoolean(record["BettermentMaterials"].ToString());
                detail.BettermentPaint = InputHelper.GetBoolean(record["PaintBettermentFlag"].ToString());
                detail.BettermentParts = InputHelper.GetBoolean(record["BettermentParts"].ToString());

                detail.IsPartsQuantity = InputHelper.GetBoolean(record["IsPartsQuantity"].ToString());
                detail.IsLaborQuantity = InputHelper.GetBoolean(record["IsLaborQuantity"].ToString());
                detail.IsPaintQuantity = InputHelper.GetBoolean(record["IsPaintQuantity"].ToString());
                detail.IsOtherCharges = InputHelper.GetBoolean(record["IsOtherCharges"].ToString());
            }
            catch { }

            try
            {
                detail.IsPALine = InputHelper.GetBoolean(record["IsPAAddition"].ToString());
            }
            catch { }

            return detail;
        }

        private static bool GetBool(DataRow dataRow, string name)
        {
            if (dataRow == null)
            {
                return false;
            }

            try
            {
                string stringValue = dataRow[name].ToString();
                if (string.IsNullOrEmpty(stringValue))
                {
                    return false;
                }

                if (stringValue.ToLower() == "true" || stringValue == "1")
                { 
                    return true;
                }
            }
            catch { }

            return false;
        }

        public class GetPartInfo
        {
            public string id { get; set; }
            public string PartInfo { get; set; }

            public GetPartInfo(DataRow row)
            {
                id = row["id"].ToString();
                PartInfo = row["PartInfo"].ToString();
            }
        }

        public static List<GetPartInfo> GetAvailableParts(int loginID, string PartNumber, ref int SelectedIndex, string SourcePartNumber = "", int VendorContactsID = 0)
        {
            List<GetPartInfo> returnList = new List<GetPartInfo>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("PartNumber", PartNumber.Trim()));
            parameters.Add(new SqlParameter("LoginsID", loginID));
            parameters.Add(new SqlParameter("SelectedIndex", SelectedIndex));

            if (SourcePartNumber != "")
            {
                parameters.Add(new SqlParameter("SourcePartNumber", SourcePartNumber.Trim()));
            }
            if (VendorContactsID > 0)
            {
                parameters.Add(new SqlParameter("PartSourceVendorID", VendorContactsID));
            }

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("GetPartsListByPartNumber", parameters);
            if (tableResult.Success)
            {
                foreach(DataRow row in tableResult.DataTable.Rows)
                {
                    returnList.Add(new GetPartInfo(row));
                }
            }
            
            return returnList;
        }

        public static List<SimpleListItem> GetPresetList(int estimateID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetPresetsForEstimate", new SqlParameter("AdminInfoID", estimateID));

            int lastSection = 0;

            List<SimpleListItem> list = new List<SimpleListItem>();
            foreach (DataRow row in result.DataTable.Rows)
            {
                int presetID = InputHelper.GetInteger(row["id"].ToString());
                string description = InputHelper.GetString(row["Description"].ToString());
                int section = InputHelper.GetInteger(row["Section"].ToString());

                if (section != lastSection)
                {
                    list.Add(new SimpleListItem("", ""));
                    lastSection = section;
                }

                list.Add(new SimpleListItem(description, presetID.ToString()));
            }
            return list;
        }

        public static object IsNull(object o, object value)
        {
            if(o == DBNull.Value || o == null || o == "")
            {
                return value;
            }
            return o;
        }

        public static Boolean UpdateManualEntryReorder(ManualEntryListItem manualEntryListItem)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("id", manualEntryListItem.ID));
            parameters.Add(new SqlParameter("OrderNo", manualEntryListItem.OrderNumber));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("UpdateManualEntryReorder", parameters);
            if (tableResult.Success)
            {
                
            }

            return true;
        }

        public static Boolean UpdateEstmateLineItemLineNumber(ManualEntryListItem manualEntryListItem)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("id", manualEntryListItem.ID));
            parameters.Add(new SqlParameter("LineNumber", manualEntryListItem.LineNumber));

            DBAccess db = new DBAccess();
            db.ExecuteWithTable("UpdateEstmateLineItemLineNumber", parameters);

            return true;
        }

        public static bool StripHTMLAndCheckVisible(string HTMLText)
        {
            if (string.IsNullOrEmpty(HTMLText))
                return false;
            else
            {
                Regex regJs = new Regex(@"(?s)<\s?script.*?(/\s?>|<\s?/\s?script\s?>)", RegexOptions.IgnoreCase);
                HTMLText = regJs.Replace(HTMLText, "");
                Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
                HTMLText = reg.Replace(HTMLText, "");
                return string.IsNullOrEmpty(HTMLText) ? false : true;
            }
        }

    }
}
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Proestimator.ViewModel;
using ProEstimatorData.DataModel;

namespace Proestimator.ViewModelMappers
{
    public class DetailsVMMapper : IVMMapper<DetailsVM>
    {
        public DetailsVM Map(MappingConfiguration mappingConfiguration)
        {
            DetailsVMMapperConfiguration config = mappingConfiguration as DetailsVMMapperConfiguration;

            DetailsVM vm = new DetailsVM();

            vm.EstimateID = config.Estimate.EstimateID;
            vm.LoginID = config.Estimate.CreatedByLoginID;

            FillLists(config.Estimate, config.HasAddOnContract, config.ActiveLoginID);

            // Fill the rest of the data fields
            vm.EstimateDescription = config.Estimate.Description;
            vm.EstimateLocation = config.Estimate.EstimateLocation;
            vm.EstimateNotes = config.Estimate.Note;
            vm.IncludeNotesInReport = config.Estimate.PrintNote;
            vm.EstimatorID = config.Estimate.EstimatorID;
            vm.SelectedReportHeader = config.Estimate.ReportTextHeader;
            vm.SelectedRepairFacility = config.Estimate.RepairFacilityVendorID;
            vm.SelectedAlternateIdentity = config.Estimate.AlternateIdentitiesID;
            vm.CurrentSupplementLevel = config.Estimate.LockLevel.ToString();
            vm.RepairOrderNumber = config.Estimate.WorkOrderNumber.ToString();
            vm.PurchaseOrderNumber = config.Estimate.PurchaseOrderNumber;
            vm.FacilityRepairOrder = config.Estimate.FacilityRepairOrder;
            vm.FacilityRepairInvoice = config.Estimate.FacilityRepairInvoice;

            vm.InspectionDate = config.Estimate.EstimationDate.HasValue ? config.Estimate.EstimationDate.Value.ToShortDateString() : "";
            vm.AssignmentDate = config.Estimate.AssignmentDate.HasValue ? config.Estimate.AssignmentDate.Value.ToShortDateString() : "";

            vm.CreditCardFeePercentage = config.Estimate.CreditCardFeePercentage;
            vm.TaxedCreditCardFee = config.Estimate.TaxedCreditCardFee;
            vm.EstimateIsLocked = config.Estimate.IsLocked();

            SetDaysToRepair(vm, config.Estimate);

            return vm;
        }

        private void SetDaysToRepair(DetailsVM vm, Estimate estimate)
        {
            ProEstimatorData.DBAccess db = new ProEstimatorData.DBAccess();
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("AdminInfoID", estimate.EstimateID));
            parameters.Add(new SqlParameter("SupplementVersion", estimate.GetSupplementForReport()));

            ProEstimatorData.DBAccessStringResult result = db.ExecuteWithStringReturn("GetDaysToRepair", parameters);
            if (result.Success)
            {
                string temp = result.Value;
                int i = temp.IndexOf("(Supplement");
                if (i > -1)
                {
                    temp = temp.Substring(0, i - 1);
                }

                vm.NonUserDaysToRepair = string.IsNullOrEmpty(temp) ? "0" : temp;
            }
            else
            {
                vm.NonUserDaysToRepair = "0";
            }

            if (estimate.RepairDays == -1)
            {
                vm.DaysToRepair = Math.Ceiling(Convert.ToDecimal(vm.NonUserDaysToRepair)).ToString();
            }
            else
            {
                vm.DaysToRepair = estimate.RepairDays.ToString();
                vm.ManualRepairDays = true;
            }

            vm.HoursInDay = estimate.HoursInDay.ToString();
            List<SelectListItem> hours = new List<SelectListItem>();
            hours.Add(new SelectListItem() { Text = "4", Value = "4" });
            hours.Add(new SelectListItem() { Text = "5", Value = "5" });
            hours.Add(new SelectListItem() { Text = "6", Value = "6" });
            vm.HoursInDaySelections = new SelectList(hours, "Value", "Text");
        }
    }

    public class DetailsVMMapperConfiguration : MappingConfiguration
    {
        public Estimate Estimate { get; set; }
        public bool HasAddOnContract { get; set; }
        public int ActiveLoginID { get; set; }
    }
}
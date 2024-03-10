using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using Proestimator.ViewModel.PDR;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.PDR;

namespace Proestimator.ViewModelMappers.PDR
{
    public class PanelVMMapper : IVMMapper<PanelVM>
    {
        public PanelVM Map(MappingConfiguration mappingConfiguration)
        {
            PanelVMMapperConfiguration config = mappingConfiguration as PanelVMMapperConfiguration;

            PanelVM vm = new PanelVM();

            StringBuilder lineSummaryBuilder = new StringBuilder();

            vm.PanelName = config.Panel.Panel.PanelName;
            vm.PanelID = config.Panel.Panel.ID;
            vm.ID = config.Panel.ID;

            vm.Description = config.Panel.Description;
            vm.IsExpanded = config.Panel.Expanded || config.Panel.CustomCharge > 0;

            vm.CustomCharge = config.Panel.CustomCharge.ToString();

            PDR_Quantity selectedQuantity = config.Panel.Quantity;
            PDR_Size size = config.Panel.Size;

            if (config.SupplementChange != null)
            {
                vm.SelectedOversizedDentQuantity = config.SupplementChange.OversizedDents.ToString();
                vm.SelectedMultiplier = config.SupplementChange.Multiplier.ToString();

                selectedQuantity = config.SupplementChange.Quantity;
                size = config.SupplementChange.Size;
                vm.CustomCharge = config.SupplementChange.CustomCharge.ToString();
            }
            else
            {
                vm.SelectedOversizedDentQuantity = config.Panel.OversizedDents.ToString();
                vm.SelectedMultiplier = config.Panel.Multiplier.ToString();
            }

            if (selectedQuantity != null)
            {
                vm.SelectedID = config.Panel.Panel.ID.ToString() + "-" + selectedQuantity.ID.ToString() + "-" + size.ToString();

                lineSummaryBuilder.Append(selectedQuantity.Min.ToString() + " - " + selectedQuantity.Max.ToString() + " " + size.ToString() + ",");
            }
            else
            {
                vm.SelectedID = "";
            }

            vm.DentSizeSelectors = new List<DentSizeSelector>();
            foreach (PDR_Quantity quantity in PDR_Quantity.GetAll())
            {
                vm.DentSizeSelectors.Add(new DentSizeSelector(quantity, config.AllRates.Where(o => o.Quantity == quantity).ToList()));
            }

            vm.OversizedDents = new List<OversizedDentVM>();
            foreach (PDR_EstimateDataPanelOversize oversized in PDR_EstimateDataPanelOversize.GetForDataPanelID(config.Panel.ID).Where(o => o.SupplementDeleted == 0))
            {
                OversizedDentVM oversizedDentVM = new OversizedDentVM(oversized);
                vm.OversizedDents.Add(oversizedDentVM);
                lineSummaryBuilder.Append(" " + oversizedDentVM.Display + ",");
            }

            if (!string.IsNullOrEmpty(vm.CustomCharge) && vm.CustomCharge != "0")
            {
                lineSummaryBuilder.Append(" $" + vm.CustomCharge.ToString() + " custom,");
            }

            if (!string.IsNullOrEmpty(vm.SelectedOversizedDentQuantity) && vm.SelectedOversizedDentQuantity != "0")
            {
                lineSummaryBuilder.Append(" " + vm.SelectedOversizedDentQuantity + " Oversized,");
            }

            if (!string.IsNullOrEmpty(vm.SelectedMultiplier) && vm.SelectedMultiplier != "0")
            {
                int multiplierID = ProEstimatorData.InputHelper.GetInteger(vm.SelectedMultiplier);
                PDR_Multiplier multiplier = PDR_Multiplier.GetByID(multiplierID);
                lineSummaryBuilder.Append(" x " + multiplier.Name);
            }

            vm.LineSummary = lineSummaryBuilder.ToString().Trim();

            if (vm.LineSummary.EndsWith(","))
            {
                vm.LineSummary = vm.LineSummary.Substring(0, vm.LineSummary.Length - 1);
            }

            if (config.HasVehicle && config.Panel.Panel.LinkedPanelID > 0)
            {
                //Panel vehiclePanel = Panel.GetByID(config.Panel.Panel.LinkedPanelID);
                //if (vehiclePanel != null && vehiclePanel.SectionLinkRuleID > 0)
                //{
                    vm.HasAddOnLink = true;
                //}
            }

            return vm;
        }
    }

    public class PanelVMMapperConfiguration : MappingConfiguration
    {
        public PDR_EstimateDataPanel Panel { get; set; }
        public PDR_EstimateDataPanelSupplementChange SupplementChange { get; set; }
        public List<PDR_Rate> AllRates { get; set; }
        public bool HasVehicle { get; set; }

    }
}
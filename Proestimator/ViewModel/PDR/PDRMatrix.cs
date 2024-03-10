using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Proestimator.ViewModelMappers.PDR;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.PDR;

namespace Proestimator.ViewModel.PDR
{
    public class PDRMatrix
    {

        public int LoginID { get; set; }
        public int EstimateID { get; set; }

        public List<PanelVM> Panels { get; private set; }
        public List<SelectListItem> DentSizesList { get; set; }
        public List<SelectListItem> OversizedDentQuantityList { get; set; }
        public List<SelectListItem> MultipliersList { get; set; }
        public List<SelectListItem> DescriptionPresets { get; set; }

        public List<SelectListItem> OversizedSizeList { get; set; }
        public List<SelectListItem> OversizedDepthList { get; set; }

        public int ProfileID { get; set; }

        public bool ShowRAndIButton { get; set; }

        public PDRMatrix(int loginID, int estimateID)
        {
            LoginID = loginID;
            EstimateID = estimateID;
            Panels = new List<PanelVM>();

            DentSizesList = new List<SelectListItem>();

            List<PDR_Quantity> allQuantities = PDR_Quantity.GetAll();
            foreach (PDR_Quantity quantity in allQuantities)
            {
                DentSizesList.Add(new SelectListItem() { Text = quantity.Min.ToString() + " - " + quantity.Max.ToString() + " Dents", Value = quantity.ID.ToString() });
            }

            Vehicle vehicle = Vehicle.GetByEstimate(estimateID);
            if (vehicle != null && vehicle.VehicleID > 0)
            {
                ShowRAndIButton = true;
            }
        }

        public void FillPanelsData(int loginID, int estimateID, List<PDR_EstimateDataPanel> panelData, List<PDR_EstimateDataPanelSupplementChange> supplementChanges, List<PDR_Rate> allRates, List<PDR_Multiplier> multipliers)
        {
            bool hasVehicle = false;
            Vehicle vehicle = ProEstimatorData.DataModel.Vehicle.GetByEstimate(estimateID);
            if (vehicle?.VehicleID > 0)
            {
                hasVehicle = true;
            }

            PanelVMMapper mapper = new PanelVMMapper();
            
            foreach (PDR_EstimateDataPanel panel in panelData.Where(o => o.Panel != null))
            {
                PDR_EstimateDataPanelSupplementChange supplementChange = supplementChanges.Where(o => o.EstimateDataPanelID == panel.ID).OrderByDescending(o => o.SupplementVersion).FirstOrDefault();
                PanelVM panelVM = mapper.Map(new PanelVMMapperConfiguration() { Panel = panel, SupplementChange = supplementChange, AllRates = allRates.Where(o => o.Panel.ID == panel.Panel.ID).ToList(), HasVehicle = hasVehicle });
                Panels.Add(panelVM);
            }

            OversizedDentQuantityList = new List<SelectListItem>();
            for (int i = 0; i <= 50; i++)
            {
                OversizedDentQuantityList.Add(new SelectListItem() { Text = i.ToString(), Value = i.ToString() });
            }

            MultipliersList = new List<SelectListItem>();
            MultipliersList.Add(new SelectListItem() { Text = "--Choose--", Value = "0" });
            foreach(PDR_Multiplier multiplier in multipliers)
            {
                MultipliersList.Add(new SelectListItem() { Text = multiplier.Name, Value = multiplier.ID.ToString() });
            }

            DescriptionPresets = new List<SelectListItem>();
            DescriptionPresets.Add(new SelectListItem() { Value = "0", Text = "--Select Preset--" });
            List<PDR_DescriptionPreset> presets = PDR_DescriptionPreset.GetAllByLoginID(loginID);
            foreach(PDR_DescriptionPreset preset in presets)
            {
                DescriptionPresets.Add(new SelectListItem() { Text = preset.Text, Value = preset.ID.ToString() });
            }

            OversizedSizeList = new List<SelectListItem>();
            OversizedSizeList.Add(new SelectListItem() { Text = "1 in", Value = PDR_Size._1in.ToString() });
            OversizedSizeList.Add(new SelectListItem() { Text = "2 in", Value = PDR_Size._2in.ToString() });
            OversizedSizeList.Add(new SelectListItem() { Text = "3 in", Value = PDR_Size._3in.ToString() });
            OversizedSizeList.Add(new SelectListItem() { Text = "4 in", Value = PDR_Size._4in.ToString() });
            OversizedSizeList.Add(new SelectListItem() { Text = "5 in", Value = PDR_Size._5in.ToString() });

            OversizedDepthList = new List<SelectListItem>();
            OversizedDepthList.Add(new SelectListItem() { Text = "Shallow", Value = PDR_Depth.Shallow.ToString() });
            OversizedDepthList.Add(new SelectListItem() { Text = "Medium", Value = PDR_Depth.Medium.ToString() });
            OversizedDepthList.Add(new SelectListItem() { Text = "Deep", Value = PDR_Depth.Deep.ToString() });
        }

        public bool HasDataForLayer(int layer)
        {
            foreach(PanelVM panel in Panels)
            {
                if (panel.HasAnyPrice(layer))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
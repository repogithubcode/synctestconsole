using ProEstimator.Business.PDR.Model;
using ProEstimatorData.DataModel.LinkRules;
using ProEstimatorData.DataModel.PDR;
using ProEstimatorData.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using ProEstimator.Business.Panels;
using ProEstimator.Business.Panels.LinkRules;
using ProEstimator.DataRepositories.Parts;
using ProEstimator.Business.Model;

namespace ProEstimator.Business.PDR
{
    public class PDRAdditionalOperationsService : IPDRAdditionalOperationsService
    {
        private IPanelService _panelService;
        private ILinkRuleService _linkRuleService;
        private IPartsService _partsService;

        public PDRAdditionalOperationsService(IPanelService panelService, ILinkRuleService linkRuleService, IPartsService partsService)
        {
            _panelService = panelService;
            _linkRuleService = linkRuleService;
            _partsService = partsService;
        }

        /// <summary>
        /// Pass an estimate ID and a pdr Panel ID.  
        /// This function finds the section in the estimate's vehicle that matches the PDR panel and returns all additional operations for that panel.
        /// </summary>
        public SectionsForPDRPanelFunctionResult GetAdditionalOperations(int estimateID, int pdrPanelID)
        {
            SectionsForPDRPanelFunctionResult results = new SectionsForPDRPanelFunctionResult();

            PDR_Panel pdrPanel = PDR_Panel.GetByID(pdrPanelID);
            if (pdrPanel != null && pdrPanel.LinkedPanelID > 0)
            {
                Vehicle vehicle = Vehicle.GetByEstimate(estimateID);

                // Get details about the main panel.
                Panel panel = _panelService.GetPanel(pdrPanel.LinkedPanelID);

                PanelDetailsVM panelDetailsVM = GetPanelDetailsVM(vehicle, estimateID, panel, pdrPanel.IsLeftSide, pdrPanel.IsRightSide);
                if (panelDetailsVM != null)
                {
                    results.PanelDetails = panelDetailsVM;
                    results.Success = true;
                }

                // Get details for any adjacent panels.
                List<Panel> adjacentPanels = _panelService.GetLinkedPanels(pdrPanel.LinkedPanelID);
                foreach(Panel adjacentPanel in adjacentPanels)
                {
                    PanelDetailsVM adjacentPanelDetailsVM = GetPanelDetailsVM(vehicle, estimateID, adjacentPanel, pdrPanel.IsLeftSide, pdrPanel.IsRightSide);
                    if (adjacentPanelDetailsVM != null)
                    {
                        results.AdjacentPanelDetails.Add(adjacentPanelDetailsVM);
                    }
                }
            }

            if (results.PanelDetails == null)
            { 
                results.Success = false;
                results.ErrorMessage = "No R&I Data found for the selected panel.";
            }

            return results;
        }

        private PanelDetailsVM GetPanelDetailsVM(Vehicle vehicle, int estimateID, Panel panel, bool isLeftSide, bool isRightSide)
        {
            if (panel != null)
            {
                SectionDetailsResult details = _linkRuleService.GetSectionDetailsForEstimate(estimateID, panel);
                if (details.Success)
                {
                    PanelDetailsVM panelDetailsVM = new PanelDetailsVM();
                    panelDetailsVM.PanelID = panel.ID;
                    panelDetailsVM.PanelName = panel.PanelName;

                    bool allYears = false;

                    VehicleIDResult vehicleIDData = ProEstimatorData.DataModel.Vehicle.GetVehicleIDFromInfo(vehicle.Year, vehicle.MakeID, vehicle.ModelID, vehicle.TrimID);
                    if (vehicleIDData != null && vehicleIDData.CarryUp)
                    {
                        allYears = true;
                    }

                    foreach (SectionDetails detail in details.Details)
                    {
                        SectionDetailsVM sectionVM = new SectionDetailsVM();
                        sectionVM.Section = detail.Section;
                        sectionVM.SectionKey = detail.SectionKey;
                        sectionVM.Header = detail.Header;
                        sectionVM.Name = detail.SectionName;

                        // Get the parts for the section
                        List<SectionPartInfo> sectionParts = _partsService.GetPartsForSection(estimateID, detail.Header, detail.Section, allYears);
                        List<SectionPartInfo> pdrParts = GetPartsForPDRAdditionalOperations(sectionParts, isLeftSide, isRightSide);

                        foreach (SectionPartInfo partInfo in pdrParts)
                        {
                            bool hasAirBagInTitle = partInfo.Description.ToLower().Contains("air bag");

                            bool hasTime =
                                partInfo.AddTime > 0
                                || partInfo.AITime > 0
                                || partInfo.AlignTime > 0
                                || partInfo.CATime > 0
                                || partInfo.LaborPaintTime > 0
                                || partInfo.LaborTime > 0
                                || partInfo.PaintTime > 0
                                || partInfo.RITime > 0
                                || partInfo.RRTime > 0;

                            if (!hasAirBagInTitle && hasTime)
                            {
                                sectionVM.Parts.Add(partInfo);
                            }
                        }

                        if (sectionVM.Parts.Count > 0)
                        {
                            panelDetailsVM.SectionDetails.Add(sectionVM);
                        }
                    }

                    if (isLeftSide)
                    {
                        panelDetailsVM.PanelName = "Left ";
                    }
                    else if (isRightSide)
                    {
                        panelDetailsVM.PanelName = "Right ";
                    }
                    panelDetailsVM.PanelName += panel.PanelName;

                    return panelDetailsVM;
                }
            }

            return null;
        }

        public List<SectionPartInfo> GetPartsForPDRAdditionalOperations( List<SectionPartInfo> sectionParts, bool isLeftSide, bool isRightSide)
        {
            List<SectionPartInfo> parts = new List<SectionPartInfo>();

            foreach (SectionPartInfo partInfo in sectionParts)
            {
                bool addPart = partInfo.DetailLineCallout == 0 && parts.FirstOrDefault(o => o.Barcode == partInfo.Barcode) == null;

                // If we are going to add the part, run through rules to see if we don't want to add the part
                if (addPart)
                {

                    string comment = partInfo.comment.ToLower();

                    // For left and right, don't add parts with the opposite sign
                    if (isLeftSide && partInfo.Description.StartsWith("R "))
                    {
                        addPart = false;
                    }
                    else if (isRightSide && partInfo.Description.StartsWith("L "))
                    {
                        addPart = false;
                    }

                    else if (partInfo.Price > 0)
                    {
                        addPart = false;
                    }
                    else if (comment.Contains("refinish") || comment.Contains("section") || comment.Contains("transfer") || comment.Contains("section") || comment.Contains("jambs") || comment.Contains("adhesive"))
                    {
                        addPart = false;
                    }
                }

                if (addPart)
                {
                    parts.Add(partInfo);
                }
            }

            return parts;
        }
    }
}

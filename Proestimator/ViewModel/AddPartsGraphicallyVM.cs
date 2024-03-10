using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.Models.EditorTemplateModel;
using ProEstimatorData.Models.SubModel;
using ProEstimatorData.DataModel.ProAdvisor;
using ProEstimatorData.DataModel.Profiles;

using Proestimator.ViewModel.PDR;
using ProEstimatorData;
using ProEstimator.Business.Logic;
using Kendo.Mvc.UI;

namespace Proestimator.ViewModel
{
    public class AddPartsGraphicallyVM : PDRMatrixVMBase
    {
        

        public bool EstimateIsLocked { get; set; }

        public ManualEntry ManualEntry { get; set; }

        public int PaintType { get; set; }

        public List<SimpleListItem> PaintList { get; set; }

        public List<SimpleListItem> RefinishList { get; set; }

        public bool ShowSectionsDropdown { get; set; }
        public List<SimpleListItem> Sections { get; set; }
        public List<DropDownTreeItemModel> SectionsDropDownTreeItemModel { get; set; }
        public List<TreeViewItemModel> SectionsTreeViewItemModel { get; set; }
        public int Supplement { get; set; }

        public bool HasPDRContract { get; set; }
        public bool HasPartsNowContract { get; set; }

        public bool EstimateIsImported { get; set; }

        public bool VehicleDataFound { get; set; }

        public bool ForceSmallTop { get; set; }

        public bool ProAdvisorTrial { get; set; }
        public bool ShowAllYears { get; set; }
        public bool ShowCarryUpMessage { get; set; }

        public int PartsSectionTreeFontSize { get; set; }

        public AddPartsGraphicallyVM(int loginID, int estimateID, bool isMobile)
            : base(loginID, estimateID, isMobile)
        {
            ShowSectionsDropdown = true;
            ShowCarryUpMessage = false;

            PaintList = new List<SimpleListItem>();
            PaintList.Add(new SimpleListItem("-No Paint Type-", "0")); // blank
            PaintList.Add(new SimpleListItem("Single Stage", "16"));
            PaintList.Add(new SimpleListItem("2 Stage", "19"));
            PaintList.Add(new SimpleListItem("3 Stage", "18"));
            PaintList.Add(new SimpleListItem("2 Tone", "29"));

            RefinishList = new List<ProEstimatorData.Models.SubModel.SimpleListItem>();
            RefinishList.Add(new SimpleListItem("-Add To Rfn-", ""));
            RefinishList.Add(new SimpleListItem("Blend", "B"));
            RefinishList.Add(new SimpleListItem("Edge", "E"));
            RefinishList.Add(new SimpleListItem("Underside", "U"));
            RefinishList.Add(new SimpleListItem("Edge/Underside", "EU"));

            ShowAllYears = InputHelper.GetBoolean(SiteSettings.Get(loginID, "ShowAllYears", "AddPartsOptions", (false).ToString()).ValueString);
        }
                    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel;
using System.Web.Mvc;
using ProEstimatorData;

namespace Proestimator.ViewModel.CustomReports
{
    public class CustomReportTemplateVM
    {

        public int UserID { get; set; }
        public int LoginID { get; set; }
        public int TemplateID { get; set; }

        public string ErrorMessage { get; set; }

        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }


        [AllowHtml]
        public string Template { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public int SelectedReportHeaderID { get; set; }
        public SelectList ReportHeaderSelections { get; set; }

        public int SelectedReportFooterID { get; set; }
        public SelectList ReportFooterSelections { get; set; }

        public List<string> Tags { get; set; }

        public int EstimateID { get; set; }
        public Boolean IsSystemReport { get; set; }
        public bool IsRegularReport { get; set; }

        public string DeleteRestoreImgName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public CustomReportTemplateVM()
        {

        }

        public CustomReportTemplateVM(CustomReportTemplate template)
        {
            ID = template.ID;
            Name = template.Name;
            Description = template.Description;
            Template = template.Template;
            IsActive = template.IsActive;
            IsDeleted = template.IsDeleted;
            SelectedReportHeaderID = template.ReportHeaderID;
            SelectedReportFooterID = template.ReportFooterID;
            IsSystemReport = template.IsSystemReport;
            CreatedOn = template.CreatedOn;
            ModifiedOn = template.ModifiedOn;
        }

    }

}
using System;
using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel
{
    public class EmailTemplateVM
    {
        public int ID { get; set; }
        public int LoginID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Template { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsDefault { get; set; }
        public string DefaultTemplate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public EmailTemplateVM()
        {

        }

        public EmailTemplateVM(EmailCustomTemplate template)
        {
            ID = template.ID;
            LoginID = template.LoginID;
            Name = template.Name;
            Description = template.Description;
            Template = template.Template;
            IsDeleted = template.IsDeleted;
            IsDefault = template.IsDefault;
            CreatedDate = template.CreatedDate;
            ModifiedDate = template.ModifiedDate;
            DefaultTemplate = IsDefault ? "Yes" : "";
        }

    }

}

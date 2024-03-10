using System.Collections.Generic;
using dm = ProEstimatorData.Models.EditorTemplateModel;

namespace ProEstimator.Business.Model
{
    public class PnWorkflowResponse
    {
        public PnWorkflowResponse()
        {
            List = new System.Collections.Generic.List<dm.ManualEntryListItem>();
            Lines = new List<VmPnRequestLineItem>();
        }

        public bool Enrollment { get; set; }
        public string Uri { get; set; }

        public System.Collections.Generic.List<dm.ManualEntryListItem> List { get; set; }

        public Admin.VmUserMaintenanceEdit UserEditModels { get; set; }

        public Admin.VmOrganizationEdit shopDetail { get; set; }

        public VmPnEnrollResponse enrollResult { get; set; }

        public VmPnAuthorizeResponse authorizeResult { get; set; }

        public ProEstimatorData.DataModel.Vehicle vehicle { get; set; }

        public VmPnEnrollResponse enrollTableResult { get; set; }

        public List<VmPnRequestLineItem> Lines { get; set; }

        public VmPnEstimateResponse estimate { get; set; }
    }
}

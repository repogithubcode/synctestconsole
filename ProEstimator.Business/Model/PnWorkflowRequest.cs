using ProEstimatorData.Models.EditorTemplateModel;
using System.Collections.Generic;

namespace ProEstimator.Business.Model
{
    public class PnWorkflowRequest
    {
        public PnWorkflowRequest()
        {
            this.List = new List<ManualEntryListItem>();
        }

        public string EstimatorName { get; set; }
        public int LoginID { get; set; }
        public int Id { get; set; }
        public List<ManualEntryListItem> List { get; set; }
        public string CustomerName { get; set; }
    }
}

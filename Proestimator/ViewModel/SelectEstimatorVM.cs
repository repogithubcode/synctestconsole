using System.Collections.Generic;
namespace Proestimator.ViewModel
{
    public class SelectEstimatorVM
    {
        public int UserID { get; set; }
        public int LoginID { get; set; }
        public int EstimateID { get; set; }
        public int SelectedProfileID { get; set; }
        public List<EstimatorInfo> Estimators { get; set; }
        public SelectEstimatorVM()
        {
            Estimators = new List<EstimatorInfo>();
        }
    }
    public class EstimatorInfo
    {
        public int ID { get; set; }
        public string Description { get; set; }
    }
}
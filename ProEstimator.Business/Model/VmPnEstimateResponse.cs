namespace ProEstimator.Business.Model
{
    public class VmPnEstimateResponse
    {
        public string RequestId { get; set; }
        public bool Success { get; set; }
        public string FailedReason { get; set; }
        public string EstimateId { get; set; }
        public string EstimateUri { get; set; }
    }
}

namespace ProEstimator.Business.Model.Admin
{
    public class VmImportEstimate
    {
        public bool Success { get; set; }
        public string TimingMessage { get; set; }
        public bool HasNextEstimate { get; set; }
        public string ErrorMessage { get; set; }
        public int RemainingEstimateCount { get; set; }
        public int LoginId { get; set; }

        public string Message { get; set; }
        public VmImportEstimate()
        {
            Success = false;
            TimingMessage = "";
            ErrorMessage = "";
        }
    }
}

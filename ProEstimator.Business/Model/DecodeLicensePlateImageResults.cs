namespace Proestimator.ViewModel
{
    public class DecodeLicensePlateImageResults
    {
        public string LicensePlateNumber { get; set; }
        public string LicensePlateStateAbbreviation { get; set; }
        public string ErrorMessage { get; set; }
        public double CorrectPlateNumberConfidenceScore { get; set; }
        public double CorrectPlateStateConfidenceScore { get; set; }
    }
}
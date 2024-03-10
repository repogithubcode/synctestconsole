namespace Proestimator.Models.LicensePlateReader
{
    public class PlateReaderRequest
    {
        public string Upload { get; set; }
        public string[] Regions => new string[] { "us" };
    }
}
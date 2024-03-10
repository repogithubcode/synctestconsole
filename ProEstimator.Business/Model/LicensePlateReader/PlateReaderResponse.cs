using System.Linq;

namespace Proestimator.Models.LicensePlateReader
{
    public class PlateReaderResponse
    {
        public PlateReaderResponseResult[] Results { get; set; }

        public double PlateNumberScore 
        { 
            get
            {
                if (Results?.FirstOrDefault() == null)
                    return 0;

                return Results.FirstOrDefault().Score;
            } 
        }

        public double PlateStateScore
        {
            get
            {
                if (Results?.FirstOrDefault() == null || Results[0].Region == null)
                    return 0;

                return Results[0].Region.Score;
            }
        }

        public double TotalScore => PlateNumberScore + PlateStateScore;
    }

    public class PlateReaderResponseResult
    {
        public string Plate { get; set; }
        public double Score { get; set; }
        public PlateReaderResponseRegion Region { get; set; }
    }

    public class PlateReaderResponseRegion
    {
        public string Code { get; set; }
        public double Score { get; set; }
    }
}
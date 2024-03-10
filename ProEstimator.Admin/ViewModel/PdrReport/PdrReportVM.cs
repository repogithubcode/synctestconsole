using System.Data;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;
using ProEstimatorData.DataModel.Admin;

namespace ProEstimator.Admin.ViewModel
{
    public class PdrReportVM
    {
        public int LoginId { get; set; }
        public string CompanyName { get; set; }
        public string SalesRep { get; set; }
        public int NumberOfEstimates { get; set; }
        public string Date { get; set; }

        public PdrReportVM()
        {
            
        }

        public PdrReportVM(PdrReport pdrReport)
        {
            LoginId = pdrReport.LoginId;
            CompanyName = pdrReport.CompanyName;
            SalesRep = pdrReport.SalesRep;
            NumberOfEstimates = pdrReport.NumberOfEstimates;
            Date = pdrReport.Date;
        }
    }
}

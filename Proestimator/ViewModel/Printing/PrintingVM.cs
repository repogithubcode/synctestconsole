using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel;
using ProEstimatorData.Models;
using ProEstimatorData;

using ProEstimatorData.Models.SubModel;
using Proestimator.ViewModel.SendEstimate;

namespace Proestimator.ViewModel.Printing
{
    public class PrintingVM
    {
        public int LoginID { get; set; }
        public int EstimateID { get; set; }

        public bool OverrideShowHourlyRate { get; set; }
        public string ShowHideRateLabel { get; private set; }

        public bool IsIncludeImagesInPDF { get; set; }

        public List<ProEstimatorData.DataModel.Report> ReportHistory { get; set; }
       
        public ReportCreator ReportCreator { get; set; }

        public List<ImageVM> Images { get; set; }

        public PDFDownloadSetting DownloadPDF { get; set; }

        public PrintingVM()
        {
            ReportCreator = new ReportCreator(0, 0, false, false, false);
        }

        public PrintingVM(int loginID, int estimateID, bool hasEMSContract, bool isTrial)
        {
            ReportCreator = new ReportCreator(loginID, estimateID, hasEMSContract, isTrial, false);

            LoginID = loginID;
            EstimateID = estimateID;

            LoginInfo loginInfo = LoginInfo.GetByID(loginID);
            if (loginInfo.ShowLaborTimeWO)
            {
                ShowHideRateLabel = Proestimator.Resources.ProStrings.HideRatesBox;
            }
            else
            {
                ShowHideRateLabel = Proestimator.Resources.ProStrings.ShowRatesBox;
            }

            ReportHistory = ProEstimatorData.DataModel.Report.GetReportsForEstimate(estimateID).OrderByDescending(o => o.DateCreated).ToList();
                        
        }

    }
}
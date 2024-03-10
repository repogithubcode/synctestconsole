using ProEstimator.Business.ILogic;
using ProEstimatorData.DataAdmin;
using ProEstimatorData.DataModel.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ProEstimator.Admin.ViewModel
{
    public class TrialReportVM
    {
        public List<TrialRecordVM> Records { get; set; }
        public LineGraphVM GraphData { get; set; }
        public LineGraphVM SingleRepGraphData { get; set; }

        public int TotalWeTrials { get; set; }
        public int TotalActiveWeTrials { get; set; }
        public int TotalWeTrialsConverted { get; set; }

        public List<TrialRecordVM> TotalWeTrialDetail { get; set; }
        public List<TrialRecordVM> TotalActiveWeTrialDetail { get; set; }
        public List<TrialRecordVM> TotalWeTrialsConvertedDetail { get; set; }

        public List<SelectListItem> SalesRepDDL { get; set; }
        public string SelectedSalesRep { get; set; }

        public string SalesRepName { get; set; }

        public static TrialReportVM GetTrials(List<TrialReport> trialReports, int repId)
        {
            TrialReportVM trialReportVM = new TrialReportVM();
            trialReportVM.Records = new List<TrialRecordVM>();

            foreach (TrialReport trialReport in trialReports)
            {
                trialReportVM.Records.Add(new TrialRecordVM(trialReport));
            }

            trialReportVM.Records.ForEach(record =>
            {
                Contract activeContract = Contract.GetActive(record.Id);
                record.HasConverted = activeContract != null;
            });

            trialReportVM.Records = trialReportVM.Records.OrderBy(x => x.TrialStartDate).ToList();
            
            trialReportVM.GraphData = trialReportVM.ToGraphObject(trialReportVM.Records);
            trialReportVM.SingleRepGraphData = trialReportVM.ToFilteredGraphObject(trialReportVM.Records, repId);

            trialReportVM.TotalWeTrialDetail = trialReportVM.Records.Where(x => x.Trial).ToList();
            trialReportVM.TotalActiveWeTrialDetail = trialReportVM.Records.Where(x => x.Trial && DateTime.Parse(x.TrialStartDate) <= DateTime.Now && DateTime.Parse(x.TrialEndDate) >= DateTime.Now).ToList();
            trialReportVM.TotalWeTrialsConvertedDetail = trialReportVM.Records
                .Where(x => x.HasConverted && !x.Trial)
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .ToList();

            trialReportVM.TotalWeTrials = trialReportVM.TotalWeTrialDetail.Count();
            trialReportVM.TotalActiveWeTrials = trialReportVM.TotalActiveWeTrialDetail.Count();
            trialReportVM.TotalWeTrialsConverted = trialReportVM.TotalWeTrialsConvertedDetail.Count();

            return trialReportVM;
        }

        private LineGraphVM ToGraphObject(List<TrialRecordVM> trialRecordVMs)
        {
            LineGraphVM lineGraphVM = new LineGraphVM();
            trialRecordVMs.OrderBy(u => Convert.ToDateTime(u.TrialStartDate)).ToList().ForEach(x =>
            {
                var date = x.TrialStartDate.Split(' ')[0];
                if (!lineGraphVM.Xaxis.Contains(date))
                {
                    lineGraphVM.Xaxis.Add(date);
                    lineGraphVM.Yaxis.Add(1);
                }
                else
                {
                    lineGraphVM.Yaxis[lineGraphVM.Xaxis.IndexOf(date)] += 1;
                }
            });

            return lineGraphVM;
        }


        private LineGraphVM ToFilteredGraphObject(List<TrialRecordVM> model, int filter)
        {
            var result = new LineGraphVM();
            model.OrderBy(u => Convert.ToDateTime(u.TrialStartDate)).ToList().ForEach(x =>
            {
                var date = x.TrialStartDate.Split(' ')[0];
                if (!result.Xaxis.Contains(date))
                {
                    result.Xaxis.Add(date);
                    if(x.SalesRepID == filter)
                    {
                        result.Yaxis.Add(1);
                    }
                    else
                    {
                        result.Yaxis.Add(0);
                    }
                }
                else
                {
                    if (x.SalesRepID == filter)
                    {
                        result.Yaxis[result.Xaxis.IndexOf(date)] += 1;
                    }
                }
            });

            return result; throw new NotImplementedException();
        }
    }
}

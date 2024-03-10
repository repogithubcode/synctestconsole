using ProEstimator.Business.ILogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.Model.Admin.Reports
{
    public class VmTrialReport : ILineGraphMap<VmTrialRecord>
    {
        public List<VmTrialRecord> Records { get; set; }
        public VmLineGraph GraphData { get; set; }
        public VmLineGraph SingleRepGraphData { get; set; }

        public int TotalWeTrials { get; set; }
        public int TotalActiveWeTrials { get; set; }
        public int TotalWeTrialsConverted { get; set; }

        public List<VmTrialRecord> TotalWeTrialDetail { get; set; }
        public List<VmTrialRecord> TotalActiveWeTrialDetail { get; set; }
        public List<VmTrialRecord> TotalWeTrialsConvertedDetail { get; set; }

        public VmLineGraph ToGraphObject(List<VmTrialRecord> model)
        {
            var result = new VmLineGraph();
            model.OrderBy(u => u.TrialStartDate).ToList().ForEach(x =>
            {
                var date = x.TrialStartDate.Split(' ')[0];
                if (!result.Xaxis.Contains(date))
                {
                    result.Xaxis.Add(date);
                    result.Yaxis.Add(1);
                }
                else
                {
                    result.Yaxis[result.Xaxis.IndexOf(date)] += 1;
                }
            });

            return result;
        }


        public VmLineGraph ToFilteredGraphObject(List<VmTrialRecord> model, int filter)
        {
            var result = new VmLineGraph();
            model.OrderBy(u => u.TrialStartDate).ToList().ForEach(x =>
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

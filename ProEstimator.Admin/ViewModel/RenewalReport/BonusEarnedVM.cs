using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProEstimator.Admin.ViewModel.RenewalReport
{
    public class BonusEarnedVM
    {
        public string Name { get; set; }
        public string Goal { get; set; }
        public string Forecast { get; set; }
        public string Actual { get; set; }
        public string MTD { get; set; }
        public string MTDPercent { get; set; }

        public BonusEarnedVM() { }

        public BonusEarnedVM(string name, string goal, string forcast, string actual, string mtd, string mtdPercent) 
        {
            Name = name;
            Goal = goal;
            Forecast = forcast;
            Actual = actual;
            MTD = mtd;
            MTDPercent = mtdPercent;
        }
    }
}
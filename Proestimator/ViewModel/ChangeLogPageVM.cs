using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData;
using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel
{
    public class ChangeLogPageVM
    {
        public List<ChangeLogDateVM> DateGroups { get; set; }

        public ChangeLogPageVM()
        {
            DateGroups = new List<ChangeLogDateVM>();    
        }
    }

    public class ChangeLogDateVM
    {
        public DateTime Date { get; set; }

        public List<ChangeLogItemVM> Items { get; set; }

        public ChangeLogDateVM()
        {
            Items = new List<ChangeLogItemVM>();
        }
    }

    public class ChangeLogItemVM
    {
        public string ShortNote { get; set; }
        public string LongNote { get; set; }

        public ChangeLogItemVM(SiteChangeLog log)
        {
            ShortNote = log.ShortNote;
            LongNote = log.LongNote;
        }
    }
}
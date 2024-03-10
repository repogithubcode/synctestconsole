using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData;
using ProEstimatorData.DataModel;

namespace ProEstimator.Admin.ViewModel
{
    public class ChangeLogPageVM
    {
   
    }

    public class ChangeLogDateVM
    {
        public string Date { get; set; }

        public List<ChangeLogItemVM> Items { get; set; }

        public ChangeLogDateVM()
        {
            Items = new List<ChangeLogItemVM>();
        }
    }

    public class ChangeLogItemVM
    {
        public int ID { get; private set; }
        public string Date { get; set; }
        public string ShortNote { get; set; }
        public string LongNote { get; set; }
        public bool IsActive { get; set; }

        public ChangeLogItemVM(SiteChangeLog log)
        {
            ID = log.ID;
            Date = log.Date.ToShortDateString();
            ShortNote = log.ShortNote;
            LongNote = log.LongNote;
            IsActive = log.IsActive;
        }
    }
}
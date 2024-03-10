using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

using ProEstimatorData;

namespace ProEstimator.Admin.ViewModel.LinkRules
{
    public class SectionVM
    {
        public int Header { get; set; }
        public int Section { get; set; }

        public string Name { get; set; }

        public SectionVM()
        { }

        public SectionVM(DataRow row)
        {
            Header = InputHelper.GetInteger(row["nheader"].ToString());
            Section = InputHelper.GetInteger(row["nsection"].ToString());
            Name = InputHelper.GetString(row["SectionName"].ToString());
        }
    }
}
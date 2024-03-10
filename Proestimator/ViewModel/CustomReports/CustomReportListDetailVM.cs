using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.CustomReports
{
    public class CustomReportListDetailVM
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string DeleteRestoreImgName { get; set; }
    }
}
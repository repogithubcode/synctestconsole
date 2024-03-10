using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.PDR
{
    public class PDRLine
    {
        public int ID { get; set; }
        public string SelectedID { get; set; }
        public int SelectedMultiplier { get; set; }
        public int SelectedOversizedDentQuantity { get; set; }
        public string Description { get; set; }
        public string CustomCharge { get; set; }
        public bool IsExpanded { get; set; }
        public int QuantityID { get; set; }
        public int SizeID { get; set; }

        public bool SaveDetails { get; set; }
    }
}
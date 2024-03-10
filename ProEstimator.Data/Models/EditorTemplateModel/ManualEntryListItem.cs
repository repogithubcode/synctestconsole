using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProEstimatorData.Models.EditorTemplateModel
{
    public class ManualEntryListItem
    {
        public int ID { get; set; }
        public int LineNumber { get; set; }
        public string Group { get; set; }
        public string OP { get; set; }
        public string OPDescription {get;set;}
        public string PartName { get; set; }
        public string PartNumber { get; set; }
        public double PartPrice { get; set; }
        public int Quantity { get; set; }
        public string PartSource { get; set; }
        public string Overhaul { get; set; }
        public string LaborItems { get; set; }
        public bool Locked { get; set; }
        public int Modified { get; set; }
        public int OrderNumber { get; set; }
        public int LineNumberCalculated { get; set; }
        public int EstimationDataSuppVer { get; set; }
        public int ProcessedLineSuppVer { get; set; }
        public bool HasManualNotes { get; set; }
    }
}
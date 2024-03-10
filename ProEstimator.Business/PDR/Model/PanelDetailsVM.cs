using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.PDR.Model
{
    public class PanelDetailsVM
    {
        public int PanelID { get; set; }
        public string PanelName { get; set; }
        public List<SectionDetailsVM> SectionDetails { get; private set; } = new List<SectionDetailsVM>();
    }
}

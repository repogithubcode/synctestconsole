using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.Model.Admin
{
    public class VmLineGraph
    {
        public List<string> Xaxis {get;set;}
        public List<int> Yaxis {get;set;}

        public VmLineGraph()
        {
            Xaxis = new List<string>();
            Yaxis = new List<int>();
        }
    }
}

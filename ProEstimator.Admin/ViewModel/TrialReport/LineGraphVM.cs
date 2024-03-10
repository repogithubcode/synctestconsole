using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Admin.ViewModel
{
    public class LineGraphVM
    {
        public List<string> Xaxis {get;set;}
        public List<int> Yaxis {get;set;}

        public LineGraphVM()
        {
            Xaxis = new List<string>();
            Yaxis = new List<int>();
        }
    }
}

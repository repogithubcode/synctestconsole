using ProEstimator.Business.Model.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.ILogic
{
    public interface ILineGraphMap<T>
    {
        VmLineGraph ToGraphObject(List<T> model);
        VmLineGraph ToFilteredGraphObject(List<T> model, int filter);
    }
}

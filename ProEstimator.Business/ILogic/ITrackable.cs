using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.ILogic
{
    public interface ITrackable<T>
    {
        string Serialized { get; set; }
        string ToTrackable();
        T FromTrackable(string item);
        bool CheckForNulls(T obj);
    }
}

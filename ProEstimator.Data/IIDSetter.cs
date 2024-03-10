using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimatorData
{
    /// <summary>
    /// This lets us set a DB classes ID from external code, but makes it kind of difficult to do so it's not done by accident or when not intended.
    /// </summary>
    public interface IIDSetter
    {
        int ID { set; }
    }
}

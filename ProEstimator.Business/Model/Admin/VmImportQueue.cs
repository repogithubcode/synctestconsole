using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.Model.Admin
{
    public class VmImportQueue
    {
        public List<Import> Detail { get; set; }
        public List<VmQueueException> Exceptions { get; set; }
    }
}

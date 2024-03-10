using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.Model
{
    public class VmPnAuthRequest
    {
        public Guid ShopId { get; set; }
        public string Email { get; set; }
    }
}

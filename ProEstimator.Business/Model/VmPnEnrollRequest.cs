using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProEstimator.Business.Model
{
    public class VmPnEnrollRequest
    {
        public string ShopName { get; set; }
        public string ShopAddress1 { get; set; }
        public string ShopAddress2 { get; set; }
        public string ShopCity { get; set; }
        public string ShopState { get; set; }
        public string ShopZip { get; set; }
    }
}
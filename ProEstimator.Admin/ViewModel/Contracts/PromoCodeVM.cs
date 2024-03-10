using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

using ProEstimator.Business.Logic;

namespace ProEstimator.Admin.ViewModel.Contracts
{
    public class PromoCodeVM
    {
        public string PromoCode { get; set; }
        public string Description { get; set; }
    }
}
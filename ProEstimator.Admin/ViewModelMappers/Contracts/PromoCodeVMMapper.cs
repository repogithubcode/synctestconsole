using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using ProEstimator.Admin.ViewModel.Contracts;

namespace Proestimator.Admin.ViewModelMappers.Contracts
{
    public class PromoCodeVMMapper : IVMMapper<PromoCodeVM>
    {
        public PromoCodeVM Map(MappingConfiguration mappingConfiguration)
        {
            PromoCodeVMMapperConfiguration config = mappingConfiguration as PromoCodeVMMapperConfiguration;
            PromoCode promoCode = config.PromoCode;

            PromoCodeVM vm = new PromoCodeVM(); 

            string expirationMessage = "active";

            if (promoCode.EndDate < DateTime.Now)
            {
                expirationMessage = "Ended " + promoCode.EndDate.ToShortDateString();
            }
            else if (promoCode.StartDate > DateTime.Now)
            {
                expirationMessage = "Starts " + promoCode.StartDate.ToShortDateString();
            }

            vm.PromoCode = promoCode.Code;
            vm.Description = promoCode.Code + ": " + promoCode.PromoAmount.ToString("C") + " (" + expirationMessage + ")";

            return vm;
        }
    }

    public class PromoCodeVMMapperConfiguration : MappingConfiguration
    {
        public PromoCode PromoCode { get; set; }

    }
}
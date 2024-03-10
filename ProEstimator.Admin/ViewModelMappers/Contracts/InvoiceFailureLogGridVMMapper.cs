using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimator.Admin.ViewModel.Contracts;
using ProEstimatorData.DataModel.Contracts;

namespace Proestimator.Admin.ViewModelMappers.Contracts
{
    public class InvoiceFailureLogGridVMMapper : IVMMapper<InvoiceFailureLogGridVM>
    {
        public InvoiceFailureLogGridVM Map(MappingConfiguration mappingConfiguration)
        {
            InvoiceFailureLogGridVMMapperConfiguration config = mappingConfiguration as InvoiceFailureLogGridVMMapperConfiguration;
            InvoiceFailureLog failureLog = config.FailureLog;

            InvoiceFailureLogGridVM vm = new InvoiceFailureLogGridVM();
            vm.TimeStamp = failureLog.TimeStamp;
            vm.Note = failureLog.Note;
            vm.LastFour = failureLog.LastFour;
            vm.Expiration = failureLog.Expiration;
            vm.StripeCardID = failureLog.StripeCardID;

            return vm;
        }
    }

    public class InvoiceFailureLogGridVMMapperConfiguration : MappingConfiguration
    {
        public InvoiceFailureLog FailureLog { get; set; }
    }

}
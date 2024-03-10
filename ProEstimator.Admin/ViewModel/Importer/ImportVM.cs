using System;
using System.Data;
using ProEstimator.Business.ILogic;
using ProEstimator.Business.Model.Admin;

namespace ProEstimator.Admin.ViewModel.Import
{
    public class ImportVM
    {
        public int LoginID { get; set; }
        public string LoginName { get; set; }
        public bool LoginImported { get; set; }
        public int SourceEstimates { get; set; }
        public int UnimportedEstimates { get; set; }
        public string Message { get; set; }
        public int SessionSalesRepID { get; set; }
        public string ImpersonateLink { get; set; }

        public ImportVM()
        {

        }

        public ImportVM(VmImport vmImport)
        {
            LoginID = vmImport.LoginID;
            LoginName = vmImport.LoginName;
            LoginImported = vmImport.LoginImported;
            SourceEstimates = vmImport.SourceEstimates;
            UnimportedEstimates = vmImport.UnimportedEstimates;
            Message = vmImport.Message;
        }
    }
}

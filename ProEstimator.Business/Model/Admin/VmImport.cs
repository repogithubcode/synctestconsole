using System;
using System.Data;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmImport : IModelMap<VmImport>
    {
        public int LoginID { get; set; }
        public string LoginName { get; set; }
        public bool LoginImported { get; set; }
        public int SourceEstimates { get; set; }
        public int UnimportedEstimates { get; set; }
        public string Message { get; set; }

        public VmImport()
        {
            LoginID = 0;
            LoginName = "";
            LoginImported = false;
            SourceEstimates = 0;
            UnimportedEstimates = 0;
            Message = "";
        }
        public VmImport ToModel(DataRow row)
        {
            throw new NotImplementedException();
        }
    }
}

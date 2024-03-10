using ProEstimator.Business.ILogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Extension;

namespace ProEstimator.Business.Model.Admin
{
    public class VmQueueException : IModelMap<VmQueueException>
    {
        public int Id {get;set;}
        public int LoginId {get;set;}
        public string Message {get;set;}
        public string CreateDate { get; set; }

      public VmQueueException ToModel(System.Data.DataRow row)
      {
          var model = new VmQueueException();
          model.Id = (int)row["id"];
          model.LoginId = (int)row["LoginId"];
          model.Message = new string(row["Message"].SafeString().Take(500).ToArray());
          model.CreateDate = row["CreateDate"].SafeDate();

          return model;
      }
    }
}

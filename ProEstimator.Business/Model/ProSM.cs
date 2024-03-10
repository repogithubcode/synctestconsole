using ProEstimator.Business.ILogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Extension;

namespace ProEstimator.Business.Model
{
    [DataContract]
    public class QueueImportRequest :  ImportRequest
    {
        
    }

    [DataContract]
    public class QueueImportResponse
    {
        [DataMember]
        public bool Success { get; set; }
    }

    [DataContract]
    public class QueueSizeRequest
    {
        [DataMember]
        public string QueueName { get; set; }
    }

    [DataContract]
    public class QueueSizeResponse
    {
        [DataMember]
        public int Size { get; set; }
    }

    [DataContract]
    public enum ImportStatus
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Waiting = 1,
        [EnumMember]
        Pending = 2,
        [EnumMember]
        Complete = 3,
        [EnumMember]
        Error = 4
    }

    [DataContract]
    public enum Priority
    {
        [EnumMember]
        Standard = 0,
        [EnumMember]
        Priority = 1
    }

    //ImportType
    [DataContract]
    public enum ImportType
    {
        [EnumMember]
        Contracts = 0,
        [EnumMember]
        Estimates = 1,
        [EnumMember]
        All = 2
    }


    [DataContract]
    public enum ImportSource
    {
        [EnumMember]
        Admin = 0,
        [EnumMember]
        WebEst = 1
    }

    //ImportFilter
    [DataContract]
    public enum ImportFilter
    {
        [EnumMember]
        Last3Years = 0,
        [EnumMember]
        All = 1
    }

    [DataContract]
    public class QueueQueryRequest
    {
        [DataMember]
        public string QueueName { get; set; }

        //[DataMember]
        //public Expression<Func<T, bool>> Query { get; set; }
    }

    [DataContract]
    public class ImportRequest
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int LoginId { get; set; }

        [DataMember]
        public int WeEstimates { get; set; }

        [DataMember]
        public int PeEstimates { get; set; }

        [DataMember]
        public string EmailAdddress { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public ImportStatus Status { get; set; }

        [DataMember]
        public Priority Priority { get; set; }

        [DataMember]
        public ImportType Type { get; set; }

        [DataMember]
        public ImportSource Source { get; set; }

        [DataMember]
        public ImportFilter Filter { get; set; }

        [DataMember]
        public string StrStatus { get; set; }

        [DataMember]
        public DateTime EnqueueDate { get; set; }

        [DataMember]
        public DateTime? DequeueDate { get; set; }
    }

    [DataContract]
    public class QueueQueryReponse
    {
        [DataMember]
        public List<Import> Imports { get; set; }
    }

    [DataContract]
    public class Import : ImportRequest, IModelMap<Import>
    {
        [DataMember]
        public int WebEstEstCount { get; set; }

        [DataMember]
        public int PEEstCount { get; set; }

        [DataMember]
        public string TimeRemaining { get; set; }

        public Import ToModel(System.Data.DataRow row)
        {
            var model = new Import();
            model.Id = (int)row["id"];
            model.LoginId = (int)row["LoginId"];
            model.EmailAdddress = row["EmailAdddress"].SafeString();
            model.Content = row["Content"].SafeString();
            model.Status = (ImportStatus) (int)row["Status"];
            model.EnqueueDate = (DateTime)row["EnqueueDate"];

            DateTime myDate;
            //model.DequeueDate = (DateTime?) DateTime.TryParse(row["DequeueDate"].SafeDate(), out myDate);
            DateTime.TryParse(row["DequeueDate"].SafeDate(), out myDate);
            model.DequeueDate = (DateTime) myDate;

            return model;
        }

        public decimal Ratio { get; set; }
    }

    [DataContract]
    public class QueueProgressRequest
    {
        [DataMember]
        public int LoginId { get; set; }
    }

    [DataContract]
    public class ImportProgressItem
    {
        [DataMember]
        public int WebBestEstimates { get;set; }
        [DataMember]
        public int ProEstimatorEstimates { get; set; }
    }

    [DataContract]
    public class QueueProgressResponse
    {
        [DataMember]
        public int LoginId { get; set; }
        [DataMember]
        public ImportProgressItem ImportProgressItem { get; set; }
    }
}

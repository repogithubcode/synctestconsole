using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ProEstimator.Business.Extension;
using ProEstimator.Business.Model.Admin.Reports;
using ProEstimatorData;
using System.Diagnostics;
using ProEstimator.Business.Model;
using System.Messaging;
using System.Dynamic;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Transactions;
using System.Threading;
using Newtonsoft.Json;
using ProEstimator.Business.Model.Admin;
using System.Configuration;

using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Business.Logic.Admin
{
    public class WebEstImportService
    {
        private static WebEstImportService _instance = null;
        private readonly DBAccess _data;
        private AdminService _adminService;

        private WebEstImportService()
        {
            _data = new DBAccess();
            _adminService = new AdminService();
        }

        public static WebEstImportService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new WebEstImportService();
                return _instance;
            }
        }

        public VmImportReport GetImports(VmImportReport input)
        {
            var result = new VmImportReport();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@startdate", input.FromDate.FormatDate()));
            parameters.Add(new SqlParameter("@enddate", input.ToDate.FormatDate()));

            result.FromDate = input.FromDate.FormatDate();
            result.ToDate = input.ToDate.FormatDate();

            var item = _data.ExecuteWithTable(AdminConstants.GetWebestImports, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result.Detail = Tabulate((from DataRow row in ds.Tables[0].Rows select row.ToModel<ImportLineItem>()).ToList());
            }

            return result;
        }

        private List<ImportLineItem> Tabulate(List<ImportLineItem> list)
        {
            var result = new List<ImportLineItem>();
            list.ForEach(item =>
            {
                if (!result.Exists(x => x.LoginId == item.LoginId))
                {
                    Contract currentContract = Contract.GetActive(item.LoginId);
                    var converted = list.Select(x => x.LoginId == item.LoginId && item.Trial).Any() && currentContract != null;
                    item.ConversionComplete = converted ? "Y" : "N";
                    result.Add(item);
                }


            });

            return result;
        }
        /// <summary>
        /// * Done in ProEstimator Service
        //  2.  Send an email to the user with current conversion status (Waiting)(Copy 
        //    Sales Rep, Becky & Me)
        //  * Get status of account conversion for login
        //  * Query the queue for the following
        //  *      Location in Queue, item 5 of 10
        //  *      Status of account conversion
        //  *          Waiting - If currently in the queue
        //  *          Pending - If the current account being processed
        //  *          Complete - If in the journal queue
        //  *          Error - The presence of the message in the dead letter queue??
        //  *    
        /// </summary>
        /// <param name="loginId"></param>
        /// <returns></returns>
        public bool AddImportToQueue(QueueImportRequest import)
        {
            if (PreviousImportExists(import.LoginId))
                return false;

            import.Status = ImportStatus.Waiting;
            import.EnqueueDate = DateTime.Now;

            import.Content = GetEstimateCounts(import.LoginId);

            import.Id = InsertLogImport(import);

            var queue = new MessageQueue(string.Format(@"{0}\private$\{1}", Environment.MachineName, "importer"));
            Message msg = new Message();
            msg.Body = import;
            msg.Priority = import.Priority == Priority.Standard ?
                MessagePriority.Normal :
                MessagePriority.High;

            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            {
                queue.Send(msg, MessageQueueTransactionType.Automatic);
                scope.Complete();
            }

            return true;
        }

        private bool PreviousImportExists(int loginId)
        {
            var result = new List<Import>();
            var previousImportCheckResult = new DBAccessTableResult();
            try
            {
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@loginId", loginId));
                previousImportCheckResult = _data.ExecuteWithTable(AdminConstants.DataMigration_PrevCheck, new SqlParameter("LoginID", loginId));
                if (previousImportCheckResult != null && previousImportCheckResult.DataTable != null)
                {
                    var ds = new DataSet();
                    ds.Tables.Add(previousImportCheckResult.DataTable);
                    result =
                        (from DataRow row in ds.Tables[0].Rows select row.ToModel<Import>()).ToList();
                }
                if (result.Count > 0)
                {
                    var errorMessage = string.Format("Login {0} was previously imported, import haulted exception", loginId);
                    _adminService.LogException(loginId, errorMessage);
                }
                if (!previousImportCheckResult.Success)
                {
                    var errorMessage = previousImportCheckResult.ErrorMessage;
                    //Log error message for loginId to be parsed by the admin project
                    _adminService.LogException(loginId, errorMessage);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = previousImportCheckResult.ErrorMessage;
                //Log error message for loginId to be parsed by the admin project
                _adminService.LogException(loginId, errorMessage);
            }

            return result.Count != 0;
        }

        private string GetEstimateCounts(int p)
        {
            var response = string.Empty;
            var ratio = _adminService.GetEstimateCountsById(p);

            //var myObj = new { ProE = (int)counts.X, WE = (int)counts.Y };
            response = JsonConvert.SerializeObject(ratio);
            //response = myObj.ToJson();

            return response;
        }

        private int InsertLogImport(QueueImportRequest model)
        {
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginId", model.LoginId));
            parameters.Add(new SqlParameter("EmailAdddress", model.EmailAdddress));
            parameters.Add(new SqlParameter("Content", model.Content));
            parameters.Add(new SqlParameter("Status", model.Status));
            parameters.Add(new SqlParameter("EnqueueDate", model.EnqueueDate));
            parameters.Add(new SqlParameter("DequeueDate", null));

            var item = _data.ExecuteWithTable(AdminConstants.INSERT_IMPORT, parameters);
            if (item != null && item.DataTable != null)
            {
                model.Id = (int)(decimal)item.DataTable.Rows[0][0];
            }

            return model.Id;
        }

        private void UpdateLogImport(QueueImportRequest model)
        {
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("Id", model.Id));
            parameters.Add(new SqlParameter("Content", model.Content));
            parameters.Add(new SqlParameter("Status", model.Status));
            parameters.Add(new SqlParameter("DequeueDate", model.DequeueDate));

            var item = _data.ExecuteNonQuery(AdminConstants.UPDATE_IMPORT, parameters);

            if (!string.IsNullOrEmpty(item.ErrorMessage))
            {
                model.Content = item.ErrorMessage;
            }
        }

        public int GetQueueSize(QueueSizeRequest request)
        {
            int result = 0;
            var machineName = Environment.MachineName;
            var queueCounter = new PerformanceCounter(
                "MSMQ Queue",
                "Messages in Queue",
                string.Format(@"{0}\private$\{1}", machineName, request.QueueName)
            );
            result = (int)queueCounter.NextValue();

            return result;
        }

        public List<Import> MapToResponse(Message[] messages)
        {
            var response = new List<Import>();
            messages.ToList().ForEach(message =>
            {
                message.Formatter = new XmlMessageFormatter(new System.Type[1] { typeof(QueueImportRequest) });
                var item = (QueueImportRequest)message.Body;
                var import = new Import()
                {
                    Id = item.Id,
                    LoginId = item.LoginId,
                    EmailAdddress = item.EmailAdddress,
                    Content = item.Content,
                    Status = item.Status,
                    StrStatus = ((ImportStatus)item.Status).ToString(),
                    EnqueueDate = item.EnqueueDate,
                    DequeueDate = item.DequeueDate,
                    PEEstCount = (int)JsonConvert.DeserializeObject<VmRatio>(item.Content).X,
                    WebEstEstCount = (int)JsonConvert.DeserializeObject<VmRatio>(item.Content).Y,
                    Ratio = JsonConvert.DeserializeObject<VmRatio>(item.Content).Payload
                };
                response.Add(import);
            });

            return response;
        }

        public List<Import> GetQueuedImports(QueueQueryRequest request)
        {
            var machineName = Environment.MachineName;
            var path = string.Format(@"{0}\private$\{1}", machineName, request.QueueName);
            var myQueue = new MessageQueue(path);
            myQueue.Formatter = new XmlMessageFormatter(new System.Type[1] { typeof(QueueImportRequest) });
            var queuedItems = myQueue.GetAllMessages();
            var imports = MapToResponse(queuedItems);

            var waitingViaLog = GetWaitingMessages();
            foreach (var m in waitingViaLog)
            {
                if (!imports.Where(item => item.Id == m.Id).Any())
                {
                    imports.Add(m);
                }
            }
            imports.AddRange(GetPendingMessages());
            imports.AddRange(GetCompleteMessages());

            //var journalQ = GetJournaledMessages(path);
            //journalQ.ToList().ForEach(m => {
            //    m.WebEstEstCount = GetWebEstEstCount(m.LoginId);
            //    m.PEEstCount = GetPEEstCount(m.LoginId);

            //    if(m.PEEstCount < m.WebEstEstCount) {
            //        m.Status = ImportStatus.Pending;
            //        m.StrStatus = ((ImportStatus)m.Status).ToString();
            //    } else {
            //        m.Status = ImportStatus.Complete;
            //        m.StrStatus = ((ImportStatus)m.Status).ToString();
            //    }
            //});
            //imports.AddRange(journalQ);

            return imports;
        }

        private List<VmQueueException> GetQueueExceptions(QueueQueryRequest queueQueryRequest)
        {
            var result = new List<VmQueueException>();
            var item = _data.ExecuteWithTable(AdminConstants.GET_QUEUE_EXCEPTIONS, new List<SqlParameter>());
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<VmQueueException>()).ToList();
            }

            return result;
        }

        private IEnumerable<Import> GetWaitingMessages()
        {
            int complete = (int)ImportStatus.Waiting;
            var result = new List<Import>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@status", complete));

            var item = _data.ExecuteWithTable(AdminConstants.GET_IMPORT_MESSAGE_BY_STATUS, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<Import>()).ToList();
                result.ForEach(x =>
                {
                    var counts = JsonConvert.DeserializeObject<VmRatio>(x.Content);

                    x.PEEstCount = (int)counts.X;
                    x.WebEstEstCount = (int)counts.Y;
                    x.Ratio = counts.Payload;
                });
            }

            return result;
        }

        private IEnumerable<Import> GetCompleteMessages()
        {
            int complete = (int)ImportStatus.Complete;
            var result = new List<Import>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@status", complete));

            var item = _data.ExecuteWithTable(AdminConstants.GET_IMPORT_MESSAGE_BY_STATUS, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<Import>()).ToList();
                result.ForEach(x =>
                {
                    var counts = JsonConvert.DeserializeObject<VmRatio>(x.Content);

                    x.PEEstCount = (int)counts.X;
                    x.WebEstEstCount = (int)counts.Y;
                    x.Ratio = counts.Payload;
                });
            }

            return result;
        }

        private IEnumerable<Import> GetPendingMessages()
        {
            int complete = (int)ImportStatus.Pending;
            var result = new List<Import>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@status", complete));

            var item = _data.ExecuteWithTable(AdminConstants.GET_IMPORT_MESSAGE_BY_STATUS, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                result = (from DataRow row in ds.Tables[0].Rows select row.ToModel<Import>()).ToList();

                result.ForEach(x => {
                    var counts = _adminService.GetEstimateCountsById(x.LoginId);

                    x.PEEstCount = (int)counts.X;
                    x.WebEstEstCount = (int)counts.Y;
                    x.Ratio = counts.Payload;
                    x.TimeRemaining = GetRemainingTime(x);
                });
            }

            return result;
        }

        private string GetRemainingTime(Import input)
        {
            var result = string.Empty;
            try
            {
                if (input.PEEstCount <= input.WebEstEstCount)
                {
                    var t = TimeSpan.FromSeconds((input.WebEstEstCount - input.PEEstCount) / (input.PEEstCount / (DateTime.Now - (DateTime)input.DequeueDate).TotalSeconds));
                    result = string.Format("Time Remaining: {0}", t.ToString("h'h 'm'm 's's'"));
                }
                else
                {
                    result = "finishing up...";
                }

            }
            catch (Exception ex)
            {
                _adminService.LogException(input.LoginId, ex.Message);
            }

            return result;
        }

        private int GetPEEstCount(int loginId)
        {
            return 10;
        }

        private int GetWebEstEstCount(int loginId)
        {
            return 100;
        }

        public IEnumerable<Import> GetJournaledMessages(string path)
        {
            List<Import> response = new List<Import>();
            try
            {
                var queue = new MessageQueue(string.Format("{0};journal", path));
                queue.Formatter = new XmlMessageFormatter(new System.Type[1] { typeof(QueueImportRequest) });
                var messages = queue.GetAllMessages();
                response = MapToResponse(messages);
            }
            catch (Exception ex)
            {

            }



            return response;
        }

        public List<Import> GetQueuedImports(string queueName)
        {
            return GetQueuedImports(new QueueQueryRequest() { QueueName = queueName });
        }

        private List<VmQueueException> GetQueueExceptions()
        {
            return GetQueueExceptions(new QueueQueryRequest() { QueueName = string.Empty });
        }

        public VmImportQueue GetQueueDetail(string queueName)
        {
            var result = new VmImportQueue();
            result.Detail = GetQueuedImports(queueName);
            result.Exceptions = GetQueueExceptions();

            return result;
        }

        public VmImportEstimate ConversionComplete(int loginID)
        {
            var result = new VmImportEstimate();
            result.LoginId = loginID;

            DBAccess db = new DBAccess();
            DBAccessTableResult table = db.ExecuteWithTable("LoginsConversionCompleteGet", new SqlParameter("LoginID", loginID));
            if (!table.Success)
            {
                QueueImportRequest data = new QueueImportRequest();
                data.LoginId = loginID;
                data.EnqueueDate = DateTime.Now;

                data.DequeueDate = DateTime.Now;
                data.Status = ImportStatus.Pending;

                //UpdateLogImport(data);
                //SendConversionStatus(data);

                DisableWebEst(data.LoginId);
                MigrateContracts(data.LoginId);
                MigrateVendors(data.LoginId);
                _adminService.MigrateImages(data.LoginId);

                data.Status = ImportStatus.Complete;
                data.Content = GetEstimateCounts(data.LoginId);
                UpdateLogImport(data);

                LogConversionComplete(data);

                result.Success = true;
            }
            else
            {
                result.ErrorMessage = "Login already marked as conversion complete";
            }

            return result;
        }

        public void ImportWF(QueueImportRequest data)
        {
            /*
             * Done in Pro Estimator
            1.  A message is displayed to the user indicating that their account has 
                been scheduled for a conversion & user should work exclusively from 
                PE moving forward.  Supplements can be added until the migration begins.
             * End done in Pro Estimator
             * 
             * Done in ProEstimator Service
            2.  Send an email to the user with current conversion status (Waiting)(Copy 
                Sales Rep, Becky & Me)
             * Get status of account conversion for login
             * Query the queue for the following
             *      Location in Queue, item 5 of 10
             *      Status of account conversion
             *          Waiting - If currently in the queue
             *          Pending - If the current account being processed
             *          Complete - If in the journal queue
             *          Error - The presence of the message in the dead letter queue??
             *    
            3.  Account is enqueued for conversion
            4.  Account status is displayed in the Admin project as Waiting
             * Expose a method to do the following 
             *      Retrieve a list of all message in the queue waiting to be processed
             *      Retrieve the journaled messages
             *  End done in ProEstimator Service
             *  
             * Begin done in ProEstimator.MSMQ.Listener
            5.  Account begins the conversion process & WE is disabled.
             *  Set the disabled flag in the Web Est database's login table for the login ID
             *  
            6.  Account status is displayed in the Admin project as Pending
             * The method described in step #4 will cover this step
             * 
            7.  Send an email to the user with the current conversion status (Pending) 
                and WE has been disabled.  Work exclusively from PE moving forward.
                a. Update sales rep - This account is currently being imported, please 
                do not make any changes to the company profile until the account is completed.
            8.  Account conversion is complete – List completion date
            9.  Account status is displayed in the Admin project as Complete
            10. Send an email to the user with the current conversion status (Successful) 
                (Copy Sales Rep, Becky & Me) – should state that the WE account is now 
                locked, and to work exclusively in PE. (we can get together to wordsmith 
                the email – Most likely will need to get Eric’s input on the verbiage)
             * End done in ProEstimator.MSMQ.Listener
             * 
             * Begin done in ProEstimator Service
            11. Need ability to manually enter shops into the que in a specific position 
                of the rotation.  Also, need to ensure that Becky & I can access the 
                importer for specific sheet import without interfering with the 
                programmatic migrations.
             * Add status enumeration to declare priority status for message to be enqueued
             * End done in ProEstimator Service
            */

            data.DequeueDate = DateTime.Now;

            data.Status = ImportStatus.Pending;
            UpdateLogImport(data);
            LogConversionComplete(data);

            switch (data.Type)
            {
                case ImportType.Contracts:
                    DisableWebEst(data.LoginId);
                    MigrateContracts(data.LoginId);
                    MigrateVendors(data.LoginId);
                    break;
                case ImportType.Estimates:
                    DisableWebEst(data.LoginId);
                    MigrateContracts(data.LoginId);
                    MigrateEstimates(data.LoginId);
                    //_adminService.MigrateImages(data.LoginId);
                    MigrateVendors(data.LoginId);
                    break;
                case ImportType.All:
                    DisableWebEst(data.LoginId);
                    MigrateContracts(data.LoginId);
                    MigrateEstimates(data.LoginId);
                    _adminService.MigrateImages(data.LoginId);
                    break;
                default:
                    break;
            }

            //DisableWebEst(data.LoginId);
            //MigrateContracts(data.LoginId);
            //MigrateEstimates(data.LoginId);
            //_adminService.MigrateImages(data.LoginId);

            // simulate timely process of importing web est estimate data
            ///Thread.Sleep(30000);

            data.Status = ImportStatus.Complete;
            data.Content = GetEstimateCounts(data.LoginId);
            UpdateLogImport(data);
        }

        public void LogConversionComplete(QueueImportRequest data)
        {
            var disableWebEstResult = _data.ExecuteNonQuery(AdminConstants.LoginsConversionCompleteInsert, new SqlParameter("LoginID", data.LoginId), 0);
            if (!disableWebEstResult.Success)
            {
                var errorMessage = disableWebEstResult.ErrorMessage;
                //Log error message for loginId to be parsed by the admin project
                _adminService.LogException(data.LoginId, errorMessage);
            }
        }

        private void MigrateVendors(int loginId)
        {
            var webEstConnectionString = ConfigurationManager.AppSettings["WEConnectionString"];
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginId));
            var migrateVendorResult = _data.ExecuteNonQuery(AdminConstants.MIGRATE_VENDOR, parameters, 0, webEstConnectionString);
            if (!migrateVendorResult.Success)
            {
                var errorMessage = migrateVendorResult.ErrorMessage;
                //Log error message for loginId to be parsed by the admin project
                _adminService.LogException(loginId, errorMessage);
            }
        }

        private void DisableWebEst(int loginId)
        {
            var disableWebEstResult = _data.ExecuteNonQuery(AdminConstants.DISABLE_WEBEST, new SqlParameter("LoginID", loginId), 0);
            if (!disableWebEstResult.Success)
            {
                var errorMessage = disableWebEstResult.ErrorMessage;
                //Log error message for loginId to be parsed by the admin project
                _adminService.LogException(loginId, errorMessage);
            }
        }

        public QueueProgressResponse GetQueueProgressForLoginId(QueueProgressRequest item)
        {
            return new QueueProgressResponse()
            {
                LoginId = item.LoginId,
                ImportProgressItem = new ImportProgressItem
                {
                    WebBestEstimates = 100,
                    ProEstimatorEstimates = 50
                }
            };
        }

        public void MigrateContracts(int loginId)
        {
            var contractMigrationResult = new FunctionResult();
            try
            {
                contractMigrationResult = _data.ExecuteNonQuery(AdminConstants.DataMigration_Contracts, new SqlParameter("LoginID", loginId), 0);
                if (!contractMigrationResult.Success)
                {
                    var errorMessage = contractMigrationResult.ErrorMessage;
                    //Log error message for loginId to be parsed by the admin project
                    _adminService.LogException(loginId, errorMessage);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = contractMigrationResult.ErrorMessage;
                //Log error message for loginId to be parsed by the admin project
                _adminService.LogException(loginId, errorMessage);
            }

            //var contractMigrationResult = _data.ExecuteNonQuery(AdminConstants.DataMigration_Contracts, new SqlParameter("LoginID", loginId), 0);
            //if (!contractMigrationResult.Success)
            //{
            //    var errorMessage = contractMigrationResult.ErrorMessage;
            //    //Log error message for loginId to be parsed by the admin project
            //    _adminService.LogException(loginId, errorMessage);
            //}
        }

        public void MigrateEstimates(int loginId)
        {
            //MigrateContracts(loginId);
            var estimateMigrationResult = new FunctionResult();
            try
            {
                estimateMigrationResult = _data.ExecuteNonQuery(AdminConstants.DataMigration_EstimatesForLogin, new SqlParameter("LoginID", loginId), 0);
                if (!estimateMigrationResult.Success)
                {
                    var errorMessage = estimateMigrationResult.ErrorMessage;
                    //Log error message for loginId to be parsed by the admin project
                    _adminService.LogException(loginId, errorMessage);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = estimateMigrationResult.ErrorMessage;
                //Log error message for loginId to be parsed by the admin project
                _adminService.LogException(loginId, errorMessage);
            }

            //estimateMigrationResult = _data.ExecuteNonQuery(AdminConstants.DataMigration_EstimatesForLogin, new SqlParameter("LoginID", loginId), 0);
            //if (!estimateMigrationResult.Success)
            //{
            //    var errorMessage = estimateMigrationResult.ErrorMessage;
            //    //Log error message for loginId to be parsed by the admin project
            //    _adminService.LogException(loginId, errorMessage);
            //}
        }
    }
}
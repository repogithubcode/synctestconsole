//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.ComponentModel;

//using ProEstimatorData;
//using ProEstimator.Business.Logic;
//using ProEstimatorData.DataModel;

//namespace ProEstimator.TimedEvents
//{
//    public class SyncSuccessBoxEvent : TimedEvent 
//    {

//        public override TimeSpan TimeSpan { get { return new TimeSpan(0, 1, 0); } }

//        private BackgroundWorker _worker;

//        protected override void LoadData()
//        {
//            base.LoadData();

//            LastExecution = DateTime.Now.AddHours(-2);

//            _worker = new BackgroundWorker();
//            _worker.DoWork += _worker_DoWork;
//            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
//            _worker.WorkerSupportsCancellation = true;
//        }
        
//        void _worker_DoWork(object sender, DoWorkEventArgs e)
//        {
//            StringBuilder messageBuilder = new StringBuilder();

//            try
//            {
//                SuccessBoxDataSyncer dataSyncer = new SuccessBoxDataSyncer();

//                // Sync Logins
//                StringBuilder loginsBuilder = new StringBuilder();
//                dataSyncer.SyncLogins(loginsBuilder);

//                //ErrorLogger.LogError(loginsBuilder.ToString(), "SuccessBox SyncLogins");
//                string logData = loginsBuilder.ToString();
//                if (!string.IsNullOrEmpty(logData))
//                {
//                    messageBuilder.AppendLine(logData);
//                }

//                // Sync contracts
//                StringBuilder contractsBuilder = new StringBuilder();
//                dataSyncer.SyncContracts(contractsBuilder);

//                //ErrorLogger.LogError(contractsBuilder.ToString(), "SuccessBox SyncContracts");
//                logData = contractsBuilder.ToString();
//                if (!string.IsNullOrEmpty(logData))
//                {
//                    messageBuilder.AppendLine(logData);
//                }

//                // Sync contracts
//                StringBuilder addOnsBuilder = new StringBuilder();
//                dataSyncer.SyncContractAddOns(addOnsBuilder);

//                //ErrorLogger.LogError(addOnsBuilder.ToString(), "SuccessBox SyncContractAddOns");
//                logData = addOnsBuilder.ToString();
//                if (!string.IsNullOrEmpty(logData))
//                {
//                    messageBuilder.AppendLine(logData);
//                }

//                // Sync invoices
//                StringBuilder invoicesBuilder = new StringBuilder();
//                dataSyncer.SyncInvoices(invoicesBuilder);

//                //ErrorLogger.LogError(invoicesBuilder.ToString(), "SuccessBox SyncInvoices");
//                logData = invoicesBuilder.ToString();
//                if (!string.IsNullOrEmpty(logData))
//                {
//                    messageBuilder.AppendLine(logData);
//                }

//                // Sync features 
//                try
//                {
//                    SuccessBoxDataPoster dataPoster = new SuccessBoxDataPoster();

//                    List<SuccessBoxFeatureLog> features = SuccessBoxFeatureLog.GetUnsyncedFeatureLogs();
//                    if (features.Count > 0)
//                    {
//                        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
//                        stopwatch.Start();

//                        foreach (SuccessBoxFeatureLog feature in features)
//                        {
//                            FunctionResult result = dataPoster.PostFeature(feature);
//                            if (!result.Success)
//                            {
//                                messageBuilder.AppendLine("Error posting feature " + feature.ID + ": " + result.ErrorMessage);
//                            }
//                            else
//                            {
//                                feature.LogSyncedTime();
//                            }
//                        }

//                        // ErrorLogger.LogError("Processed " + features.Count.ToString() + " feature logs in " + stopwatch.ElapsedMilliseconds + " MS" + Environment.NewLine + messageBuilder.ToString(), "SuccessBox SyncFeatures");

//                        messageBuilder.AppendLine("Processed " + features.Count.ToString() + " Success Box feature logs in " + stopwatch.ElapsedMilliseconds + " MS");
//                    }
//                }
//                catch (Exception ex)
//                {
//                    messageBuilder.AppendLine("SuccessBox Error: " + ex.Message + Environment.NewLine + ex.StackTrace);
//                    ProEstimatorData.ErrorLogger.LogError(ex, 0, "SuccessBoxTimedEvent DoWork");
//                }

//            }
//            catch (Exception ex)
//            {
//                messageBuilder.AppendLine("SuccessBox Error: " + ex.Message + Environment.NewLine + ex.StackTrace);
//                ProEstimatorData.ErrorLogger.LogError(ex, 0, "SuccessBoxTimedEvent DoWork");
//            }

//            string messages = messageBuilder.ToString();
//            if (!string.IsNullOrEmpty(messages))
//            {
//                StringBuilder globalBuilder = e.Argument as StringBuilder;
//                globalBuilder.AppendLine(messages);

//                //ErrorLogger.LogError(messages, "SuccessBoxSync");
//            }
//        }

//        void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
//        {
//            ExecutionFinished();
//        }

//        public override void DoWork(StringBuilder messageBuilder)
//        {
//#if !DEBUG
//            base.DoWork(messageBuilder);
//            _worker.RunWorkerAsync(messageBuilder);
//#endif
//        }

//        public override void Cancel()
//        {
//            _worker.CancelAsync();
//        }

//    }
//}
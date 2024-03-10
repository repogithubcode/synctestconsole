using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

using ProEstimator.TimedEvents;
using ProEstimator.Business.Logic;

namespace ProEstimator.TimedEventsUI
{
    public partial class MainForm : Form
    {
        private bool _isRunning = false;
        private bool _isWorking = false;

        private BackgroundWorker _worker;
        TimedEventManager _manager;

        private StringBuilder _builder = new StringBuilder();
        private string fileName = "";

        public MainForm()
        {
            InitializeComponent();

            ProEstimator.Business.Logic.Admin.SalesRepPermissionManager.TurnOffCache();
            ProEstimatorData.DataModel.Contracts.ContractTerms.LoadAll();

            _manager = new TimedEventManager();

            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += _worker_DoWork;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
            _worker.ProgressChanged += _worker_ProgressChanged;

            ToggleRun();
        }

        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (_isWorking)
            {
                try
                {
                    _manager.ProcessEvents(_builder);
                }
                catch (Exception ex)
                {
                    _builder.AppendLine("Process Events error: " + ex.Message);
                    _builder.AppendLine(ex.StackTrace);
                }               

                if (_builder.Length > 0)
                {
                    _worker.ReportProgress(0);
                }

                System.Threading.Thread.Sleep(10000);
            }            
        }

        void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PrintText(_builder.ToString(), false);
            _builder.Clear();
        }

        void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _isRunning = false;
            _isWorking = false;
            PrintText("Background process stopped.", true);
            RefreshButton();
        }

        private void btnToggle_Click(object sender, EventArgs e)
        {
            ToggleRun();
        }

        private void ToggleRun()
        {
            if (_isRunning)
            {
                _worker.CancelAsync();
                _manager.CancelAllEvents();
                PrintText("Canceling background process...", true);
                _isWorking = false;
            }
            else
            {
                _isWorking = true;
                _isRunning = true;
                PrintText("Background process started.", true);
                _worker.RunWorkerAsync();
            }

            RefreshButton();
        }

        private void RefreshButton()
        {
            if (_isRunning && _isWorking)
            {
                btnToggle.Text = "Cancel";
                btnToggle.Enabled = true;
            }
            else if (_isRunning && !_isWorking)
            {
                btnToggle.Text = "Canceling...";
                btnToggle.Enabled = false;
            }
            else if (!_isRunning && !_isWorking)
            {
                btnToggle.Text = "Start";
                btnToggle.Enabled = true;
            }
        }

        private void PrintText(string text, bool addDate)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            // Remove empty lines
            text = System.Text.RegularExpressions.Regex.Replace(text, @"^\s+$[\r\n]*", string.Empty, System.Text.RegularExpressions.RegexOptions.Multiline);

            string newText = (addDate ? DateTime.Now.ToString() + "\t" : "") + text + (addDate ? Environment.NewLine : "") + txtMessages.Text;
            if (newText.Length > 50000)
            {
                newText = newText.Substring(0, 50000);
            }
            txtMessages.Text = newText;

            SaveToFile(text, addDate);
        }

        private void SaveToFile(string text, bool addDate)
        {
            if(fileName == "" || new FileInfo(fileName).Length > 2000000)
            {
                var location = new Uri(Assembly.GetEntryAssembly().CodeBase).LocalPath;
                var path = Path.GetDirectoryName(location);
                fileName = Path.Combine(path, "Log" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");
            }

            using (FileStream fs = File.Open(fileName, FileMode.Append))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes((addDate ? DateTime.Now.ToString() + "\t" : "") + text + Environment.NewLine);

                fs.Write(info, 0, info.Length);
            }
        }

        private void btnSuccessBoxSync_Click(object sender, EventArgs e)
        {
            //SyncSuccessBoxAccountsEvent syncEvent = new SyncSuccessBoxAccountsEvent();
            //StringBuilder builder = new StringBuilder();
            //syncEvent.DoWork(builder);
            //PrintText(builder.ToString(), false);
        }

        private void btnSuccessBoxFeatures_Click(object sender, EventArgs e)
        {
            //SyncSuccessBoxFeaturesEvent syncEvent = new SyncSuccessBoxFeaturesEvent();
            //StringBuilder builder = new StringBuilder();
            //syncEvent.DoWork(builder);
            //PrintText(builder.ToString(), false);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var invoicesEvent = new ProcessInvoicesTimedEvent();

            StringBuilder builder = new StringBuilder();
            invoicesEvent.DoWork(builder);
            PrintText(builder.ToString(), true);
        }
    }

    public class ProcessInvoiceTax
    {
        public void ProcessInvoices()
        {
            List<Invoice> invoices = GetInvoices();

            foreach (Invoice invoice in invoices)
            {
                ProcessInvoice(invoice);
            }
        }

        private List<Invoice> GetInvoices()
        {
            List<Invoice> invoices = new List<Invoice>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("aaaGetInvoicesForTaxFix");

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                invoices.Add(new Invoice(row));
            }

            return invoices;
        }

        private void ProcessInvoice(Invoice invoice)
        {
            Address address = Address.GetForLoginID(invoice.LoginID);
            TaxManager.CalculateTaxForInvoice(invoice, address);
        }

    }

}

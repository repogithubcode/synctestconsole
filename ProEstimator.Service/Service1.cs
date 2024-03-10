using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace ProEstimator.Service
{
    public partial class Service1 : ServiceBase
    {
        private int processId;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var location = new Uri(Assembly.GetEntryAssembly().CodeBase).LocalPath;
            var path = Path.GetDirectoryName(location);
            var serverPath = Path.Combine(path, "ProEstimator.TimedEventsUI.exe");
            
            Process cmd = new Process();
            cmd.StartInfo.FileName = serverPath;
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            cmd.Start();
            processId = cmd.Id;
        }

        protected override void OnStop()
        {
            Process process = null;
            try
            {
                process = Process.GetProcessById(processId);
            }
            finally
            {
                if (process != null)
                {
                    process.Kill();
                    process.WaitForExit();
                    process.Dispose();
                }
            }
        }
    }
}

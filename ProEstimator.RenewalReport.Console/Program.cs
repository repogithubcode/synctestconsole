using ProEstimator.Business.Logic;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.RenewalReport.Console
{
    class Program
    {
        static AdminService _service = new AdminService();
        static void Main(string[] args)
        {
            var connectionString = ConfigurationManager.AppSettings["ProConnectionString"];

            // PE Report
            System.Console.WriteLine(string.Format("Renewal Report Start: {0}, Connection String: {1}", DateTime.Now, connectionString));

            _service.RunRenewalReport("PE");

            System.Console.WriteLine(string.Format("Renewal Report End: {0}, Connection String: {1}", DateTime.Now, connectionString));

            //WE Report
            System.Console.WriteLine(string.Format("Renewal Report Start: {0}, Connection String: {1}", DateTime.Now, connectionString));

            _service.RunRenewalReport("WE");

            System.Console.WriteLine(string.Format("Renewal Report End: {0}, Connection String: {1}", DateTime.Now, connectionString));
        }
    }
}

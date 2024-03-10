using Microsoft.Office.Interop.Excel;
using ProEstimator.Business.Logic;
using ProEstimatorData.DataModel.Admin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DownloadData("", "11/01/2022", "03/10/2024", "false", "false", "false", "false");
        }

        public static void DownloadData(string loginIDFilter, string rangeStartFilter,
                                                        string rangeEndFilter, string hasCardErrorFilter, string autoPayFilter, string paidFilter, string hasStripeInfoFilter)
        {
            try
            {
                SpreadsheetWriter spreadsheetWriter = new SpreadsheetWriter();

                loginIDFilter = string.IsNullOrEmpty(loginIDFilter) ? null : loginIDFilter;
                rangeStartFilter = string.IsNullOrEmpty(rangeStartFilter) ? null : rangeStartFilter;
                rangeEndFilter = string.IsNullOrEmpty(rangeEndFilter) ? null : rangeEndFilter;

                autoPayFilter = string.Compare(autoPayFilter, "false", true) == 0 ? null : autoPayFilter;
                paidFilter = string.Compare(paidFilter, "false", true) == 0 ? null : paidFilter;
                hasStripeInfoFilter = string.Compare(hasStripeInfoFilter, "false", true) == 0 ? null : hasStripeInfoFilter;
                hasCardErrorFilter = string.Compare(hasCardErrorFilter, "false", true) == 0 ? null : hasCardErrorFilter;

                string startDateName = rangeStartFilter.Replace("/", string.Empty);
                string endDateName = rangeEndFilter.Replace("/", string.Empty);

                System.Data.DataTable table = DueInvoice.GetForFilterData(loginIDFilter, rangeStartFilter, rangeEndFilter, hasCardErrorFilter, autoPayFilter, paidFilter, hasStripeInfoFilter);

                table.Columns.Remove("ContractID");
                table.Columns.Remove("StripeCardID");

                string adminFolder = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), "Admin");
                string diskPath = Path.Combine(adminFolder, "DueInvoiceReport_" + startDateName + "_" + endDateName + ".xlsx");

                if (!Directory.Exists(adminFolder))
                {
                    Directory.CreateDirectory(adminFolder);
                }

                if (System.IO.File.Exists(diskPath))
                {
                    System.IO.File.Delete(diskPath);
                }

                spreadsheetWriter.WriteSpreadshet(table, diskPath);

                byte[] fileBytes = System.IO.File.ReadAllBytes(diskPath);
                string filename = "DueInvoiceReport_" + startDateName + "_" + endDateName + ".xlsx";
                File.WriteAllBytes(diskPath, fileBytes);
            }
            catch (Exception ex)
            {
              
            }
        }
    }
}

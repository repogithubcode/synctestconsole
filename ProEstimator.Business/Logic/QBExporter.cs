using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;

using ProEstimatorData;
using ProEstimatorData.DataModel;

using ClosedXML;
using ClosedXML.Excel;

namespace ProEstimator.Business.Logic
{
    public class QBExporter
    {

        public QBExporterResult ExportXLSX(int loginID, DateTime startDate, DateTime endDate, List<int> idsToInclude)
        {
            try
            {
                // Before saving the file, we need to log it to the database to get a unique ID
                QBExportLog exportLog = new QBExportLog();
                exportLog.LoginID = loginID;
                exportLog.StartDate = startDate;
                exportLog.EndDate = endDate;
                exportLog.TimeStamp = DateTime.Now;

                SaveResult exportLogSaveResult = exportLog.Save();
                if (!exportLogSaveResult.Success)
                {
                    return new QBExporterResult(exportLogSaveResult.ErrorMessage);
                }
                
                // Get all of the data, but we will only be adding the one's whos ID is in idsToInclude
                List<QBExportRow> data = QBExportRow.GetData(loginID, startDate, endDate);

                // Start a spreadsheet and add the headers
                XLWorkbook workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Sheet1");

                worksheet.Cell("A1").Value = "Date";
                worksheet.Cell("B1").Value = "EstimateID";
                worksheet.Cell("C1").Value = "Customer Name";
                worksheet.Cell("D1").Value = "Customer Address 1";
                worksheet.Cell("E1").Value = "Customer Address 2";
                worksheet.Cell("F1").Value = "Customer City";
                worksheet.Cell("G1").Value = "Customer State";
                worksheet.Cell("H1").Value = "Customer Zip";
                worksheet.Cell("I1").Value = "Customer Phone";
                worksheet.Cell("J1").Value = "Customer Email";
                worksheet.Cell("K1").Value = "VIN";
                worksheet.Cell("L1").Value = "Item Description";
                worksheet.Cell("M1").Value = "Quantity";
                worksheet.Cell("N1").Value = "Rate";
                worksheet.Cell("O1").Value = "Taxed";
                worksheet.Cell("P1").Value = "Amount";
                worksheet.Cell("Q1").Value = "Item Name";
                worksheet.Cell("R1").Value = "Closed RO Date";
                worksheet.Cell("S1").Value = "State Tax ID";
                worksheet.Cell("T1").Value = "Insurance Company";
                worksheet.Cell("U1").Value = "Insurance Claim Details";
                worksheet.Cell("V1").Value = "QB Invoice ID";
                worksheet.Cell("W1").Value = "Notes";
                worksheet.Cell("X1").Value = "DirectLineItemTotal";
                worksheet.Cell("Y1").Value = "DiscountMarkupLineItemTotal";

                int rowIndex = 2;

                // Add each row to the spreadsheet.  Only add rows who's ID is in the passed list of included estimate IDs.
                int lastEstimateID = 0;

                foreach(QBExportRow row in data)
                {
                    if (idsToInclude.Contains(row.EstimateID))
                    {
                        worksheet.Cell("A" + rowIndex).Value = row.Date.ToShortDateString();
                        worksheet.Cell("B" + rowIndex).Value = row.EstimateID;
                        worksheet.Cell("C" + rowIndex).Value = row.CustomerName;
                        worksheet.Cell("D" + rowIndex).Value = row.CustomerAddress1;
                        worksheet.Cell("E" + rowIndex).Value = row.CustomerAddress2;
                        worksheet.Cell("F" + rowIndex).Value = row.CustomerCity;
                        worksheet.Cell("G" + rowIndex).Value = row.CustomerState;
                        worksheet.Cell("H" + rowIndex).Value = row.CustomerZip;
                        worksheet.Cell("I" + rowIndex).Value = row.CustomerPhone;
                        worksheet.Cell("J" + rowIndex).Value = row.CustomerEmail;
                        worksheet.Cell("K" + rowIndex).Value = row.VIN;
                        worksheet.Cell("L" + rowIndex).Value = row.ItemDescription;
                        worksheet.Cell("M" + rowIndex).Value = row.Quantity;
                        worksheet.Cell("N" + rowIndex).Value = row.Rate;
                        worksheet.Cell("O" + rowIndex).Value = row.Taxed.ToString();
                        worksheet.Cell("P" + rowIndex).Value = row.Amount;
                        worksheet.Cell("Q" + rowIndex).Value = row.ItemName;
                        worksheet.Cell("R" + rowIndex).Value = row.ClosedRODate.ToShortDateString();
                        worksheet.Cell("S" + rowIndex).Value = row.StateTaxID;
                        worksheet.Cell("T" + rowIndex).Value = row.InsuranceCompany;
                        worksheet.Cell("U" + rowIndex).Value = row.InsuranceClaimDetails;
                        worksheet.Cell("V" + rowIndex).Value = row.QBInvoiceID;
                        worksheet.Cell("W" + rowIndex).Value = row.Notes;
                        worksheet.Cell("X" + rowIndex).Value = row.DirectLineItemTotal;
                        worksheet.Cell("Y" + rowIndex).Value = row.DiscountMarkupLineItemTotal;

                        // Log that this estimate has been exported as part of this export.  The data can have multiple rows per estimate (for supplements), only log the estimate once.
                        if (lastEstimateID != row.EstimateID)
                        {
                            QBExportEstimateLog estimateLog = new QBExportEstimateLog();
                            estimateLog.EstimateID = row.EstimateID;
                            estimateLog.ExportLogID = exportLog.ID;

                            Estimate estimate = new Estimate(row.EstimateID);
                            if (estimate != null)
                            {
                                estimateLog.Supplement = estimate.LockLevel;
                            }
                            
                            estimateLog.Save();
                        }

                        lastEstimateID = row.EstimateID;

                        rowIndex++;
                    }
                }

                // Save the xlsx file to disk.
                string savePath = exportLog.GetDiskPath();
                workbook.SaveAs(savePath);

                return new QBExporterResult(exportLog.ID, savePath);
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "QBExporter");
                return new QBExporterResult(ex.Message);
            }
        }

        private List<string> GetColumnLetters()
        {
            List<string> columnLetters = new List<string>();

            for (int i = 65; i <= 90; i++)
            {
                columnLetters.Add(((char)i).ToString());
            }

            return columnLetters;
        }
    }

    public class QBExporterResult : FunctionResult
    {
        public int ExportID { get; private set; }
        public string DiskPath { get; private set; }

        public QBExporterResult(int exportID, string diskPath)
        {
            Success = true;
            ExportID = exportID;
            DiskPath = diskPath;
        }

        public QBExporterResult(string errorMessage)
            : base(errorMessage)
        {

        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Text;
using System.IO;
using System.Configuration;
using System.IO.Compression;
using System.Threading;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel
{

    public class EMSExportManager
    {

        public Report Report { get; private set; }

       

        /// <param name="version">Either 26 or 201</param>
        public ReportFunctionResult ExportEMS(int estimateID, string version, string useVersion)
        {
            StringBuilder errorBuilder = new StringBuilder();
            bool exportBroken = false;

            // This fills the Report property with the new report
            SaveResult saveResult = SaveReportToDB(estimateID, (!string.IsNullOrEmpty(useVersion) ? " " : "") + useVersion);
            if (!saveResult.Success)
            {
                return new ReportFunctionResult(saveResult.ErrorMessage);
            }

            Estimate estimate = new Estimate(estimateID);

            string tempFolder = "";

            try
            {
                // Get paths.
                string reportsFolder = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), estimate.CreatedByLoginID.ToString(), estimate.EstimateID.ToString(), "Reports");
                tempFolder = Path.Combine(reportsFolder, Report.ID.ToString());
                string zipPath = Report.GetDiskPath();

                // Create the temp folder and copy the EMS template files into it
                Directory.CreateDirectory(tempFolder);

                string templateFolder = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), "EMSTemplate", version);
                DirectoryInfo templateDirInfo = new DirectoryInfo(templateFolder);
                foreach (FileInfo fileInfo in templateDirInfo.GetFiles())
                {
                    File.Copy(fileInfo.FullName, Path.Combine(tempFolder, fileInfo.Name));
                }

                // Fill the copied template files and rename them
                if (version == "201")
                {
                    ExportDBF("EMS_Export_GetTotals1_201", estimateID, Report.ID, "totals1", "dbf", ".stl", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetDetails_201", estimateID, Report.ID, "detail", "dbf", ".lin", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetProfileRates1_201", estimateID, Report.ID, "profile1", "dbf", ".pfh", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetProfileRates2_201", estimateID, Report.ID, "profile2", "dbf", ".pfl", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetProfileRates3_201", estimateID, Report.ID, "profile3", "dbf", ".pfp", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetProfileRates4_201", estimateID, Report.ID, "profile4", "dbf", ".pfm", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetProfileRates5_201", estimateID, Report.ID, "profile5", "dbf", ".pft", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetProfileRates6_201", estimateID, Report.ID, "profile6", "dbf", ".pfo", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetEnvelopeData_201", estimateID, Report.ID, "envelope", "dbf", ".env", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetAdministrative1_201", estimateID, Report.ID, "admin1", "dbf", ".ad1", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetAdministrative2_201", estimateID, Report.ID, "admin2", "dbf", ".ad2", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetVehicle_201", estimateID, Report.ID, "vehicle", "dbf", ".veh", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetTotals2_201", estimateID, Report.ID, "totals2", "dbf", ".ttl", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetNonOEM_201", estimateID, Report.ID, "ven", "dbf", ".ven", tempFolder, errorBuilder);

                    // Rename the .dbt files
                    File.Move(Path.Combine(tempFolder, "vehicle.dbt"), Path.Combine(tempFolder, Report.ID + ".dbt"));
                }
                else  // version == "26"
                {
                    ExportDBF("EMS_Export_GetTotals1", estimateID, Report.ID, "Totals1", "dbf", ".STL", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetDetails", estimateID, Report.ID, "Detail", "dbf", ".LIN", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetProfileRates1", estimateID, Report.ID, "Profile1", "dbf", ".PFH", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetProfileRates2", estimateID, Report.ID, "Profile2", "dbf", ".PFL", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetProfileRates3", estimateID, Report.ID, "Profile3", "dbf", ".PFP", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetProfileRates4", estimateID, Report.ID, "Profile4", "dbf", ".PFM", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetProfileRates5", estimateID, Report.ID, "Profile5", "dbf", ".PFT", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetProfileRates6", estimateID, Report.ID, "Profile6", "dbf", ".PFO", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetEnvelopeData", estimateID, Report.ID, "envelope", "dbf", ".ENV", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetAdministrative1", estimateID, Report.ID, "admin1", "dbf", "a.AD1", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetAdministrative2", estimateID, Report.ID, "admin2", "dbf", "b.AD2", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetVehicle", estimateID, Report.ID, "vehicle", "dbf", "v.VEH", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetTotals2", estimateID, Report.ID, "Totals2", "dbf", ".TTL", tempFolder, errorBuilder);
                    ExportDBF("EMS_Export_GetNonOEM", estimateID, Report.ID, "ven", "dbf", ".VEN", tempFolder, errorBuilder);

                    // Rename the .dbt files
                    File.Move(Path.Combine(tempFolder, "admin1.DBT"), Path.Combine(tempFolder, Report.ID + "A.DBT"));
                    File.Move(Path.Combine(tempFolder, "admin2.DBT"), Path.Combine(tempFolder, Report.ID + "B.DBT"));
                    File.Move(Path.Combine(tempFolder, "vehicle.DBT"), Path.Combine(tempFolder, Report.ID + "V.DBT"));
                }

                // Zip the new files 
                ZipFile.CreateFromDirectory(tempFolder, zipPath, CompressionLevel.Fastest, false);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, estimate.CreatedByLoginID, estimateID, "ExportEMS");
                errorBuilder.Append("Error exporting EMS: " + ex.Message);
                exportBroken = true;
            }

            try
            {
                Directory.Delete(tempFolder, true);
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, estimate.CreatedByLoginID, "EMSExportDeleteTemp");
            }

            ReportFunctionResult result = new ReportFunctionResult(Report);
            result.Success = !exportBroken;
            result.ErrorMessage = errorBuilder.ToString();

            return result;
        }

        private SaveResult SaveReportToDB(int estimateID, string version)
        {
            Report = new Report();
            Report.EstimateID = estimateID;
            Report.ReportType = ReportType.GetAll().FirstOrDefault(o => o.Tag == "EMS");
            Report.Version = version;
            Report.FileName = ProEstimatorData.DataModel.Report.GetUniqueReportName(estimateID, "EMS");
            return Report.Save();
        }

        public void ExportDBF(string storedProcedure, int estimateID, int envelopeID, string fileName, string oldExtension, string newExtension, string folderPath, StringBuilder errorBuilder)
        {
            string commandText = "";

            try
            {
                // Get the data to insert
                DBAccess dbAccess = new DBAccess();
                DBAccessTableResult tableResult = dbAccess.ExecuteWithTable(storedProcedure, new SqlParameter("AdminInfoID", estimateID));
                DataTable data = tableResult.DataTable;

                // Open a connection to the dbf file
                string fullFilePath = Path.Combine(folderPath, fileName);
                //string connection = "Provider=Microsoft.Jet.OleDb.4.0;Data Source=" + folderPath + "; Extended Properties=dBase IV";
                string connection = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + folderPath + "; Extended Properties=dBase IV";

                using (OleDbConnection conn = new OleDbConnection(connection))
                {
                    conn.Open();

                    foreach (DataRow row in data.Rows)
                    {
                        StringBuilder insertBuilder = new StringBuilder();
                        StringBuilder valuesBuilder = new StringBuilder();

                        insertBuilder.Append("INSERT INTO " + fileName + " (");
                        valuesBuilder.Append("VALUES (");

                        for (int i = 0; i < data.Columns.Count; i++)
                        {
                            if (i > 0)
                            {
                                insertBuilder.Append(", ");
                                valuesBuilder.Append(", ");
                            }

                            insertBuilder.Append(data.Columns[i].ColumnName);
                            valuesBuilder.Append("@" + data.Columns[i].ColumnName);
                        }

                        insertBuilder.Append(")");
                        valuesBuilder.Append(")");

                        commandText = insertBuilder.ToString() + " " + valuesBuilder.ToString();

                        using (OleDbCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = insertBuilder.ToString() + " " + valuesBuilder.ToString();

                            for (int i = 0; i < data.Columns.Count; i++)
                            {
                                cmd.Parameters.Add(new OleDbParameter("@" + data.Columns[i].ColumnName, row[i]));
                            }

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // Rename the file by Moving it
                string oldFileName = System.IO.Path.Combine(folderPath, fileName + "." + oldExtension);
                string newFileName = System.IO.Path.Combine(folderPath, envelopeID + newExtension);
                System.IO.File.Move(oldFileName, newFileName);
            }
            catch (System.Exception ex)
            {
                errorBuilder.AppendLine("Error creating " + fileName + ": " + ex.Message);
            }
        }

    }
}
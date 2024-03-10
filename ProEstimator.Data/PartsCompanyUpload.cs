using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

using ProEstimatorData.Models;

namespace ProEstimatorData
{
    public static class PartsCompanyUpload
    {
        public static Boolean UploadPartsCompanyData(DataTable partsCompanyDataTable, string partsCompanyName, List<string> sourceIDCollection)
        {
            Boolean isCompleted = false;
            int bulkCopyTimeout = 0;

            string consString = ConfigurationManager.AppSettings["ProConnectionString"];
            using (SqlConnection con = new SqlConnection(consString))
            {
                con.Open();

                using (SqlTransaction sqlTransaction = con.BeginTransaction())
                {
                    for (int eachIndex = 0; eachIndex < sourceIDCollection.Count; eachIndex++)
                    {
                        SqlCommand command = con.CreateCommand();
                        command.CommandText = "[dbo].[DeleteOtherPartsData]";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@SourceName", partsCompanyName));

                        // Must assign both transaction object and connection
                        // to Command object for a pending local transaction
                        command.Connection = con;
                        command.Transaction = sqlTransaction;

                        string sourceIDCurr = sourceIDCollection[eachIndex];

                        command.Parameters.Add(new SqlParameter("@SourceID", sourceIDCurr));

                        if (eachIndex >= 1)
                        {
                            string sourceIDPrev = sourceIDCollection[eachIndex - 1];
                            partsCompanyDataTable.Select(string.Format("[SourceID] = '{0}'", sourceIDPrev)).ToList<DataRow>().
                                                                                                ForEach(r => r["SourceID"] = sourceIDCurr);
                        }

                        try
                        {
                            using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con, SqlBulkCopyOptions.Default, sqlTransaction))
                            {
                                //Set the database table name
                                sqlBulkCopy.DestinationTableName = "dbo.otherparts";

                                //[OPTIONAL]: Map the DataTable columns with that of the database table
                                sqlBulkCopy.ColumnMappings.Add("Source", "Source");
                                sqlBulkCopy.ColumnMappings.Add("SourceName", "SourceName");
                                sqlBulkCopy.ColumnMappings.Add("SourceID", "SourceID");

                                sqlBulkCopy.ColumnMappings.Add("PartsLink #", "SourcePartNumber");
                                sqlBulkCopy.ColumnMappings.Add("OEM", "OEMPartNumber");
                                sqlBulkCopy.ColumnMappings.Add("Description", "Description");
                                sqlBulkCopy.ColumnMappings.Add("Side", "Side");
                                sqlBulkCopy.ColumnMappings.Add("Std Price", "Price");
                                sqlBulkCopy.ColumnMappings.Add("CAPA", "CAPA");
                                sqlBulkCopy.ColumnMappings.Add("MQVP", "MQVP");

                                //sqlBulkCopy.ColumnMappings.Add("Part Number", "PartNumber");
                                //sqlBulkCopy.ColumnMappings.Add("Free Stk", "FreeStk");

                                // Set the timeout.
                                bulkCopyTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["BulkCopyTimeout"]);
                                sqlBulkCopy.BulkCopyTimeout = bulkCopyTimeout;
                                command.ExecuteNonQuery();
                                sqlBulkCopy.WriteToServer(partsCompanyDataTable);

                                isCompleted = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            isCompleted = false;
                            sqlTransaction.Rollback();
                            break;
                        }
                    }

                    if (isCompleted == true)
                    {
                        sqlTransaction.Commit();
                    }
                }
                con.Close();
            }

            return isCompleted;
        }
    }
}
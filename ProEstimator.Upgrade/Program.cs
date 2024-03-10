using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.IO;

using ProEstimatorData;

namespace ProEstimator.Upgrade
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Changing Company Logo images.");

            try
            {
                string query = "SELECT CustomerNumber AS LoginID, ID AS OrganizationInfoID, LogoImageType FROM OrganizationInfo WHERE ISNULL(CustomerNumber, 0) > 0 AND ISNULL(LogoImageType, '') <> '' ORDER BY LoginID";

                DBAccess db = new DBAccess("data source=10.0.7.13,1433;initial catalog=FocusWrite;password=focuswrite;user id=fw;");
                DBAccessTableResult tableResult = db.ExecuteWithTableForQuery(query);

                if (tableResult.Success)
                {
                    Console.WriteLine("Found " + tableResult.DataTable.Rows.Count + " rows to process...");

                    string oldRoot = "D:\\ProEstimatorStorage\\CompanyLogos";
                    string newRoot = "D:\\ProEstimatorStorage\\NewCompanyLogos";

                    if (!Directory.Exists(newRoot))
                    {
                        Directory.CreateDirectory(newRoot);
                    }

                    int counter = 1;

                    foreach (DataRow row in tableResult.DataTable.Rows)
                    {
                        string loginID = row["LoginID"].ToString();
                        string orgInfoID = row["OrganizationInfoID"].ToString();
                        string extension = row["LogoImageType"].ToString();

                        string oldPath = Path.Combine(oldRoot, orgInfoID + "." + extension);
                        string newPath = Path.Combine(newRoot, loginID + "." + extension);

                        if (File.Exists(oldPath))
                        {
                            try
                            {
                                File.Copy(oldPath, newPath);
                                Console.WriteLine(counter + " of " + tableResult.DataTable.Rows.Count + ": Success.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(counter + " of " + tableResult.DataTable.Rows.Count + ": Error: " + ex.Message);
                            }
                        }
                        else
                        {
                            Console.WriteLine(counter + " of " + tableResult.DataTable.Rows.Count + ": Old file not found.");
                        }

                        counter++;
                    }
                }
                else
                {
                    Console.WriteLine("Error loading data: " + tableResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message + Environment.NewLine + ex.StackTrace);
            }

            Console.Read();
        }
    }
}

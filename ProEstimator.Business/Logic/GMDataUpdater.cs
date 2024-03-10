using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;

using Newtonsoft.Json;

using ProEstimatorData.DataModel;
using ProEstimatorData;

namespace ProEstimator.Business.Logic
{
    public class Token
    {
        public string tokenId { get; set; }
    }

    public class PartsPriceResult
    {
        public string ReferenceID { get; set; }
        public bool? QuoteRequired { get; set; }
        public List<PartPrice> PartPrices { get; set; }
    }

    public class PartPrice
    {
        public string DisplayedPartNumber { get; set; }
        public string PartNumber { get; set; }
        public int LineBarcode { get; set; }
        public string Description { get; set; }
        public bool IsDynamicPrice { get; set; }
        public decimal? Price { get; set; }
    }

    public class GMDataUpdater
    {

        private static List<int> _makeIDs = new List<int>() { 6, 7, 9, 15, 16, 26, 28, 29, 33, 36, 41, 94 };

        private static int? _days;

        private int _loginID = 0;
        private int _estimateID = 0;

        public GMDataUpdater(int loginID, int estimateID)
        {
            _loginID = loginID;
            _estimateID = estimateID;
        }

        public void UpdateForVehicle(Vehicle vehicle)
        {
            try
            {
                if (_makeIDs.Contains(vehicle.MakeID))
                {
                    if (!_days.HasValue)
                    {
                        _days = InputHelper.GetInteger(System.Configuration.ConfigurationManager.AppSettings.Get("GMDataDays"), 1);

                        if (_days < 1)
                        {
                            _days = 1;
                        }
                    }

                    DBAccess db = new DBAccess();
                    DBAccessTableResult result = db.ExecuteWithTable("getServiceBarcodeFromVehicleIDGM", new System.Data.SqlClient.SqlParameter("VehicleID", vehicle.VehicleID));
                    if (result.Success)
                    {
                        DateTime lastUpdated = InputHelper.GetDateTime(result.DataTable.Rows[0]["LastUpdated"].ToString(), DateTime.MinValue);

                        if (lastUpdated < DateTime.Now.AddDays(_days.Value * -1))
                        {
                            int serviceBarCode = InputHelper.GetInteger(result.DataTable.Rows[0]["ServiceBarcode"].ToString());

                            Thread thread = new Thread(() => DoUpdate(serviceBarCode));
                            thread.Start();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, _loginID, _estimateID, "GMDataUpdater UpdateForVehicle");
            }
        }

        private void DoUpdate(int serviceBarcode)
        {
            try
            {
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                int results = 0;

                string url = "https://estimate.mymitchell.com/PartsSelectionService/7/PartPrices/" + serviceBarcode + "?manufacturer=GM";
                    //+ "&year=" + year.ToString()
                    //+ "&makeId=" + make.ToString()
                    //+ "&modelId=" + model.ToString();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "application/json";

                string json = GetToken();
                Token token = JsonConvert.DeserializeObject<Token>(json);
                request.Headers["ID_Token"] = token.tokenId;

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode.ToString().ToLower() == "ok")
                    {
                        DBAccess db = new DBAccess(DatabaseName.Mitchell);

                        string logPath = "";

                        using (var contentReader = new StreamReader(response.GetResponseStream()))
                        {
                            string contentText = contentReader.ReadToEnd();

                            PartsPriceResult result = JsonConvert.DeserializeObject<PartsPriceResult>(contentText);

                            List<PartPrice> parts = result.PartPrices.Where(x => x.Price > 0 && x.IsDynamicPrice).ToList();
                            results = parts.Count;

                            StringBuilder updateBuilder = new StringBuilder();

                            foreach (PartPrice p in parts)
                            {
                                int price = Decimal.ToInt32((decimal)p.Price * 100);

                                if (p.DisplayedPartNumber == "ORDER FROM DEALER")
                                {
                                    updateBuilder.AppendLine("update detail set price_1 = " + price + ", Effective_Date_1 = '" + DateTime.Today + "', referenceID = '" + result.ReferenceID + "' where Service_Barcode = " + serviceBarcode + " and Part_Number = '" + p.DisplayedPartNumber + "' and barcode = " + p.LineBarcode + ";");
                                }
                                else
                                {
                                    updateBuilder.AppendLine("update detail set price_1 = " + price + ", Effective_Date_1 = '" + DateTime.Today + "', referenceID = '" + result.ReferenceID + "' where Service_Barcode = " + serviceBarcode + " and (Part_Number_Unpunc = '" + p.DisplayedPartNumber.Replace("GM PART", "") + "' OR Part_Number_Unpunc = '" + p.DisplayedPartNumber.Replace("GM PART", "") + "GMPART' OR Part_Number = '" + p.DisplayedPartNumber + "');");
                                }
                            }

                            db.ExecuteSql(updateBuilder.ToString(), 120);

                            if (!string.IsNullOrEmpty(logPath))
                            {
                                System.IO.File.AppendAllText(logPath, Environment.NewLine + Environment.NewLine + updateBuilder.ToString());
                            }
                        }

                        db.ExecuteSql("update vinn.dbo.Vehicle_Service_Xref set LastUpdated = getdate() where Service_Barcode = " + serviceBarcode + ";");

                        //ErrorLogger.LogError("Service barcode: " + serviceBarcode + " Results: " + results + " Milliseconds: " + stopwatch.ElapsedMilliseconds, _loginID, _estimateID, "GMDataUpdater DoUpdate Log");
                    }
                }                
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, _loginID, _estimateID, "GMDataUpdater DoUpdate Error");
            }
        }

        private string GetToken()
        {
            try
            {
                //estimate-uat for test
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://estimate.mymitchell.com/ngeauthenticationservice/6.0/identity/authenticate?username=Z4_WEBEST1&password=password");
                request.Method = "GET";
                request.ContentType = "application/json";

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode.ToString().ToLower() == "ok")
                    {
                        using (var contentReader = new StreamReader(response.GetResponseStream()))
                        {
                            return contentReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, _loginID, _estimateID, "GMDataUpdater GetToken");
                return string.Empty;
            }

            return string.Empty;
        }

    }
}
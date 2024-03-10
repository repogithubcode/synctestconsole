using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using ProEstimator.Business.Model;
using ProEstimator.Business.Model.Admin;
using ProEstimatorData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Configuration;

using ProEstimator.Business.Extension;
using ProEstimatorData.Models.EditorTemplateModel;
using ProEstimator.Business.Type;
using ProEstimatorData.DataModel;

namespace ProEstimator.Business.Logic
{
    public class PartsNowService
    {
        private readonly DBAccess _data;

        string API = ConfigurationManager.AppSettings["PNAPIADDRESS"];      // "https://api-stage.partsnow.io/webest/";
        string key = ConfigurationManager.AppSettings["PNAPIKEY"];          // "Webest-stage hXJqDTFMXLGGCQpCngXyvZnWQRVGcj9U";

        VmPnEnrollResponse PnGetEnrollmentQueryRes;

        public PartsNowService()
        {
            _data = new DBAccess();
        }

        public async Task<PnWorkflowResponse> PnWorkFlow(PnWorkflowRequest request, int estimateID)
        {
            var result = new PnWorkflowResponse();
            LoginInfo loginInfo = LoginInfo.GetByID(request.LoginID);
            result.UserEditModels = new VmUserMaintenanceEdit(loginInfo);

            if (loginInfo != null)
            {
                Address address = Address.GetForContact(loginInfo.ContactID);
                if (address != null)
                {
                    var enrollRequest = new VmPnEnrollRequest();
                    enrollRequest.ShopName = loginInfo.CompanyName;
                    enrollRequest.ShopAddress1 = address.Line1;
                    enrollRequest.ShopAddress2 = address.Line2;
                    enrollRequest.ShopCity = address.City;
                    enrollRequest.ShopState = address.State;
                    enrollRequest.ShopZip = address.Zip;

                    var resEnrollShop = await EnrollShop(enrollRequest, request.LoginID);
                    result.enrollResult = resEnrollShop;
                    result.enrollTableResult = PnGetEnrollmentQueryRes;

                    if (resEnrollShop.Success)
                    {
                        ProEstimatorData.ErrorLogger.LogError("EnrollShop success for " + loginInfo.ID, "PartsNowWorkFlow");

                        result.Enrollment = resEnrollShop.Enrollment;
                        var authRequest = new VmPnAuthRequest();
                        authRequest.ShopId = Guid.Parse(resEnrollShop.ShopId);
                        authRequest.Email = result.UserEditModels.EmailAddress;
                        var resAuthorize = await Authorize(authRequest);
                        result.authorizeResult = resAuthorize;

                        if (resAuthorize.Success)
                        {
                            ProEstimatorData.ErrorLogger.LogError("Authorize success for " + loginInfo.ID, "PartsNowWorkFlow");

                            var vehicle = ProEstimatorData.DataModel.Vehicle.GetByEstimate(request.Id);
                            result.vehicle = vehicle;

                            var estimateRequest = new VmPnEstimateRequest();
                            estimateRequest.ShopId = Guid.Parse(resEnrollShop.ShopId);
                            estimateRequest.ReferenceNumber = request.Id.ToString();
                            estimateRequest.CreateDate = DateTime.Now.ToString();

                            estimateRequest.Vin = vehicle.Vin;
                            estimateRequest.Year = vehicle.Year.ToString();
                            estimateRequest.Make = GetVehicleMakes(vehicle.Year).FirstOrDefault(i => i.Key == vehicle.MakeID.ToString()).Value;
                            //vehicle.MakeID.ToString();
                            estimateRequest.Model = GetVehicleModels(vehicle.Year, vehicle.MakeID).FirstOrDefault(i => i.Key == vehicle.ModelID.ToString()).Value;
                            //vehicle.ModelID.ToString();
                            estimateRequest.Color = vehicle.ExteriorColor;

                            estimateRequest.CustomerName = request.CustomerName;

                            request.List.ForEach(line =>
                            {
                                if (!string.IsNullOrEmpty(line.PartNumber))
                                {
                                    var r = new VmPnRequestLineItem();
                                    r.LineNumber = line.LineNumber;
                                    r.PartType = TranslatePartSource(line.PartSource);
                                    r.OemPartNumber = line.PartNumber;
                                    r.PartDescription = line.PartName;
                                    r.Quantity = 1;
                                    r.ListPrice = (decimal)line.PartPrice;
                                    r.Price = (decimal)line.PartPrice;

                                    estimateRequest.Lines.Add(r);
                                    result.Lines.Add(r);
                                }
                            });

                            var resCreateEstimate = await CreateEstimate(request.LoginID, estimateRequest);
                            result.estimate = resCreateEstimate;
                            if (resCreateEstimate.Success)
                            {
                                MiscTracking.Insert(loginInfo.ID, estimateID, "PartsNow");

                                ProEstimatorData.ErrorLogger.LogError("CreateEstimate success for " + loginInfo.ID, "PartsNowWorkFlow");
                                result.Uri = resAuthorize.RedirectUri;
                            }
                            else
                            {
                                ProEstimatorData.ErrorLogger.LogError("CreateEstimate fail for " + loginInfo.ID, "PartsNowWorkFlow");
                            }
                        }
                    }
                }
                else
                {
                    ProEstimatorData.ErrorLogger.LogError("Address not found for " + loginInfo.ID + " ContactID: " + loginInfo.ContactID, "PartsNowWorkFlow");
                }
            }
            else
            {
                ProEstimatorData.ErrorLogger.LogError("Login Info not found for " + request.LoginID, "PartsNowWorkFlow");
            }

            return result;
        }
        private string TranslatePartSource(string partSource)
        {
            var result = string.Empty;
            switch (partSource)
            {
                case "After":
                    result = CeicaPartType.Aftermarket;
                    break;
                case "LKQ":
                    result = CeicaPartType.LKQ;
                    break;
                case "OEM":
                    result = CeicaPartType.OEM;
                    break;
                case "Other":
                    result = CeicaPartType.Other;
                    break;
                case "Reman":
                    result = CeicaPartType.Remanufactured;
                    break;
            }

            return result;
        }
        public static List<KeyValuePair<string, string>> GetVehicleModels(int year, int makeid)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("Year", year));
            parameters.Add(new SqlParameter("MakeID", makeid));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("SP_GetModels", parameters);

            List<KeyValuePair<string, string>> results = new List<KeyValuePair<string, string>>();

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    results.Add(new KeyValuePair<string, string>(row["ModelId"].ToString(), row["Model"].ToString()));
                }
            }

            return results;
        }
        public static List<KeyValuePair<string, string>> GetVehicleMakes(int year)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("SP_GetMakes", new SqlParameter("Year", year));

            List<KeyValuePair<string, string>> results = new List<KeyValuePair<string, string>>();

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    results.Add(new KeyValuePair<string, string>(row["MakeId"].ToString(), row["Make"].ToString()));
                }
            }

            return results;
        }
        public async Task<VmPnEnrollResponse> EnrollShop(VmPnEnrollRequest item, int loginId)
        {
            VmPnEnrollResponse res = Retrieve(item, loginId);
            PnGetEnrollmentQueryRes = res;
            if (res == null)
            {
                var enroll = "enroll";
                res = await PerformHttpRequest<VmPnEnrollRequest, VmPnEnrollResponse>(item, enroll, null);
                Commit(res, loginId);
                res.Enrollment = true;
            }
            else
            {
                res.Success = true;
            }

            return res;
        }
        private VmPnEnrollResponse Retrieve(VmPnEnrollRequest item, int loginId)
        {
            var parameters = new List<SqlParameter>();
            var dataSet = new DataSet();
            parameters.Add(new SqlParameter("LoginID", loginId));

            var res = _data.ExecuteWithTable(PnConstants.GET_ENROLLMENT, parameters);
            if (res != null && res.DataTable != null)
            {
                res.DataTable.TableName = AdminConstants.NO_OF_LOGINS_TABLE;
                dataSet.Tables.Add(res.DataTable);
            }

            return (from DataRow row in dataSet.Tables[0].Rows select row.ToModel<VmPnEnrollResponse>()).FirstOrDefault();
        }
        private void Commit(VmPnEnrollResponse arg, int loginId)
        {
            var result = new DataSet();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("loginId", loginId));
            parameters.Add(new SqlParameter("ShopId", arg.ShopId));
            parameters.Add(new SqlParameter("ShopUri", arg.ShopUri));
            parameters.Add(new SqlParameter("RequestId", arg.RequestId));

            _data.ExecuteNonQuery(PnConstants.INSERT_ENROLLMENT, parameters);
        }
        private void CommitEstimate(int loginId, string item)
        {
            var result = new DataSet();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("loginId", loginId));
            parameters.Add(new SqlParameter("request", item));

            _data.ExecuteNonQuery(PnConstants.INSERT_ESTIMATE, parameters);
        }
        public bool DisEnrollShop(VmPnEnrollRequest item)
        {
            var result = true;

            return result;
        }
        public async Task<VmPnAuthorizeResponse> Authorize(VmPnAuthRequest item)
        {
            var result = new VmPnAuthorizeResponse();

            var authorize = string.Format("{0}/users/{1}/auth", item.ShopId, item.Email);
            result = await PerformHttpRequest<VmPnAuthRequest, VmPnAuthorizeResponse>(item, authorize, null);

            return result;
        }
        public VmPnDeauthorize Deauthorize(VmPnAuthRequest item)
        {
            var result = new VmPnDeauthorize();

            return result;
        }
        public async Task<VmPnEstimateResponse> CreateEstimate(int loginID, VmPnEstimateRequest item)
        {
            var result = new VmPnEstimateResponse();

            var estimate = string.Format("{0}/estimates", item.ShopId);
            var json = new JavaScriptSerializer().Serialize(item);
            CommitEstimate(loginID, json);
            result = await PerformHttpRequest<VmPnEstimateRequest, VmPnEstimateResponse>(item, estimate, json);

            return result;
        }
        public async Task<U> PerformHttpRequest<T, U>(T request, string endpoint, string data) where U : new()
        {
            var result = new U();
            var strJson = string.Empty;

            using (var client = new HttpClient())
            {
                var url = string.Format("{0}{1}", API, endpoint);
                var myJson = new JObject();

                if (request != null && string.IsNullOrEmpty(data))
                {
                    var strPropNamesAndValues =
                    request
                        .GetType()
                        .GetProperties()
                        .Where(i => (i.PropertyType == typeof(string) && i.GetGetMethod() != null) ||
                                    (i.PropertyType == typeof(int) && i.GetGetMethod() != null) ||
                                    (i.PropertyType == typeof(decimal) && i.GetGetMethod() != null))
                        .Select(p => new
                        {
                            Name = p.Name,
                            Value = p.GetGetMethod().Invoke(request, null)
                        });
                    foreach (var item in strPropNamesAndValues)
                    {
                        myJson.Add(item.Name, (string)item.Value);
                    }

                    var jsonSerializerSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };
                    var interimObject = JsonConvert.DeserializeObject<ExpandoObject>(myJson.ToString());
                    strJson = JsonConvert.SerializeObject(interimObject, jsonSerializerSettings);
                }
                else
                {
                    strJson = data;
                }

                try
                {
                    ErrorLogger.LogError("Json: " + strJson + "   To url: " + url, "PartsNowService PerformHttpRequest");

                    var content = new StringContent(strJson, Encoding.UTF8, "application/json");
                    client.DefaultRequestHeaders.Add("Authorization", key);
                    var response = await client.PostAsync(url, content);
                    var strData = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<U>(strData);
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, 0, "PartsNowService PerformHttpRequest");
                    throw ex;
                }
            }

            return result;
        }
        public List<VmPartsNowCustomer> GetPartsNowClients()
        {
            var result = new List<VmPartsNowCustomer>();
            var parameters = new List<SqlParameter>();
            var item = _data.ExecuteWithTable(AdminConstants.GET_PARTS_NOW_CLIENTS, parameters);
            if (item != null && item.DataTable != null)
            {
                var ds = new DataSet();
                ds.Tables.Add(item.DataTable);
                return (from DataRow row in ds.Tables[0].Rows select row.ToModel<VmPartsNowCustomer>()).OrderBy(x => x.companyname).ToList();
            }

            return result;
        }
        public static DataTable GetPartsNowClientsFilterData()
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable(AdminConstants.GET_PARTS_NOW_CLIENTS);
            return tableResult.DataTable;
        }

        public bool UpdatePartsNowClientStatus(VmPartsNowSave item)
        {
            var result = new List<VmPartsNowCustomer>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@id", item.Id));
            parameters.Add(new SqlParameter("@status", item.Status));
            _data.ExecuteNonQuery(AdminConstants.UPDATE_PARTS_NOW_CLIENT, parameters);

            return true;
        }
        public bool UpdatePartsNowClientStatus(int Id, Boolean status)
        {
            var result = new List<VmPartsNowCustomer>();
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@id", Id));
            parameters.Add(new SqlParameter("@status", status));
            _data.ExecuteNonQuery(AdminConstants.UPDATE_PARTS_NOW_CLIENT, parameters);

            return true;
        }
        public bool GetPartsNowByLogin(int id)
        {
            LoginInfo loginInfo = LoginInfo.GetByID(id);
            return loginInfo.PartsNow;
        }

        public async Task<List<VmPartsNowReport>> GetPartsNowReport(DateTime fromDate, DateTime toDate)
        {
            var result = new List<VmPartsNowReport>();
            foreach (var shopId in GetShopIds())
            {
                var x = await GetReportForShopId(shopId, fromDate, toDate);
                result.AddRange(x);
            }

            return result;
        }
        private async Task<List<VmPartsNowReport>> GetReportForShopId(string shopId, DateTime fromDate, DateTime toDate)
        {
            var res = new List<VmPartsNowReport>();
            var url = string.Format("{0}/estimates/search?from={1}&to={2}", shopId, fromDate.ToString(), toDate.ToString());
            var apiRes = await PerformHttpRequest<PnApiResponse>(url);
            foreach (var item in apiRes.results)
            {
                res.Add(new VmPartsNowReport()
                {
                    Id = item.estimateId,
                    LoginId = item.summary.buyerId,
                    EstimateUri = item.estimateUri,
                    EstimateId = item.summary.referenceNumber,
                    Status = item.summary.status,
                    SupercededId = item.summary.supercededByEstimateId
                });
            }

            return res;
        }
        private List<string> GetShopIds()
        {
            var result = new List<string>();
            result.Add("bc308c2e-a27c-414f-b379-9d9e609aa8d2");
            result.Add("f061df20-aa4e-4f4e-9648-12a876fa126b");
            result.Add("eb85dafb-3beb-4138-abf5-70efb23f2721");
            result.Add("0ad1cf40-238f-4f9e-805c-defb3ceb210e");
            result.Add("7e971767-7f26-4042-99da-68ab8896e176");
            result.Add("b232debf-1334-4e12-96e7-f6cdb149d140");
            result.Add("9ce9eb15-2d77-4d69-be6f-d7263ac819fe");
            result.Add("127626fd-191b-4392-8057-8a806135cfc5");
            result.Add("9181b1a0-2b99-4e50-be6c-2f8372528d21");
            result.Add("c85bce9f-f460-4b39-9484-45f44822b8c9");
            result.Add("b3389770-cd0c-41b0-8bbd-1fd2ff7f4e75");
            result.Add("8f0a8948-2ab9-40c2-b2d7-b4fd5c6ff9c8");
            result.Add("0ad1cf40-238f-4f9e-805c-defb3ceb210e");

            return result;
        }
        public async Task<U> PerformHttpRequest<U>(string endpoint) where U : new()
        {
            var result = new U();

            using (var client = new HttpClient())
            {
                var url = string.Format("{0}{1}", API, endpoint);

                try
                {
                    client.DefaultRequestHeaders.Add("Authorization", key);
                    var response = await client.GetAsync(url, HttpCompletionOption.ResponseContentRead);
                    var strData = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<U>(strData);
                }
                catch (Exception ex)
                {

                }
            }

            return result;
        }
        public async Task<U> PerformHttpRequest<T, U>(T request, string endpoint) where U : new()
        {
            var result = new U();

            using (var client = new HttpClient())
            {
                var url = string.Format("{0}{1}", API, endpoint);
                var myJson = new JObject();
                var strPropNamesAndValues =
                    request
                        .GetType()
                        .GetProperties()
                        .Where(i => i.PropertyType == typeof(string) && i.GetGetMethod() != null)
                        .Select(p => new
                        {
                            Name = p.Name,
                            Value = p.GetGetMethod().Invoke(request, null)
                        });
                foreach (var item in strPropNamesAndValues)
                {
                    myJson.Add(item.Name, (string)item.Value);
                }

                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                var interimObject = JsonConvert.DeserializeObject<ExpandoObject>(myJson.ToString());
                var strJson = JsonConvert.SerializeObject(interimObject, jsonSerializerSettings);

                try
                {
                    var content = new StringContent(strJson, Encoding.UTF8, "application/json");
                    client.DefaultRequestHeaders.Add("Authorization", key);
                    var response = await client.PostAsync(url, content);
                    var strData = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<U>(strData);
                }
                catch (Exception ex)
                {

                }
            }

            return result;
        }
        public List<ManualEntryListItem> getManualEntryList(string Mode, int ID)
        {
            //ID is AdminInfoID or CustomerProfileID depending on the MODE

            List<ManualEntryListItem> list = new List<ManualEntryListItem>();
            if (Mode != "Presets") // Line Items
            {
                DBAccess db = new DBAccess();
                DBAccessTableResult tableResult = db.ExecuteWithTable("Estimate_GetLineItemsPreview", new SqlParameter("AdminInfoID", ID));
                if (tableResult.Success)
                {
                    foreach (DataRow row in tableResult.DataTable.Rows)
                    {
                        ManualEntryListItem listitem = new ManualEntryListItem();
                        listitem.ID = InputHelper.GetInteger(row["LineID"].ToString());
                        listitem.Group = row["Step"].ToString();

                        listitem.LaborItems = row["LaborItems"].ToString().Trim();
                        if (listitem.LaborItems.StartsWith(", "))
                        {
                            listitem.LaborItems = listitem.LaborItems.Substring(2, listitem.LaborItems.Length - 2);
                        }
                        if (listitem.LaborItems.EndsWith(","))
                        {
                            listitem.LaborItems = listitem.LaborItems.Substring(0, listitem.LaborItems.Length - 1);
                        }

                        listitem.LineNumber = InputHelper.GetInteger(row["LineNumber"].ToString());
                        listitem.OP = row["ActionCode"].ToString();
                        listitem.OPDescription = row["ActionDescription"].ToString();
                        listitem.Overhaul = row["PartOfOverHaul"].ToString();
                        listitem.PartName = row["PartDescription"].ToString();
                        listitem.PartNumber = row["PartNumber"].ToString();
                        listitem.PartPrice = InputHelper.GetDouble(row["Price"].ToString());
                        listitem.PartSource = row["PartSource"].ToString();
                        listitem.Locked = row["Locked"].ToString() == "1";
                        listitem.Modified = InputHelper.GetInteger(row["Modified"].ToString());
                        list.Add(listitem);
                    }
                }
            }

            return list.OrderByDescending(o => o.LineNumber).ToList();
        }
    }
}

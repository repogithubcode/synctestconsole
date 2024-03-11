using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.IO;

using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

using Proestimator.ViewModel;
using ProEstimator.Business.ILogic;
using ProEstimator.Business.Logic;
using ProEstimator.Business.Model;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using ProEstimatorData.Models;
using ProEstimatorData.Models.SubModel;
using ProEstimatorData.DataModel.PDR;
using ProEstimatorData.DataModel.Profiles;

using Proestimator.ViewModel.CustomReports;
using Proestimator.ViewModel.PartSearch;

using Telerik.Reporting;
using Telerik.Reporting.Processing;

using CommandType = System.Data.CommandType;
using Contact = ProEstimatorData.DataModel.Contact;
using Proestimator.ViewModel.CreateReports;
using Proestimator.ViewModel.Customer;

namespace Proestimator.Controllers
{
    public class HomeController : SiteController
    {
        private IEstimateService _estimateService;

        public HomeController(IEstimateService estimateService)
        {
            _estimateService = estimateService;
        }

        #region Open Estimates Page

        public ActionResult GetEstimateSearch(
              [DataSourceRequest] DataSourceRequest request
            , string userID
            , int customerID
            , byte searchFilter
            , string searchText
            , bool showDeleted
            , bool advancedEnabled
            , string advancedEstimateID
            , string advancedEstimateNumber
            , string advancedEstimateDescription
            , string advancedWorkOrderNumber
            , string advancedClaimNumber
            , string advancedLicenseNumber
            , string advancedVin
            , string advancedCustomer
            , string advancedVehicle
            , string advancedStockNumber
            , bool showWebEstEstimates
            , string advancedPhoneNumber
            , string advancedInsuranceCompanyName
        )

        {
            List<EstimateInfoVM> estimateList = new List<EstimateInfoVM>();

            int userIDInt = InputHelper.GetInteger(userID);
            CacheActiveLoginID(userIDInt);

            if (IsUserAuthorized(userIDInt))
            {
                DBAccess db = new DBAccess();
                DBAccessTableResult tableResult = null;

                SiteUser user = SiteUser.Get(userIDInt);

                if (showWebEstEstimates)
                {
                    //GetWebEstEstimates
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter("LoginID", user.LoginID));
                    parameters.Add(new SqlParameter("FilterOption", searchFilter));
                    parameters.Add(new SqlParameter("SearchText", searchText.Trim()));

                    tableResult = db.ExecuteWithTable("GetWebEstEstimates", parameters);
                }
                else
                {
                    // Do the regular search
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter("LoginsID", user.LoginID));
                    parameters.Add(new SqlParameter("CustomerID", customerID));
                    parameters.Add(new SqlParameter("FilterOption", searchFilter));
                    parameters.Add(new SqlParameter("SearchText", customerID == 0 ? searchText.Trim() : ""));
                    parameters.Add(new SqlParameter("Deleted", showDeleted));

                    parameters.Add(new SqlParameter("AdvancedSearch", advancedEnabled));
                    parameters.Add(new SqlParameter("Advanced_EstimateID", advancedEstimateID));
                    parameters.Add(new SqlParameter("Advanced_EstimateNumber", advancedEstimateNumber.Trim()));
                    parameters.Add(new SqlParameter("Advanced_EstimateDescription", advancedEstimateDescription.Trim()));
                    parameters.Add(new SqlParameter("Advanced_WorkOrderNumber", advancedWorkOrderNumber.Trim()));
                    parameters.Add(new SqlParameter("Advanced_ClaimNumber", advancedClaimNumber.Trim()));
                    parameters.Add(new SqlParameter("Advanced_LicensePlate", advancedLicenseNumber.Trim()));
                    parameters.Add(new SqlParameter("Advanced_Vin", advancedVin.Trim()));
                    parameters.Add(new SqlParameter("Advanced_StockNumber", advancedStockNumber.Trim()));
                    parameters.Add(new SqlParameter("Advanced_PhoneNumber", advancedPhoneNumber.Trim()));
                    parameters.Add(new SqlParameter("Advanced_InsuranceCompanyName", advancedInsuranceCompanyName.Trim()));

                    tableResult = db.ExecuteWithTable("sp_GetEstimateList", parameters);

                    if (advancedEnabled)
                    {
                        SuccessBoxFeatureLog.LogFeature(user.LoginID, SuccessBoxModule.Search, "Doing an advanced estimate search", GetActiveLoginID(userIDInt));
                    }
                }
                 
                List<string> nameParts = string.IsNullOrEmpty(advancedCustomer) ? new List<string>() : advancedCustomer.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                List<string> vehicleParts = string.IsNullOrEmpty(advancedVehicle) ? new List<string>() : advancedVehicle.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                StringListToLower(nameParts);
                StringListToLower(vehicleParts);

                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    EstimateInfoVM info = new EstimateInfoVM(row);

                    if (advancedEnabled)
                    {
                        bool addInfo = true;

                        // Don't add the record if any of the entered name or vehicle parts aren't matched
                        foreach (string namePart in nameParts)
                        {
                            if (!info.Name.ToLower().Contains(namePart))
                            {
                                addInfo = false;
                                break;
                            }
                        }

                        if (addInfo)
                        {
                            foreach (string vehiclePart in vehicleParts)
                            {
                                if (!info.Vehicle.ToLower().Contains(vehiclePart))
                                {
                                    addInfo = false;
                                    break;
                                }
                            }
                        }

                        if (addInfo)
                        {
                            estimateList.Add(info);
                        }
                    }
                    else
                    {
                        estimateList.Add(info);
                    }
                }
            }
            //else
            //{
            //    ErrorLogger.LogError("Unauthorized login.", userIDInt, 0, "GetEstimateSearch Unauthorized");
            //}

            return Json(estimateList.ToDataSourceResult(request));
        }

        private void StringListToLower(List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = list[i].ToLower();
            }
        }

        public JsonResult GridDeleteEstimate(int userID, int estimateID)
        {
            CacheActiveLoginID(userID);

            string returnString = "";

            try
            {
                if (!IsUserAuthorized(userID))
                {
                    returnString = @Proestimator.Resources.ProStrings.UnauthorizedLoginID;
                }
                else
                {
                    Estimate estimate = new Estimate(estimateID);
                    SiteUser siteUser = SiteUser.Get(userID);

                    if (estimate == null || estimate.EstimateID == 0)
                    {
                        returnString = string.Format(@Proestimator.Resources.ProStrings.EstimateNotFound, estimateID);
                    }
                    else if (estimate.CreatedByLoginID != siteUser.LoginID)
                    {
                        returnString = string.Format(@Proestimator.Resources.ProStrings.EstimateNotBelongToAccountCannotDeleteRestore, estimateID, "delete");
                    }
                    else
                    {
                        estimate.IsDeleted = true;
                        SaveResult saveResult = estimate.Save(GetActiveLoginID(userID));

                        if (saveResult.Success)
                        {
                            returnString = "success";

                            EstimateDeleteHistory.LogDeletion(estimateID);
                        }
                        else
                        {
                            returnString = string.Format(@Proestimator.Resources.ProStrings.ErrorDeletingRestoreEstimate + " ", "deleting", saveResult.ErrorMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, userID, "Delete Estimate");
                returnString = string.Format(@Proestimator.Resources.ProStrings.ErrorDeletingRestoreEstimate + " ", "deleting", ex.Message);
            }

            return Json(returnString, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GridRestoreEstimate(int userID, int estimateID)
        {
            CacheActiveLoginID(userID);

            string returnString = "";

            try
            {
                if (!IsUserAuthorized(userID))
                {
                    returnString = @Proestimator.Resources.ProStrings.UnauthorizedLoginID;
                }
                else
                {
                    Estimate estimate = new Estimate(estimateID);
                    SiteUser siteUser = SiteUser.Get(userID);

                    if (estimate == null || estimate.EstimateID == 0)
                    {
                        returnString = string.Format(@Proestimator.Resources.ProStrings.EstimateNotFound, estimateID);
                    }
                    else if (estimate.CreatedByLoginID != siteUser.LoginID)
                    {
                        returnString = string.Format(@Proestimator.Resources.ProStrings.EstimateNotBelongToAccountCannotDeleteRestore, estimateID, "restore");
                    }
                    else
                    {
                        estimate.IsDeleted = false;
                        SaveResult saveResult = estimate.Save(GetActiveLoginID(userID));

                        if (saveResult.Success)
                        {
                            returnString = "success";

                            EstimateDeleteHistory.LogRestore(estimateID);
                        }
                        else
                        {
                            returnString = string.Format(@Proestimator.Resources.ProStrings.ErrorDeletingRestoreEstimate + " ", "restoring", saveResult.ErrorMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, userID, "Restore Estimate");
                returnString = string.Format(@Proestimator.Resources.ProStrings.ErrorDeletingRestoreEstimate + " ", "restoring", ex.Message);
            }

            return Json(returnString, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GridDeleteEstimates(int userID, string ids)
        {
            CacheActiveLoginID(userID);

            if (!IsUserAuthorized(userID))
            {
                return Json(@Proestimator.Resources.ProStrings.UnauthorizedLoginID, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (!string.IsNullOrEmpty(ids))
                {
                    List<string> idPieces = ids.Split(',').ToList();

                    foreach (string id in idPieces)
                    {
                        int estimateID = InputHelper.GetInteger(id);
                        Estimate estimate = new Estimate(estimateID);
                        SiteUser siteUser = SiteUser.Get(userID);

                        if (estimate != null && estimate.CreatedByLoginID == siteUser.LoginID)
                        {
                            estimate.IsDeleted = true;
                            estimate.Save(GetActiveLoginID(userID));

                            EstimateDeleteHistory.LogDeletion(estimateID);
                        }
                    }

                    return Json("success", JsonRequestBehavior.AllowGet);
                }

                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GridRestoreEstimates(int userID, string ids)
        {
            CacheActiveLoginID(userID);

            if (!IsUserAuthorized(userID))
            {
                return Json(@Proestimator.Resources.ProStrings.UnauthorizedLoginID, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (!string.IsNullOrEmpty(ids))
                {
                    List<string> idPieces = ids.Split(',').ToList();

                    foreach (string id in idPieces)
                    {
                        int estimateID = InputHelper.GetInteger(id);
                        Estimate estimate = new Estimate(estimateID);
                        SiteUser siteUser = SiteUser.Get(userID);

                        if (estimate != null && estimate.CreatedByLoginID == siteUser.LoginID)
                        {
                            estimate.IsDeleted = false;
                            estimate.Save(GetActiveLoginID(userID));

                            EstimateDeleteHistory.LogRestore(estimateID);
                        }
                    }

                    return Json("success", JsonRequestBehavior.AllowGet);
                }

                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult StartWebEstImport(int userID, int estimateID)
        {
            CacheActiveLoginID(userID);

            string result = "Error";

            DBAccess db = new DBAccess();

            DBAccessTableResult importLog = db.ExecuteWithTable("WebEstImportLog_Get", new SqlParameter("AdminInfoID", estimateID));

            // If there is no data it means the estimate hasn't been imported yet
            if (importLog.Success)
            {
                result = "In Progress";
            }
            else
            {
                db.ExecuteNonQuery("WebEstImportLog_Insert", new SqlParameter("AdminInfoID", estimateID));
                db.ExecuteNonQuery("DataMigration_Estimate", new SqlParameter("AdminInfoID", estimateID), 120);

                Estimate estimate = new Estimate(estimateID);
                if (estimate != null)
                {
                    // Copy images
                    DBAccessTableResult imagesTable = db.ExecuteWithTable("DataMigration_GetImages", new SqlParameter("AdminInfoID", estimateID));
                    if (imagesTable.Success)
                    {
                        foreach(DataRow row in imagesTable.DataTable.Rows)
                        {
                            string diskPath = row["ImageURL"].ToString();
                            if (System.IO.File.Exists(diskPath))
                            {
                                System.Drawing.Image image = System.Drawing.Image.FromFile(diskPath);
                                _estimateService.SaveImage(estimate.CreatedByLoginID, estimateID, image, System.IO.Path.GetFileNameWithoutExtension(diskPath), System.IO.Path.GetExtension(diskPath));
                            }
                        }
                    }

                    result = "Success";
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Frame Data

        [HttpGet]
        [Route("{userID}/frame-data/{estimateID?}")]
        public ActionResult FrameData(int userID, int estimateID = 0)
        {
            FrameDataVM model = new FrameDataVM();

            model.IsTrial = ActiveLogin.IsTrial;

            if (!ActiveLogin.HasFrameDataContract)
            {
                // There is no active frame data contract.  There could be one made with no payments yet
                Contract activeContract = Contract.GetActive(ActiveLogin.LoginID);
                List<ContractAddOn> addOns = ContractAddOn.GetForContract(activeContract.ID);
                ContractAddOn frameDataAddOn = addOns.FirstOrDefault(o => o.AddOnType.ID == 2);
                if (frameDataAddOn != null)
                {
                    if (!frameDataAddOn.Active)
                    {
                        model.ContractDeactivated = true;
                    }
                    else
                    {
                        return Redirect("/" + userID + "/invoice/customer-invoice");
                    }
                }
                else
                {
                    return Redirect("/" + userID + "/invoice/pick-addon/" + activeContract.ID);
                }
            }

            FillFrameDataVM(model, estimateID);
            model.LoginID = ActiveLogin.LoginID;
            model.EstimateID = estimateID;

            LoadFrameDataModels(model);
            GetFrameDataSearchResults(model);

            if (!string.IsNullOrEmpty(model.SelectedImage))
            {
                model.ImagePath = ConfigurationManager.AppSettings["BaseURL"] + "/FrameMatrixImages/" + model.SelectedImage;
            }

            ViewBag.NavID = "frame";

            ViewBag.EstimateID = estimateID;

            return View(model);
        }

        [HttpPost]
        [Route("{userID}/frame-data/{estimateID?}")]
        public ActionResult FrameData(FrameDataVM model)
        {
            FillFrameDataVM(model, model.EstimateID);

            LoadFrameDataModels(model);
            GetFrameDataSearchResults(model);

            if (!string.IsNullOrEmpty(model.SelectedImage))
            {
                model.ImagePath = ConfigurationManager.AppSettings["BaseURL"] + "/FrameMatrixImages/" + model.SelectedImage;
            }

            ViewBag.NavID = "frame";

            ViewBag.EstimateID = model.EstimateID;

            return View(model);
        }

        private void FillFrameDataVM(FrameDataVM model, int estimateID)
        {
            // Fill the years list
            DBAccess db = new DBAccess();

            List<SelectListItem> yearItems = new List<SelectListItem>();
            DBAccessTableResult yearResult = db.ExecuteWithTable("FrameMatrix_GetVehicleYears");          
            if (yearResult.Success)
            {
                yearItems.Add(new SelectListItem() { Value = "0", Text = "-----Select Year-----" });
                foreach (DataRow row in yearResult.DataTable.Rows)
                {
                    yearItems.Add(new SelectListItem() { Value = row["carYear"].ToString(), Text = row["carYear"].ToString() });
                }
            }
            model.YearList = new SelectList(yearItems, "Value", "Text");

            // ProEstimator uses the Vinn database's make and model tables.  FrameMatrix has their own tables with different IDs.  To select the
            // vehicle for the current estimate we first need to get the make and model ID for the FrameMatrix database.
            if (estimateID > 0)
            {
                if (model.MakeID == 0 && model.ModelID == 0)
                {
                    Vehicle vehicle = Vehicle.GetByEstimate(estimateID);

                    List<SqlParameter> parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter("vinnMakeID", vehicle.MakeID));
                    parameters.Add(new SqlParameter("vinnModelID", vehicle.ModelID));

                    DBAccessTableResult tableResult = db.ExecuteWithTable("GetFrameMatrixIDsFromVinnIDs", parameters);
                    if (tableResult.Success)
                    {
                        model.MakeID = InputHelper.GetInteger(tableResult.DataTable.Rows[0]["MakeID"].ToString());
                        model.ModelID = InputHelper.GetInteger(tableResult.DataTable.Rows[0]["ModelID"].ToString());

                        model.Year = vehicle.Year;
                    }
                }
                else if (model.Year == 0)
                {
                    Vehicle vehicle = Vehicle.GetByEstimate(estimateID);
                    if (vehicle != null)
                    {
                        model.Year = vehicle.Year;
                    }
                }
            }

            if (model.Year != 0 && yearItems.Count > 1)
            {
                bool exist = false;
                foreach(SelectListItem y in yearItems)
                {
                    if(y.Text == model.Year.ToString())
                    {
                        exist = true;
                    }
                }
                if (!exist)
                {
                    model.Year = 0;
                }
            }

            List <SelectListItem> makeItems = new List<SelectListItem>();
            DBAccessTableResult vehicleMakesResult = db.ExecuteWithTable("FrameMatrix_GetVehicleMakes");
            if (vehicleMakesResult.Success)
            {
                makeItems.Add(new SelectListItem() { Value = "0", Text = "-----Select Make-----" });
                foreach (DataRow row in vehicleMakesResult.DataTable.Rows)
                {
                    makeItems.Add(new SelectListItem() { Value = row["makId"].ToString(), Text = row["makDescription"].ToString() });
                }
            }
            model.MakeList = new SelectList(makeItems, "Value", "Text");
        }

        private void LoadFrameDataModels(FrameDataVM model)
        {
            List<SelectListItem> modelItems = new List<SelectListItem>();

            if (model.MakeID > 0)
            {

                DBAccess db = new DBAccess();
                DBAccessTableResult tableResult = db.ExecuteWithTable("FrameData_GetAllVehiclesModel", new SqlParameter("VehicleMakesID", model.MakeID));
                if (tableResult.Success)
                {
                    modelItems.Add(new SelectListItem() { Value = "0", Text = "-----Select Model-----" });
                    foreach (DataRow row in tableResult.DataTable.Rows)
                    {
                        modelItems.Add(new SelectListItem() { Value = row["vehModelId"].ToString(), Text = row["model"].ToString() });
                    }
                }
            }
            else
            {
                modelItems.Add(new SelectListItem() { Value = "0", Text = "-----Select Model-----" });
            }

            model.ModelList = new SelectList(modelItems, "Value", "Text");
        }

        private void GetFrameDataSearchResults(FrameDataVM model)
        {
            //get search results
            List<FrameDetailsVM> results = new List<FrameDetailsVM>();
            try
            {
                string makedesc = model.MakeList.Where(m => m.Value == model.MakeID.ToString()).First().Text;
                string modeldesc = model.ModelList.Where(m => m.Value == model.ModelID.ToString()).First().Text;

                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("CarMake", makedesc));
                parameters.Add(new SqlParameter("CarModel", modeldesc));
                parameters.Add(new SqlParameter("CarYear", model.Year));

                DBAccess db = new DBAccess();
                DBAccessTableResult tableResult = db.ExecuteWithTable("FrameMatrix_GetVehicleFrameDetails", parameters);

                if (tableResult.Success)
                {
                    foreach (DataRow row in tableResult.DataTable.Rows)
                    {
                        FrameDetailsVM details = new FrameDetailsVM();
                        details.CarID = InputHelper.GetInteger(row["CarID"].ToString());
                        details.CarMakes = row["CarMakes"].ToString();
                        details.CarModels = row["CarModels"].ToString();
                        details.Year = InputHelper.GetInteger(row["Year"].ToString());
                        details.CarFrameDetails = row["CarFrameDetails"].ToString();
                        details.dwf = row["dwf"].ToString();
                        details.jpg = row["jpg"].ToString();

                        results.Add(details);
                    }
                }
            }
            catch { }

            model.Results = results;
        }

        public JsonResult FrameDataGetModels(int userID, int makeID)
        {
            CacheActiveLoginID(userID);

            List<SelectListItem> models = new List<SelectListItem>();

            if (IsUserAuthorized(userID))
            {
                DBAccess db = new DBAccess();
                DBAccessTableResult tableResult = db.ExecuteWithTable("FrameData_GetAllVehiclesModel", new SqlParameter("VehicleMakesID", makeID));
                if (tableResult.Success)
                {
                    models.Add(new SelectListItem() { Value = "0", Text = "-----Select Model-----" });
                    foreach (DataRow row in tableResult.DataTable.Rows)
                    {
                        models.Add(new SelectListItem() { Value = row["vehModelId"].ToString(), Text = row["model"].ToString() });
                    }
                }
            }

            return Json(models, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PrintImagePdf(int userID, string imagePath)
        {
            CacheActiveLoginID(userID);

            imagePath = Path.Combine(ConfigurationManager.AppSettings["FrameJPGPath"], imagePath);

            ReportGenerator generator = new ReportGenerator();
            ReportFunctionResult result = generator.GenerateImagePdfReport(imagePath);
            result.ReportFullName = result.ReportFullName;

            PrintImagePdfReportJson printImagePdfReportJson = new PrintImagePdfReportJson();

            if (result != null)
            {
                if (result.Success)
                {
                    printImagePdfReportJson.ReportFullName = result.ReportFullName;
                }
                else
                {
                    printImagePdfReportJson.Success = false;
                    printImagePdfReportJson.ErrorMessage = result.ErrorMessage;
                }
            }
            else
            {
                printImagePdfReportJson.Success = false;
                printImagePdfReportJson.ErrorMessage = @Proestimator.Resources.ProStrings.NoResultFromReportGenerator;
            }

            return Json(printImagePdfReportJson, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("view-print-image-pdf/{filename}")]
        public ActionResult ViewAttachment(string filename = "")
        {
            string diskPath = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), "FrameImagePdf", filename);
            if (!string.IsNullOrEmpty(diskPath) && Path.GetFullPath(diskPath).StartsWith(Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), "FrameImagePdf"), StringComparison.OrdinalIgnoreCase))
            {
                if (System.IO.File.Exists(diskPath))
                {
                    var fileStream = new FileStream(diskPath, FileMode.Open, FileAccess.Read);
                    return new FileStreamResult(fileStream, "application/pdf");
                }
            }

            //filename from querystring or estimate id from session is not valid...show the error message instead of blank screen.
            return Content("Error: report not found.");
        }

        public class PrintImagePdfReportJson
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public string ReportFullName { get; set; }

            public PrintImagePdfReportJson()
            {
                Success = true;
                ErrorMessage = "";
                ReportFullName = "";
            }
        }

        #endregion

        #region Parts Search

        [HttpGet]
        [Route("{userID}/parts-look-up/{estimateID?}")]
        public ActionResult PartsLookUp(int userID, int estimateID = 0)
        {
            ViewBag.NavID = "parts";

            ViewBag.EstimateID = estimateID;

            return View(new PartsSearchVM(userID, estimateID));
        }

        public ActionResult DoPartSearchSections(
              [DataSourceRequest] DataSourceRequest request
            , int userID
            , int estimateID
            , int year
            , int make
            , int model
            , string searchText
            , string searchPartNumber
        )
        {
            CacheActiveLoginID(userID);

            List<PartSearchSection> results = new List<PartSearchSection>();

            if (IsUserAuthorized(userID))
            {
                int vehicleID = GetVehicleID(userID, estimateID, year, make, model);
                if (vehicleID > 0 && (!string.IsNullOrEmpty(searchText) || !string.IsNullOrEmpty(searchPartNumber)))
                {
                    results = PartSearchManager.GetSectionsSearchByVehicle(vehicleID, searchText.Trim(), searchPartNumber.Trim());
                }
            }

            return Json(results.ToDataSourceResult(request));
        }

        public ActionResult DoPartSearchParts(
              [DataSourceRequest] DataSourceRequest request
            , int userID
            , int estimateID
            , int year
            , int make
            , int model
            , string searchText
            , string searchPartNumber
            , int sectionKey
        )
        {
            CacheActiveLoginID(userID);

            List<ProEstimator.Business.Logic.PartSearchPart> results = new List<ProEstimator.Business.Logic.PartSearchPart>();

            if (IsUserAuthorized(userID))
            {
                int vehicleID = GetVehicleID(userID, estimateID, year, make, model);
                if (vehicleID > 0 && (!string.IsNullOrEmpty(searchText) || !string.IsNullOrEmpty(searchPartNumber)) && sectionKey > 0)
                {
                    results = PartSearchManager.GetPartsSearchByVehicleAndSection(vehicleID, searchText.Trim(), searchPartNumber.Trim(), sectionKey);
                }
            }

            return Json(results.ToDataSourceResult(request));
        }

        public ActionResult DoPartSearchDetails(
             [DataSourceRequest] DataSourceRequest request
           , int userID
           , int estimateID
           , int year
           , int make
           , int model
           , string searchText
           , string searchPartNumber
           , int sectionKey
           , string partDescription
       )
        {
            CacheActiveLoginID(userID);

            List<ProEstimator.Business.Logic.PartSearchDetail> results = new List<ProEstimator.Business.Logic.PartSearchDetail>();

            if (IsUserAuthorized(userID))
            {
                int vehicleID = GetVehicleID(userID, estimateID, year, make, model);
                if (vehicleID > 0 && !string.IsNullOrEmpty(partDescription))
                {
                    results = PartSearchManager.GetPartsSearchDetails(vehicleID, searchText.Trim(), searchPartNumber.Trim(), sectionKey, partDescription);
                }
            }

            return Json(results.ToDataSourceResult(request));
        }

        private int GetVehicleID(int userID, int estimateID, int year, int make, int model)
        {
            // Either get the Vehicle ID from the estimate or by searching by year/make/model
            int vehicleID = 0;
            Vehicle vehicle = Vehicle.GetByEstimate(estimateID);

            if (estimateID == 0 || vehicle.VehicleID == 0)
            {
                VehicleIDResult vehicleIDResult = Vehicle.GetVehicleIDFromInfo(year, make, model);
                if (vehicleIDResult != null)
                {
                    vehicleID = vehicleIDResult.VehicleID;
                }
            }
            else
            {
                Estimate estimate = new Estimate(estimateID);
                SiteUser user = SiteUser.Get(userID);

                if (estimate != null && estimate.CreatedByLoginID == user.LoginID)
                {
                    vehicleID = vehicle.VehicleID;
                }
            }

            return vehicleID;
        }


        public ActionResult DoSearchByPartNumber([DataSourceRequest] DataSourceRequest request, string searchPartNumber)
        {
            List<PartSearchResultVM> results = new List<PartSearchResultVM>();

            if (!string.IsNullOrEmpty(searchPartNumber))
            {
                searchPartNumber = searchPartNumber.Replace(" ", "").Replace("-", "");
                DBAccess db = new DBAccess();
                DBAccessTableResult tableResult = db.ExecuteWithTable("VehiclePartSearch_ByPartNumber", new SqlParameter("PartNumber", searchPartNumber));
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    results.Add(new PartSearchResultVM(row));
                }
            }

            return Json(results.ToDataSourceResult(request));
        }

        public ActionResult GetVehiclesForBarcode([DataSourceRequest] DataSourceRequest request, int userID, string barcode, string category, string subCategory, string partDesc,
                                                    string partNumber, string prtcDescription, string price)
        {
            CacheActiveLoginID(userID);

            List<SearchVehicleDetailVM> results = new List<SearchVehicleDetailVM>();

            if (IsUserAuthorized(userID))
            {
                DBAccess db = new DBAccess();

                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("Category", category));
                parameters.Add(new SqlParameter("SubCategory", subCategory));
                parameters.Add(new SqlParameter("Part_Desc", partDesc));
                parameters.Add(new SqlParameter("Part_Number", partNumber));
                parameters.Add(new SqlParameter("Prtc_Description", prtcDescription));
                parameters.Add(new SqlParameter("Price", InputHelper.GetDouble(price) * 100));

                DBAccessTableResult tableResult = db.ExecuteWithTable("VehiclePartSearch_GetVehiclesForBarcode", parameters);
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    results.Add(new SearchVehicleDetailVM(row));
                }
            }

            return Json(results.ToDataSourceResult(request));
        }

        #endregion

        #region Reports

        [HttpGet]
        [Route("{userID}/reports/create-report")]
        public ActionResult CreateReport(int userID)
        {
            ReportsVM model = new ReportsVM(ActiveLogin.LoginID, ActiveLogin.ID);

            string defaultStart = DateTime.Now.AddDays(-7).ToShortDateString();
            string defaultEnd = DateTime.Now.ToShortDateString();

            model.SalesReportRange.DateStart = defaultStart;
            model.SalesReportRange.DateEnd = defaultEnd;
            model.CustomerListReportRange.DateStart = defaultStart;
            model.CustomerListReportRange.DateEnd = defaultEnd;
            model.SupplierListReportRange.DateStart = defaultStart;
            model.SupplierListReportRange.DateEnd = defaultEnd;
            model.CloseRatioReportRange.DateStart = defaultStart;
            model.CloseRatioReportRange.DateEnd = defaultEnd;

            model.SaveReportsHistorySalesReport = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "SaveReportsHistorySalesReport", "ReportOptions", (true).ToString()).ValueString);
            model.SaveReportsHistoryCustomerList = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "SaveReportsHistoryCustomerList", "ReportOptions", (true).ToString()).ValueString);
            model.SaveReportsHistorySupplierList = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "SaveReportsHistorySupplierList", "ReportOptions", (true).ToString()).ValueString);
            model.SaveReportsHistorySavedCustomerList = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "SaveReportsHistorySavedCustomerList", "ReportOptions", (true).ToString()).ValueString);
            model.SaveReportsHistoryCloseRatioReport = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "SaveReportsHistoryCloseRatioReport", "ReportOptions", (true).ToString()).ValueString);

            string qbExportActive = ConfigurationManager.AppSettings["QBExportActive"];
            if (!string.IsNullOrEmpty(qbExportActive) && qbExportActive.ToLower() == "true")
            {
                model.QBExportActive = true;
            }

            model.ShowQBExport = ActiveLogin.HasQBExportContract;
            if (!ActiveLogin.HasQBExportContract)
            {
                // There might be created but un paid for QB contract
                Contract activeContract = Contract.GetActive(ActiveLogin.LoginID);
                if (activeContract != null)
                {
                    List<ContractAddOn> addOns = ContractAddOn.GetForContract(activeContract.ID);
                    ContractAddOn qbAddOn = addOns.FirstOrDefault(o => o.AddOnType.ID == 9);
                    if (qbAddOn != null)
                    {
                        model.QBAddOnLink = "customer-invoice";
                    }
                    else
                    {
                        model.QBAddOnLink = "pick-addon/" + activeContract.ID;
                    }
                }
            }

            ViewBag.NavID = "reports";

            ViewBag.NavID = "reports";
            ViewBag.settingsTopMenuID = 0;
            ViewBag.EstimateNavID = 0;
            ViewBag.QBExport = !ActiveLogin.IsTrial || ActiveLogin.HasQBExportContract;

            return View(model);
        }

        public ActionResult GetCustomerByDate([DataSourceRequest] DataSourceRequest request, int loginID, DateTime sDate, DateTime eDate)
        {
            List<Customer> customers = Customer.GetForDate(loginID, sDate, eDate, true);
            List<CustomerSelectVM> customerGrid = new List<CustomerSelectVM>();

            bool hasEmpty = false;
            foreach (Customer customer in customers)
            {
                if (customer.Contact.FirstName.Trim().Length == 0 && customer.Contact.LastName.Trim().Length == 0)
                {
                    hasEmpty = true;
                }
                else
                {
                    if (customerGrid.FirstOrDefault(o => o.CustomerID == customer.ID) == null)
                    {
                        customerGrid.Add(new CustomerSelectVM(customer));
                    }
                }
            }
            if (hasEmpty)
            {
                customerGrid.Insert(0, new CustomerSelectVM(0, "", ""));
            }

            return Json(customerGrid.ToDataSourceResult(request));
        }

        [HttpGet]
        [Route("{userID}/reports/custom-report")]
        public ActionResult CustomReports(int userID)
        {
            if (!ActiveLogin.HasCustomReportsContract)
            {
                Contract contract = Contract.GetActive(ActiveLogin.LoginID);
                if (contract != null)
                {
                    List<ContractAddOn> addOns = ContractAddOn.GetForContract(contract.ID);
                    ContractAddOn customAddOn = addOns.FirstOrDefault(o => o.AddOnType.ID == 13 || o.AddOnType.ID == 12);
                    if (customAddOn != null)
                    {
                        if (!customAddOn.Active)
                        {
                            ViewBag.ContractDeactivated = true;
                            return View("CreateReport", new ReportsVM(ActiveLogin.LoginID, ActiveLogin.ID));
                        }
                        else
                        {
                            return Redirect("/" + userID + "/invoice/customer-invoice");
                        }
                    }
                    else
                    {
                        return Redirect("/" + userID + "/invoice/pick-addon/" + contract.ID);
                    }
                }
            }

            CustomReportsListVM vmodel = new CustomReportsListVM();
            vmodel.LoginID = ActiveLogin.LoginID;

            ViewBag.NavID = "reports";
            ViewBag.settingsTopMenuID = 1;
            ViewBag.EstimateNavID = 1;
            ViewBag.QBExport = !ActiveLogin.IsTrial || ActiveLogin.HasQBExportContract;

            LoginInfo loginInfo = LoginInfo.GetByID(ActiveLogin.LoginID);
            ViewBag.StaffAccount = loginInfo.StaffAccount;

            return View(vmodel);
        }

        public ActionResult GetCreateReportHisotry([DataSourceRequest] DataSourceRequest request, int userID, string reportType)
        {
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

            List<CreateReportHistoryListVM> createReportListDetailVMs = new List<CreateReportHistoryListVM>();

            List<CreateReportHistory> createReportHistoryColl = CreateReportHistory.GetForLogin(activeLogin.LoginID, reportType);

            foreach (CreateReportHistory eachCreateReportHistory in createReportHistoryColl)
            {
                CreateReportHistoryListVM createReportListDetailVM = new CreateReportHistoryListVM();
                createReportListDetailVM.ID = eachCreateReportHistory.ID;
                createReportListDetailVM.LoginID = eachCreateReportHistory.LoginID;
                createReportListDetailVM.ReportType = eachCreateReportHistory.ReportType;
                createReportListDetailVM.FileName = eachCreateReportHistory.FileName;
                createReportListDetailVM.CreatedTimeStamp = eachCreateReportHistory.CreatedTimeStamp.ToString("MM/dd/yyyy hh:mm tt");

                createReportListDetailVMs.Add(createReportListDetailVM);
            }

            return Json(createReportListDetailVMs.ToDataSourceResult(request));
        }

        [HttpPost]
        [Route("{userID}/reports/custom-report")]
        public ActionResult CustomReports(CustomReportsListVM vm)
        {
            RedirectResult redirectResult = DoRedirect("");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            CustomReportTemplate customTemplate = new CustomReportTemplate();
            customTemplate.LoginID = vm.LoginID;
            customTemplate.Save(ActiveLogin.ID);

            return Redirect("/" + ActiveLogin.SiteUserID + "/settings/custom-report/" + customTemplate.ID.ToString());
        }

        [HttpGet]
        [Route("{userID}/reports/qb-report")]
        public ActionResult QBReport(int userID)
        {
            ReportsVM model = new ReportsVM(ActiveLogin.LoginID, ActiveLogin.ID);

            string defaultStart = DateTime.Now.AddDays(-7).ToShortDateString();
            string defaultEnd = DateTime.Now.ToShortDateString();

            model.SalesReportRange.DateStart = defaultStart;
            model.SalesReportRange.DateEnd = defaultEnd;
            model.CustomerListReportRange.DateStart = defaultStart;
            model.CustomerListReportRange.DateEnd = defaultEnd;
            model.SupplierListReportRange.DateStart = defaultStart;
            model.SupplierListReportRange.DateEnd = defaultEnd;
            model.CloseRatioReportRange.DateStart = defaultStart;
            model.CloseRatioReportRange.DateEnd = defaultEnd;

            string qbExportActive = ConfigurationManager.AppSettings["QBExportActive"];
            if (!string.IsNullOrEmpty(qbExportActive) && qbExportActive.ToLower() == "true")
            {
                model.QBExportActive = true;
            }

            model.ShowQBExport = ActiveLogin.HasQBExportContract;
            if (!ActiveLogin.HasQBExportContract)
            {
                // There might be created but un paid for QB contract
                Contract activeContract = Contract.GetActive(ActiveLogin.LoginID);
                if (activeContract != null)
                {
                    List<ContractAddOn> addOns = ContractAddOn.GetForContract(activeContract.ID);
                    ContractAddOn qbAddOn = addOns.FirstOrDefault(o => o.AddOnType.ID == 9 || o.AddOnType.ID == 12);
                    if (qbAddOn != null)
                    {
                        if (!qbAddOn.Active)
                        {
                            ViewBag.ContractDeactivated = true;
                            return View("CreateReport", model);
                        }
                        else
                        {
                            model.QBAddOnLink = "customer-invoice";
                        }
                    }
                    else
                    {
                        model.QBAddOnLink = "pick-addon/" + activeContract.ID;
                    }
                }
            }

            if (model.ShowQBExport)
            {
                return Redirect("/" + ActiveLogin.SiteUserID + "/reports/qb-export/");
            }
            else
            {
                return Redirect("/" + ActiveLogin.SiteUserID + "/invoice/" + model.QBAddOnLink);
            }

        }

        public JsonResult DeleteCustomReports(int userID, int loginID, int customReportTemplateID, Boolean restoreDeleteNotes)
        {
            CacheActiveLoginID(userID);

            string errorMessage = "";

            if (IsUserAuthorized(userID))
            {
                CustomReportTemplate customReportTemplate = CustomReportTemplate.Get(customReportTemplateID);

                if (customReportTemplate != null)
                {
                    if (customReportTemplate.LoginID == loginID)
                    {
                        ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                        if (restoreDeleteNotes)
                        {
                            customReportTemplate.Restore(activeLogin.ID);
                        }
                        else
                        {
                            customReportTemplate.Delete(activeLogin.ID);
                        }
                    }
                    else
                    {
                        errorMessage = string.Format(@Proestimator.Resources.ProStrings.ErrorDeletingReportInvalidReportID, customReportTemplateID);
                    }
                }
                else
                {
                    errorMessage = string.Format(@Proestimator.Resources.ProStrings.ErrorDeletingReportInvalidReportID, customReportTemplateID);
                }
            }

            return Json(errorMessage, JsonRequestBehavior.AllowGet);
        }

        private void SplitDate(string extraParam, out string dateStart, out string dateEnd)
        {
            char[] splitChar = new char[1];
            splitChar[0] = ':';
            string[] splitStringArr = extraParam.Split(splitChar);
            dateStart = string.Empty;
            dateEnd = string.Empty;

            dateStart = splitStringArr[0];
            dateEnd = splitStringArr[1];
        }

        [HttpPost]
        public JsonResult ShowReport(int userID, string reportType, string reportformat, string estimatorID, string includeClosedDeletedRO, string saveReportsHistory, string customerID, string extraParam = "")
        {
            CacheActiveLoginID(userID);

            string reportName = string.Empty;
            Boolean status = false;
            string errorMessage = string.Empty;

            string dateStart = null;
            string dateEnd = null;

            SaveReportsHistorySetting(userID, reportType, saveReportsHistory);

            if (extraParam.Contains(":"))
            {
                SplitDate(extraParam, out dateStart, out dateEnd);
            }

            if (reportformat == "pdf")
            {
                reportName = GenerateReport(userID, dateStart, dateEnd, reportType, estimatorID, includeClosedDeletedRO,saveReportsHistory, customerID);
            }
            else if (reportformat == "xlsx")
            {
                reportName = GenerateReportExcel(userID, dateStart, dateEnd, reportType, estimatorID, includeClosedDeletedRO, saveReportsHistory, customerID);
            }

            SuccessBoxFeatureLog.LogFeature(userID, SuccessBoxModule.Report, "Printing a " + reportType + " report in " + reportformat + " format", GetActiveLoginID(userID));

            if (string.IsNullOrEmpty(reportName))
            {
                status = false;
                errorMessage = @Proestimator.Resources.ProStrings.NoRecordInExcelToDisplay;
            }
            else
            {
                status = true;
            }

            string reportPath = "/" + userID.ToString() + "/view-report/" + reportName;

            var reportJson = new { Success = status, reportPath = reportPath, ErrorMessage = errorMessage, filename = reportName };
            return Json(reportJson, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Download(int userID, string file)
        {
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
            string reportsFolder = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), activeLogin.LoginID.ToString(), "Reports");
            string diskPath = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), activeLogin.LoginID.ToString(), "Reports", file);

            if (!string.IsNullOrEmpty(diskPath) && Path.GetFullPath(diskPath).StartsWith(reportsFolder, StringComparison.OrdinalIgnoreCase))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(diskPath);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, file);
            }

            //filename from querystring or estimate id from session is not valid...show the error message instead of blank screen.
            return Content(@Proestimator.Resources.ProStrings.SomethingWentWrongOpenGenerateReport);
        }

        [Route("{userID}/view-report/{pdfName}")]
        public ActionResult ViewReport(int userID, string pdfName)
        {
            string reportsFolder = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), ActiveLogin.LoginID.ToString(), "Reports");
            string diskPath = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), ActiveLogin.LoginID.ToString(), "Reports", pdfName);

            if (!string.IsNullOrEmpty(diskPath) && Path.GetFullPath(diskPath).StartsWith(reportsFolder, StringComparison.OrdinalIgnoreCase))
            {
                if(System.IO.File.Exists(diskPath))
                {
                    var fileStream = new FileStream(diskPath, FileMode.Open, FileAccess.Read);
                    var fsResult = new FileStreamResult(fileStream, "application/pdf");
                    return fsResult;
                }
            }

            //filename from querystring or estimate id from session is not valid...show the error message instead of blank screen.
            return Content(@Proestimator.Resources.ProStrings.SomethingWentWrongOpenGenerateReport);
        }

        private string GenerateReport(int userID, string startDate, string endDate, string reportName, string estimatorID, string includeClosedDeleted,string saveReportsHistory, string customerID)
        {
            string pdfName = reportName + "-" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 5) + ".pdf";

            SiteUser user = SiteUser.Get(userID);

            // Create a Report record
            if (InputHelper.GetBoolean(saveReportsHistory) == true)
            {
                ProEstimatorData.DataModel.CreateReportHistory createReportHistory = new ProEstimatorData.DataModel.CreateReportHistory();
                createReportHistory.LoginID = user.LoginID;
                createReportHistory.ReportType = reportName;
                createReportHistory.FileName = pdfName;
                SaveResult reportRecordSave = createReportHistory.Save();
            }

            try
            {
                // Set up a Report Source for the report file
                var uriReportSource = new Telerik.Reporting.UriReportSource();
                uriReportSource.Uri = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", reportName + ".trdx");

                // Pass the report parameters
                uriReportSource.Parameters.Add(new Telerik.Reporting.Parameter("LoginID", user.LoginID));

                if(!string.IsNullOrEmpty(startDate))
                {
                    uriReportSource.Parameters.Add(new Telerik.Reporting.Parameter("StartDate", InputHelper.GetDateTime(startDate)));
                }

                if (!string.IsNullOrEmpty(endDate))
                {
                    uriReportSource.Parameters.Add(new Telerik.Reporting.Parameter("EndDate", InputHelper.GetDateTime(endDate)));
                }

                uriReportSource.Parameters.Add(new Telerik.Reporting.Parameter("EstimatorID", InputHelper.GetInteger(estimatorID)));

                uriReportSource.Parameters.Add(new Telerik.Reporting.Parameter("IncludeClosedDeleted", InputHelper.GetBoolean(includeClosedDeleted)));

                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                LoginInfo loginInfo = LoginInfo.GetByID(activeLogin.LoginID);
                uriReportSource.Parameters.Add(new Telerik.Reporting.Parameter("CompanyName", HttpUtility.HtmlEncode(loginInfo.CompanyName)));

                uriReportSource.Parameters.Add(new Telerik.Reporting.Parameter("CustomerIDs", customerID));

                // Generate the PDF
                ReportProcessor reportProcessor = new ReportProcessor();
                RenderingResult result = reportProcessor.RenderReport("PDF", uriReportSource, null);

                // Save the report to the disk

                string diskPath = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), activeLogin.LoginID.ToString(), "Reports", pdfName);

                // Make sure the folder exists
                string folderPath = Path.GetDirectoryName(diskPath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                using (FileStream fs = new FileStream(diskPath, FileMode.Create))
                {
                    fs.Write(result.DocumentBytes, 0, result.DocumentBytes.Length);
                }

                return pdfName;
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, userID, "HomeController GenerateReport");
                //return new ReportFunctionResult(ex.Message.Contains("Check the InnerException") ? ex.InnerException.Message : ex.Message);
            }

            return "";
        }

        private string GenerateReportExcel(int userID, string startDate, string endDate, string reportName, string estimatorID, string includeClosedDeletedRO, string saveReportsHistory, string customerID)
        {
            string filename = string.Empty;

            try
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                // Set up a Report Source for the report file
                LoginInfo loginInfo = LoginInfo.GetByID(activeLogin.LoginID);

                string excelName = reportName + "-" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 5) + ".xlsx";
                string diskPath = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), activeLogin.LoginID.ToString(), "Reports", excelName);

                // Create a Report record
                if (InputHelper.GetBoolean(saveReportsHistory) == true)
                {
                    ProEstimatorData.DataModel.CreateReportHistory createReportHistory = new ProEstimatorData.DataModel.CreateReportHistory();
                    createReportHistory.LoginID = activeLogin.LoginID;
                    createReportHistory.ReportType = reportName;
                    createReportHistory.FileName = excelName;
                    SaveResult reportRecordSave = createReportHistory.Save();
                }

                // Generate the PDF
                CustomReportGenerator customReportGenerator = new CustomReportGenerator();

                if (reportName == "SalesReport")
                {
                    filename = customReportGenerator.GetSalesReportData(loginInfo.ID, startDate, endDate, estimatorID, diskPath, customerID);
                }
                if (reportName == "CustomerList")
                {
                    filename = customReportGenerator.GetCustomerListData(loginInfo.ID, startDate, endDate, diskPath, includeClosedDeletedRO);
                }
                if (reportName == "VendorsReport")
                {
                    filename = customReportGenerator.GetSupplierListData(loginInfo.ID, diskPath);
                }
                if (reportName == "SavedCustomerList")
                {
                    filename = customReportGenerator.GetSavedCustomerListData(loginInfo.ID, diskPath, includeClosedDeletedRO);
                }
                if (reportName == "CloseRatioReport")
                {
                    filename = customReportGenerator.GetCloseRatioReportListData(loginInfo.ID, startDate, endDate, diskPath);
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, userID, "HomeController GenerateReportExcel");
                //return new ReportFunctionResult(ex.Message.Contains("Check the InnerException") ? ex.InnerException.Message : ex.Message);
            }

            return filename;
        }

        private void SaveReportsHistorySetting(int userID, string reportType, string saveReportsHistory)
        {
            string saveTagName;
            switch (reportType)
            {
                case "SalesReport":
                case "CustomerList":
                case "SavedCustomerList":
                case "CloseRatioReport":
                    saveTagName = $"SaveReportsHistory{reportType}";
                    break;
                case "VendorsReport":
                    saveTagName = "SaveReportsHistorySupplierList";
                    break;
                default:
                    saveTagName = "";
                    break;
            }

            if (!string.IsNullOrEmpty(saveTagName))
            {
                var activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                SiteSettings.SaveSetting(activeLogin.ID, activeLogin.LoginID, saveTagName, "ReportOptions", InputHelper.GetBoolean(saveReportsHistory).ToString());
            }
        }

        [HttpGet]
        [Route("{userID}/createreport/download/{filename}")]
        public ActionResult DownloadCreateReport(int userID, string filename)
        {
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

            string reportsFolder = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), activeLogin.LoginID.ToString(), "Reports");
            string diskPath = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), activeLogin.LoginID.ToString(), "Reports", filename);

            if (!string.IsNullOrEmpty(diskPath) && Path.GetFullPath(diskPath).StartsWith(reportsFolder, StringComparison.OrdinalIgnoreCase))
            {
                if(System.IO.File.Exists(diskPath))
                {
                    FileInfo fileInfo = new FileInfo(filename);

                    var fileStream = new FileStream(diskPath, FileMode.Open, FileAccess.Read);
                    if (fileInfo.Extension.Contains("pdf"))
                    {
                        return new FileStreamResult(fileStream, "application/pdf");
                    }
                    else
                    {
                        return File(diskPath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml", filename + ".xlsx");
                    }
                }
            }

            //filename from querystring or estimate id from session is not valid...show the error message instead of blank screen.
            return Content("Error: report not found.");
        }

        #endregion

        #region Support

        [HttpGet]
        [Route("{userID}/support/{id?}")]
        public ActionResult Support(int userID, int id = -1)
        {
            SuccessBoxFeatureLog.LogFeature(ActiveLogin.LoginID, SuccessBoxModule.Search, "Support page visited", ActiveLogin.ID);

            ViewBag.NavID = "support";
            ViewBag.GotoID = id;

            return View();
        }

        #endregion

        #region Survey

        [HttpGet]
        [Route("{userID}/survey")]
        public ActionResult Survey(int userID)
        {
            SurveyVM model = new SurveyVM();
            model.LoginID = ActiveLogin.LoginID;

            return View(model);
        }

        [HttpPost]
        [Route("{userID}/survey")]
        public ActionResult Survey(SurveyVM model)
        {
            if (string.IsNullOrEmpty(model.SelectedValue))
            {
                model.ErrorMessage = @Proestimator.Resources.ProStrings.PleaseMakeSelection;
                return View(model);
            }

            Contract activeContract = Contract.GetActive(model.LoginID);

            RenewalSurvey surveyData = new RenewalSurvey();
            surveyData.ContractID = activeContract.ID;
            surveyData.LoginID = model.LoginID;
            surveyData.Comments = string.IsNullOrEmpty(model.Comments) ? "" : model.Comments;
            surveyData.Reason = model.SelectedValue;
            surveyData.TimeStamp = DateTime.Now;

            FunctionResult result = surveyData.Insert();
            if (result.Success)
            {
                return Redirect("/" + ActiveLogin.SiteUserID);
            }
            else
            {
                model.ErrorMessage = result.ErrorMessage;
                return View(model);
            }
        }

        #endregion

        #region Change Log And Messages

        public JsonResult GetMessageAndUpdates(int userID)
        {
            CacheActiveLoginID(userID);

            MessageAndUpdates result = new MessageAndUpdates();
            result.HasUnseenChanges = !SiteChangeLog.HasAccountSeenChanges(userID);
            // result.UnreadMessageCount = UserMessage.GetUnseenCount(userID);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public class MessageAndUpdates
        {
            public bool HasUnseenChanges { get; set; }
            public int UnreadMessageCount { get; set; }
        }

        [HttpGet]
        [Route("{userID}/change-log")]
        public ActionResult ChangeLog(int userID)
        {
            SiteChangeLog.LogHasSeen(userID);

            ChangeLogPageVM model = new ChangeLogPageVM();

            DateTime lastDate = DateTime.MinValue;

            List<SiteChangeLog> logItems = SiteChangeLog.GetAll();
            logItems = logItems.Where(o => o.IsActive).ToList();
            ChangeLogDateVM currentDateVM = null;

            foreach(SiteChangeLog logItem in logItems)
            {
                if (lastDate.Date != logItem.Date.Date)
                {
                    lastDate = logItem.Date.Date;

                    currentDateVM = new ChangeLogDateVM();
                    currentDateVM.Date = logItem.Date.Date;
                    model.DateGroups.Add(currentDateVM);
                }

                currentDateVM.Items.Add(new ChangeLogItemVM(logItem));
            }

            return View(model);
        }

        [HttpGet]
        [Route("{userID}/messages")]
        public ActionResult Messages(int userID)
        {
            MessagesPageVM vm = new MessagesPageVM();

            List<UserMessage> messages = UserMessage.GetForUser(userID);

            foreach(UserMessage message in messages)
            {
                vm.Messages.Add(new MessageVM(message, userID));
            }

            return View(vm);
        }

        public JsonResult MarkMessageSeen(int userID, int messageID)
        {
            CacheActiveLoginID(userID);

            UserMessage.MarkSeen(messageID, userID);

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult ReloadSiteGlobals()
        {
            SiteGlobalsManager.LoadData();
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult CleanActiveLogins()
        {
            _siteLoginManager.RefreshCache();
            return Json("", JsonRequestBehavior.AllowGet);
        }

        #endregion

        /// <summary>
        /// This public url is used to clear cached data from the admin site 
        /// </summary>
        /// <param name="one">The sales rep ID of the admin who called this.</param>
        /// <param name="two">A confirmation code (the sales rep ID + the current minute) encrypted.</param>
        /// <param name="three">A code to tell which cache to clear.</param>
        /// <returns></returns>
        public JsonResult LKJLKDFLKE(string one, string two, string three)
        {
            int adminId = InputHelper.GetInteger(one);
            string code = three;

            string decrypted = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Decode(two);
            decrypted = ProEstimatorData.Encryptor.Decrypt(decrypted);
            int decryptedNumber = InputHelper.GetInteger(decrypted);
            int remainder = decryptedNumber - adminId;

            if (remainder != DateTime.Now.Minute)
            {
                return Json("AAA", JsonRequestBehavior.AllowGet);
            }

            return Json("XXX", JsonRequestBehavior.AllowGet);
        }

        public JsonResult CSBLogLiveSupport(int userID)
        {
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
            if (activeLogin != null)
            {
                SuccessBoxFeatureLog.LogFeature(activeLogin.LoginID, SuccessBoxModule.EstimateWriting, "Live support link clicked", activeLogin.ID);
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult CSBLogAddOnsPage(int userID)
        {
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
            if (activeLogin != null)
            {
                SuccessBoxFeatureLog.LogFeature(activeLogin.LoginID, SuccessBoxModule.Search, "Add on menu button clicked", activeLogin.ID);
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult LogMiscInfo(int userID, int estimateID, string tag, string otherData)
        {
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
            if (activeLogin != null)
            {
                MiscTracking.Insert(activeLogin.LoginID, estimateID, tag ?? "", otherData ?? "");
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
    }
}
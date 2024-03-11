using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using Proestimator.Helpers;
using Proestimator.Resources;
using Proestimator.ViewModel;
using ProEstimator.Business.ILogic;
using ProEstimator.Business.Logic;
using ProEstimator.Business.Model;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.ProAdvisor;
using ProEstimatorData.Models;
using ProEstimatorData.Models.EditorTemplateModel;
using ProEstimatorData.Models.SubModel;
using ProEstimatorData.DataModel.PDR;
using ProEstimatorData.DataModel.Profiles;

using Contact = ProEstimatorData.DataModel.Contact;
using Exception = System.Exception;
using Report = ProEstimatorData.DataModel.Report;
using VehicleInfoManual = ProEstimatorData.DataModel.VehicleInfoManual;
using Vendor = ProEstimatorData.DataModel.Vendor;
using Ghostscript.NET.Rasterizer;
using ProEstimator.DataRepositories.Vendors;
using ProEstimator.Business.Panels;
using ProEstimator.Business.Panels.LinkRules;
using ProEstimatorData.DataModel.LinkRules;
using ProEstimator.DataRepositories.Parts;
using ProEstimator.Business.PDR;
using ProEstimator.Business.PDR.Model;
using ProEstimator.Business.ProAdvisor;

namespace Proestimator.Controllers
{
    public class EstimateController : SiteController
    {
        CarfaxService carfaxService = new CarfaxService();

        private IEstimateService _estimateService;
        private IVendorRepository _vendorRepository;
        private IPanelService _panelService;
        private ILinkRuleService _linkRuleService;
        private IPartsService _partsService;
        private IPDRAdditionalOperationsService _pdrAdditionalOperationsService;
        private IProAdvisorService _proAdvisorService;
        private IProAdvisorProfileService _proAdvisorProfileService;
        private ICreditCardPaymentService _creditCardPaymentService;

        public EstimateController(
            IEstimateService estimateService
            , IVendorRepository vendorRepository
            , IPanelService panelService
            , ILinkRuleService linkRuleService
            , IPartsService partsService
            , IPDRAdditionalOperationsService pdrAdditionalOperationsService
            , IProAdvisorService proAdvisorService
            , IProAdvisorProfileService proAdvisorProfileService
            , ICreditCardPaymentService creditCardPaymentService
        )
        {
            _estimateService = estimateService ?? throw new ArgumentNullException(nameof(estimateService));
            _vendorRepository = vendorRepository ?? throw new ArgumentNullException(nameof(vendorRepository));
            _panelService = panelService ?? throw new ArgumentNullException(nameof(panelService));
            _linkRuleService = linkRuleService ?? throw new ArgumentNullException(nameof(linkRuleService));
            _partsService = partsService ?? throw new ArgumentNullException(nameof(partsService));
            _pdrAdditionalOperationsService = pdrAdditionalOperationsService ?? throw new ArgumentNullException(nameof(pdrAdditionalOperationsService));
            _proAdvisorService = proAdvisorService ?? throw new ArgumentNullException(nameof(proAdvisorService));
            _proAdvisorProfileService = proAdvisorProfileService ?? throw new ArgumentNullException(nameof(proAdvisorProfileService));
            _creditCardPaymentService = creditCardPaymentService ?? throw new ArgumentNullException(nameof(creditCardPaymentService));
        }

        // Test Git (DevOps) 2
        [HttpGet]
        [Route("{userID}/new-estimate")]
        public async Task<ActionResult> NewEstimate(int userID)
        {
            Estimate newEstimate = _estimateService.CreateNewEstimate(ActiveLogin);

            return Redirect("/" + userID + "/estimate/" + newEstimate.EstimateID + "/customer");
        }

        [HttpGet]
        [Route("{userID}/new-estimate/{customerID}")]
        public ActionResult NewEstimate(int userID, int customerID)
        {
            // Make sure the customer belongs to the login
            Customer customer = ProEstimatorData.DataModel.Customer.Get(customerID);
            if (customer == null || customer.LoginID != ActiveLogin.LoginID)
            {
                return View(@Proestimator.Resources.ProStrings.InvalidCustomer);
            }

            Estimate newEstimate = _estimateService.CreateNewEstimate(ActiveLogin);

            newEstimate.CustomerID = customerID;
            newEstimate.Save(ActiveLogin.ID);

            return Redirect("/" + userID + "/estimate/" + newEstimate.EstimateID + "/customer");
        }

        #region Customer Page

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/customer")]
        public ActionResult Customer(int userID, int estimateID)
        {
            Estimate.RefreshProcessedLines(estimateID);

            // Make sure the estimate belongs to the login
            Estimate estimate = new Estimate(estimateID);
            if (estimate.CreatedByLoginID != ActiveLogin.LoginID)
            {
                return Redirect("/" + userID + "?estimate=" + estimateID);
            }

            // Update the last view
            estimate.LastView = DateTime.Now;
            estimate.Save(ActiveLogin.ID);
            estimate = new Estimate(estimateID);

            // If there's no profile, redirect to pick one
            if (estimate.CustomerProfileID == 0)
            {
                LoginInfo loginInfo = LoginInfo.GetByID(ActiveLogin.LoginID);

                if (loginInfo.UseDefaultRateProfile == false)
                {
                    return Redirect("/" + userID + "/estimate/" + estimateID + "/select-rate-profile");
                }
                else
                {
                    List<RateProfile> profiles = RateProfile.GetAllForLogin(ActiveLogin.LoginID);
                    if (profiles != null && (profiles.Where(each => each.IsDefault == true).Count() == 1))
                    {
                        RateProfile profile = profiles.Where(each => each.IsDefault == true).FirstOrDefault();
                        ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                        estimate.CustomerProfileID = RateProfileManager.CopyProfile(activeLogin.LoginID, profile.ID, estimateID);

                        // Set the Credit Card fee and flag based the values on the rate profile
                        estimate.CreditCardFeePercentage = profile.CreditCardFeePercentage;
                        estimate.ApplyCreditCardFee = profile.ApplyCreditCardFee;
                        estimate.TaxedCreditCardFee = profile.TaxedCreditCardFee;

                        estimate.Save(ActiveLogin.ID);
                        estimate = new Estimate(estimateID);
                    }
                    else
                    {
                        return Redirect("/" + userID + "/estimate/" + estimateID + "/select-rate-profile");
                    }
                }
            }

            // If there is not a selected Estimator, then redirect the user to select one
            if (estimate.EstimatorID == 0)
            {
                List<Estimator> estimators = Estimator.GetByLogin(ActiveLogin.LoginID);

                if (estimators.Count > 0)
                {
                    return Redirect("/" + userID + "/estimate/" + estimateID + "/select-estimator");
                }
            }

            // If Pro Advisor is on, make sure the estimate has a rate profile
            if (ActiveLogin.HasProAdvisorContract && !estimate.EstimateIsImported)
            {
                if (estimate.AddOnProfileID == 0 && estimateID > InputHelper.GetInteger(ConfigurationManager.AppSettings["FirstAdminInfoIDForAddOns"]))
                {
                    // The estimate has no ProAdvisor proile but needs one.
                    if (_proAdvisorProfileService.UseDefaultProfile(ActiveLogin.LoginID))
                    {
                        ProEstimatorData.DataModel.ProAdvisor.ProAdvisorPresetProfile defaultProfile = _proAdvisorProfileService.GetDefaultProfile(ActiveLogin.LoginID);
                        if (defaultProfile != null)
                        {
                            estimate.AddOnProfileID = defaultProfile.ID;
                            estimate.Save(ActiveLogin.ID);
                            estimate = new Estimate(estimateID);
                        }
                        else
                        {
                            return Redirect("/" + userID + "/estimate/" + estimateID + "/select-add-on-profile");
                        }
                    }
                    else
                    {
                        return Redirect("/" + userID + "/estimate/" + estimateID + "/select-add-on-profile");
                    }
                }
            }

            // Everything checks out, load the view model
            CustomerVM model = new CustomerVM();

            model.EstimateCount = Estimate.GetCountForCustomer(ActiveLogin.LoginID, estimate.CustomerID);

            // Fill the list of existing customers
            List<ProEstimatorData.DataModel.Customer> customers = ProEstimatorData.DataModel.Customer.GetForLogin(ActiveLogin.LoginID).Where(o => o.IsDeleted == false).ToList();
            if (customers.Count > 0)
            {
                model.ExistingCustomers.Add(new ExistingCustomerVM() { CustomerID = 0, CustomerName = "--Select Existing Customer--" });

                foreach (ProEstimatorData.DataModel.Customer customer in customers.OrderBy(o => o.Contact.FirstName).ToList())
                {
                    model.ExistingCustomers.Add(new ExistingCustomerVM() { CustomerID = customer.ID, CustomerName = customer.Contact.FirstName + " " + customer.Contact.LastName });
                }
            }

            model.UserID = userID;
            model.LoginID = ActiveLogin.LoginID;
            model.EstimateID = estimateID;

            // Fill the list of existing customers
            if (estimate.CustomerID > 0)
            {
                ProEstimatorData.DataModel.Customer customer = ProEstimatorData.DataModel.Customer.Get(estimate.CustomerID);

                model.Contact.LoadFromContact(customer.Contact);
                model.Address.LoadFromAddress(customer.Address);

            }
            model.EstimateIsLocked = estimate.IsLocked();

            ViewBag.States = new SelectList(GetStates(), "Value", "Text");

            ViewBag.NavID = "estimate";
            ViewBag.EstimateNavID = "customer";

            ViewBag.EstimateID = estimateID;



            return View(model);
        }

        private IEnumerable<DropDownTreeItemModel> GetData()
        {
            List<DropDownTreeItemModel> data = new List<DropDownTreeItemModel>
            {
                new DropDownTreeItemModel
                {
                    Text = "SPECIAL CAUTIONS",
                    HasChildren = true,
                    Items = new List<DropDownTreeItemModel>
                    {
                        new DropDownTreeItemModel()
                        {
                            Text = "AUTHORIZED REPAIR FACILITY",
                            Value = "257"
                        },
                        new DropDownTreeItemModel
                        {
                            Text = "STEERING GEAR",
                            Value = "258"
                        },
                        new DropDownTreeItemModel
                        {
                            Text = "AIR BAGS",
                            Value = "259"
                        },
                        new DropDownTreeItemModel
                        {
                            Text = "SUSPENSION",
                            Value = "260"
                        }
                    }
                },
                new DropDownTreeItemModel
                {
                    Text = "PAINT CODE LOCATION",
                    HasChildren = false
                },
                new DropDownTreeItemModel
                {
                    Text = "CLEAR COAT IDENTIFICATION",
                    HasChildren = false
                },
                new DropDownTreeItemModel
                {
                    Text = "INFORMATION LABELS",
                    HasChildren = false
                },
                new DropDownTreeItemModel
                {
                    Text = "FRONT BUMPER",
                    Items = new List<DropDownTreeItemModel>
                    {
                        new DropDownTreeItemModel()
                        {
                            Text = "330i  W/M SPORT PKG",
                            Value = "1281"
                        },
                        new DropDownTreeItemModel
                        {
                            Text = "330i  W/O M SPORT PKG",
                            Value = "1282"
                        },
                        new DropDownTreeItemModel
                        {
                            Text = "M340i",
                            Value = "1283"
                        }
                    }
                }
            };

            return data;
        }

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/new-customer")]
        public ActionResult NewCustomer(int userID, int estimateID)
        {
            // Make sure the estimate belongs to the login
            Estimate estimate = new Estimate(estimateID);
            if (estimate.CreatedByLoginID != ActiveLogin.LoginID)
            {
                return Redirect("/" + userID);
            }

            Customer newCustomer = new Customer();
            newCustomer.LoginID = ActiveLogin.LoginID;
            newCustomer.Save(GetActiveLoginID());

            estimate.CustomerID = newCustomer.ID;
            estimate.Save(ActiveLogin.ID);

            return Redirect("/" + userID + "/estimate/" + estimateID + "/customer");
        }

        [HttpPost]
        [Route("{userID}/estimate/{estimateID}/customer")]
        public ActionResult Customer(CustomerVM model)
        {
            Estimate estimate = new Estimate(model.EstimateID);

            if (model.SelectedExistingCustomer > 0)
            {
                estimate.CustomerID = model.SelectedExistingCustomer;
                estimate.Save(ActiveLogin.ID);
                return Redirect("/" + model.UserID + "/estimate/" + model.EstimateID + "/customer");
            }

            // The customer record might not exist yet
            ProEstimatorData.DataModel.Customer customer;

            if (estimate.CustomerID == 0)
            {
                customer = new Customer();
                customer.LoginID = estimate.CreatedByLoginID;
            }
            else
            {
                customer = ProEstimatorData.DataModel.Customer.Get(estimate.CustomerID);
            }

            model.Contact.CopyToContact(customer.Contact);
            model.Address.CopyToAddress(customer.Address);

            customer.Save(GetActiveLoginID());

            if (estimate.CustomerID == 0)
            {
                estimate.CustomerID = customer.ID;
                estimate.Save(ActiveLogin.ID);
            }

            return DoRedirect("Customer");
        }

        #endregion

        #region Vehicle Page

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/vehicle")]
        public ActionResult Vehicle(int userID, int estimateID)
        {
            // Make sure the estimate belongs to the login
            Estimate estimate = new Estimate(estimateID);
            if (estimate.CreatedByLoginID != ActiveLogin.LoginID)
            {
                return Redirect("/" + userID);
            }

            Estimate.RefreshProcessedLines(estimateID);

            // Make sure the estimate is attached to a customer
            if (estimate.CustomerID == 0)
            {
                return Redirect("/" + userID + "/estimate/" + estimate.EstimateID + "/customer-selection");
            }

            Vehicle vehicle = ProEstimatorData.DataModel.Vehicle.GetByEstimate(estimateID);

            VehicleVM vehicleVM = new VehicleVM();
            vehicleVM.LoadFromModel(vehicle);
            vehicleVM.LoginID = ActiveLogin.LoginID;
            vehicleVM.EstimateID = estimateID;
            vehicleVM.EstimateIsLocked = estimate.IsLocked();
            LoginInfo loginInfo = LoginInfo.GetByID(ActiveLogin.LoginID);
            CarfaxInfo carfaxInfo = CarfaxInfo.GetByVin(vehicleVM.Vin);
            if (loginInfo.CarfaxExcludeDate == null)
            {
                vehicleVM.Carfax = true;
            }
            ViewBag.Valid = true;
            if (carfaxInfo != null)
            {
                vehicleVM.CarfaxInfo = true;
                using (var str = new StringReader(carfaxInfo.VinInfo))
                {
                    var xmlSerializer = new XmlSerializer(typeof(Carfaxresponse));
                    Carfaxresponse response = (Carfaxresponse)xmlSerializer.Deserialize(str);
                    if (!valid(response))
                    {
                        ViewBag.Valid = false;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(vehicleVM.Vin))
            {
                ViewBag.Valid = false;
            }
                
            vehicleVM.YearDDL = new SelectList(VehicleSearchManager.GetYearSimpleListItemList(), "Value", "Text");
            vehicleVM.POIDDL = new SelectList(GetPOI(estimateID), "Value", "Text");
            vehicleVM.StateDDL = new SelectList(GetStates(), "Value", "Text");

            if (vehicle != null)
            {
                vehicleVM.MakeDDL = new SelectList(VehicleSearchManager.GetVehicleMakes(vehicle.Year), "Value", "Text");
                vehicleVM.ModelDDL = new SelectList(VehicleSearchManager.GetVehicleModels(vehicle.Year, vehicle.MakeID), "Value", "Text");
                vehicleVM.SubModelDDL = new SelectList(LookupService.VehicleSubModels(vehicle.Year, vehicle.MakeID, vehicle.ModelID), "Value", "Text");
                vehicleVM.BodyTypeDDL = new SelectList(LookupService.VehicleBodyTypes(vehicle.Year, vehicle.MakeID, vehicle.ModelID, vehicle.TrimID), "Value", "Text");
                vehicleVM.DriveTypeDDL = new SelectList(LookupService.VehicleDriveTypes(vehicle.VehicleID, estimateID), "Value", "Text");
                vehicleVM.EngineDDL = new SelectList(LookupService.VehicleEngineTypes(vehicle.BodyID, vehicle.VehicleID, estimateID), "Value", "Text");
                vehicleVM.TransmissionDDL = new SelectList(LookupService.VehicleTransmissionTypes(vehicle.BodyID, vehicle.VehicleID, estimateID), "Value", "Text");
                vehicleVM.PaintCodeDDL = new SelectList(LookupService.VehiclePaintCodes(vehicle.Year, vehicle.MakeID, vehicle.ModelID, vehicle.BodyID), "Value", "Text");
                vehicleVM.PaintCodeDDL2 = vehicleVM.PaintCodeDDL;
                vehicleVM.Accessories = vehicle.GetAccessoryList();
                vehicleVM.ProductionMonthDDL = new SelectList(LookupService.GetMonthSelectList(), "Value", "Text");

                VehicleInfoManual vehicleInfoManual = VehicleInfoManual.GetByEstimate(estimateID);
                if (vehicleInfoManual != null)
                {
                    vehicleVM.LoadFromModel(vehicleInfoManual);
                }
            }
            vehicleVM.UserID = userID;

            List<ManualEntryListItem> manualEntryListItemColl = ManualEntryHelper.getManualEntryList("Graphical", vehicleVM.EstimateID);
            vehicleVM.EstimateLineItemsExist = manualEntryListItemColl.Count > 0;
            vehicleVM.AllowEstimateVehicleModsWithLineItems = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "AllowEstimateVehicleModsWithLineItems", "ProgramPreferences", "0").ValueString);

            ViewBag.NavID = "estimate";
            ViewBag.EstimateNavID = "vehicle";

            ViewBag.EstimateID = estimateID;

            return View(vehicleVM);
        }

        [HttpPost]
        [Route("{userID}/estimate/{estimateID}/vehicle")]
        public ActionResult Vehicle(VehicleVM vehicleVM)
        {
            // Make sure the estimate belongs to the login
            Estimate estimate = new Estimate(vehicleVM.EstimateID);
            if (estimate.CreatedByLoginID != vehicleVM.LoginID)
            {
                return Redirect("/" + vehicleVM.LoginID);
            }

            if (!estimate.IsLocked())
            {

                Vehicle vehicle = ProEstimatorData.DataModel.Vehicle.GetByEstimate(vehicleVM.EstimateID);
                vehicleVM.CopyToModel(vehicle);

                var vehicleIDInfo = ProEstimatorData.DataModel.Vehicle.GetVehicleIDFromInfo(InputHelper.GetInteger(vehicleVM.Year), vehicleVM.Make, vehicleVM.Model, vehicleVM.Trim, vehicleVM.Body);
                if (vehicleIDInfo != null)
                {
                    vehicle.VehicleID = vehicleIDInfo.VehicleID;
                    vehicle.ServiceBarcode = vehicleIDInfo.ServiceBarcode;
                }
                else
                {
                    vehicle.VehicleID = 0;
                    vehicle.ServiceBarcode = "";
                }

                SaveResult result = vehicle.Save(ActiveLogin.ID);
                if (!result.Success)
                {
                    ViewBag.NavID = "estimate";
                    ViewBag.EstimateNavID = "vehicle";

                    ViewBag.EstimateID = vehicleVM.EstimateID;

                    vehicleVM.ErrorMessage = result.ErrorMessage;
                    return View(vehicleVM);
                }
                else
                {
                    vehicle.SaveAccessoryList(vehicleVM.Accessories);
                }

                VehicleInfoManual vehicleInfoManual = VehicleInfoManual.GetByEstimate(vehicleVM.EstimateID);

                if (vehicleVM.ManualSelection)
                {
                    if (vehicleInfoManual == null)
                    {
                        vehicleInfoManual = new VehicleInfoManual();
                    }

                    vehicleVM.CopyToModel(vehicleInfoManual);
                    vehicleInfoManual.UseManualSelection = true;
                    vehicleInfoManual.VehicleInfoID = vehicle.VehicleInfoID;
                    vehicleInfoManual.Save(ActiveLogin.ID, ActiveLogin.LoginID);
                }
                else
                {
                    if (vehicleInfoManual != null)
                    {
                        vehicleVM.CopyToModel(vehicleInfoManual);
                        vehicleInfoManual.UseManualSelection = false;
                        vehicleInfoManual.Save(ActiveLogin.ID, ActiveLogin.LoginID);
                    }
                }

                // If this is a GM vehicle, make sure we have the most recent data
                GMDataUpdater updater = new GMDataUpdater(vehicleVM.LoginID, vehicleVM.EstimateID);
                updater.UpdateForVehicle(vehicle);

                if (vehicleVM.UpdatingExistingLineItems)
                {
                    SaveEstimateLineItemListOnPaintTypeChange(vehicleVM);
                }
            }
            else
            {
                vehicleVM.ErrorMessage = ProStrings.Message_CannotSaveLockedEstimate;
            }

            RedirectResult redirectResult = DoRedirect("");
            if (redirectResult == null)
            {
                return RedirectToAction("vehicle");
            }
            else
            {
                return redirectResult;
            }
        }

        public JsonResult NoFrameData(int year, string make, string model)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("CarMake", make));
                parameters.Add(new SqlParameter("CarModel", model));
                parameters.Add(new SqlParameter("CarYear", year));

                DBAccess db = new DBAccess();
                DBAccessTableResult tableResult = db.ExecuteWithTable("FrameMatrix_GetVehicleFrameDetails", parameters);
                return Json(tableResult.ErrorMessage == "No data." && tableResult.DataTable.Rows.Count == 0, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SaveVehicleJSON(string JSON)
        {
            JavaScriptSerializer objJavascript = new JavaScriptSerializer();

            VehicleVM vehicleVM = objJavascript.Deserialize<VehicleVM>(JSON);
            //proest.sp_InsertUpdateVehicleInfo(CurrentSession.EstimateID, Convert.ToInt32(Session["VehicleID"]), vehicleVM.MileageIn, vehicleVM.MileageOut, vehicleVM.License, vehicleVM.LicenseState, vehicleVM.VehicleNotes, vehicleVM.ExteriorColor, vehicleVM.InteriorColor, vehicleVM.Trim.ToString(), vehicleVM.Vin, vehicleVM.Vin, vehicleVM.ProductionDate, vehicleVM.DriveType
            //    , vehicleVM.EstimatedValue, vehicleVM.Year, vehicleVM.Make, vehicleVM.Model, vehicleVM.Trim, vehicleVM.Engine, vehicleVM.Trans, vehicleVM.PaintCode, vehicleVM.POI, vehicleVM.Body, vehicleVM.ServiceBarcode, vehicleVM.PaintType);

            return null;
        }

        public void SaveEstimateLineItemListOnPaintTypeChange(VehicleVM vehicleVM, string meMode = "")
        {
            CacheActiveLoginID(vehicleVM.UserID);

            string mode = "Graphical";
            List<ManualEntryListItem> manualEntryListItemColl = ManualEntryHelper.getManualEntryList(mode, vehicleVM.EstimateID);

            manualEntryListItemColl = manualEntryListItemColl.OrderBy(eachItem => eachItem.LineNumber).ToList<ManualEntryListItem>();

            foreach (ManualEntryListItem eachManualEntryListItem in manualEntryListItemColl)
            {
                if (eachManualEntryListItem.Modified > -1)
                {
                    continue;
                }
                if (eachManualEntryListItem.EstimationDataSuppVer != eachManualEntryListItem.ProcessedLineSuppVer)
                {
                    continue;
                }
                ManualEntryDetail detail = Helpers.ManualEntryHelper.GetMELine(_vendorRepository, vehicleVM.EstimateID, eachManualEntryListItem.ID, meMode);
                int paintType = detail.PaintType;
                if (paintType == vehicleVM.PaintType)
                {
                    continue;
                }
                detail.PaintType = vehicleVM.PaintType;
                detail.Action = "Update";
                detail.TheID = detail.LineID;
                FillGainValues(vehicleVM.EstimateID, detail.PaintGainValues);
                calculatePaintValues(detail, paintType);
                ManualEntryHelper.SaveManualEntry(vehicleVM.EstimateID, detail, mode, detail.Action, detail.TheID);
            }
        }

        public void calculatePaintValues(ManualEntryDetail detail, int oriPaintType)
        {
            // The Panel Type drop down should only be visible for a Replace or Refinish operation
            var panelType = "";

            var operationType = detail.OperationType;
            if (operationType == "Replace" || operationType == "Refinish")
            {
                panelType = detail.PanelType;
            }

            var paintType = detail.PaintType;

            double blendGain = detail.PaintGainValues.PaintGain_Blend;
            if (paintType == 18 || paintType == 29)
            {
                blendGain = detail.PaintGainValues.PaintGain_ThreeTwoBlend;
            }

            // Calculate blend before the adjacent deduction
            decimal paintHours = detail.PaintHours;
            if (detail.LockBlend == false)
            {
                detail.BlendHours = paintHours * Convert.ToDecimal(blendGain);
            }

            // If Adjacent or NonAdjacent panel is selected, remove the deduction from the base paint hours
            if (panelType == "Adjacent")
            {
                paintHours = paintHours - Convert.ToDecimal(detail.PaintGainValues.PaintDeduction_Adjacent);
            }
            else if (panelType == "NonAdjacent")
            {
                paintHours = paintHours - Convert.ToDecimal(detail.PaintGainValues.PaintDeduction_NonAdjacent);
            }

            if (detail.LockEdging == false)
            {
                if (paintHours >= Convert.ToDecimal(detail.PaintGainValues.PaintGain_EdgingMin))
                {
                    detail.EdgingHours = Convert.ToDecimal(0.5);
                }
            }

            if (detail.LockUnderside == false)
            {
                detail.UndersideHours = paintHours * Convert.ToDecimal(detail.PaintGainValues.PaintGain_Underside);
            }

            //    if (isNotLocked(prefix + "CheckBoxLockUnderside"))
            //    {
            //$("#" + prefix + "TextboxUndersideTime").val(roundNumber(parseFloat(paintHours * paintGain_Underside)));
            //        ShouldIncludeUnderside();
            //    }

            if (detail.LockClearcoat == false)
            {
                var singleStageMult = 1;
                if (paintType == 16)
                {
                    singleStageMult = 0;
                }

                if (panelType == "First" || panelType == "")
                {
                    detail.ClearcoatHours = paintHours * Convert.ToDecimal(detail.PaintGainValues.PaintGain_ClearCoatMajor) * singleStageMult;
                }
                else if (panelType == "Adjacent" || panelType == "NonAdjacent")
                {
                    detail.ClearcoatHours = paintHours * Convert.ToDecimal(detail.PaintGainValues.PaintGain_ClearCoatNonAdj) * singleStageMult;
                }

                if (paintHours != 0 && paintType != 16)
                {
                    detail.IncludeClearcoat = true;
                }
                else
                {
                    detail.IncludeClearcoat = false;
                }

                if (paintType == 0)
                {
                    detail.IncludeClearcoat = false;
                }
            }

            // Calculations for 3 stage or 2 tone
            if (paintType == 18 || paintType == 29)
            {
                detail.ClearcoatHours = 0;
                detail.IncludeClearcoat = false;

                if (detail.LockAllowance == false)
                {
                    double toneStageMult = 0;

                    // 2 tone
                    if (paintType == 29)
                    {
                        toneStageMult = detail.PaintGainValues.PaintGain_2ToneMajor;

                        if (panelType == "NonAdjacent")
                        {
                            toneStageMult = detail.PaintGainValues.PaintGain_2ToneNonAdjacent;
                        }
                    }
                    // or 3 stage
                    else if (paintType == 18)
                    {
                        toneStageMult = detail.PaintGainValues.PaintGain_3StageMajor;

                        if (panelType == "NonAdjacent")
                        {
                            toneStageMult = Convert.ToInt32(detail.PaintGainValues.PaintGain_3StageNonAdjacent);
                        }
                    }

                    detail.AllowanceHours = paintHours * Convert.ToDecimal(toneStageMult);

                    if (detail.AllowanceHours > 0)
                    {
                        detail.IncludeAllowance = true;
                    }
                }

                if (oriPaintType == 16 || oriPaintType == 19)
                {
                    detail.LockAllowance = detail.LockClearcoat;
                }
                detail.LockClearcoat = false;
            }

            if (paintType == 19)
            {
                if (oriPaintType == 18 || oriPaintType == 29)
                {
                    detail.LockClearcoat = detail.LockAllowance;
                }
            }
        }

        //public JsonResult DecodeVINImage()
        //{
        //    DecodeVINImageResults results = new DecodeVINImageResults();
        //    results.Success = false;
        //    results.VIN = "";
        //    results.ErrorMessage = "";

        //    if (System.Web.HttpContext.Current.Request.Files.AllKeys.Any())
        //    {
        //        var file = System.Web.HttpContext.Current.Request.Files["Images"];

        //        try
        //        {
        //            if (file != null)
        //            {
        //                Bitmap image = Image.FromStream(file.InputStream, true, true) as Bitmap;

        //                ProEstimatorData.ErrorLogger.LogError("DecodeVINImage got image", SiteSession.Current.LoginID, SiteSession.Current.EstimateID, "Vin Decode");

        //                StringBuilder builder = new StringBuilder();

        //                GetVinResults getVinResult = ImageToVin.GetVinFromImage(image, true, builder);
        //                ProEstimatorData.ErrorLogger.LogError("Image to vin log: " + builder.ToString(), SiteSession.Current.LoginID, SiteSession.Current.EstimateID, "Vin Decode");
        //                if (!string.IsNullOrEmpty(getVinResult.VIN))
        //                {
        //                    results.VIN = getVinResult.VIN;
        //                    results.Success = true;
        //                }
        //                else
        //                {
        //                    results.ErrorMessage = "No VIN found.";
        //                }

        //                ProEstimatorData.ErrorLogger.LogError("Result: " + results.Success.ToString() + " VIN: " + results.VIN, SiteSession.Current.LoginID, SiteSession.Current.EstimateID, "Vin Decode");

        //                // For debugging, save the results in the database and save the original image and the tagged image
        //                DBAccess db = new DBAccess();
        //                List<SqlParameter> parameters = new List<SqlParameter>();
        //                parameters.Add(new SqlParameter("LoginID", CurrentSession.LoginID));
        //                parameters.Add(new SqlParameter("VinResult", results.VIN));
        //                parameters.Add(new SqlParameter("Success", results.Success));

        //                DBAccessIntResult insertResult = db.ExecuteWithIntReturn("InsertImageToVinDecodeResult", parameters);
        //                if (insertResult.Success)
        //                {
        //                    string folderPath = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), "VinDecodeImages");
        //                    if (!Directory.Exists(folderPath))
        //                    {
        //                        Directory.CreateDirectory(folderPath);
        //                    }

        //                    image.Save(Path.Combine(folderPath, insertResult.Value.ToString() + "_Original.jpg"));
        //                    getVinResult.HilightBitmap.Save(Path.Combine(folderPath, insertResult.Value.ToString() + "_Tagged.jpg"));

        //                    ProEstimatorData.ErrorLogger.LogError("DecodeVinImage logged data.", SiteSession.Current.LoginID, SiteSession.Current.EstimateID, "Vin Decode");
        //                }
        //            }
        //            else
        //            {
        //                results.ErrorMessage = "No file attached.";
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            results.ErrorMessage = "Error uploading image: " + ex.Message;
        //            ProEstimatorData.ErrorLogger.LogError(ex, SiteSession.Current.LoginID, SiteSession.Current.EstimateID, "Vin Decode DecodeVINImage");
        //        }
        //    }

        //    ProEstimatorData.ErrorLogger.LogError("Returning.  Success: " + results.Success.ToString() + "  VIN: " + results.VIN, SiteSession.Current.LoginID, SiteSession.Current.EstimateID, "Vin Decode DecodeVINImage");

        //    return Json(results, JsonRequestBehavior.AllowGet);
        //}

        public class DecodeVINImageResults
        {
            public bool Success { get; set; }
            public string VIN { get; set; }
            public string ErrorMessage { get; set; }
        }

        private List<SelectListItem> GetPOI(int estimateID)
        {
            List<SelectListItem> returnList = new List<SelectListItem>();

            returnList.Add(new SelectListItem() { Text = "--" + ProStrings.SelectPOI + "--", Value = "0" });

            List<POIOption> poiOptions = POIOption.GetAll();
            foreach (POIOption option in poiOptions)
            {
                returnList.Add(new SelectListItem() { Text = option.Name, Value = option.ID.ToString() });
            }

            return returnList;
        }

        public JsonResult GetPaintCode(int userID, int estimateID, int year, int make, int model, int body, bool useData)
        {
            CacheActiveLoginID(userID);

            if (useData)
            {
                return Json(new SelectList(LookupService.VehiclePaintCodes(year, make, model, body), "Value", "Text"), JsonRequestBehavior.AllowGet);
            }
            else
            {
                Vehicle vehicle = ProEstimatorData.DataModel.Vehicle.GetByEstimate(estimateID);
                return Json(new SelectList(LookupService.VehiclePaintCodes(vehicle.Year, vehicle.MakeID, vehicle.ModelID, vehicle.BodyID), "Value", "Text"), JsonRequestBehavior.AllowGet);
            }
        
        }

        #endregion

        #region Add Parts Manual

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/add-parts-manual")]
        public ActionResult AddParts(int userID, int estimateID, string PDR = "")
        {
            // Make sure the estimate belongs to the login
            Estimate estimate = new Estimate(estimateID);
            if (estimate.CreatedByLoginID != ActiveLogin.LoginID)
            {
                return Redirect("/" + userID);
            }

            Estimate.RefreshProcessedLines(estimateID);

            // Make sure the estimate is attached to a customer
            if (estimate.CustomerID == 0)
            {
                return Redirect("/" + userID + "/estimate/" + estimate.EstimateID + "/customer-selection");
            }

            ViewBag.NavID = "estimate";
            ViewBag.EstimateNavID = "graphical";
            ViewBag.EstimateID = estimateID;

            ManualEntry me = new ManualEntry();
            me.List = new ManualEntryList();
            me.List.ItemID = estimateID;
            me.List.MEMode = "Manual";
            me.List.EstimateIsLocked = estimate.IsLocked();
            me.List.HasSupplement = estimate.LockLevel > 0;
            me.Details = new ManualEntryDetail(ManualEntryDetail.UseAluminum(estimateID));
            me.Details.LineID = 0;
            me.Details.PresetList = ManualEntryHelper.GetPresetList(estimateID);
            me.Details.Quantity = 1;
            me.Details.MEMode = "Manual";
            me.Details.EstimateIsLocked = estimate.IsLocked();
            me.Details.EstimateID = estimateID;

            List<Vendor> vendors = _vendorRepository.GetAllForType(ActiveLogin.LoginID, VendorType.OEM);
            List<SimpleListItem> vendorList = new List<SimpleListItem>();
            vendorList.Add(new SimpleListItem("---" + ProStrings.SelectVendor + "---", "0"));
            foreach (Vendor vendor in vendors)
            {
                SimpleListItem item = new SimpleListItem(vendor.CompanyName, vendor.ID.ToString());
                vendorList.Add(item);
            }

            me.Details.VendorList = vendorList.OrderBy(o => o.Text).ToList();

            AddPartsVM model = new AddPartsVM(ActiveLogin.LoginID, estimateID, this.IsMobile);
            model.ManualEntry = me;
            model.LoginID = ActiveLogin.LoginID;
            model.EstimateID = estimateID;
            model.EstimateIsLocked = estimate.IsLocked();
            model.Supplement = estimate.LockLevel;

            model.ShowGraphicalButton = true;
            VehicleInfoManual infoManual = VehicleInfoManual.GetByEstimate(estimateID);
            if (infoManual != null)
            {
                model.ShowGraphicalButton = !infoManual.UseManualSelection;
            }

            model.HasPDRContract = ActiveLogin.HasPDRContract;

            FillGainValues(estimateID, model.ManualEntry.Details.PaintGainValues);

            Vehicle vehicle = ProEstimatorData.DataModel.Vehicle.GetByEstimate(estimateID);

            model.ManualEntry.Details.SectionList = _estimateService.  GetSections(estimateID, vehicle.VehicleID);

            SetupPDRVM(ActiveLogin.LoginID, estimateID, model, PDR.ToLower() == "true");

            ViewBag.EstimateID = estimateID;
            ViewBag.VehicleID = vehicle.VehicleID;

            return View(model);
        }

        private void FillGainValues(int estimateID, PaintGainValues paintGainValues)
        {
            Estimate estimate = new Estimate(estimateID);
            PaintSettings paintRates = PaintSettings.GetForProfile(estimate.CustomerProfileID);

            paintGainValues.PaintGain_Blend = paintRates.Blend != 0 ? (paintRates.Blend / 100) : 0;
            paintGainValues.PaintGain_ThreeTwoBlend = paintRates.ThreeTwoBlend != 0 ? (paintRates.ThreeTwoBlend / 100) : paintGainValues.PaintGain_Blend;
            paintGainValues.PaintGain_Underside = paintRates.Underside != 0 ? (paintRates.Underside / 100) : 0;
            paintGainValues.PaintGain_EdgingMin = paintRates.Edging != 0 ? paintRates.Edging : 0;
            paintGainValues.PaintGain_2ToneMajor = paintRates.MajorTwoTone != 0 ? (paintRates.MajorTwoTone / 100) : 0;
            paintGainValues.PaintGain_2ToneNonAdjacent = paintRates.OverLapTwoTone != 0 ? (paintRates.OverLapTwoTone / 100) : 0;
            paintGainValues.PaintGain_3StageMajor = paintRates.MajorThreeStage != 0 ? (paintRates.MajorThreeStage / 100) : 0;
            paintGainValues.PaintGain_3StageNonAdjacent = paintRates.OverlapThreeStage != 0 ? (paintRates.OverlapThreeStage / 100) : 0;
            paintGainValues.PaintGain_ClearCoatMajor = paintRates.MajorClearCoat != 0 ? (paintRates.MajorClearCoat / 100) : 0;
            paintGainValues.PaintGain_ClearCoatNonAdj = paintRates.OverlapClearCoat != 0 ? (paintRates.OverlapClearCoat / 100) : 0;

            paintGainValues.PaintDeduction_AllowAdjacentDeductions = paintRates.AllowDeductions;
            paintGainValues.PaintDeduction_Adjacent = paintRates.AdjacentDeduction != 0 ? paintRates.AdjacentDeduction : 0;
            paintGainValues.PaintDeduction_NonAdjacent = paintRates.NonAdjacentDeduction != 0 ? paintRates.NonAdjacentDeduction : 0;
        }

        [HttpPost]
        public ActionResult AddParts(AddPartsVM model)
        {
            if (model.EstimateID == 0)
            {
                return RedirectToAction("OpenEstimate", "Home");
            }

            Estimate estimate = new Estimate(model.EstimateID);
            if (!estimate.IsLocked())
            {
                ManualEntryDetail detail = model.ManualEntry.Details;

                string Action = "Add";

                int TheID = model.EstimateID;
                if (detail.LineID > 0)
                {
                    Action = "Update";
                    TheID = detail.LineID;
                }
                ManualEntryHelper.SaveManualEntry(model.EstimateID, detail, "LineItem", Action, TheID);
            }

            return DoRedirect("AddParts");
        }

        public JsonResult AddUpdateLineItem(
             int userID
            , int estimateID
            , string meMode
            , decimal AllowanceHours
            , string Barcode
            , string BettermentType
            , decimal BettermentValue
            , decimal BlendHours
            , decimal ClearcoatHours
            , decimal EdgingHours
            , string ExternalNotes
            , bool IncludeAllowance
            , bool IncludeBlend
            , bool IncludeClearcoat
            , bool IncludeEdging
            , bool IncludeUnderside
            , string InternalNotes
            , decimal LaborHours
            , int LaborType
            , bool LaborIncluded
            , int LineID
            , bool LockPanelType
            , string OperationDescription
            , string OperationType
            , string OtherCharge
            , int OtherChargeType
            , bool Overhaul
            , decimal PaintHours
            , int PaintType
            , string PanelType
            , string PartDescription
            , string PartNumber
            , string SourcePartNumber
            , string PartPrice
            , string PartSource
            , int Quantity
            , string SelectedPart
            , string SelectedSection
            , string SelectedVendor
            , int StepID
            , bool Sublet
            , decimal UndersideHours
            , string Action1
            , int TheID
            , bool LockAllowance
            , bool LockClearcoat
            , bool LockBlend
            , bool LockEdging
            , bool LockUnderside
            , bool BettermentParts
            , bool BettermentMaterials
            , bool BettermentLabor
            , bool BettermentPaint
            , bool UpdateBaseRecord
            , bool isPreset
            , bool IsPartsQuantity
            , bool IsLaborQuantity
            , bool IsPaintQuantity
            , bool IsOtherCharges
        )
        {
            bool success = false;
            string message = "";

            CacheActiveLoginID(userID);

            try
            {
                ManualEntryDetail detail = new ManualEntryDetail(ManualEntryDetail.UseAluminum(estimateID));
                detail.Action = (Action1 == "" ? "Add" : Action1);
                detail.AllowanceHours = AllowanceHours;
                detail.Barcode = Barcode;
                detail.BettermentType = BettermentType;
                detail.BettermentValue = BettermentValue.ToString();
                detail.BlendHours = BlendHours;
                detail.ClearcoatHours = ClearcoatHours;
                detail.EdgingHours = EdgingHours;
                detail.ExternalNotes = ExternalNotes;
                detail.IncludeAllowance = IncludeAllowance;
                detail.IncludeBlend = IncludeBlend;
                detail.IncludeClearcoat = IncludeClearcoat;
                detail.IncludeEdging = IncludeEdging;
                detail.IncludeUnderside = IncludeUnderside;
                detail.InternalNotes = InternalNotes;
                detail.LaborHours = LaborHours;
                detail.LaborType = LaborType;
                detail.LaborIncluded = LaborIncluded;
                detail.LineID = LineID;
                detail.LockPanelType = LockPanelType;
                detail.OperationDescription = OperationDescription;
                detail.OperationType = OperationType;
                detail.OtherCharge = OtherCharge.ToString();
                detail.OtherChargeType = OtherChargeType;
                detail.Overhaul = Overhaul;
                detail.PaintHours = PaintHours;
                detail.PaintType = PaintType;
                detail.PanelType = PanelType;
                detail.PartDescription = PartDescription;
                detail.PartNumber = PartNumber;
                detail.SourcePartNumber = SourcePartNumber;
                detail.PartPrice = PartPrice;
                detail.PartSource = PartSource;
                detail.Quantity = Quantity;
                detail.SelectedPart = SelectedPart;
                detail.SelectedSection = SelectedSection;
                detail.SelectedVendor = InputHelper.GetInteger(SelectedVendor);
                detail.StepID = StepID;
                detail.Sublet = Sublet;
                detail.TheID = TheID;
                detail.UndersideHours = UndersideHours;
                detail.LockAllowance = LockAllowance;
                detail.LockClearcoat = LockClearcoat;
                detail.LockBlend = LockBlend;
                detail.LockEdging = LockEdging;
                detail.LockUnderside = LockUnderside;
                detail.BettermentParts = BettermentParts;
                detail.BettermentLabor = BettermentLabor;
                detail.BettermentPaint = BettermentPaint;
                detail.BettermentMaterials = BettermentMaterials;

                detail.IsPartsQuantity = IsPartsQuantity;
                detail.IsLaborQuantity = IsLaborQuantity;
                detail.IsPaintQuantity = IsPaintQuantity;
                detail.IsOtherCharges = IsOtherCharges;

                detail.UpdateBaseRecord = UpdateBaseRecord;

                if (detail.LineID > 0)
                {
                    detail.TheID = detail.LineID;
                }
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                ManualEntryHelper.SaveManualEntry(estimateID, detail, meMode, detail.Action, detail.TheID, activeLogin.ID, activeLogin.LoginID);

                if (isPreset)
                {
                    Estimate estimate = new Estimate(estimateID);
                    if (estimate != null)
                    {
                        SuccessBoxFeatureLog.LogFeature(estimate.CreatedByLoginID, SuccessBoxModule.EstimateWriting, "Preset added", GetActiveLoginID(userID));
                    }
                }

                success = true;
                message = ProStrings.Message_EntryDataSaved;
            }
            catch (Exception ex)
            {
                success = false;
                message = ProStrings.Message_ErrorUpdatingLineItem + ": " + ex.Message;
            }

            return Json(new { Success = success, Message = message }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetManualEntryList([DataSourceRequest] DataSourceRequest request, int userID, int estimateID, string meMode)
        {
            CacheActiveLoginID(userID);

            string mode = (meMode == "Preset" ? "Presets" : "LineItems");
            List<ManualEntryListItem> list = ManualEntryHelper.getManualEntryList(mode, estimateID);

            list = list.OrderBy(eachItem => eachItem.LineNumber).ToList<ManualEntryListItem>();

            return Json(list.ToDataSourceResult(request));
        }

        public JsonResult GetVendorList(int userID, int loginID, int type)
        {
            CacheActiveLoginID(userID);

            VendorType vendorType = (VendorType)type;
            List<Vendor> vendors = _vendorRepository.GetAllForType(loginID, vendorType);

            List<SimpleListItem> vendorlist = new List<SimpleListItem>();
            vendorlist.Add(new SimpleListItem("---" + ProStrings.SelectVendor + "---", "0"));

            foreach (Vendor vendor in vendors)
            {
                SimpleListItem item = new SimpleListItem(vendor.CompanyName, vendor.ID.ToString());
                vendorlist.Add(item);
            }

            return Json(vendorlist, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Add parts graphically

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/add-parts")]
        public ActionResult AddPartsGraphically(int userID, int estimateID, string PDR = "")
        {
            // Make sure the estimate belongs to the login
            Estimate estimate = new Estimate(estimateID);
            if (estimate.CreatedByLoginID != ActiveLogin.LoginID)
            {
                return Redirect("/" + userID);
            }

            Estimate.RefreshProcessedLines(estimateID);

            // Make sure the estimate is attached to a customer
            if (estimate.CustomerID == 0)
            {
                return Redirect("/" + userID + "/estimate/" + estimate.EstimateID + "/customer-selection");
            }

            // Make sure the estimate has a rate profile
            if (estimate.CustomerProfileID == 0)
            {
                Redirect("/" + userID + "/estimate/" + estimate.EstimateID + "/select-rate-profile");
            }

            Vehicle vehicle = ProEstimatorData.DataModel.Vehicle.GetByEstimate(estimateID);

            if (vehicle.VehicleID == 0)
            {
                return Redirect("/" + userID + "/estimate/" + estimateID + "/add-parts-manual?PDR=" + PDR);
            }

            ViewBag.VehicleID = vehicle.VehicleID;

            bool showAllYears = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "ShowAllYears", "AddPartsOptions", (false).ToString()).ValueString);
            List<SimpleListItem> sections = _estimateService.GetSections(estimateID, vehicle.VehicleID, showAllYears);

            List<DropDownTreeItemModel> sectionsDropDownTreeItemModel = _estimateService.GetSectionsListByVehicleforDropDownTreeItem(estimateID, vehicle.VehicleID, showAllYears);
            List<TreeViewItemModel> sectionsTreeViewItemModel = _estimateService.GetSectionsListByVehicleforTreeViewItem(estimateID, vehicle.VehicleID, showAllYears);

            ViewBag.NavID = "estimate";
            ViewBag.EstimateNavID = "graphical";
            ViewBag.EstimateID = estimateID;

            ManualEntry me = new ManualEntry();
            me.List = new ManualEntryList();
            me.List.ItemID = estimateID;
            me.List.MEMode = "Graphical";
            me.List.EstimateIsLocked = estimate.IsLocked();
            me.List.HasSupplement = estimate.LockLevel > 0;
            me.Details = new ManualEntryDetail(ManualEntryDetail.UseAluminum(estimateID));
            me.Details.LineID = 0;
            me.Details.MEMode = "Graphical";
            me.Details.PresetList = ManualEntryHelper.GetPresetList(estimateID);
            me.Details.SectionList = sections;
            me.Details.SectionListDropDownTreeItemModel = sectionsDropDownTreeItemModel;
            me.Details.SectionListTreeViewItemModel = sectionsTreeViewItemModel;
            me.Details.VehicleProuctionDate = vehicle.ProductionDate;
            me.Details.Action = "Add";

            if (ActiveLogin.IsImpersonated)
            {
                me.Details.OverlapDetails = true;
            }
            else
            {
                LoginInfo loginInfo = LoginInfo.GetByID(ActiveLogin.LoginID);
                me.Details.OverlapDetails = loginInfo.OverlapAdmin;
            }

            AddPartsGraphicallyVM model = new AddPartsGraphicallyVM(ActiveLogin.LoginID, estimateID, this.IsMobile);
            model.ForceSmallTop = ActiveLogin.LanguagePreference == "es";
            model.ManualEntry = me;
            model.Sections = sections;
            model.SectionsDropDownTreeItemModel = sectionsDropDownTreeItemModel;
            model.SectionsTreeViewItemModel = sectionsTreeViewItemModel;
            model.LoginID = ActiveLogin.LoginID;
            model.EstimateID = estimateID;
            model.HasPDRContract = ActiveLogin.HasPDRContract;
            model.HasPartsNowContract = ActiveLogin.HasPartsNow;
            model.EstimateIsLocked = estimate.IsLocked();
            model.EstimateIsImported = estimate.EstimateIsImported;
            model.Supplement = estimate.LockLevel;
            model.VehicleDataFound = vehicle.HasData;

            model.ProAdvisorTrial = ActiveLogin.ProAdvisorIsTrial;

            // If the vehicle has no Body Type selected, hide the sections
            if (vehicle.BodyID == 0)
            {
                model.ShowSectionsDropdown = false;
            }

            // Get the paint type set on the Vehicles page.
            if (vehicle != null)
            {
                model.PaintType = Convert.ToInt32(vehicle.DefaultPaintType);
            }

            if (model.PaintType == 0)
            {
                model.PaintType = 19;
            }

            // If the vehicle has "CarryUp" set, turn on the Show All Years checkbox.
            VehicleIDResult vehicleIDData = ProEstimatorData.DataModel.Vehicle.GetVehicleIDFromInfo(vehicle.Year, vehicle.MakeID, vehicle.ModelID, vehicle.TrimID);
            if (vehicleIDData != null && vehicleIDData.CarryUp)
            {
                model.ShowAllYears = true;
                model.ShowCarryUpMessage = true;
            }

            FillGainValues(estimateID, model.ManualEntry.Details.PaintGainValues);
            SetupPDRVM(ActiveLogin.LoginID, estimateID, model, PDR.ToLower() == "true");

            model.PartsSectionTreeFontSize = InputHelper.GetInteger(SiteSettings.Get(ActiveLogin.LoginID, "PartsSectionTreeFontSize", "AddParts", ((int)FontSize.PartsSectionTreeDefaultFontSize).ToString()).ValueString);
            //ViewBag.dropdowntreeData = GetData();

            return View(model);
        }

        private void SetupPDRVM(int loginID, int estimateID, Proestimator.ViewModel.PDR.PDRMatrixVMBase pdrVM, bool pdrAutoOpen = false)
        {
            // Fill the PDR model
            if (ActiveLogin.HasPDRContract)
            {
                // If the estimate has no PDR rate profile, select one now
                PDR_EstimateData estimateData = PDR_EstimateData.GetForEstimate(estimateID);
                if (estimateData != null && estimateData.RateProfileID > 0)
                {
                    pdrVM.PDREnabled = true;
                    pdrVM.PDRAutoOpen = pdrAutoOpen;

                    // Get the data need for the page
                    List<PDR_EstimateDataPanel> panels = PDR_EstimateDataPanel.GetForEstimate(estimateID);
                    List<PDR_EstimateDataPanelSupplementChange> supplementChanges = PDR_EstimateDataPanelSupplementChange.GetForEstimate(estimateID);
                    List<PDR_Rate> allRates = PDR_Rate.GetByProfile(estimateData.RateProfileID);
                    List<PDR_Multiplier> allMultipliers = PDR_Multiplier.GetByProfile(estimateData.RateProfileID);

                    if (pdrVM.PDRMatrix != null)
                    {
                        pdrVM.PDRMatrix.FillPanelsData(loginID, estimateID, panels, supplementChanges, allRates, allMultipliers);
                        pdrVM.PDRMatrix.ProfileID = estimateData.RateProfileID;
                    }
                    if (pdrVM.PDRMatrixMobile != null)
                    {
                        pdrVM.PDRMatrixMobile.FillPanelsData(loginID, estimateID, panels, supplementChanges, allRates, allMultipliers);
                        pdrVM.PDRMatrixMobile.ProfileID = estimateData.RateProfileID;
                    }
                }
            }
        }

        public ActionResult GetSectionsDropDown(int userID, int estimateID, bool ignoreYearFilter, string sectionsListTreeControlTypeId)
        {
            CacheActiveLoginID(userID);

            ProEstimatorData.DataModel.Vehicle vehicle = ProEstimatorData.DataModel.Vehicle.GetByEstimate(estimateID);

            if(ViewBag.Browser == "safari")
            {
                if(ViewBag.UseLegacyPartsSectionDropdown == false)
                {
                    List<SimpleListItem> sections = _estimateService.GetSections(estimateID, vehicle.VehicleID, ignoreYearFilter);
                    return Json(sections, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (sectionsListTreeControlTypeId == "sectionslistdropdowntree")
                    {
                        List<DropDownTreeItemModel> dropDownTreeItemModelSections = _estimateService.GetSectionsListByVehicleforDropDownTreeItem(estimateID, vehicle.VehicleID, ignoreYearFilter);
                        return Json(dropDownTreeItemModelSections, JsonRequestBehavior.AllowGet);
                    }
                    else if (sectionsListTreeControlTypeId == "sectionslisttreeview")
                    {
                        List<TreeViewItemModel> treeViewItemModelSections = _estimateService.GetSectionsListByVehicleforTreeViewItem(estimateID, vehicle.VehicleID, ignoreYearFilter);
                        return Json(treeViewItemModelSections, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            else
            {
                if (sectionsListTreeControlTypeId == "sectionslistdropdowntree")
                {
                    List<DropDownTreeItemModel> dropDownTreeItemModelSections = _estimateService.GetSectionsListByVehicleforDropDownTreeItem(estimateID, vehicle.VehicleID, ignoreYearFilter);
                    return Json(dropDownTreeItemModelSections, JsonRequestBehavior.AllowGet);
                }
                else if (sectionsListTreeControlTypeId == "sectionslisttreeview")
                {
                    List<TreeViewItemModel> treeViewItemModelSections = _estimateService.GetSectionsListByVehicleforTreeViewItem(estimateID, vehicle.VehicleID, ignoreYearFilter);
                    return Json(treeViewItemModelSections, JsonRequestBehavior.AllowGet);
                }
            }

            return null;
        }

        private string GetPaintCodeLocation(int estimateID)
        {
            DBAccess db = new DBAccess();
            DBAccessStringResult result = db.ExecuteWithStringReturn("GetPaintCodeLocation", new SqlParameter("AdminInfoID", estimateID));

            if (result.Success)
            {
                return result.Value;
            }

            return "";
        }

        public JsonResult GetLineOverlapDetails(int userID, int lineItemID)
        {
            CacheActiveLoginID(userID);

            List<OverlapDetailVM> vms = new List<OverlapDetailVM>();

            List<OverlapDetails> overlapDetails = OverlapDetails.GetForLineItem(lineItemID);
            foreach (OverlapDetails detail in overlapDetails)
            {
                OverlapDetailVM vm = new OverlapDetailVM();
                vm.OverlapID = detail.OverlapID;
                vm.IsIncluded = detail.Amount == -99.9;
                vm.Amount = detail.Amount.ToString();
                vm.Minimum = detail.Minimum.ToString();
                vm.UserAccepted = detail.UserAccepted;
                vm.Description = detail.Description;

                vms.Add(vm);
            }

            return Json(vms, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveOverlaps(SaveOverlapsData data)
        {
            StringBuilder errors = new StringBuilder();

            CacheActiveLoginID(data.UserID);

            if (data.Details != null && data.Details.Count > 0)
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(data.UserID, GetComputerKey());

                List<OverlapDetails> allDetails = OverlapDetails.GetForLineItem(data.LineItemID);

                foreach (OverlapDetailVM detailVM in data.Details)
                {
                    OverlapDetails detail = allDetails.FirstOrDefault(o => o.OverlapID == detailVM.OverlapID);
                    if (detail != null)
                    {
                        if (detailVM.IsIncluded)
                        {
                            detail.Amount = -99.9;
                        }
                        else
                        {
                            detail.Amount = InputHelper.GetDouble(detailVM.Amount);
                        }

                        if (detail.Amount > 0)
                        {
                            detail.Amount *= -1;
                        }

                        detail.Minimum = InputHelper.GetDouble(detailVM.Minimum);

                        detail.UserAccepted = detailVM.UserAccepted;

                        SaveResult saveResult = detail.Save(activeLogin.ID, activeLogin.LoginID);
                        if (!saveResult.Success)
                        {
                            errors.AppendLine(saveResult.ErrorMessage);
                        }
                    }
                }

                Estimate.RefreshProcessedLines(data.EstimateID);
            }

            return Json(errors.ToString(), JsonRequestBehavior.AllowGet);
        }

        public class SaveOverlapsData
        {
            public int UserID { get; set; }
            public int EstimateID { get; set; }
            public int LineItemID { get; set; }
            public List<OverlapDetailVM> Details { get; set; }
        }

        public class OverlapDetailVM
        {
            public int OverlapID { get; set; }
            public bool IsIncluded { get; set; }
            public string Amount { get; set; }
            public string Minimum { get; set; }
            public bool UserAccepted { get; set; }
            public string Description { get; set; }

            public OverlapDetailVM()
            {

            }
        }

        public JsonResult CSBLogShowPartsAndLabor(int userID)
        {
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
            if (activeLogin != null)
            {
                SuccessBoxFeatureLog.LogFeature(activeLogin.LoginID, SuccessBoxModule.EstimateWriting, "Parts & labor popup shown", activeLogin.ID);
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult CSBLogAddFromPartsAndLabor(int userID)
        {
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
            if (activeLogin != null)
            {
                SuccessBoxFeatureLog.LogFeature(activeLogin.LoginID, SuccessBoxModule.EstimateWriting, "Adding a part from the parts & labor popup", activeLogin.ID);
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region VehicleTabDropdowns Jsons

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetImagesAndHotspots(int UserID, int VehicleID, int GroupNumber)
        {
            CacheActiveLoginID(UserID);

            List<ImageAndGraphic> imageAndGraphics = GetImageList(VehicleID, GroupNumber);
            List<ImageAndHotspots> imageAndHotspotsList = new List<ImageAndHotspots>();

            foreach (ImageAndGraphic imageData in imageAndGraphics)
            {
                ImageAndHotspots imageAndHotspots = new ImageAndHotspots();

                //Convert tif (from disk) to gif in memory stream -> convert memory stream to base64 string to pass back to html via json
                if (imageData.GraphicFileName != "No Image")
                {
                    FileStream ImageStream = new FileStream(ConfigurationManager.AppSettings.Get("VehicleImagesLocation").ToString() + imageData.GraphicFileName + ".tif", FileMode.Open, FileAccess.Read, FileShare.Read);
                    Image DrawingImg = Image.FromStream(ImageStream);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        DrawingImg.Save(ms, ImageFormat.Gif);
                        imageAndHotspots.Base64Img = Convert.ToBase64String(ms.ToArray());

                        imageAndHotspots.ImageWidth = DrawingImg.Size.Width;
                        imageAndHotspots.ImageHeight = DrawingImg.Size.Height;
                    }
                }

                if (imageData.NImage != -1)
                {
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter("VehicleID", VehicleID));
                    parameters.Add(new SqlParameter("GroupNumber", GroupNumber));
                    parameters.Add(new SqlParameter("nImage", imageData.NImage));

                    DBAccess db = new DBAccess(DatabaseName.Mitchell);
                    DBAccessTableResult hotspotTable = db.ExecuteWithTable("GetSectionHotspotsByVehicleID", parameters);

                    foreach (DataRow row in hotspotTable.DataTable.Rows)
                    {
                        imageAndHotspots.hotspot.Add(new ImageHotspot(row));
                    }

                    imageAndHotspots.image = imageData;
                }
                else
                {
                    imageAndHotspots.image = imageData;
                    imageAndHotspots.hotspot = null;
                }

                imageAndHotspotsList.Add(imageAndHotspots);
            }

            JsonResult result = Json(imageAndHotspotsList, JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = Int32.MaxValue;
            return result;
        }

        public JsonResult GetProfileLaborRates2(int userID, int estimateID)
        {
            CacheActiveLoginID(userID);

            List<LaborRateVM> laborRates = new List<LaborRateVM>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("CustomerProfile_GetRates", new SqlParameter("EstimateID", estimateID));
            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    laborRates.Add(new LaborRateVM(row));
                }
            }

            return Json(laborRates, JsonRequestBehavior.AllowGet);
        }

        public class LaborRateVM
        {
            public int id { get; set; }
            public string RateName { get; set; }
            public double Rate { get; set; }

            public LaborRateVM()
            {

            }

            public LaborRateVM(DataRow row)
            {
                id = InputHelper.GetInteger(row["id"].ToString());
                RateName = row["RateName"].ToString();
                Rate = InputHelper.GetDouble(row["Rate"].ToString());
            }
        }

        public JsonResult GetProfileInfo(int userID, int estimateID)
        {
            CacheActiveLoginID(userID);

            Estimate estimate = new Estimate(estimateID);
            PaintSettings paintRates = PaintSettings.GetForProfile(estimate.CustomerProfileID);

            paintRates.ClearRowAsLoaded();

            return Json(paintRates, JsonRequestBehavior.AllowGet);
        }        

        /// <summary>
        /// Returns a list of Sub Sections and their parts for an estimate and panel combo.
        /// </summary>
        public JsonResult GetSectionsForPDRPanel(int userID, int estimateID, int panelID)
        {
            CacheActiveLoginID(userID);

            SectionsForPDRPanelFunctionResult results = _pdrAdditionalOperationsService.GetAdditionalOperations(estimateID, panelID);
            return Json(results, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPartInfo(int userID, int estimateID, int sectionKey, bool allYears)
        {
            CacheActiveLoginID(userID);

            if (sectionKey == -2)
            {
                return Json("NOTE:" + _estimateService.GetACCapacitiesNote(estimateID), JsonRequestBehavior.AllowGet);
            }
            else if (sectionKey == 512)
            {
                return Json("NOTE:" + GetPaintCodeLocation(estimateID), JsonRequestBehavior.AllowGet);
            }
            else
            {
                GetPartInfoData getPartInfoData = new GetPartInfoData();
                List<SectionPartInfo> partInfoList = _partsService.GetPartsForSection(estimateID, sectionKey, allYears);
                getPartInfoData.Parts = partInfoList;

                try
                {
                    // If a part is already in the estimate, we want to use the selected labor data instead of the default data.  This way when the part is 
                    // replaced the paint labor data is prefilled with the existing data
                    DBAccess db = new DBAccess();
                    DBAccessTableResult result = db.ExecuteWithTable("GetLaborForEstimate", new SqlParameter("AdminInfoID", estimateID));

                    if (result.Success)
                    {
                        foreach (SectionPartInfo partInfo in partInfoList)
                        {
                            foreach (DataRow row in result.DataTable.Rows)
                            {
                                string barcode = row["Barcode"].ToString();
                                if (barcode == partInfo.Barcode)
                                {
                                    int laborType = InputHelper.GetInteger(row["LaborType"].ToString());
                                    float laborTime = (float)InputHelper.GetDecimal(row["LaborTime"].ToString());

                                    if (laborType == 1)
                                    {
                                        partInfo.LaborTime = laborTime;
                                    }
                                    else if (laborType == 19)
                                    {
                                        partInfo.LaborPaintTime = laborTime;
                                    }
                                    else if (laborType == 26)
                                    {
                                        partInfo.LaborTimeBlend = laborTime;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    getPartInfoData.ErrorMessage = ProStrings.Message_ErrorGettingPartInfo + ": " + ex.Message;
                }

                return Json(getPartInfoData, JsonRequestBehavior.AllowGet);
            }
        }

        public class GetPartInfoData
        {
            public List<SectionPartInfo> Parts { get; set; }
            public string ErrorMessage { get; set; }

            public GetPartInfoData()
            {
                Parts = new List<SectionPartInfo>();
                ErrorMessage = "";
            }
        }

        public JsonResult DisableOverlapPrompting(int userID, int estimateID)
        {
            CacheActiveLoginID(userID);

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("Overlaps_DisablePrompting", new SqlParameter("AdminInfoID", estimateID));
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult RejectAllOverlaps(int userID, int estimateID)
        {
            CacheActiveLoginID(userID);

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("Overlaps_RejectProposals", new SqlParameter("AdminInfoID", estimateID));
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult AcceptCheckedOverlaps(int userID, int estimateID, string AcceptList)
        {
            CacheActiveLoginID(userID);

            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("AdminInfoID", estimateID));
                parameters.Add(new SqlParameter("IDsChecked", AcceptList));

                DBAccess db = new DBAccess();
                FunctionResult functionResult = db.ExecuteNonQuery("Overlaps_AcceptChecked", parameters);

                Estimate.RefreshProcessedLines(estimateID);

                return Json(functionResult.ErrorMessage, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, userID, "AcceptAllOverlaps");
                return Json(ProStrings.Message_ErrorAcceptingOverlaps + ": " + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult AcceptAllOverlaps(int userID, int estimateID)
        {
            CacheActiveLoginID(userID);

            try
            {
                DBAccess db = new DBAccess();
                FunctionResult functionResult = db.ExecuteNonQuery("Overlaps_AcceptAll", new SqlParameter("AdminInfoID", estimateID));

                Estimate.RefreshProcessedLines(estimateID);

                return Json(functionResult.ErrorMessage, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, userID, "AcceptAllOverlaps");
                return Json(ProStrings.Message_ErrorAcceptingOverlaps + ": " + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult PopulateOverlapChanges(int userID, int estimateID)
        {
            CacheActiveLoginID(userID);

            DBAccess db = new DBAccess();

            db.ExecuteNonQuery("Overlaps_FillProposals", new SqlParameter("AdminInfoID", estimateID));

            // If the overlap table is suppressed, accept all overlaps and return nothing
            Estimate estimate = new Estimate(estimateID);
            MiscSettings miscSettings = MiscSettings.GetForProfile(estimate.CustomerProfileID);

            DBAccessTableResult tableResult = db.ExecuteWithTable("Overlaps_GetProposals", new SqlParameter("AdminInfoID", estimateID));

            // Check for auto accept
            if (miscSettings.SuppressAddRelatedPrompt)
            {
                if (tableResult.Success)
                {
                    return Json("AUTO_ACCEPT_ALL_OVERLAPS", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    // No overlaps to auto accept
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }

            // See if there are any overlaps and return them as an HTML table to show to the user
            int ActionPartCount = 0;
            string OldActionPart = "*";
            string TDClass = null;
            bool DisplayActionPart = true;
            StringBuilder builder = new StringBuilder();

            if (tableResult.Success)
            {
                builder.Append("<TABLE  Width=\"100%\" Border=\"1\">");
                builder.Append("<TR Class=\"TableHeader\">");
                builder.Append("<TH Align=\"Left\">" + ProStrings.AcceptQuestion + "</TH>");
                builder.Append("<TH Align=\"Left\">" + ProStrings.Part1 + "</TH>");
                builder.Append("<TH Align=\"Left\">" + ProStrings.Info + "</TH>");
                builder.Append("<TH Align=\"Left\">" + ProStrings.Part2 + "</TH>");
                builder.Append("<TH Align=\"Right\">" + ProStrings.OverlapAmount + "</TH>");
                builder.Append("<TH Align=\"Right\">" + ProStrings.MinLabor + "</TH>");
                builder.Append("</TR>\n");
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    builder.Append("<TR>");
                    if (OldActionPart != row["ActionPart"].ToString())
                    {
                        OldActionPart = row["ActionPart"].ToString();
                        ActionPartCount = ActionPartCount + 1;
                        DisplayActionPart = true;
                    }

                    if (ActionPartCount % 2 != 0)
                    {
                        TDClass = "TableData";
                    }
                    else
                    {
                        TDClass = "TableDataAlt";
                    }

                    if (DisplayActionPart)
                    {
                        //Display ActionPart Column
                        DisplayActionPart = false;
                        builder.Append("<TD Align=\"Left\" Class=\"" + TDClass + "\"><input type=\"checkbox\" id=\"Accept" + ActionPartCount + "\" checked><input type=\"hidden\" id=\"AcceptID" + ActionPartCount + "\" value=\"" + row["ID"].ToString() + "\"></TD>");
                        builder.Append("<TD Align=\"Left\" Class=\"" + TDClass + "\">" + row["ActionPart"].ToString() + "</TD>");
                    }
                    else
                    {
                        //Put &nbsp; into the ActionPart Column
                        //Hide AcceptFlag too?
                        builder.Append("<TD Align=\"Left\" Class=\"" + TDClass + "\">&nbsp;</TD>");
                        builder.Append("<TD Align=\"Left\" Class=\"" + TDClass + "\">&nbsp;</TD>");
                    }

                    builder.Append("<TD Align=\"Left\" Class=\"" + TDClass + "\">" + row["Reason"].ToString() + "</TD>");
                    builder.Append("<TD Align=\"Left\" Class=\"" + TDClass + "\">" + row["BecauseOf"].ToString() + "</TD>");

                    string overlapAmount = row["OverlapAmount"].ToString();
                    if (string.IsNullOrEmpty(overlapAmount))
                    {
                        overlapAmount = "0";
                    }

                    if (overlapAmount != "0")
                    {
                        if (Math.Abs(Convert.ToDouble(overlapAmount)) > 80)
                        {
                            builder.Append("<TD Align=\"Right\" Class=\"" + TDClass + "\">All</TD>");
                        }
                        else
                        {
                            builder.Append("<TD Align=\"Right\" Class=\"" + TDClass + "\">");
                            builder.Append(row["OverlapAmount"].ToString());
                            builder.Append("</TD>");
                        }
                    }
                    else
                    {
                        builder.Append("<TD Align=\"Right\" Class=\"" + TDClass + "\">All</TD>");
                    }

                    string minimumLabor = row["MinimumLabor"].ToString();
                    if (string.IsNullOrEmpty(minimumLabor))
                    {
                        minimumLabor = "0";
                    }
                    if (minimumLabor != "0")
                    {
                        builder.Append("<TD Align=\"Right\" Class=\"" + TDClass + "\">" + row["MinimumLabor"].ToString() + "</TD>");
                    }
                    else
                    {
                        builder.Append("<TD Align=\"Right\" Class=\"" + TDClass + "\">&nbsp;</TD>");
                    }

                    builder.Append("</TR>");
                }
                builder.Append("</TABLE>");
            }

            return Json(builder.ToString(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteLineItem(int userID, int estimateID, int lineID, string meMode)
        {
            CacheActiveLoginID(userID);

            ManualEntryHelper.DeleteLineItem(lineID, meMode, false);
            Estimate.RefreshProcessedLines(estimateID);

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult PopulatePartSource(int userID, string partNumber, string currentPart)
        {
            CacheActiveLoginID(userID);

            StringBuilder builder = new StringBuilder();

            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("PartNumber", partNumber));
            parameters.Add(new SqlParameter("LoginsID", activeLogin.LoginID));

            DBAccess dbAccess = new DBAccess();
            DBAccessTableResult tableResult = dbAccess.ExecuteWithTable("GetAfterMarketParts", parameters);

            if (tableResult.Success)
            {
                builder.AppendLine("<TABLE  Width=\"100%\" Border=\"1\">");

                builder.AppendLine("<TR Class=\"TableHeader\">");
                builder.AppendLine("<TH Align=\"Left\"> </TH>");
                builder.AppendLine("<TH Align=\"Left\">" + ProStrings.Price + "</TH>");
                builder.AppendLine("<TH Align=\"Left\">" + ProStrings.VendorName + "</TH>");
                builder.AppendLine("<TH Align=\"Left\">" + ProStrings.VendorPartNumber + "</TH>");
                builder.AppendLine("<TH Align=\"Left\">" + ProStrings.Description + "</TH>");
                builder.AppendLine("</TR>\n");

                int lineIndex = 0;

                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    builder.AppendLine("<TR>");

                    string tdClass = "";
                    if (lineIndex % 2 != 0)
                        tdClass = "TableData";
                    else
                        tdClass = "TableDataAlt";

                    builder.AppendLine("<TD Align=\"Left\" Class=\"" + tdClass + "\"><INPUT Type=\"Button\" id=\"SelectNonOEM" + lineIndex + "\"  value=\"Select\" ");
                    builder.AppendLine("onclick=\"javascript:SelectNonOem('" + row["VendorPartNumber"] + "'," + lineIndex + "," + currentPart + "," + row["Price"].ToString().Replace(",", "") + "," + row["VendorID"] + ")\">");
                    builder.AppendLine("</TD>");
                    builder.AppendLine("<TD Align=\"Left\" Class=\"" + tdClass + "\">" + row["Price"] + "</TD>");
                    builder.AppendLine("<TD Align=\"Left\" Class=\"" + tdClass + "\">" + row["VendorName"] + "</TD>");
                    builder.AppendLine("<TD Align=\"Left\" Class=\"" + tdClass + "\">" + row["VendorPartNumber"] + "</TD>");
                    builder.AppendLine("<TD Align=\"Left\" Class=\"" + tdClass + "\">" + row["Description"] + "</TD>");

                    builder.AppendLine("</TR>");

                    lineIndex++;
                }

                builder.AppendLine("</TABLE>");
            }

            return Json(builder.ToString(), JsonRequestBehavior.AllowGet);
        }

        public class AddLaborResult : FunctionResult
        {
            public int CurrentPartIndex { get; set; }
            public int NewID { get; set; }
            public bool RIAdded { get; set; }

            public List<ProAdvisorRecommendation> AddOnResults { get; set; }

            public AddLaborResult()
            {
                AddOnResults = new List<ProAdvisorRecommendation>();
            }
        }

        public JsonResult AddLabor(
              int userID
            , int estimateID
            , bool allYears
            , string meMode
            , int sectionKey
            , int index
            , int qty
            , float? laborTimeOverride
            , int? laborTypeOverride
            , string paintTimeString
            , int paintType
            , float allowance
            , float blendTime
            , float edgingTime
            , float undersideTime
            , float adjacentDeductionType
            , bool lockAllowance
            , bool lockClearcoat
            , bool lockBlend
            , bool lockEdging
            , bool lockUnderside
            , bool lockAdjacentDeduction
            , bool includeAllowance
            , bool includeClearcoat
            , bool includeBlend
            , bool includeEdging
            , bool includeUnderside
            , string mode       // Add or Update
            , bool includeRIFlag
            , float rrTime
            , int repairID
            , double price
            , string vehiclePosition
            , string defaultSide
            , string partSource
            , string addAction
            , string vendorPartNumber
            , int vendorID
            , float clearcoatTime
            , string customerNotes
        )
        {
            AddLaborResult result = new AddLaborResult();

            CacheActiveLoginID(userID);

            Estimate estimate = new Estimate(estimateID);

            if (userID == 0)
            {
                result.Success = false;
                result.ErrorMessage = "Session Expired.";
            }
            else if (estimate.IsLocked())
            {
                result.Success = false;
                result.ErrorMessage = ProStrings.Message_ErrorLockedEstimate;
            }
            else
            {
                try
                {
                    if (addAction == "RI")
                    {
                        addAction = "R&I";
                    }

                    List<SectionPartInfo> sectionInfo = _partsService.GetPartsForSection(estimateID, sectionKey, allYears);
                    SectionPartInfo sectionPartInfo = sectionInfo[index];
                    if (sectionPartInfo == null)
                    {
                        result.Success = false;
                        result.ErrorMessage = "error,Cached part info data not found.";
                    }
                    else
                    {
                        decimal laborTime = 0;
                        string action = "";

                        bool sectionPaintTime = paintTimeString == null;
                        decimal paintTime = InputHelper.GetDecimal(paintTimeString);

                        // 1 = Body, 3 = Structure, 4 = Mechanical, 25 = Glass
                        if (sectionPartInfo.LaborType == 1 || sectionPartInfo.LaborType == 3 || sectionPartInfo.LaborType == 4 || sectionPartInfo.LaborType == 25)
                        {
                            if (sectionPartInfo.OHTime > 0)
                            {
                                action = "Over";
                                laborTime = sectionPartInfo.OHTime;
                            }
                            if (sectionPartInfo.AddTime > 0)
                            {
                                action = "Add";
                                laborTime = sectionPartInfo.AddTime;
                            }
                            if (sectionPartInfo.AITime > 0)
                            {
                                action = "Access/Ins";
                                laborTime = sectionPartInfo.AITime;
                            }
                            if (sectionPartInfo.CATime > 0)
                            {
                                action = "Align";
                                laborTime = sectionPartInfo.CATime;
                                qty = 1;
                            }
                            if (sectionPartInfo.AlignTime > 0)
                            {
                                action = "Align";
                                laborTime = sectionPartInfo.AlignTime;
                            }
                            if (sectionPartInfo.RITime > 0)
                            {
                                action = "R&I";
                                laborTime = sectionPartInfo.RITime;
                            }

                            if (addAction != "Blend" && paintTime == 0 && sectionPaintTime)
                            {
                                paintTime = sectionPartInfo.PaintTime;
                            }

                        }
                        else
                        {
                            action = "Refinish";
                            paintTime = sectionPartInfo.PaintTime;
                            laborTime = 0;
                        }

                        int edgingType = (paintTime > 0 & edgingTime > 0 ? 21 : 0);

                        if (addAction == "Refinish" || addAction == "Blend")
                        {
                            laborTime = 0;
                        }

                        // R&I has only body labor
                        if (addAction == "R&I")
                        {
                            allowance = 0;
                            paintTime = 0;
                            clearcoatTime = 0;
                            blendTime = 0;
                            edgingTime = 0;
                            undersideTime = 0;
                        }



                        string addOrUpdate = mode;
                        if (string.IsNullOrEmpty(addOrUpdate))
                        {
                            addOrUpdate = "Add";
                        }
                        addOrUpdate = addOrUpdate.Replace("Labor", "");

                        // If the paint type is 3 Stage or 2 Tone, it's a Refinish part, and there is an allowance, include the allowance
                        if (action == "Refinish" && (paintType == 18 || paintType == 29) && allowance > 0)
                        {
                            includeAllowance = true;
                        }

                        if (addOrUpdate == "Add" && addAction == "Replace" && !string.IsNullOrEmpty(sectionPartInfo.Barcode))
                        {
                            List<AssemblyMatrix> listAssembly = GetComponents(sectionPartInfo.Service_Barcode, sectionKey, "");

                            //first warn the user if a component if it is a part of assembly that has been already added
                            foreach (AssemblyMatrix oAssembly in listAssembly.Where(o => o.PartBarcode == sectionPartInfo.Barcode))
                            {
                                SectionPartInfo part = sectionInfo.FirstOrDefault(o => o.Barcode == oAssembly.AssemblyBarcode);
                                if (part != null && part.ID > -1)
                                {
                                    result.ErrorMessage = "WARNING: The part you just added is already on the estimate in the assembly '" + oAssembly.AssemblyPartnum + " - " + oAssembly.AssemblyPartdesc + "'";
                                }
                            }

                            //Now delete components that belong to an assembly that is about to be added 
                            foreach (AssemblyMatrix oAssembly in listAssembly.Where(o => o.AssemblyBarcode == sectionPartInfo.Barcode))
                            {
                                SectionPartInfo part = sectionInfo.FirstOrDefault(o => o.Barcode == oAssembly.PartBarcode);
                                if (part != null && part.ID > -1)
                                {
                                    ManualEntryHelper.DeleteLineItem(part.ID, meMode, false);
                                }
                            }
                        }

                        int NewID = ManualEntryHelper.AddUpdateEstimateLine(
                        estimateID,
                        addOrUpdate,
                        sectionPartInfo.ID,
                        sectionPartInfo.PartNumber,
                        sectionPartInfo.Description,
                        partSource,
                        !string.IsNullOrEmpty(addAction) ? addAction : action,
                        "",
                        laborTimeOverride.HasValue ? Convert.ToDecimal(laborTimeOverride.Value) : laborTime,
                        laborTypeOverride.HasValue ? laborTypeOverride.Value : sectionPartInfo.LaborType,
                        false,
                        Convert.ToDecimal(paintTime),
                        paintType,
                        Convert.ToDecimal(price),
                        0,
                        0,
                        Convert.ToDecimal(clearcoatTime),
                        Convert.ToDecimal(blendTime),
                        edgingType,
                        Convert.ToDecimal(edgingTime),
                        "",
                        customerNotes,
                        false,
                        qty,
                        0,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        vendorID,
                        Convert.ToDecimal(allowance),
                        Convert.ToDecimal(undersideTime),
                        Convert.ToInt32(adjacentDeductionType),
                        false,
                        sectionPartInfo.Barcode,
                        Convert.ToDecimal(price),
                        "",
                        0,
                        sectionKey,
                        vehiclePosition,
                        vendorPartNumber,
                        lockAdjacentDeduction,
                        lockAllowance,
                        lockClearcoat,
                        lockBlend,
                        lockEdging,
                        lockUnderside,
                        includeAllowance,
                        includeClearcoat,
                        includeBlend,
                        includeEdging,
                        includeUnderside
                    );

                        result.Success = true;
                        result.CurrentPartIndex = index;
                        result.NewID = NewID;

                        if (includeRIFlag && rrTime > 0)
                        {
                            int theID = 0;
                            addOrUpdate = "";

                            if (repairID > 0)
                            {
                                theID = repairID;
                                addOrUpdate = "Update";
                            }
                            else
                            {
                                theID = estimateID;
                                addOrUpdate = "Add";
                            }

                            NewID = ManualEntryHelper.AddUpdateEstimateLine(
                                estimateID,
                                addOrUpdate,
                                theID,
                                sectionPartInfo.PartNumber,
                                sectionPartInfo.Description,
                                "",
                                "R&I",
                                "",
                                Convert.ToDecimal(rrTime),
                                laborTypeOverride.HasValue ? laborTypeOverride.Value : sectionPartInfo.LaborType,
                                false,
                                0,
                                paintType,
                                0,
                                0,
                                0,
                                0,
                                0,
                                edgingType,
                                0,
                                "",
                                customerNotes,
                                false,
                                qty,
                                0,
                                false,
                                false,
                                false,
                                false,
                                false,
                                false,
                                false,
                                false,
                                -1,
                                0,
                                0,
                                Convert.ToInt32(adjacentDeductionType),
                                false,
                                sectionPartInfo.Barcode + "REP",
                                0,
                                "",
                                0,
                                sectionKey,
                                vehiclePosition,
                                ""
                            );

                            result.RIAdded = true;
                        }

                        try
                        {
                            SiteActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, ComputerKey);

                            bool proAdvisorEnabled = InputHelper.GetBoolean(SiteSettings.Get(activeLogin.LoginID, "ProAdvisorEnabled", "AddPartsOptions", (true).ToString()).ValueString);

                            if (activeLogin != null && activeLogin.HasProAdvisorContract && proAdvisorEnabled)
                            {
                                List<ProAdvisorRecommendation> recommendations = _proAdvisorService.GetRecommendations(estimate, sectionKey, sectionPartInfo.Description, addAction);
                                if (recommendations.Count > 0)
                                {
                                    result.AddOnResults = recommendations;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.LogError(ex, estimate.CreatedByLoginID, "ProAdvisor Recomendations");
                        }
                    }
                }
                catch (Exception ex)
                {
                    string rawUrl = "";
                    try
                    {
                        rawUrl = Request.RawUrl;
                    }
                    catch { }

                    ProEstimatorData.ErrorLogger.LogError(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + rawUrl, userID, estimateID, "EstimateController AddLabor");

                    result.Success = false;
                    result.ErrorMessage = ex.Message;
                }
            }

            Estimate.RefreshProcessedLines(estimateID);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAddedComponents(int estimateID, bool allYears, int sectionKey, int index)
        {
            string result = "";
            List<SectionPartInfo> sectionInfo = _partsService.GetPartsForSection(estimateID, sectionKey, allYears);
            if (index > -1 && sectionInfo.Count > index)
            {
                SectionPartInfo sectionPartInfo = sectionInfo[index];
                List<AssemblyMatrix> listAssembly = GetComponents(sectionPartInfo.Service_Barcode, sectionKey, sectionPartInfo.Barcode);
                foreach (AssemblyMatrix oAssembly in listAssembly)
                {
                    SectionPartInfo part = sectionInfo.FirstOrDefault(o => o.Barcode == oAssembly.PartBarcode);
                    if (part != null && part.ID > -1)
                    {
                        result += part.PartNumber + " - " + part.Description + "\n";
                    }
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddAdditionalOperation(
              int userID
            , int estimateID
            , int panelID
            , int header
            , int section
            , int index
        )
        {
            AddLaborResult result = new AddLaborResult();

            CacheActiveLoginID(userID);

            Estimate estimate = new Estimate(estimateID);

            if (userID == 0)
            {
                result.Success = false;
                result.ErrorMessage = "Session Expired.";
            }
            else if (estimate.IsLocked())
            {
                result.Success = false;
                result.ErrorMessage = ProStrings.Message_ErrorLockedEstimate;
            }
            else
            {
                try
                {
                    PDR_Panel pdrPanel = PDR_Panel.GetByLinkedPanelID(panelID);

                    bool allYears = false;

                    Vehicle vehicle = ProEstimatorData.DataModel.Vehicle.GetByEstimate(estimateID);
                    VehicleIDResult vehicleIDData = ProEstimatorData.DataModel.Vehicle.GetVehicleIDFromInfo(vehicle.Year, vehicle.MakeID, vehicle.ModelID, vehicle.TrimID);
                    if (vehicleIDData != null && vehicleIDData.CarryUp)
                    {
                        allYears = true;
                    }

                    List<SectionPartInfo> sectionParts = _partsService.GetPartsForSection(estimateID, header, section, allYears);
                    List<SectionPartInfo> parts = _pdrAdditionalOperationsService.GetPartsForPDRAdditionalOperations(sectionParts, pdrPanel.IsLeftSide, pdrPanel.IsRightSide);
                    SectionPartInfo partInfo = parts[index];

                    if (partInfo == null)
                    {
                        result.Success = false;
                        result.ErrorMessage = "error,Cached part info data not found.";
                    }
                    else
                    {
                        decimal laborTime = 0;
                        string action = "";
                        int paintType = 0;

                        decimal paintTime = partInfo.PaintTime;
                        decimal clearCoatTime = 0;

                        if (partInfo.PaintType > 1)
                        {
                            paintType = partInfo.PaintType;
                            string notes = partInfo.Notes.ToLower();

                            if (notes.Contains("clear coat") || notes.Contains("clearcoat") || partInfo.PaintName == "Clearcoat")
                            {
                                PaintSettings paintRates = PaintSettings.GetForProfile(estimate.CustomerProfileID);

                                clearCoatTime = (decimal)partInfo.PaintTime * (decimal)(paintRates.MajorClearCoat / 100.0);
                                // _partList[index].LaborRefinishTime * (_rateProfile.MajorClearCoat / 100);
                            }
                        }

                        if (partInfo.LaborType == 1 || partInfo.LaborType == 3 || partInfo.LaborType == 4 || partInfo.LaborType == 25)
                        {
                            if (partInfo.OHTime > 0)
                            {
                                action = "Over";
                                laborTime = partInfo.OHTime;
                            }
                            if (partInfo.AddTime > 0)
                            {
                                action = "Add";
                                laborTime = partInfo.AddTime;
                            }
                            if (partInfo.AITime > 0)
                            {
                                action = "Access/Ins";
                                laborTime = partInfo.AITime;
                            }
                            if (partInfo.CATime > 0)
                            {
                                action = "Align";
                                laborTime = partInfo.CATime;
                            }
                            if (partInfo.AlignTime > 0)
                            {
                                action = "Align";
                                laborTime = partInfo.AlignTime;
                            }
                            if (partInfo.RITime > 0)
                            {
                                action = "R&I";
                                laborTime = partInfo.RITime;
                            }
                        }
                        else
                        {
                            action = "Refinish";
                            paintTime = partInfo.PaintTime;
                            laborTime = 0;
                        }

                        int NewID = ManualEntryHelper.AddUpdateEstimateLine(
                            estimateID,
                            "Add",
                            partInfo.ID,
                            partInfo.PartNumber,
                            partInfo.Description,
                            "",
                            action,
                            "",
                            laborTime,
                            partInfo.LaborType,
                            false,
                            Convert.ToDecimal(paintTime),
                            paintType,
                            partInfo.Price,
                            0,
                            0,
                            clearCoatTime,
                            0,
                            0,
                            0,
                            "",
                            "",
                            false,
                            0,
                            0,
                            false,
                            false,
                            false,
                            false,
                            false,
                            false,
                            false,
                            false,
                            0,
                            0,
                            0,
                            0,
                            false,
                            partInfo.Barcode,
                            partInfo.Price,
                            "",
                            0,
                            partInfo.SectionID,
                            "",
                            "",
                            false,
                            false,
                            false,
                            false,
                            false,
                            false,
                            false,
                            clearCoatTime > 0,
                            false,
                            false,
                            false
                        );

                        result.Success = true;
                        result.CurrentPartIndex = index;
                        result.NewID = NewID;
                    }
                }
                catch (Exception ex)
                {
                    string rawUrl = "";
                    try
                    {
                        rawUrl = Request.RawUrl;
                    }
                    catch { }

                    ProEstimatorData.ErrorLogger.LogError(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + rawUrl, userID, estimateID, "EstimateController AddLabor");

                    result.Success = false;
                    result.ErrorMessage = ex.Message;
                }
            }

            Estimate.RefreshProcessedLines(estimateID);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private List<AssemblyMatrix> GetComponents(string serviceBarCode, int sectionKey, string barcode)
        {
            List<AssemblyMatrix> assemblies = new List<AssemblyMatrix>();

            int nHeader = sectionKey / 256;
            int nSection = sectionKey % 256;

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ServiceBarCode", serviceBarCode));
            parameters.Add(new SqlParameter("nHeader", nHeader));
            parameters.Add(new SqlParameter("nSection", nSection));
            parameters.Add(new SqlParameter("BarCode", barcode));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("GetAssemblyParts", parameters);

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    assemblies.Add(new AssemblyMatrix(row));
                }
            }

            return assemblies;
        }

        public class AssemblyMatrix
        {
            public string AssemblyBarcode { get; set; }
            public string AssemblyPartnum { get; set; }
            public string AssemblyPartdesc { get; set; }
            public string PartBarcode { get; set; }
            public string Partnum { get; set; }
            public string Partdesc { get; set; }

            public AssemblyMatrix(DataRow row)
            {
                AssemblyBarcode = row["AssemblyBarCode"].ToString();
                AssemblyPartnum = row["AssemblyPartNumber"].ToString();
                AssemblyPartdesc = row["AssemblyPartDescription"].ToString();
                PartBarcode = row["PartBarcode"].ToString();
                Partnum = row["PartNumber"].ToString();
                Partdesc = row["PartDescription"].ToString();
            }
        }

        #endregion


        #region Images

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/images")]
        public ActionResult Images(int userID, int estimateID)
        {
            // Make sure the estimate belongs to the login
            Estimate estimate = new Estimate(estimateID);
            if (estimate.CreatedByLoginID != ActiveLogin.LoginID)
            {
                return Redirect("/" + userID);
            }

            // Make sure the estimate is attached to a customer
            if (estimate.CustomerID == 0)
            {
                return Redirect("/" + userID + "/estimate/" + estimate.EstimateID + "/customer-selection");
            }

            ImagesVM model = new ImagesVM(estimate.LockLevel);
            model.LoginID = ActiveLogin.LoginID;
            model.EstimateID = estimateID;
            model.Images = GetAllImagePaths(ActiveLogin.LoginID, estimateID);
            model.HasImagesContract = ActiveLogin.HasImagesContract;
            model.EditorIsTrial = ActiveLogin.ImageEditorIsTrial;
            model.EstimateIsLocked = estimate.IsLocked();

            ViewBag.NavID = "estimate";
            ViewBag.EstimateNavID = "images";

            ViewBag.UserID = userID;
            ViewBag.EstimateID = estimateID;
            ViewBag.ActiveLoginID = ActiveLogin.ID;

            ViewBag.IsMobileDevice = Request.Browser.IsMobileDevice;

            return View(model);
        }

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/image/{imageID}/{selectedPageIndex}/{boolEdit}")]
        public ActionResult ImageZoom(int userID, int estimateID, int imageID, int selectedPageIndex, bool boolEdit)
        {
            CacheActiveLoginID(userID);

            ImageVM model = new ImageVM();

            EstimationImage image = EstimationImage.GetEstimationImage(imageID);
            image.SelectedPageIndex = selectedPageIndex;

            if (image != null && image.AdminInfoID == estimateID)
            {
                model = GetImageVM(ActiveLogin.LoginID, image, false);
                //model.ImageExtra = image.GetImageExtraInfo(loginID);
                model.UserID = userID;
                model.LoginID = ActiveLogin.LoginID;
                //model.ImagePath = image.GetWebPath(model.LoginID, false, selectedPageIndex);
                //model.ImagePath = Regex.Replace(image.GetWebPath(model.LoginID, false, selectedPageIndex), ".pdf", ".jpg", RegexOptions.IgnoreCase);
                string imagePath = image.GetWebPath(model.LoginID, true, selectedPageIndex).Replace("_thumb", "");//.Replace(".", "_edit.");
                if (boolEdit)
                {
                    int i = imagePath.LastIndexOf("/");
                    imagePath = imagePath.Substring(0, i + 1) + imagePath.Substring(i + 1, imagePath.Length - i - 1).Replace(".", "_edit.");
                }
                model.ImagePath = imagePath + "?r=" + new Random().Next();
            }

            return View(model);
        }

        [HttpPost]
        public Boolean ReOrderImages(List<string> data, int estimateID)
        {
            try
            {
                List<EstimationImage> images = EstimationImage.GetForEstimate(estimateID);
                int index = 0;
                foreach (var imageId in data)
                {
                    EstimationImage image = images.FirstOrDefault(x => x.ID == int.Parse(imageId));
                    if (image != null)
                    {
                        image.OrderNumber = index++;
                        image.Save();
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        public JsonResult RemoveEditImage(int loginID, int imageID, int userID, int estimateID)
        {
            CacheActiveLoginID(userID);

            UploadImageResult result = new UploadImageResult();
            result.ImageInfo = new ImageExtraInfo();

            try
            {
                EstimationImage image = EstimationImage.GetEstimationImage(imageID);
                if (image != null && image.AdminInfoID == estimateID)
                {
                    string imageFolder = Path.Combine(GetEstimateDiskPath(loginID, estimateID), "Images");

                    if (System.IO.Directory.Exists(imageFolder))
                    {
                        string ext = (image.FileType.ToLower() == "pdf" ? "jpg" : image.FileType);
                        string file = Path.Combine(imageFolder, imageID.ToString() + "_edit." + ext);////
                        if (System.IO.File.Exists(file))
                        {
                            System.IO.File.Delete(file);
                        }

                        string thumbFile = Path.Combine(imageFolder, imageID.ToString() + "_thumbedit." + ext);
                        if (System.IO.File.Exists(thumbFile))
                        {
                            System.IO.File.Delete(thumbFile);
                        }

                        result.Success = true;

                        using (System.IO.Stream stream = System.IO.File.OpenRead(Path.Combine(imageFolder, imageID.ToString() + "." + ext)))
                        {
                            System.Drawing.Image tempImage = System.Drawing.Image.FromStream(stream, false, false);
                            result.ImageInfo.Width = tempImage.Width;
                            result.ImageInfo.Height = tempImage.Height;
                        }
                        string diskPath = Path.Combine(imageFolder, imageID.ToString() + "." + ext);
                        if(Path.GetFullPath(diskPath).StartsWith(imageFolder, StringComparison.OrdinalIgnoreCase))
                        {
                            result.ImageInfo.DiskSize = GetDiskSize(diskPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UploadImage()
        {
            //return Json("", JsonRequestBehavior.AllowGet); 
            BulkUploadImageResult bulkUploadImageResult = new BulkUploadImageResult();
            UploadImageResult[] result = null;
            StringBuilder strBldrErrorMessage = new StringBuilder();

            int loginID = 0;
            int estimateID = 0;
            int activeLoginID = 0;

            if (System.Web.HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpRequest = System.Web.HttpContext.Current.Request;

                if (httpRequest.Files.Count > 0)
                {
                    result = new UploadImageResult[httpRequest.Files.Count];
                    bulkUploadImageResult.UploadImageResultArray = result;

                    for (int eachIndex = 0; eachIndex < httpRequest.Files.Count; eachIndex++)
                    {
                        var file = System.Web.HttpContext.Current.Request.Files[eachIndex];
                        if (file.FileName != "")
                        {

                            loginID = InputHelper.GetInteger(System.Web.HttpContext.Current.Request.Form["LoginID"].ToString());
                            estimateID = InputHelper.GetInteger(System.Web.HttpContext.Current.Request.Form["EstimateID"].ToString());
                            activeLoginID = InputHelper.GetInteger(System.Web.HttpContext.Current.Request.Form["ActiveLoginID"].ToString());

                            result[eachIndex] = _estimateService.UploadImage(loginID, estimateID, file);
                            bulkUploadImageResult.Success = true;

                            if (!string.IsNullOrEmpty(result[eachIndex].ErrorMessage))
                            {
                                ErrorLogger.LogError(result[eachIndex].ErrorMessage, loginID, estimateID, "ImageUpload");
                                strBldrErrorMessage.AppendLine(result[eachIndex].ErrorMessage);
                            }
                        }

                    }
                }
            }

            if (!string.IsNullOrEmpty(strBldrErrorMessage.ToString()))
            {
                bulkUploadImageResult.Success = false;
                bulkUploadImageResult.ErrorMessage = strBldrErrorMessage.ToString();
            }

            if (loginID > 0)
            {
                SuccessBoxFeatureLog.LogFeature(loginID, SuccessBoxModule.EstimateWriting, "Image uploaded", activeLoginID);
            }

            return Json(bulkUploadImageResult, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteImage(int imageID, int userID, int estimateID)
        {
            CacheActiveLoginID(userID);

            UploadImageResult result = new UploadImageResult();

            try
            {
                EstimationImage image = EstimationImage.GetEstimationImage(imageID);
                if (image != null && image.AdminInfoID == estimateID)
                {
                    image.Delete();
                    result.Success = true;
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = @Proestimator.Resources.ProStrings.ErroDeletingImage + " " + ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetImageIncluded(int userID, int estimateID, int imageID, bool included)
        {
            if (estimateID > 0 && imageID > 0)
            {
                CacheActiveLoginID(userID);

                EstimationImage image = EstimationImage.GetEstimationImage(imageID);
                if (image != null && image.AdminInfoID == estimateID)
                {
                    image.Include = included;
                    image.Save();
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> DecodeLicensePlateImage()
        {
            var results = new DecodeLicensePlateImageResults
            {
                ErrorMessage = "No file attached."
            };

            if (System.Web.HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var file = System.Web.HttpContext.Current.Request.Files["Images"];
                if (file != null)
                {
                    results = await _estimateService.DecodeLicensePlateImage(file, ProStrings.UploadLicensePlatePhoto_ReadFailure);
                }
            }

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveImage(int userID, int loginID, int estimateID, int imageID, int tagID, string tag, int supplementVersion, bool copy)
        {
            CacheActiveLoginID(userID);

            SaveResult saveResult;
            UploadImageResult result = new UploadImageResult();

            EstimationImage image = EstimationImage.GetEstimationImage(imageID);
            if (image != null && image.AdminInfoID == estimateID)
            {
                if (copy)
                {
                    EstimationImage newImage = new EstimationImage();
                    newImage.AdminInfoID = image.AdminInfoID;
                    newImage.FileName = image.FileName;
                    newImage.FileType = image.FileType;
                    newImage.PDFPageCount = image.FileType.ToUpper() == "PDF" ? 1 : image.PDFPageCount;
                    newImage.Include = image.Include;
                    newImage.Deleted = image.Deleted;
                    newImage.OrderNumber = image.OrderNumber;
                    newImage.ImageTag = tagID;
                    newImage.ImageTagCustom = tag;
                    newImage.SupplementVersion = supplementVersion;
                    saveResult = newImage.Save();
                    result.ImageID = newImage.ID;
                    result.NewImagePath = newImage.GetWebPath(loginID, true);
                    result.Include = newImage.Include;
                }
                else
                {
                    image.ImageTag = tagID;
                    image.ImageTagCustom = tag;
                    image.SupplementVersion = supplementVersion;
                    saveResult = image.Save(GetActiveLoginID(userID));
                    result.ImageID = imageID;
                }
                if (saveResult.Success)
                {
                    result.Success = true;
                }
                else
                {
                    result.ErrorMessage = saveResult.ErrorMessage;
                }
            }
            else
            {
                result.ErrorMessage = @Proestimator.Resources.ProStrings.ImageNotFoundCannotSave;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetImageDetails(int userID, int loginID, int estimateID, int imageID, int selectedPageIndex)
        {
            CacheActiveLoginID(userID);

            ImageVM imageVM = new ImageVM();

            EstimationImage image = EstimationImage.GetEstimationImage(imageID);
            image.SelectedPageIndex = selectedPageIndex;

            if (image != null && image.AdminInfoID == estimateID)
            {
                imageVM = GetImageVM(loginID, image, true);
                //imageVM.ImageExtra = image.GetImageExtraInfo(loginID);
            }

            //
            string filePath = System.IO.Path.Combine(GetEstimateDiskPath(loginID, estimateID), "Images", imageID.ToString() + (selectedPageIndex > 1 ? "_" + selectedPageIndex : "") + "_edit." + (image.FileType.ToLower() == "pdf" ? "jpg" : image.FileType));////
            //string filePath = System.IO.Path.Combine(GetEstimateDiskPath(loginID, estimateID), "Images", imageID.ToString() + "_thumb" + (selectedPageIndex > 1 ? "_" + selectedPageIndex : "") + "_edit." + (image.FileType.ToLower() == "pdf" ? "jpg" : image.FileType));////
            if (System.IO.File.Exists(filePath))
            {
                int i = imageVM.ImagePath.LastIndexOf("/");
                string filePart = imageVM.ImagePath.Substring(i + 1, imageVM.ImagePath.Length - i - 1);
                string fileName = filePart.Replace("_thumb", "").Replace(".", "_edit.");////
                //string fileName = filePart.Replace(".", "_edit.");////
                imageVM.ImagePath = imageVM.ImagePath.Substring(0, i + 1) + fileName;
                imageVM.ImageExtra = image.GetImageExtraInfo(loginID, true);
            }
            else
            {
                imageVM.ImagePath = imageVM.ImagePath.Replace("_thumb", "");////
                imageVM.ImageExtra = image.GetImageExtraInfo(loginID, false);
            }

            return Json(imageVM, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UploadEditImage()
        {
            UploadImageResult result = new UploadImageResult();
            result.Success = false;
            result.ImageInfo = new ImageExtraInfo();

            int loginID = 0;

            try
            {
                loginID = InputHelper.GetInteger(System.Web.HttpContext.Current.Request.Form["LoginID"].ToString());
                int estimateID = InputHelper.GetInteger(System.Web.HttpContext.Current.Request.Form["EstimateID"].ToString());
                byte[] imageBytes = Convert.FromBase64String(System.Web.HttpContext.Current.Request.Form["Image"].ToString());
                string filePath = System.Web.HttpContext.Current.Request.Form["FilePath"].ToString();
                bool copy = InputHelper.GetBoolean(System.Web.HttpContext.Current.Request.Form["IsCopy"].ToString());
                int imageID = InputHelper.GetInteger(System.Web.HttpContext.Current.Request.Form["ImageID"].ToString());

                MemoryStream imageStream = new MemoryStream(imageBytes);
                Image image = Image.FromStream(imageStream, true, true);
                Image imageSmall = EstimateService.ScaleImage(image, 600, 800);
                string imageFolder = Path.Combine(GetEstimateDiskPath(loginID, estimateID), "Images");
                Directory.CreateDirectory(imageFolder);
                int i = filePath.LastIndexOf("/");
                string filePart = filePath.Substring(i + 1, filePath.Length - i - 1);

                if (copy)
                {
                    int j = filePart.LastIndexOf(".");
                    string extension = filePart.Substring(j + 1, filePart.Length - j - 1);
                    Image imageLarge = EstimateService.ScaleImage(image, 2000, 2000);
                    ImageSave(imageSmall, Path.Combine(imageFolder, imageID.ToString() + "_thumb." + extension));
                    ImageSave(imageLarge, Path.Combine(imageFolder, imageID.ToString() + "." + extension));
                    result.ImageInfo.Height = imageLarge.Height;
                    result.ImageInfo.Width = imageLarge.Width;
                    result.ImageInfo.DiskSize = GetDiskSize(Path.Combine(imageFolder, imageID.ToString() + "." + extension));
                }
                else
                {
                    string fileName = filePart.Replace("_edit", "").Replace(".", "_edit.");

                    //System.IO.File.WriteAllBytes(Path.Combine(imageFolder, fileName + "_edit." + extension), imageBytes);
                    string diskPath = Path.Combine(imageFolder, fileName);
                    if (Path.GetFullPath(diskPath).StartsWith(imageFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        System.IO.File.WriteAllBytes(Path.Combine(imageFolder, fileName), imageBytes);
                    }
                    /*if (System.IO.File.Exists(Path.Combine(imageFolder, fileName)))
                    {
                        System.IO.File.Delete(Path.Combine(imageFolder, fileName));
                    }

                    using (FileStream stream = new FileStream(Path.Combine(imageFolder, fileName), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        stream.Write(imageBytes, 0, imageBytes.Length);
                        stream.Close();
                    }*/
                    imageSmall.Save(Path.Combine(imageFolder, fileName.Replace("_edit", "_thumbedit")));
                    
                    if(Path.GetFullPath(diskPath).StartsWith(imageFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        using (System.IO.Stream stream = System.IO.File.OpenRead(Path.Combine(imageFolder, fileName)))
                        {
                            System.Drawing.Image tempImage = System.Drawing.Image.FromStream(stream, false, false);
                            result.ImageInfo.Width = tempImage.Width;
                            result.ImageInfo.Height = tempImage.Height;
                        }
                    }

                    result.ImageInfo.DiskSize = GetDiskSize(Path.Combine(imageFolder, fileName));
                }

                result.Success = true;
                //result.ImageID = estimationImage.ID;
                //result.NewImagePath = estimationImage.GetWebPath(loginID, true);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "UploadEditImage");
                result.ErrorMessage = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Files for an estimate are stored on disk in a folder like C:/UserContent/LoginID/EstimateID.  This function returns that as a path.
        /// </summary>
        public string GetEstimateDiskPath(int loginID, int estimateID)
        {
            return Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), loginID.ToString(), estimateID.ToString());
        }

        public List<ImageVM> GetAllImagePaths(int loginID, int estimateID)
        {
            List<ImageVM> imageData = new List<ImageVM>();

            List<EstimationImage> images = EstimationImage.GetForEstimate(estimateID);
            foreach (EstimationImage image in images)
            {
                imageData.Add(GetEditImageVM(loginID, image, estimateID));
            }

            return imageData;
        }

        private ImageVM GetEditImageVM(int loginID, EstimationImage image, int estimateID)
        {
            ImageVM imageVM = GetImageVM(loginID, image);
            string filePath = System.IO.Path.Combine(GetEstimateDiskPath(loginID, estimateID), "Images", image.ID.ToString() + (image.SelectedPageIndex > 1 ? "_" + image.SelectedPageIndex : "") + "_thumbedit." + (image.FileType.ToLower() == "pdf" ? "jpg" : image.FileType));
            if (System.IO.File.Exists(filePath))
            {
                int i = imageVM.ImagePath.LastIndexOf("/");
                string filePart = imageVM.ImagePath.Substring(i + 1, imageVM.ImagePath.Length - i - 1);
                string fileName = filePart.Replace("_thumb", "").Replace(".", "_thumbedit.");
                imageVM.ImagePath = imageVM.ImagePath.Substring(0, i + 1) + fileName;
            }
            return imageVM;
        }

        private ImageVM GetImageVM(int loginID, EstimationImage image, Boolean isGettingImageDetails = false)
        {
            ImageVM vm = new ImageVM();
            vm.ImageID = image.ID;

            if (string.Compare(image.FileType.ToLower(), "pdf", true) == 0)
            {
                if (isGettingImageDetails)
                {
                    if (image.PDFPageCount > 1)
                    {
                        string diskPath = image.GetDiskPath(loginID, false);

                        if (System.IO.File.Exists(diskPath))
                        {
                            Image thumbnailImage;
                            string filePath = string.Empty;

                            using (var rasterizer = new GhostscriptRasterizer())
                            {
                                rasterizer.Open(diskPath);
                                int pageCount = rasterizer.PageCount;

                                string imageFolder = Path.Combine(GetEstimateDiskPath(loginID, image.AdminInfoID), "Images");

                                for (int pageIndex = 2; pageIndex <= pageCount; pageIndex++)
                                {
                                    filePath = Path.Combine(imageFolder, image.ID.ToString() + "_thumb_" + pageIndex + ".jpg");

                                    if (!System.IO.File.Exists(filePath))
                                    {
                                        thumbnailImage = rasterizer.GetPage(96, 96, pageIndex);

                                        if (thumbnailImage != null)
                                        {
                                            thumbnailImage = EstimateService.ScaleImage(thumbnailImage, 600, 800);

                                            thumbnailImage.Save(Path.Combine(imageFolder, image.ID.ToString() + "_thumb_" + pageIndex + ".jpg"));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            vm.ImagePath = image.GetWebPath(loginID, true, image.SelectedPageIndex);
            vm.ImageTag = image.ImageTag;
            vm.ImageTagCustom = image.ImageTagCustom;
            vm.FileExtension = image.FileType.ToLower();
            vm.SupplementVersion = image.SupplementVersion;
            vm.Include = image.Include;
            vm.Deleted = image.Deleted;
            vm.PDFPageCount = image.PDFPageCount;
            return vm;
        }

        public void CopyImages(int loginID, int sourceAdminInfoID, int targetAdminInfoID)
        {
            string sourceFolder = Path.Combine(GetEstimateDiskPath(loginID, sourceAdminInfoID), "Images");
            string targetFolder = Path.Combine(GetEstimateDiskPath(loginID, targetAdminInfoID), "Images");

            try
            {
                if (System.IO.Directory.Exists(sourceFolder))
                {
                    Directory.CreateDirectory(targetFolder);

                    List<EstimationImage> allImages = EstimationImage.GetForEstimate(sourceAdminInfoID);
                    if (allImages != null)
                    {
                        foreach (EstimationImage image in allImages)
                        {
                            EstimationImage newImage = new EstimationImage();
                            newImage.AdminInfoID = targetAdminInfoID;
                            newImage.FileName = image.FileName;
                            newImage.FileType = image.FileType;
                            newImage.ImageTag = image.ImageTag;
                            newImage.ImageTagCustom = image.ImageTagCustom;

                            SaveResult saveResult = newImage.Save();

                            if (saveResult.Success)
                            {
                                // Copy the main image
                                string sourcePath = Path.Combine(sourceFolder, image.ID.ToString() + "." + image.FileType);
                                string targetPath = Path.Combine(targetFolder, newImage.ID.ToString() + "." + image.FileType);

                                if (System.IO.File.Exists(sourcePath))
                                {
                                    FileInfo fileInfo = new FileInfo(sourcePath);
                                    fileInfo.CopyTo(targetPath);

                                    // Copy the thumbnail
                                    sourcePath = Path.Combine(sourceFolder, image.ID.ToString() + "_thumb." + image.FileType);
                                    targetPath = Path.Combine(targetFolder, newImage.ID.ToString() + "_thumb." + image.FileType);

                                    fileInfo = new FileInfo(sourcePath);
                                    fileInfo.CopyTo(targetPath);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "CopyImages");
            }
        }

        public List<ImageAndGraphic> GetImageList(int vehicleID, int groupNumber)
        {
            List<ImageAndGraphic> imageList = new List<ImageAndGraphic>();

            string imagesLocation = ConfigurationManager.AppSettings.Get("VehicleImagesLocation").ToString();

            // Get list of image number and file names from the mitchell database
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("VehicleID", vehicleID));
            parameters.Add(new SqlParameter("GroupNumber", groupNumber));

            DBAccess mitchellDB = new DBAccess(DatabaseName.Mitchell);
            DBAccessTableResult tableResult = mitchellDB.ExecuteWithTable("GetImageListByVehicleID", parameters);

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    ImageAndGraphic imageAndGraphic = new ImageAndGraphic(row);

                    // Only add if the image file exists
                    string graphicFileName = Path.Combine(imagesLocation, imageAndGraphic.GraphicFileName + ".tif");
                    if(Path.GetFullPath(graphicFileName).StartsWith(imagesLocation, StringComparison.OrdinalIgnoreCase))
                    {
                        if (System.IO.File.Exists(imagesLocation + imageAndGraphic.GraphicFileName + ".tif"))
                        {
                            imageList.Add(imageAndGraphic);
                        }
                        else
                        {
                            ErrorLogger.LogError("Image " + imageAndGraphic.GraphicFileName + " not found for vehicleID: " + vehicleID + " and GroupNumber: " + groupNumber, "PartsImageNotFound");
                        }
                    }
                    else
                    {
                        ErrorLogger.LogError("Image " + imageAndGraphic.GraphicFileName + " not found for vehicleID: " + vehicleID + " and GroupNumber: " + groupNumber, "PartsImageNotFound");
                    }
                }
            }

            if (imageList.Count == 0)
            {
                imageList.Add(new ImageAndGraphic());
            }

            return imageList;
        }

        public JsonResult RotateImage(int userID, int loginID, int estimateID, int imageID, bool left)
        {
            CacheActiveLoginID(userID);

            // Make sure the passed LoginID is ok
            if (!IsUserAuthorized(userID))
            {
                return Json(@Proestimator.Resources.ProStrings.BadLogin, JsonRequestBehavior.AllowGet);
            }

            // Make sure the estimate belongs to the login
            Estimate estimate = new Estimate(estimateID);
            if (estimate.CreatedByLoginID != loginID)
            {
                return Json(@Proestimator.Resources.ProStrings.BadEstimate, JsonRequestBehavior.AllowGet);
            }

            EstimationImage image = EstimationImage.GetEstimationImage(imageID);
            if (image.AdminInfoID != estimate.EstimateID)
            {
                return Json(@Proestimator.Resources.ProStrings.ImageNotBelongToEstimate, JsonRequestBehavior.AllowGet);
            }

            StringBuilder errorBuilder = new StringBuilder();

            string fullPath = image.GetDiskPath(loginID, false);

            if (System.IO.File.Exists(fullPath))
            {
                Bitmap imageFull = LoadBitmap(fullPath);
                string fullError = RotateImage(loginID, imageFull, left, fullPath);
                if (string.IsNullOrEmpty(fullError))
                {
                    errorBuilder.AppendLine(fullError);

                    string thumbPath = image.GetDiskPath(loginID, true);
                    Bitmap imageSmall = LoadBitmap(thumbPath);
                    string thumbError = RotateImage(loginID, imageSmall, left, thumbPath);
                    if (!string.IsNullOrEmpty(thumbError))
                    {
                        errorBuilder.AppendLine(thumbError);
                    }
                }
            }

            string result = errorBuilder.ToString();
            if (string.IsNullOrEmpty(result) || result == "\r\n")
            {
                result = "good";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RotatePDFImage(int userID, int estimateID, int imageID, bool left, int selectedPageIndex)
        {
            CacheActiveLoginID(userID);

            // Make sure the passed LoginID is ok
            if (!IsUserAuthorized(userID))
            {
                return Json(@Proestimator.Resources.ProStrings.BadLogin, JsonRequestBehavior.AllowGet);
            }

            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey(), true);

            // Make sure the estimate belongs to the login
            Estimate estimate = new Estimate(estimateID);
            if (estimate.CreatedByLoginID != activeLogin.LoginID)
            {
                return Json(@Proestimator.Resources.ProStrings.BadEstimate, JsonRequestBehavior.AllowGet);
            }

            EstimationImage image = EstimationImage.GetEstimationImage(imageID);
            if (image.AdminInfoID != estimate.EstimateID)
            {
                return Json(@Proestimator.Resources.ProStrings.ImageNotBelongToEstimate, JsonRequestBehavior.AllowGet);
            }

            StringBuilder errorBuilder = new StringBuilder();

            string thumbPath = image.GetPDFImageDiskPath(activeLogin.LoginID, true, selectedPageIndex);

            string result = string.Empty;
            if (System.IO.File.Exists(thumbPath))
            {
                Bitmap imageSmall = LoadBitmap(thumbPath);
                string thumbError = RotateImage(activeLogin.LoginID, imageSmall, left, thumbPath);
                if (!string.IsNullOrEmpty(thumbError))
                {
                    errorBuilder.AppendLine(thumbError);
                }

                result = errorBuilder.ToString();
                if (string.IsNullOrEmpty(result) || result == "\r\n")
                {
                    result = "good";
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private Bitmap LoadBitmap(string path)
        {
            Bitmap bitmap;

            using (Stream stream = System.IO.File.OpenRead(path))
            {
                bitmap = System.Drawing.Bitmap.FromStream(stream) as Bitmap;
            }

            return bitmap;
        }

        private string RotateImage(int loginID, Bitmap image, bool left, string savePath)
        {
            try
            {
                if (left)
                {
                    image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                }
                else
                {
                    image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                }

                // Save the image
                System.IO.File.Delete(savePath);
                image.Save(savePath);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "RotateImage");
                return ex.Message;
            }

            return "";
        }

        #endregion

        #region Details

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/details")]
        public ActionResult Details(int userID, int estimateID)
        {
            // Make sure the estimate belongs to the login
            Estimate estimate = new Estimate(estimateID);
            if (estimate.CreatedByLoginID != ActiveLogin.LoginID)
            {
                return Redirect("/" + userID);
            }

            Estimate.RefreshProcessedLines(estimateID);

            // Make sure the estimate is attached to a customer
            if (estimate.CustomerID == 0)
            {
                return Redirect("/" + userID + "/estimate/" + estimate.EstimateID + "/customer-selection");
            }

            if (estimate.CustomerProfileID == 0)
            {
                Redirect("/" + userID + "/estimate/" + estimate.EstimateID + "/select-rate-profile");
            }

            DetailsVM model = new DetailsVM(_vendorRepository, _proAdvisorProfileService, estimate, ActiveLogin.HasProAdvisorContract, ActiveLogin.ID);
            model.LoginID = ActiveLogin.LoginID;

            LoginInfo loginInfo = LoginInfo.GetByID(ActiveLogin.LoginID);
            model.ShowRepairFacilities = loginInfo.ShowRepairShopProfiles;

            model.WhichButtonVisible = estimate.CheckCommitUncommit();

            ViewBag.NavID = "estimate";
            ViewBag.EstimateNavID = "details";

            ViewBag.EstimateID = estimateID;

            return View(model);
        }

        [HttpPost]
        [Route("{userID}/estimate/{estimateID}/change-job-status")]
        public ActionResult ChangeJobStatus(DetailsVM model)
        {
            Estimate estimate = new Estimate(model.EstimateID);

            int timezoneOffset = InputHelper.GetInteger(SiteSettings.Get(ActiveLogin.LoginID, "TimeZone", "ReportOptions", "0").ValueString);
            DateTime currentDate = DateTime.Now.AddHours(timezoneOffset);
            DateTime actionDate = InputHelper.GetDateTime(model.StatusDate, currentDate);

            // Add the time stamp if the date is today
            if (actionDate.Date == currentDate.Date)
            {
                actionDate = currentDate;
            }

            int selectedStatus = model.SelectedStatus;
            if (selectedStatus > 0)
            {
                estimate.UpdateStatus(ActiveLogin.ID, selectedStatus, actionDate, model.LoginID);
                model.SelectedReportHeader = estimate.ReportTextHeader;

                SuccessBoxFeatureLog.LogFeature(model.LoginID, SuccessBoxModule.EstimateWriting, "Changing the job status", ActiveLogin.ID);

                if (model.SelectedStatus == 3)
                {
                    SuccessBoxFeatureLog.LogFeature(model.LoginID, SuccessBoxModule.EstimateWriting, "Closed repair order", ActiveLogin.ID);
                }
            }

            return Redirect("/" + model.UserID + "/estimate/" + model.EstimateID + "/details");
        }

        [HttpPost]
        [Route("{userID}/estimate/{estimateID}/details")]
        public ActionResult Details(DetailsVM model)
        {
            Estimate estimate = new Estimate(model.EstimateID);

            estimate.Description = model.EstimateDescription;
            estimate.EstimateLocation = model.EstimateLocation;
            estimate.Note = model.EstimateNotes;
            estimate.PrintNote = model.IncludeNotesInReport;

            estimate.EstimatorID = model.EstimatorID;
            estimate.ReportTextHeader = string.IsNullOrEmpty(model.SelectedReportHeader) ? "Estimate" : model.SelectedReportHeader;

            estimate.EstimationDate = InputHelper.GetNullableDateTime(model.InspectionDate);
            estimate.AssignmentDate = InputHelper.GetNullableDateTime(model.AssignmentDate);

            estimate.RepairDays = model.ManualRepairDays ? InputHelper.GetInteger(model.DaysToRepair) : -1;
            estimate.HoursInDay = Convert.ToByte(model.HoursInDay);

            if (model.ShowRepairFacilities)
            {
                estimate.RepairFacilityVendorID = model.SelectedRepairFacility;
                estimate.FacilityRepairOrder = model.FacilityRepairOrder;
                estimate.FacilityRepairInvoice = model.FacilityRepairInvoice;
            }

            estimate.AlternateIdentitiesID = model.SelectedAlternateIdentity;

            estimate.PurchaseOrderNumber = model.PurchaseOrderNumber;

            // Keep any changes to CC fee or the flag in sync with the profile
            if (estimate.CreditCardFeePercentage != model.CreditCardFeePercentage ||
                estimate.ApplyCreditCardFee != model.ApplyCreditCardFee ||
                estimate.TaxedCreditCardFee != model.TaxedCreditCardFee)
            {
                // Update the rate profile
                var profile = RateProfile.Get(estimate.CustomerProfileID);
                if (profile != null)
                {
                    profile.CreditCardFeePercentage = model.CreditCardFeePercentage;
                    profile.ApplyCreditCardFee = model.ApplyCreditCardFee;
                    profile.TaxedCreditCardFee = model.TaxedCreditCardFee;
                    profile.Save(ActiveLogin.ID);
                }
            }

            estimate.CreditCardFeePercentage = model.CreditCardFeePercentage;
            estimate.ApplyCreditCardFee = model.ApplyCreditCardFee;
            estimate.TaxedCreditCardFee = model.TaxedCreditCardFee;

            bool updateTotals = false;

            // If the rate profile is different than the current profile's original profile, update the current rate profile.
            if (model.OriginalRateProfileID != model.RateProfileID)
            {
                RateProfileManager.DeleteProfile(estimate.CustomerProfileID);
                estimate.CustomerProfileID = RateProfileManager.CopyProfile(model.LoginID, model.RateProfileID, model.EstimateID);

                updateTotals = true;
            }

            estimate.AddOnProfileID = model.AddOnProfileID;

            PDR_EstimateData pdrEstimateData = PDR_EstimateData.GetForEstimate(model.EstimateID);
            if (pdrEstimateData != null)
            {
                PDR_RateProfile pdrRateProfile = PDR_RateProfile.GetByID(pdrEstimateData.RateProfileID);
                if (model.PDRRateProfileID != pdrRateProfile.OriginalID)
                {
                    List<PDR_Multiplier> oldMultipliers = PDR_Multiplier.GetByProfile(pdrRateProfile.ID);

                    PDR_Manager pdrManager = new PDR_Manager();
                    pdrManager.DeleteRateProfile(pdrEstimateData.RateProfileID, model.LoginID, ActiveLogin.ID);

                    PDR_RateProfileFunctionResult duplicateResult = pdrManager.DuplicateRateProfile(model.PDRRateProfileID, model.LoginID, ActiveLogin.ID);
                    if (duplicateResult.Success)
                    {
                        duplicateResult.RateProfile.AdminInfoID = model.EstimateID;
                        duplicateResult.RateProfile.Save(ActiveLogin.ID);

                        pdrEstimateData.RateProfileID = duplicateResult.RateProfile.ID;
                        pdrEstimateData.Save(ActiveLogin.ID);

                        // PDR Lines might have a multiplier link.  For each line, look for a multiplier with the same name in the new profile and update the link, otherwise delete the link.
                        List<PDR_Multiplier> newMultpiliers = PDR_Multiplier.GetByProfile(duplicateResult.RateProfile.ID);

                        List<PDR_EstimateDataPanel> allPanels = PDR_EstimateDataPanel.GetForEstimate(estimate.EstimateID);
                        List<PDR_EstimateDataPanelSupplementChange> allSupplements = PDR_EstimateDataPanelSupplementChange.GetForEstimate(estimate.EstimateID);

                        foreach (PDR_EstimateDataPanel panel in allPanels.Where(o => o.Multiplier > 0))
                        {
                            PDR_Multiplier oldMultiplier = oldMultipliers.FirstOrDefault(o => o.ID == panel.Multiplier);
                            if (oldMultiplier != null)
                            {
                                PDR_Multiplier newMatch = newMultpiliers.FirstOrDefault(o => o.Name == oldMultiplier.Name);
                                if (newMatch != null)
                                {
                                    panel.Multiplier = newMatch.ID;
                                }
                                else
                                {
                                    panel.Multiplier = 0;
                                }

                                panel.Save(ActiveLogin.ID);
                            }
                        }

                        foreach (PDR_EstimateDataPanelSupplementChange supPanel in allSupplements.Where(o => o.Multiplier > 0))
                        {
                            PDR_Multiplier oldMultiplier = oldMultipliers.FirstOrDefault(o => o.ID == supPanel.Multiplier);
                            if (oldMultiplier != null)
                            {
                                PDR_Multiplier newMatch = newMultpiliers.FirstOrDefault(o => o.Name == oldMultiplier.Name);
                                if (newMatch != null)
                                {
                                    supPanel.Multiplier = newMatch.ID;
                                }
                                else
                                {
                                    supPanel.Multiplier = 0;
                                }

                                supPanel.Save(ActiveLogin.ID, ActiveLogin.LoginID);
                            }
                        }

                        updateTotals = true;
                    }
                }
            }

            if (updateTotals || (model.CreditCardFeePercentage > 0 && model.ApplyCreditCardFee))
            {
                Estimate.RefreshProcessedLines(model.EstimateID);
            }

            estimate.Save(ActiveLogin.ID);

            return DoRedirect("details");
        }

        public async Task<JsonResult> CommitEstimate(int userID, int estimateID)
        {
            CommitEstimateResult result = new CommitEstimateResult();

            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                // Try commiting the estimate.  If any text is returned it is an error message.
                Estimate estimate = new Estimate(estimateID);

                string commitResult = estimate.CommitEstimate();

                if (string.IsNullOrEmpty(commitResult))
                {
                    estimate = new Estimate(estimateID);

                    // Save a PDF to have a hard copy record of the estimate before committing the supplement
                    ReportFunctionResult printResult = await _estimateService.MakeQuickPrintEstimateReport(userID, estimate.CreatedByLoginID, estimateID, GetActiveLoginID(userID));
                    if (printResult.Success)
                    {
                        string message = "Base estimate locked";
                        if (estimate.LockLevel > 1)
                        {
                            message = "Supplement " + (estimate.LockLevel - 1) + " locked";
                        }

                        printResult.Report.Notes = message;
                        printResult.Report.Save();
                    }

                    // Return the save message and current supplement version.
                    result.Message = ProStrings.Message_EstimateCommitted;
                    result.Success = true;
                    result.CurrentSupplementVersion = estimate.LockLevel;

                    SuccessBoxFeatureLog.LogFeature(estimate.CreatedByLoginID, SuccessBoxModule.EstimateWriting, "Creating a supplement on an estimate", GetActiveLoginID(userID));
                }
                else
                {
                    result.Message = commitResult;
                    result.Success = false;
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public class CommitEstimateResult : AjaxResult
        {
            public int CurrentSupplementVersion { get; set; }
        }

        public JsonResult UncommitEstimateSupplement(int userID, int estimateID)
        {
            CommitEstimateResult result = new CommitEstimateResult();

            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                Estimate estimate = new Estimate(estimateID);

                // Try commiting the estimate.  If any text is returned it is an error message.
                string commitResult = estimate.UncommitSupplements();

                if (string.IsNullOrEmpty(commitResult))
                {
                    // Return the save message and current supplement version.
                    result.Message = ProStrings.Message_EstimateSupplementUncommitted;
                    result.Success = true;

                    estimate = new Estimate(estimateID);
                    result.CurrentSupplementVersion = estimate.LockLevel;
                }
                else
                {
                    result.Message = commitResult;
                    result.Success = false;
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Payment Info

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/payment-info/{id?}")]
        public ActionResult PaymentInfo(int userID, int estimateID, int id = 0)
        {
            // Make sure the estimate belongs to the login
            Estimate estimate = new Estimate(estimateID);
            if (estimate.CreatedByLoginID != ActiveLogin.LoginID)
            {
                return Redirect("/" + userID);
            }

            // Make sure the estimate is attached to a customer
            if (estimate.CustomerID == 0)
            {
                return Redirect("/" + userID + "/estimate/" + estimate.EstimateID + "/customer-selection");
            }

            PaymentInfoVM vm = GetPaymentInfoVM(estimateID, id);

            vm.UserID = userID;
            vm.LoginID = ActiveLogin.LoginID;
            vm.EstimateID = estimateID;

            ViewBag.NavID = "estimate";
            ViewBag.EstimateNavID = "payment";

            ViewBag.EstimateID = estimateID;

            return View(vm);
        }

        private PaymentInfoVM GetPaymentInfoVM(int estimateID, int paymentID)
        {
            PaymentInfoVM vm = new PaymentInfoVM();

            // Add a list of all payments attached to this estimate.
            double total = 0;

            List<PaymentInfoData> allPayments = PaymentInfoData.GetAllPaymentInfo(estimateID);
            foreach (var info in allPayments)
            {
                PaymentInfoSummaryVM summaryVM = new PaymentInfoSummaryVM();
                summaryVM.Amount = String.Format("{0:C}", info.Amount);
                summaryVM.Memo = info.Memo;
                summaryVM.PayeeName = info.PayeeName;
                summaryVM.PaymentDate = InputHelper.GetDateTime(info.PaymentDate, DateTime.Now).ToShortDateString();
                summaryVM.PaymentId = info.PaymentId;
                summaryVM.CreditCardPaymentID = info.CreditCardPaymentID;

                vm.PaymentList.Add(summaryVM);

                total += (double)info.Amount;
            }

            Estimate estimate = new Estimate(estimateID);
            double grandTotal = InputHelper.GetDouble(estimate.GrandTotalString);

            vm.TotalPaid = String.Format("{0:C}", total);
            vm.EstimateTotal = String.Format("{0:C}", grandTotal);
            vm.TotalRemaining = String.Format("{0:C}", grandTotal - total);

            // If a payment ID was passed, load it.
            if (paymentID > 0)
            {
                PaymentInfoData paymentInfo = PaymentInfoData.GetPaymentInfo(paymentID);

                // Only show details if the payment ID is actually for the current estimate.
                if (paymentInfo != null && paymentInfo.AdminInfoID == estimateID)
                {
                    vm.PaymentId = paymentInfo.PaymentId;
                    vm.Amount = String.Format("{0:C}", paymentInfo.Amount);
                    vm.CheckNumber = paymentInfo.CheckNumber;
                    vm.Memo = paymentInfo.Memo;
                    vm.PayeeName = paymentInfo.PayeeName;
                    vm.PaymentDate = InputHelper.GetDateTime(paymentInfo.PaymentDate, DateTime.Now).ToShortDateString();
                    vm.PaymentType = paymentInfo.PaymentType;
                    vm.WhoPays = paymentInfo.WhoPays;
                    vm.CreditCardPaymentID = paymentInfo.CreditCardPaymentID;
                }
            }

            // Look for any names for the Who Pays list

            // Look for the customer
            Customer customer = ProEstimatorData.DataModel.Customer.Get(estimate.CustomerID);
            if (customer != null)
            {
                vm.PayeePresets.Add(new KeyValuePair<string, string>("CP", customer.Contact.FirstName + " " + customer.Contact.LastName));
            }

            // Look for the Insured and Claimant
            InsuranceInfo insuranceInfo = InsuranceInfo.Get(estimateID);

            if (insuranceInfo != null)
            {
                if (insuranceInfo.Insured != null && (!string.IsNullOrEmpty(insuranceInfo.Insured.FirstName) || !string.IsNullOrEmpty(insuranceInfo.Insured.LastName)))
                {
                    vm.PayeePresets.Add(new KeyValuePair<string, string>("I", insuranceInfo.Insured.FirstName + " " + insuranceInfo.Insured.LastName));
                }

                if (insuranceInfo.Claimant != null && (!string.IsNullOrEmpty(insuranceInfo.Claimant.FirstName) || !string.IsNullOrEmpty(insuranceInfo.Claimant.LastName)))
                {
                    vm.PayeePresets.Add(new KeyValuePair<string, string>("C", insuranceInfo.Claimant.FirstName + " " + insuranceInfo.Claimant.LastName));
                }

                if (!string.IsNullOrEmpty(insuranceInfo.InsuranceCompanyName))
                {
                    vm.PayeePresets.Add(new KeyValuePair<string, string>("IC", insuranceInfo.InsuranceCompanyName));
                }
            }

            // Set the API and key values for IntelliPay's Credit Card Payment system
            if (CreditCardPaymentService.IsAuthorized(ActiveLogin.LoginID))
            {
                var ccInfo = _creditCardPaymentService.GetMerchantCreditCardPaymentInfo(ActiveLogin.LoginID);
                if (!string.IsNullOrEmpty(ccInfo?.IntelliPayMerchantKey) && !string.IsNullOrEmpty(ccInfo?.IntelliPayAPIKey))
                {
                    vm.CreditCardPaymentLightBoxTerminalApi = ConfigurationManager.AppSettings.Get("CreditCardPaymentLightBoxTerminalApi");
                    vm.CreditCardPaymentUriParams = $"merchantkey={ccInfo.IntelliPayMerchantKey}&apikey={ccInfo.IntelliPayAPIKey}{(ccInfo.IntelliPayUseCardReader ? "&operatingenv=businessattended" : "")}";
                }
            }

            vm.EstimateIsLocked = estimate.IsLocked();

            return vm;
        }

        [Route("{userID}/estimate/{estimateID}/delete-payment/{id}")]
        public ActionResult DeletePayment(int id, int userID, int estimateID)
        {
            PaymentInfoData paymentInfo = PaymentInfoData.GetPaymentInfo(id);
            if (paymentInfo != null && paymentInfo.AdminInfoID == estimateID)
            {
                paymentInfo.Delete();
            }

            return Redirect("/" + userID + "/estimate/" + estimateID + "/payment-info");
        }

        [HttpPost]
        [Route("{userID}/estimate/{estimateID}/payment-info")]
        public ActionResult PaymentInfo(PaymentInfoVM vm)
        {
            var redirectResult = DoRedirect("");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            PaymentInfoData paymentInfo = PaymentInfoData.GetPaymentInfo(vm.PaymentId);

            if (paymentInfo == null)
            {
                paymentInfo = new PaymentInfoData();
            }

            paymentInfo.AdminInfoID = vm.EstimateID;
            paymentInfo.PaymentId = vm.PaymentId;
            paymentInfo.Amount = InputHelper.GetDecimal(vm.Amount);
            paymentInfo.CheckNumber = vm.CheckNumber;
            paymentInfo.Memo = vm.Memo;
            paymentInfo.PayeeName = vm.PayeeName;
            paymentInfo.PaymentDate = InputHelper.GetDateTime(vm.PaymentDate).ToString();
            paymentInfo.PaymentType = vm.PaymentType;
            paymentInfo.WhoPays = vm.WhoPays;

            SaveResult saveResult = paymentInfo.Save(GetActiveLoginID(vm.UserID));

            if (saveResult.Success)
            {
                return Redirect("/" + vm.UserID + "/estimate/" + vm.EstimateID + "/payment-info");
            }
            else
            {
                vm.SaveMessage = saveResult.ErrorMessage;
            }

            ViewBag.NavID = "estimate";
            ViewBag.EstimateNavID = "payment";

            ViewBag.EstimateID = vm.EstimateID;

            return View(vm);
        }

        #endregion

        #region VehicleTabDropdowns Jsons

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetYears()
        {
            var yearsData = new SelectList(VehicleSearchManager.GetYearSimpleListItemList());
            return Json(yearsData, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetAllMakes(int userID, int year)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                var MakesData = VehicleSearchManager.GetVehicleMakes(year);
                return Json(MakesData, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetAllModels(int userID, int year, int makeid)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                var ModelsData = VehicleSearchManager.GetVehicleModels(year, makeid);
                return Json(ModelsData, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetAllSubModels(int userID, int year, int makeid, int modelid)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                var subModelsData = LookupService.VehicleSubModels(year, makeid, modelid);
                return Json(subModelsData, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetBodyTypes(int userID, int year, int makeid, int modelid, int subModelID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                var bodyTypeData = LookupService.VehicleBodyTypes(year, makeid, modelid, subModelID);
                return Json(bodyTypeData, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetDriveTypes(int userID, int estimateID, int year, int makeid, int modelid, int subModelID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                var result = ProEstimatorData.DataModel.Vehicle.GetVehicleIDFromInfo(year, makeid, modelid, subModelID);
                if (result != null)
                {
                    var driveTypeData = LookupService.VehicleDriveTypes(result.VehicleID, estimateID);
                    return Json(driveTypeData, JsonRequestBehavior.AllowGet);
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetEngineTypes(int userID, int estimateID, int year, int makeid, int modelid, int subModelID, int bodyID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                var result = ProEstimatorData.DataModel.Vehicle.GetVehicleIDFromInfo(year, makeid, modelid, subModelID, bodyID);
                if (result != null)
                {
                    var engineTypeData = LookupService.VehicleEngineTypes(bodyID, result.VehicleID, estimateID);
                    return Json(engineTypeData, JsonRequestBehavior.AllowGet);
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }


        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetTransmissionTypes(int userID, int estimateID, int year, int makeid, int modelid, int subModelID, int bodyID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                var result = ProEstimatorData.DataModel.Vehicle.GetVehicleIDFromInfo(year, makeid, modelid, subModelID, bodyID);
                if (result != null)
                {
                    var transmissionTypeData = LookupService.VehicleTransmissionTypes(bodyID, result.VehicleID, estimateID);
                    return Json(transmissionTypeData, JsonRequestBehavior.AllowGet);
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetPaintCodes(int userID, int estimateID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                Vehicle vehicle = ProEstimatorData.DataModel.Vehicle.GetByEstimate(estimateID);
                var paintCodesData = LookupService.VehiclePaintCodes(vehicle.Year, vehicle.MakeID, vehicle.ModelID, vehicle.BodyID);
                return Json(paintCodesData, JsonRequestBehavior.AllowGet);
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetPaintCodesByData(int userID, int year, int make, int model, int body)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                var paintCodesData = LookupService.VehiclePaintCodes(year, make, model, body);
                return Json(paintCodesData, JsonRequestBehavior.AllowGet);
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckVehicleHasData(int userID, int year, int makeID, int modelID, int subModelID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                VehicleIDResult result = ProEstimatorData.DataModel.Vehicle.GetVehicleIDFromInfo(year, makeID, modelID, subModelID);
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        #endregion


        [HttpPost]
        public Boolean ReOrderEstimateLineItems(List<string> data, int estimateID)
        {
            try
            {
                List<ManualEntryListItem> list = ManualEntryHelper.getManualEntryList("LineItems", estimateID);

                // Ensure the number of items passed in are the same as that in the db
                if (list.Count != data.Count)
                    return false;

                int lineNumber = data.Count;
                foreach (var estimatorId in data)
                {
                    ManualEntryListItem manualEntryListItem = list.FirstOrDefault(x => x.ID == int.Parse(estimatorId));
                    if (manualEntryListItem != null)
                    {
                        manualEntryListItem.LineNumber = lineNumber--;
                        ManualEntryHelper.UpdateEstmateLineItemLineNumber(manualEntryListItem);
                    }
                }

                ProEstimatorData.DataModel.Estimate.RefreshProcessedLines(estimateID);

                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        /// <summary>Get summary information for the site header for the passed estimate ID, called by _Header</summary>
        public JsonResult GetEstimateHeaderInfo(int userID, int estimateID)
        {
            HeaderInfo headerInfo = new HeaderInfo();

            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                DBAccess db = new DBAccess();
                DBAccessTableResult tableResult = db.ExecuteWithTable("sp_GetHeaderInfo", new SqlParameter("AdminInfoID", estimateID));
                if (tableResult.Success)
                {
                    headerInfo.LoadData(tableResult.DataTable.Rows[0]);
                }
            }

            return Json(headerInfo, JsonRequestBehavior.AllowGet);
        }

        public class HeaderInfo
        {
            public int EstimateNumber { get; set; }
            public string EstimateTotal { get; set; }
            public string VehicleName { get; set; }
            public string Vin { get; set; }
            public string CustomerName { get; set; }
            public string StatusName { get; set; }
            public bool IsTotalLoss { get; set; }

            public HeaderInfo()
            {
                EstimateNumber = 0;
                EstimateTotal = "";
                VehicleName = "";
                Vin = "";
                CustomerName = "";
                StatusName = "Open";
                IsTotalLoss = false;
            }

            public void LoadData(DataRow row)
            {
                EstimateNumber = InputHelper.GetInteger(row["EstimateNumber"].ToString());
                double estimateTotal = InputHelper.GetDouble(row["GrandTotal"].ToString());
                EstimateTotal = Math.Round(estimateTotal, 2).ToString("C");
                VehicleName = row["Vehicle"].ToString();
                Vin = row["Vin"].ToString();
                CustomerName = row["Name"].ToString();

                // Put together the Status
                StatusName = row["JobStatusName"].ToString();
                if (string.IsNullOrEmpty(StatusName))
                {
                    StatusName = Proestimator.Resources.ProStrings.OpenEstimate_Filter_Open;
                }

                int statusID = InputHelper.GetInteger(row["JobStatusID"].ToString());
                if (statusID == 2)
                {
                    string workOrderNumber = row["WorkOrderNumber"].ToString();
                    if (!string.IsNullOrEmpty(workOrderNumber))
                    {
                        StatusName += " " + workOrderNumber;
                    }
                }

                // Figure out if it's a total loss
                IsTotalLoss = false;

                double totalLostPercent = InputHelper.GetDouble(row["TotalLossPercent"].ToString());
                double vehicleValue = InputHelper.GetDouble(row["VehicleValue"].ToString());

                if (totalLostPercent > 0 && vehicleValue > 0)
                {
                    if (estimateTotal > vehicleValue * (totalLostPercent / 100.0))
                    {
                        IsTotalLoss = true;
                    }
                }
            }
        }

        #region Copy Estimate

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/copy-estimate")]
        public ActionResult CopyEstimate(int userID, int estimateID)
        {
            // Make sure the estimate belongs to the login
            Estimate estimate = new Estimate(estimateID);
            if (estimate.CreatedByLoginID != ActiveLogin.LoginID)
            {
                return Redirect("/" + userID);
            }

            ViewBag.NavID = "estimate";
            ViewBag.EstimateNavID = "vehicle";
            ViewBag.EstimateID = estimateID;

            CopyEstimateVM vm = new CopyEstimateVM();
            vm.EstimateID = estimateID;
            vm.UserID = userID;
            vm.LoginID = ActiveLogin.LoginID;

            if (estimate == null || !IsUserAuthorized(userID))
            {
                vm.GoodEstimate = false;
            }
            else
            {
                vm.GoodEstimate = true;
                vm.CreateBlankCustomer = true;
                vm.CopyInsuranceInformation = true;
                vm.CopyClaimantInformation = true;
            }

            return View(vm);
        }

        [HttpPost]
        [Route("{userID}/estimate/{estimateID}/copy-estimate")]
        public ActionResult CopyEstimate(CopyEstimateVM vm)
        {
            DBAccess db = new DBAccess();

            Estimate estimate = new Estimate(vm.EstimateID);

            string newName = vm.NewName;
            if (string.IsNullOrEmpty(newName))
            {
                newName = estimate.Description + " copy";
            }

            // Make a copy of the estimate by calling the stored procedure
            int newID = _estimateService.CopyEstimate(ActiveLogin.ID, vm.EstimateID, newName, vm.CreateBlankCustomer, vm.CopyInsuranceInformation, vm.CopyClaimantInformation, true, vm.CopyAttachedImages, vm.CopyLineItems, vm.CopyLatestItemsOnly);

            // Make a new Rate Profile for the new estimate, copying from the original
            Estimate newEstimate = new Estimate(newID);
            newEstimate.CustomerProfileID = RateProfileManager.CopyProfile(vm.LoginID, estimate.CustomerProfileID, newID);
            newEstimate.Save(ActiveLogin.ID);

            if (vm.CopyAttachedImages)// Copy images
                CopyImages(vm.LoginID, vm.EstimateID, newID);

            // If the estimate has PDR attached to it, copy the PDR profile and the estimate's PDR data
            if (vm.CopyLineItems)
            {
                PDR_EstimateData pdrEstimateData = PDR_EstimateData.GetForEstimate(vm.EstimateID);
                if (pdrEstimateData != null)
                {
                    PDR_Manager pdrManager = new PDR_Manager();
                    PDR_RateProfileFunctionResult profileResult = pdrManager.DuplicateRateProfile(pdrEstimateData.RateProfileID, estimate.CreatedByLoginID, ActiveLogin.ID, "");
                    if (profileResult.Success)
                    {
                        if (profileResult.RateProfile != null)
                        {
                            profileResult.RateProfile.AdminInfoID = newID;
                            profileResult.RateProfile.Save();

                            List<SqlParameter> copyPDRParameters = new List<SqlParameter>();
                            copyPDRParameters.Add(new SqlParameter("AdminInfoID", estimate.EstimateID));
                            copyPDRParameters.Add(new SqlParameter("NewAdminInfoID", newID));
                            copyPDRParameters.Add(new SqlParameter("NewProfileID", profileResult.RateProfile.ID));

                            db.ExecuteNonQuery("CopyEstimate_PDR", copyPDRParameters);
                        }
                    }
                }
            }

            return Redirect("/" + vm.UserID + "/estimate/" + newID + "/customer");
        }

        #endregion

        #region Select Rate Profile

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/select-rate-profile")]
        public ActionResult SelectRateProfile(int userID, int estimateID)
        {
            // Make sure the estimate belongs to the login
            Estimate estimate = new Estimate(estimateID);
            if (estimate.CreatedByLoginID != ActiveLogin.LoginID)
            {
                return Redirect("/" + userID);
            }

            SelectRateProfileVM vm = new SelectRateProfileVM();

            vm.UserID = userID;
            vm.LoginID = ActiveLogin.LoginID;
            vm.EstimateID = estimateID;

            LoginInfo loginInfo = LoginInfo.GetByID(ActiveLogin.LoginID);
            vm.UseDefaultRateProfile = loginInfo.UseDefaultRateProfile;

            if (estimate.CustomerProfileID == 0)
            {
                //vm.ShowProfiles = true;

                List<RateProfile> profiles = RateProfile.GetAllForLogin(ActiveLogin.LoginID);
                foreach (RateProfile profile in profiles)
                {
                    RateProfileInfo info = new RateProfileInfo();
                    info.ID = profile.ID;
                    info.Description = profile.Name;
                    if (!string.IsNullOrEmpty(profile.Description))
                    {
                        info.Description += " - " + profile.Description;
                    }
                    vm.Profiles.Add(info);
                }
            }

            return View(vm);
        }

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/select-rate-profile/{profileID}")]
        public ActionResult SelectRateProfileID(int userID, int estimateID, int profileID)
        {
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
            int newProfileID = RateProfileManager.CopyProfile(activeLogin.LoginID, profileID, estimateID);
            if (estimateID > 0)
            {
                var estimate = new Estimate(estimateID);
                estimate.CustomerProfileID = newProfileID;
                estimate.Save(activeLogin.ID);
            }

            return Redirect("/" + userID + "/estimate/" + estimateID + "/customer");
        }

        /// <summary>
        /// Returns the rate profile list
        /// </summary>
        public ActionResult GetRateProfileList([DataSourceRequest] DataSourceRequest request, int userID, int estimateID)
        {
            List<RateProfileInfo> rateProfileInfoList = new List<RateProfileInfo>();

            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
            List<RateProfile> profiles = RateProfile.GetAllForLogin(activeLogin.LoginID);
            foreach (RateProfile profile in profiles)
            {
                RateProfileInfo info = new RateProfileInfo();
                info.ID = profile.ID;
                info.Description = profile.Name;
                info.IsDefault = profile.IsDefault;
                if (!string.IsNullOrEmpty(profile.Description))
                {
                    info.Description += " - " + profile.Description;
                }
                rateProfileInfoList.Add(info);
            }

            return Json(rateProfileInfoList.ToDataSourceResult(request));
        }

        #endregion

        #region Select Estimator
        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/select-estimator")]
        public ActionResult SelectEstimator(int userID, int estimateID)
        {
            // Make sure the estimate belongs to the login
            Estimate estimate = new Estimate(estimateID);
            if (estimate.CreatedByLoginID != ActiveLogin.LoginID)
            {
                return Redirect("/" + userID);
            }
            var vm = new SelectEstimatorVM();
            vm.UserID = userID;
            vm.LoginID = ActiveLogin.LoginID;
            vm.EstimateID = estimateID;
            if (estimate.EstimatorID == 0)
            {
                var estimators = Estimator.GetByLogin(ActiveLogin.LoginID);
                foreach (var estimator in estimators)
                {
                    var info = new EstimatorInfo();
                    info.ID = estimator.ID;
                    if ($"{estimator.FirstName} {estimator.LastName}".Length == 1)
                    {
                        info.Description = "(Estimator Name not set)";
                    }
                    else
                    {
                        info.Description = $"{estimator.FirstName} {estimator.LastName}";
                    }
                    vm.Estimators.Add(info);
                }
            }
            return View(vm);
        }
        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/select-estimator/{estimatorID}")]
        public ActionResult SelectEstimatorID(int userID, int estimateID, int estimatorID)
        {
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
            Estimate estimate = new Estimate(estimateID);
            estimate.EstimatorID = estimatorID;
            estimate.Save(activeLogin.ID);
            return Redirect("/" + userID + "/estimate/" + estimateID + "/customer");
        }
        #endregion

        #region Select Add On Profile

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/select-add-on-profile")]
        public ActionResult SelectAddOnProfile(int userID, int estimateID)
        {
            // Make sure the estimate belongs to the login
            Estimate estimate = new Estimate(estimateID);
            if (estimate.CreatedByLoginID != ActiveLogin.LoginID)
            {
                return Redirect("/" + ActiveLogin.LoginID);
            }

            SelectRateProfileVM vm = new SelectRateProfileVM();

            vm.LoginID = ActiveLogin.LoginID;
            vm.EstimateID = estimateID;

            vm.Profiles.Add(new RateProfileInfo() { ID = 1, Description = "System Default" });

            List<ProAdvisorPresetProfile> addOnProfiles = _proAdvisorProfileService.GetAllProfilesForLogin(ActiveLogin.LoginID, false);
            foreach (ProAdvisorPresetProfile profile in addOnProfiles)
            {
                RateProfileInfo info = new RateProfileInfo();
                info.ID = profile.ID;
                info.Description = profile.Name;
                vm.Profiles.Add(info);
            }

            ViewBag.EstimateID = estimateID;

            return View(vm);
        }

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/select-add-on-profile/{profileID}")]
        public ActionResult SelectAddOnProfileID(int userID, int estimateID, int profileID)
        {
            Estimate estimate = new Estimate(estimateID);
            if (estimate != null && estimate.CreatedByLoginID == ActiveLogin.LoginID)
            {
                estimate.AddOnProfileID = profileID;
                estimate.Save(ActiveLogin.LoginID);
            }

            return Redirect("/" + userID + "/estimate/" + estimateID + "/customer");
        }

        /// <summary>
        /// Returns the add on profile list
        /// </summary>
        public ActionResult GetAddOnProfileList([DataSourceRequest] DataSourceRequest request, int userID, int estimateID)
        {
            // Make sure the estimate belongs to the login
            Estimate estimate = new Estimate(estimateID);
            ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

            if (estimate.CreatedByLoginID != activeLogin.LoginID)
            {
                return Redirect("/" + activeLogin.LoginID);
            }

            SelectRateProfileVM vm = new SelectRateProfileVM();

            vm.LoginID = activeLogin.LoginID;
            vm.EstimateID = estimateID;
            vm.UseDefaultAddOnProfile = _proAdvisorProfileService.UseDefaultProfile(activeLogin.LoginID);

            vm.Profiles.Add(new RateProfileInfo() { ID = 1, Description = "System Default", IsDefault = true });

            List<ProAdvisorPresetProfile> addOnProfiles = _proAdvisorProfileService.GetAllProfilesForLogin(activeLogin.LoginID, false);
            foreach (ProAdvisorPresetProfile profile in addOnProfiles)
            {
                RateProfileInfo info = new RateProfileInfo();
                info.ID = profile.ID;
                info.Description = profile.Name;
                info.IsDefault = profile.DefaultFlag;
                if (profile.DefaultFlag)
                {
                    vm.Profiles[0].IsDefault = false;
                }
                vm.Profiles.Add(info);
            }

            return Json(vm.Profiles.ToDataSourceResult(request));
        }

        #endregion

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/edit-rate-profile")]
        public ActionResult EditRateProfile(int userID, int estimateID)
        {
            Estimate estimate = new Estimate(estimateID);

            return Redirect("/" + userID + "/rates/" + estimate.CustomerProfileID);
        }

        #region VIN Decode JSON

        public async Task<JsonResult> PlateStateDecode(int userID, string plate, string state, int estimateID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                var result = await carfaxService.GetInfoByPlateAndState(plate, state);
                VINDecodeResult vinResult = GetVehicleInfo(estimateID, result, "");
                if (vinResult != null)
                {
                    return Json(vinResult, JsonRequestBehavior.AllowGet);
                }

            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> VinDecode(int userID, int estimateID, string vin)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                vin = vin.Trim();
                vin = Regex.Replace(vin, @"[^a-zA-Z0-9]", string.Empty);
                var result = await carfaxService.GetInfoByVin(vin);
                if (!valid(result) && result.Errormessages == "")
                {
                    VINDecodeResult decodeResult = new VINDecodeResult();
                    decodeResult.ErrorMessages = Proestimator.Resources.ProStrings.VinSearch_InvalidVin;
                    return Json(decodeResult, JsonRequestBehavior.AllowGet);
                }
                VINDecodeResult vinResult = GetVehicleInfo(estimateID, result, vin);
                if (vinResult != null)
                {
                    return Json(vinResult, JsonRequestBehavior.AllowGet);
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        private bool valid(Carfaxresponse result)
        {
            if (result == null || result.Quickvinplus == null || result.Quickvinplus.Vininfo.Carfaxvindecode == null || result.Quickvinplus.Vininfo.Carfaxvindecode.Trim.Count == 0)
            {
                return false;
            }
            return true;
        }

        private VINDecodeResult GetVehicleInfo(int estimateID, Carfaxresponse result, string reqVin)
        {
            if (result == null)
            {
                return null;
            }

            VINDecodeResult vinResult = new VINDecodeResult();
            vinResult.ErrorMessages = result.Errormessages;
            if (result.Errormessages != "")
            {
                List<VehicleInfo> similarVinInfo = new List<VehicleInfo>();
                if (reqVin != "")
                {
                    List<string> allVins = CarfaxInfo.GetAllVin();
                    List<string> similarVins = InputHelper.SearchOneLetterDifferent(reqVin, allVins);
                    foreach (string vin in similarVins)
                    {
                        CarfaxInfo vinInfo = CarfaxInfo.GetByVin(vin);
                        if (vinInfo != null)
                        {
                            using (var str = new StringReader(vinInfo.VinInfo))
                            {
                                var xmlSerializer = new XmlSerializer(typeof(Carfaxresponse));
                                Carfaxresponse response = (Carfaxresponse)xmlSerializer.Deserialize(str);
                                if (valid(response))
                                {
                                    Trim vinTrim = response.Quickvinplus.Vininfo.Carfaxvindecode.Trim[0];
                                    string info = vinTrim.Baseyearmodel + " " + vinTrim.Basemakename + " " + vinTrim.Baseseriesname;
                                    similarVinInfo.Add(new VehicleInfo(vin, info));
                                }
                            }
                        }
                    }
                }
                vinResult.SimilarVins = similarVinInfo;
                return vinResult;
            }


            Trim trim = result.Quickvinplus.Vininfo.Carfaxvindecode?.Trim[0];

            if (trim == null)
                return null;

            string subModelDb = result.Quickvinplus.Vininfo.Carfaxvindecode.Trim.Count == 1 ? trim.Submodel : "";
            string general = trim.Baseyearmodel + " " + trim.Basemakename + " " + trim.Baseseriesname;
            VehicleInfo vehicleInfo = new VehicleInfo(result.Quickvinplus.Vininfo.Vin, general, trim.Oemfullbodystyle,
                trim.Oemengineinformation, trim.Nonoemdrive, trim.Oemcountryoforigin, trim.Oemtiresize, trim.Oemtransmissiontype);
            vinResult.VehicleInfo = vehicleInfo;

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("Vin", result.Quickvinplus.Vininfo.Vin));
            parameters.Add(new SqlParameter("Year", InputHelper.GetInteger(trim.Baseyearmodel)));
            parameters.Add(new SqlParameter("Make", trim.Basemakename));
            parameters.Add(new SqlParameter("Model", trim.Baseseriesname));
            parameters.Add(new SqlParameter("SubModel", subModelDb));
            parameters.Add(new SqlParameter("BodyCode", trim.Oembodytype));
            parameters.Add(new SqlParameter("Body", trim.Oemfullbodystyle));
            parameters.Add(new SqlParameter("Engine", trim.Oemengineinformation));
            parameters.Add(new SqlParameter("TransmissionCode", trim.Oemtransmission));
            parameters.Add(new SqlParameter("Transmission", trim.Oemtransmissiontype));
            parameters.Add(new SqlParameter("Drive", trim.Nonoemdrive));
            parameters.Add(new SqlParameter("AdminInfoID", estimateID));

            DBAccess db = new DBAccess();
            DBAccessDataSetResult dbResult = db.ExecuteWithDataSet("GetVehicleInfoInVinn", parameters);
            if (dbResult.Success)
            {
                DataTable dtIDs = dbResult.DataSet.Tables[0];
                DataTable dtYears = dbResult.DataSet.Tables[1];
                DataTable dtMakes = dbResult.DataSet.Tables[2];
                DataTable dtModels = dbResult.DataSet.Tables[3];
                DataTable dtSubModels = dbResult.DataSet.Tables[4];
                DataTable dtBodyTypes = dbResult.DataSet.Tables[5];
                DataTable dtEngineTypes = dbResult.DataSet.Tables[6];
                DataTable dtTransmissionTypes = dbResult.DataSet.Tables[7];
                DataTable dtDriveTypes = dbResult.DataSet.Tables[8];

                vinResult.Success = true;
                vinResult.VehicleInfo.VehicleID = dtIDs.Rows[0]["VehicleID"].ToString();
                vinResult.Year = InputHelper.GetInteger(dtIDs.Rows[0]["Year"].ToString());
                vinResult.MakeID = InputHelper.GetInteger(dtIDs.Rows[0]["MakeID"].ToString());
                vinResult.ModelID = InputHelper.GetInteger(dtIDs.Rows[0]["ModelID"].ToString());
                vinResult.SubModelID = InputHelper.GetInteger(dtIDs.Rows[0]["SubModelID"].ToString());
                vinResult.BodyID = InputHelper.GetInteger(dtIDs.Rows[0]["BodyID"].ToString());
                vinResult.EngineID = InputHelper.GetInteger(dtIDs.Rows[0]["EngineID"].ToString());
                vinResult.TransmissionID = InputHelper.GetInteger(dtIDs.Rows[0]["TransmissionID"].ToString());
                vinResult.DriveID = InputHelper.GetInteger(dtIDs.Rows[0]["DriveID"].ToString());

                List<string> years = new List<string>();
                List<MakeDropDown> makes = new List<MakeDropDown>();
                List<ModelDropDown> models = new List<ModelDropDown>();
                List<SubModelDropDown> submodels = new List<SubModelDropDown>();
                List<BodyTypeDropDown> bodytypes = new List<BodyTypeDropDown>();
                List<EngineTypeDropDown> engines = new List<EngineTypeDropDown>();
                List<TransmissionTypeDropDown> transmissions = new List<TransmissionTypeDropDown>();
                List<DriveTypeDropDown> drives = new List<DriveTypeDropDown>();

                foreach (DataRow dr in dtYears.Rows)
                {
                    string year = dr[0].ToString();
                    years.Add(year);
                }

                foreach (DataRow dr in dtMakes.Rows)
                {
                    MakeDropDown make = new MakeDropDown();
                    make.MakeID = InputHelper.GetInteger(dr[0].ToString());
                    make.Make = dr[1].ToString();
                    makes.Add(make);
                }

                foreach (DataRow dr in dtModels.Rows)
                {
                    ModelDropDown model = new ModelDropDown();
                    model.ModelID = InputHelper.GetInteger(dr[0].ToString());
                    model.Model = dr[1].ToString();
                    models.Add(model);
                }

                foreach (DataRow dr in dtSubModels.Rows)
                {
                    SubModelDropDown submodel = new SubModelDropDown();
                    submodel.SubModelID = InputHelper.GetInteger(dr[0].ToString());
                    submodel.SubModel = dr[1].ToString();
                    submodels.Add(submodel);
                }

                foreach (DataRow dr in dtBodyTypes.Rows)
                {
                    BodyTypeDropDown bodytype = new BodyTypeDropDown();
                    bodytype.BodyID = InputHelper.GetInteger(dr[0].ToString());
                    bodytype.Body = dr[1].ToString();
                    bodytypes.Add(bodytype);
                }

                foreach (DataRow dr in dtEngineTypes.Rows)
                {
                    EngineTypeDropDown enginetype = new EngineTypeDropDown();
                    enginetype.EngineID = InputHelper.GetInteger(dr[0].ToString());
                    enginetype.Engine = dr[1].ToString();
                    engines.Add(enginetype);
                }

                foreach (DataRow dr in dtTransmissionTypes.Rows)
                {
                    TransmissionTypeDropDown transmission = new TransmissionTypeDropDown();
                    transmission.TransmissionID = InputHelper.GetInteger(dr[0].ToString());
                    transmission.Transmission = dr[1].ToString();
                    transmissions.Add(transmission);
                }

                foreach (DataRow dr in dtDriveTypes.Rows)
                {
                    DriveTypeDropDown drive = new DriveTypeDropDown();
                    drive.DriveID = InputHelper.GetInteger(dr[0].ToString());
                    drive.Drive = dr[1].ToString();
                    drives.Add(drive);
                }

                vinResult.Years = years;
                vinResult.Makes = makes;
                vinResult.Models = models;
                vinResult.SubModels = submodels;
                vinResult.BodyTypes = bodytypes;
                vinResult.EngineTypes = engines;
                vinResult.TransmissionTypes = transmissions;
                vinResult.DriveTypes = drives;
            }
            return vinResult;
        }

        public JsonResult GetCarfaxInfo([DataSourceRequest] DataSourceRequest request, int userID, string vin)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                CarfaxInfo carfaxInfo = CarfaxInfo.GetByVin(vin);
                if (carfaxInfo != null)
                {
                    using (var str = new StringReader(carfaxInfo.VinInfo))
                    {
                        var xmlSerializer = new XmlSerializer(typeof(Carfaxresponse));
                        Carfaxresponse response = (Carfaxresponse)xmlSerializer.Deserialize(str);
                        if (!valid(response))
                        {
                            return Json(new VehicleFunctionResult(Proestimator.Resources.ProStrings.VinSearch_InvalidVin), JsonRequestBehavior.AllowGet);
                        }
                    }
                    List<VehicleInfoTypeData> vinInfoList = carfaxService.GetExistingInfo(carfaxInfo.VinInfo);
                    if (vinInfoList.Count > 0)
                        return Json(new VehicleFunctionResult(vinInfoList), JsonRequestBehavior.AllowGet);
                }

                return Json(new VehicleFunctionResult("No Info"), JsonRequestBehavior.AllowGet);
            }
            return Json(new VehicleFunctionResult("Not Authorized"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult VinClick(int userID, int estimateID, string vin)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                CarfaxInfo vinInfo = CarfaxInfo.GetByVin(vin);
                if (vinInfo != null)
                {
                    using (var str = new StringReader(vinInfo.VinInfo))
                    {
                        var xmlSerializer = new XmlSerializer(typeof(Carfaxresponse));
                        Carfaxresponse response = (Carfaxresponse)xmlSerializer.Deserialize(str);
                        VINDecodeResult vinResult = GetVehicleInfo(estimateID, response, "");
                        if (vinResult != null)
                        {
                            return Json(vinResult, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Functions

        public List<SelectListItem> GetExistingCustomers(int loginID)
        {
            List<SelectListItem> returnList = new List<SelectListItem>();

            returnList.Add(new SelectListItem() { Value = "-1", Text = "-----" + ProStrings.SelectExistingCustomer + "-----" });

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("sp_GetExistingCustomerName", new SqlParameter("LoginID", loginID));

            if (tableResult.Success)
            {
                List<string> usedNames = new List<string>();

                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    string name = row["firstName"].ToString();

                    if (!usedNames.Contains(name))
                    {
                        returnList.Add(new SelectListItem() { Value = row["id"].ToString(), Text = name });

                        usedNames.Add(name);
                    }
                }
            }

            return returnList;
        }

        public List<SelectListItem> GetInsuranceCompanies(int loginID)
        {
            List<SelectListItem> returnList = new List<SelectListItem>();

            returnList.Add(new SelectListItem() { Value = "-1", Text = "-----" + ProStrings.SelectInsuranceCompanies + "-----" });

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("SP_getInsuranceCompanies", new SqlParameter("LoginsID", loginID));

            if (tableResult.Success)
            {
                List<string> usedNames = new List<string>();

                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    string name = row["InsuranceCompanyName"].ToString();

                    if (!usedNames.Contains(name))
                    {
                        returnList.Add(new SelectListItem() { Value = row["AdminInfoID"].ToString() + "|EstimateId", Text = name });

                        usedNames.Add(name);
                    }
                }
            }

            List<Vendor> vendors = _vendorRepository.GetAllForType(loginID, VendorType.InsuranceProfile);
            foreach (Vendor vendor in vendors)
            {
                returnList.Add(new SelectListItem() { Value = vendor.ID + "|VendorId", Text = vendor.CompanyName });
            }

            return returnList;
        }

        #endregion


        [HttpPost]
        public JsonResult GetSentSMSStatusforReport(int reportId)
        {
            var sentStatuses = SentSmsStaus.GetAllSMS(reportId, false);
            StringBuilder builder = new StringBuilder();
            if (sentStatuses.Any())
            {
                foreach (var sms in sentStatuses)
                {
                    var smsresponse = Twilio.Rest.Api.V2010.Account.MessageResource.FromJson(sms.ResponseResource);
                    builder.AppendLine(smsresponse.To + "  \t\t\t" + sms.Status + " " + sms.ErrorMessage);
                }
            }

            return Json(builder.ToString(), JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<SelectListItem> GetStates()
        {
            var GetStatesData = State.StatesList.Select(s => (s.Code != "") ? new SelectListItem()
            {
                Text = s.Description,
                Value = s.Code
            } : new SelectListItem()
            {
                Selected = true,
                Text = "-----Select State-----",
                Value = s.Code
            });

            return GetStatesData;
        }

        [HttpGet]
        //[Route("{userID}/estimate/{estimateID}/copy-estimate")]
        [Route("rscn/{userID}/{ticketID}")]
        public ActionResult RideShareCollisionNetworkCreate(int userID, int ticketID)
        {
            string errorMessage = "";
            int newEstimateID = 0;

            StringBuilder errorBuilder = new StringBuilder();

            LoginInfo loginInfo = LoginInfo.GetByID(ActiveLogin.LoginID);
            if (loginInfo == null)
            {
                errorMessage = "Account not found for ID " + ActiveLogin.LoginID;
                errorBuilder.AppendLine("Account not found for ID " + ActiveLogin.LoginID);
            }
            else
            {
                try
                {
                    // First check to see if we've already created an estimate for this ticket ID
                    int estimateID = GetEstimateIDForRcnTicket(ActiveLogin.LoginID, ticketID);
                    if (estimateID > 0)
                    {
                        return Redirect("/" + userID + "/estimate/" + estimateID + "/customer");
                    }

                    // Get data from RCN to create the estimate
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ridesharecollisionnetwork.com/v1/estimate/webest?webEstId=" + ActiveLogin.LoginID + "&ticketId=" + ticketID);
                    request.ContentType = "application/json";
                    request.Accept = "*/*";
                    request.Method = "POST";
                    request.Headers.Add("Authorization", "Bearer QfD_esWTrwxWEg8HePAQtY-utp578g8W7j9cnER4");

                    var httpResponse = (HttpWebResponse)request.GetResponse();
                    string result = "";

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        result = streamReader.ReadToEnd();
                    }

                    // string result = "{\"created_at\":1569875998,\"id\":45,\"name\":\"Bob Maclean\",\"phone\":\"9163904467\",\"email\":\"raidersy2k@yahoo.com\",\"shop_id\":1,\"license_plate\":\"3MQA120\",\"license_plate_state\":\"CA\",\"make\":\"Toyota\",\"model\":\"Camry\",\"year\":\"1995\",\"address\":\"4009 Navajo Dr\",\"city\":\"Antelope\",\"state\":\"CA\",\"zip\":\"95843\",\"claim_adjuster_name\":\"Private Pay\",\"claim_adjuster_phone\":\"\",\"claim_number\":\"\",\"image_0\":\"http://ridesharecollisionnetwork.com/images/2/uploads/45/headlights.jpg\"}";
                    ErrorLogger.LogError(result, "RideShare Link Data");
                    Dictionary<string, string> values = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

                    // Create the estimate
                    Estimate estimate = _estimateService.CreateNewEstimate(ActiveLogin);

                    newEstimateID = estimate.EstimateID;

                    InsertRcnTicketLink(ActiveLogin.LoginID, ticketID, newEstimateID);

                    // Update the customer info
                    // The customer record might not exist yet
                    ProEstimatorData.DataModel.Customer customer;

                    if (estimate.CustomerID == 0)
                    {
                        customer = new Customer();
                        customer.LoginID = estimate.CreatedByLoginID;
                    }
                    else
                    {
                        customer = ProEstimatorData.DataModel.Customer.Get(estimate.CustomerID);
                    }

                    string[] namePieces = GetValue(values, "name").Split(" ".ToCharArray());

                    if (namePieces.Length > 0)
                    {
                        customer.Contact.FirstName = namePieces[0];
                    }
                    if (namePieces.Length > 1)
                    {
                        customer.Contact.LastName = namePieces[1];
                    }

                    customer.Contact.Phone = GetValue(values, "phone");
                    customer.Contact.Email = GetValue(values, "email");

                    customer.Address.Line1 = GetValue(values, "address");
                    customer.Address.City = GetValue(values, "city");
                    customer.Address.State = GetValue(values, "state");
                    customer.Address.Zip = GetValue(values, "zip");

                    SaveResult customerSave = customer.Save(ActiveLogin.ID);
                    if (!customerSave.Success)
                    {
                        errorBuilder.AppendLine("Error saving customer info: " + customerSave.ErrorMessage);
                    }
                    else
                    {
                        estimate.CustomerID = customer.ID;
                        estimate.Save(ActiveLogin.ID);
                    }

                    // Save vehicle data
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter("Make", GetValue(values, "make")));
                    parameters.Add(new SqlParameter("Model", GetValue(values, "model")));

                    DBAccess db = new DBAccess();
                    DBAccessTableResult makeModelResult = db.ExecuteWithTable("GetMakeAndModelFromNames", parameters);

                    Vehicle vehicle = ProEstimatorData.DataModel.Vehicle.GetByEstimate(estimate.EstimateID);

                    vehicle.Year = InputHelper.GetInteger(GetValue(values, "year"));

                    if (makeModelResult.Success)
                    {
                        vehicle.MakeID = InputHelper.GetInteger(makeModelResult.DataTable.Rows[0]["MakeID"].ToString());
                        vehicle.ModelID = InputHelper.GetInteger(makeModelResult.DataTable.Rows[0]["ModelID"].ToString());
                    }
                    else
                    {
                        errorBuilder.AppendLine("Make and model not found: " + makeModelResult.ErrorMessage);
                    }

                    var vehicleIDInfo = ProEstimatorData.DataModel.Vehicle.GetVehicleIDFromInfo(vehicle.Year, vehicle.MakeID, vehicle.ModelID);
                    if (vehicleIDInfo != null)
                    {
                        vehicle.VehicleID = vehicleIDInfo.VehicleID;
                        vehicle.ServiceBarcode = vehicleIDInfo.ServiceBarcode;
                    }

                    SaveResult vehicleSaveResult = vehicle.Save();
                    if (!vehicleSaveResult.Success)
                    {
                        errorBuilder.AppendLine("Error saving vehicle: " + vehicleSaveResult.ErrorMessage);
                    }

                    // Save insurance info
                    InsuranceInfo insuranceInfo = InsuranceInfo.Get(estimate.EstimateID);
                    insuranceInfo.ClaimRep.FirstName = GetValue(values, "claim_adjuster_name");
                    insuranceInfo.ClaimRep.Phone = GetValue(values, "claim_adjuster_phone");
                    insuranceInfo.ClaimNumber = GetValue(values, "claim_number");

                    insuranceInfo.Save(ActiveLogin.ID, estimate.EstimateID, ActiveLogin.LoginID);

                    // Download images
                    for (int i = 0; i < 10; i++)
                    {
                        string imagePath = GetValue(values, "image_" + i);
                        if (!string.IsNullOrEmpty(imagePath))
                        {
                            FunctionResult imageSaveResult = SaveImageFromUrl(imagePath, ActiveLogin.LoginID, newEstimateID);
                            if (!imageSaveResult.Success)
                            {
                                errorBuilder.AppendLine("Error saving image '" + imagePath + "': " + imageSaveResult.ErrorMessage);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    return Redirect("/" + userID + "/estimate/" + newEstimateID + "/customer");
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    ErrorLogger.LogError(ex, 0, "RideShareCollisionNetworkCreate");
                }
            }

            string internalError = errorBuilder.ToString();
            if (!string.IsNullOrEmpty(internalError))
            {
                ErrorLogger.LogError(internalError, "RideShare Link Error");
            }

            //            // Return an answer as json
            //            string response =
            //@"{
            //    ""success"": " + success.ToString().ToLower() + @",
            //    ""error_message"": """ + errorMessage + @""", 
            //    ""estimate_id"": " + newEstimateID.ToString() + @"
            //}";
            //            ViewBag.Json = response;

            ViewBag.ErrorMessage = errorMessage;

            return View();
        }

        private int GetEstimateIDForRcnTicket(int loginID, int ticketID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("TicketID", ticketID));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("RcnTicketLink_Get", parameters);
            if (result.Success)
            {
                return result.Value;
            }

            return 0;
        }

        private void InsertRcnTicketLink(int loginID, int ticketID, int estimateID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("TicketID", ticketID));
            parameters.Add(new SqlParameter("EstimateID", estimateID));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("RcnTicketLink_Insert", parameters);
        }

        private FunctionResult SaveImageFromUrl(string url, int loginID, int estimateID)
        {
            try
            {
                int lastSlash = url.LastIndexOf("/");

                string namePart = url.Substring(lastSlash + 1, url.Length - lastSlash - 1);
                string[] pieces = namePart.Split(".".ToCharArray());

                string imageName = pieces[0];
                string extension = pieces[1].ToLower();

                EstimationImage newImage = new EstimationImage();
                newImage.AdminInfoID = estimateID;
                newImage.FileName = imageName;
                newImage.FileType = extension;

                SaveResult saveResult = newImage.Save();
                if (saveResult.Success)
                {
                    WebClient client = new WebClient();
                    Stream stream = client.OpenRead(url);
                    Bitmap bitmap = new Bitmap(stream);

                    if (bitmap != null)
                    {
                        string imageFolder = Path.Combine(GetEstimateDiskPath(loginID, estimateID), "Images");
                        Directory.CreateDirectory(imageFolder);

                        Image imageLarge = EstimateService.ScaleImage(bitmap, 2000, 2000);
                        Image imageSmall = EstimateService.ScaleImage(bitmap, 600, 800);

                        imageLarge.Save(Path.Combine(imageFolder, newImage.ID.ToString() + "." + extension));
                        imageSmall.Save(Path.Combine(imageFolder, newImage.ID.ToString() + "_thumb." + extension));

                        return new FunctionResult();
                    }
                    else
                    {
                        return new FunctionResult("Image could not be downloaded.");
                    }
                }
                else
                {
                    return new FunctionResult(saveResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                return new FunctionResult(ex.Message);
            }
        }

        private string GetValue(Dictionary<string, string> values, string key)
        {
            if (values.ContainsKey(key))
            {
                return values[key];
            }

            return "";
        }

        public JsonResult GetEstimateGridShowHideColumnInfo(int userID, string gridControlID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                List<GridMappingVM> results = new List<GridMappingVM>();

                List<GridColumnInfoUserMapping> mappings = GridColumnInfoUserMapping.GetForUserControlID(userID, gridControlID);

                SiteActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, ComputerKey);

                foreach (GridColumnInfoUserMapping mapping in mappings)
                {
                    results.Add(new GridMappingVM(mapping, activeLogin.LanguagePreference));
                }

                return Json(new { Success = true, ResultObject = results }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Success = false, ResultObject = "" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveEstimateGridShowHideColumnInfo(int userID, string gridControlID, List<GridMappingVM> gridMappingVMList)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                List<GridColumnInfoUserMapping> mappings = GridColumnInfoUserMapping.GetForUserControlID(userID, gridControlID);

                foreach (GridColumnInfoUserMapping mapping in mappings)
                {
                    GridMappingVM vm = gridMappingVMList.FirstOrDefault(o => o.GridColumnID == mapping.GridColumnInfo.ID);
                    if (vm != null)
                    {
                        mapping.Visible = vm.Visible;
                        mapping.Save();
                    }
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult AcceptAddOns(int userID, int loginID, int estimateID, string idList, int parentLineID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                StringBuilder errorBuilder = new StringBuilder();

                string[] pieces = idList.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (string piece in pieces)
                {
                    int presetID = InputHelper.GetInteger(piece);
                    if (presetID > 0)
                    {
                        FunctionResult addResult = _proAdvisorService.AddPresetToEstimate(estimateID, presetID, parentLineID);
                        if (!addResult.Success)
                        {
                            errorBuilder.AppendLine(addResult.ErrorMessage);
                        }
                    }
                }

                Estimate.RefreshProcessedLines(estimateID);

                SiteActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                if (activeLogin != null)
                {
                    _proAdvisorService.RefreshTotalForEstimate(estimateID);
                }

                return Json(new FunctionResult(errorBuilder.ToString()), JsonRequestBehavior.AllowGet);
            }

            return Json(new FunctionResult("User not validated"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/EstimateNotes")]
        public ActionResult EstimateNotes(int userID, int estimateID)
        {
            EstimateNotesVM estimateNotesVM = new EstimateNotesVM();

            // Make sure the estimate belongs to the login
            Estimate estimate = new Estimate(estimateID);
            if (estimate.CreatedByLoginID != ActiveLogin.LoginID)
            {
                return Redirect("/" + userID);
            }

            ViewBag.NavID = "estimate";
            ViewBag.EstimateNavID = "estimatenotes";

            ViewBag.EstimateID = estimateID;

            estimateNotesVM.LoginID = ActiveLogin.LoginID;
            estimateNotesVM.EstimateID = estimateID;

            return View(estimateNotesVM);
        }

        [HttpPost]
        [Route("{userID}/estimate/{estimateID}/EstimateNotes")]
        public ActionResult EstimateNotes(EstimateNotesVM vm)
        {
            var redirectResult = DoRedirect("");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            FunctionResult result = new FunctionResult();
            StringBuilder builder = new StringBuilder();

            ProEstimatorData.DataModel.EstimateNotes estimateNote = new ProEstimatorData.DataModel.EstimateNotes();

            if (IsUserAuthorized(vm.UserID))
            {
                estimateNote = ProEstimatorData.DataModel.EstimateNotes.GetByID(vm.ID);

                if (estimateNote == null)
                {
                    estimateNote = new EstimateNotes();
                    estimateNote.LoginID = vm.LoginID;
                    estimateNote.EstimateID = vm.EstimateID;
                }

                if (estimateNote != null && estimateNote.LoginID == vm.LoginID)
                {
                    estimateNote.NotesText = vm.NotesText;
                }
            }

            SaveResult saveResult = estimateNote.Save(GetActiveLoginID(vm.UserID));

            if (saveResult.Success)
            {
                return Redirect("/" + vm.UserID + "/estimate/" + vm.EstimateID + "/payment-info");
            }
            else
            {
                vm.SaveMessage = saveResult.ErrorMessage;
            }

            ViewBag.NavID = "estimate";
            ViewBag.EstimateNavID = "estimatenotes";

            ViewBag.EstimateID = vm.EstimateID;

            return View(vm);
        }

        public ActionResult GetEstimateNotesList([DataSourceRequest] DataSourceRequest request, int userID, int estimateID, string showDeletedNotes)
        {
            List<EstimateNotesVM> EstimateNotesVMs = new List<EstimateNotesVM>();

            SiteActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

            List<ProEstimatorData.DataModel.EstimateNotes> estimateNotes = ProEstimatorData.DataModel.EstimateNotes.GetForEstimate(estimateID, showDeletedNotes);

            foreach (ProEstimatorData.DataModel.EstimateNotes estimateNote in estimateNotes)
            {
                EstimateNotesVMs.Add(new EstimateNotesVM(estimateNote));
            }

            return Json(EstimateNotesVMs.ToDataSourceResult(request));
        }

        public JsonResult SaveEstimateNotes(EstimateNotesVM vm)
        {
            CacheActiveLoginID(vm.UserID);

            ProEstimatorData.DataModel.EstimateNotes estimateNote = new ProEstimatorData.DataModel.EstimateNotes();

            if (IsUserAuthorized(vm.UserID))
            {
                estimateNote = ProEstimatorData.DataModel.EstimateNotes.GetByID(vm.ID);

                if (estimateNote == null)
                {
                    estimateNote = new EstimateNotes();
                    estimateNote.LoginID = vm.LoginID;
                    estimateNote.EstimateID = vm.EstimateID;
                }

                if (estimateNote != null && estimateNote.LoginID == vm.LoginID)
                {
                    estimateNote.NotesText = vm.NotesText;
                    estimateNote.TimeStamp = null;
                    SaveResult saveResult = estimateNote.Save(GetActiveLoginID(vm.UserID));
                    return Json(saveResult.ErrorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(@Proestimator.Resources.ProStrings.InvalidLoginIDEstimateID, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEstimateNote(int userID, int estimateNotesID)
        {
            CacheActiveLoginID(userID);

            SiteActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

            EstimateNotesVM estimateNotesVM = new EstimateNotesVM();

            if (IsUserAuthorized(userID))
            {
                ProEstimatorData.DataModel.EstimateNotes estimateNotes = ProEstimatorData.DataModel.EstimateNotes.GetByID(estimateNotesID);

                if (estimateNotes != null)
                {
                    if (estimateNotes.ID == estimateNotesID)
                    {
                        estimateNotesVM = new EstimateNotesVM(estimateNotes);
                    }
                    else
                    {
                        return Json(new JsonData(false, "Invalid Estimate Note", estimateNotesVM), JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new JsonData(false, "Invalid Estimate Note", estimateNotesVM), JsonRequestBehavior.AllowGet);
                }
            }

            var notesJson = new { Success = true, NotesText = estimateNotesVM.NotesText, TimeStamp = String.Format("{0:G}", estimateNotesVM.TimeStamp), ID = estimateNotesVM.ID };
            return Json(notesJson, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteEstimateNotes(int userID, int loginID, int estimateNoteID, Boolean restoreDeleteNotes)
        {
            CacheActiveLoginID(userID);

            string errorMessage = "";

            if (IsUserAuthorized(userID))
            {
                ProEstimatorData.DataModel.EstimateNotes estimateNote = ProEstimatorData.DataModel.EstimateNotes.GetByID(estimateNoteID);

                if (estimateNote != null)
                {
                    if (estimateNote.ID == estimateNoteID)
                    {
                        if (restoreDeleteNotes == true)
                            estimateNote.Restore();
                        else
                            estimateNote.Delete();
                    }
                    else
                    {
                        errorMessage = string.Format(@Proestimator.Resources.ProStrings.ErrorDeletingReportInvalidReportID, estimateNoteID);
                    }
                }
                else
                {
                    errorMessage = string.Format(@Proestimator.Resources.ProStrings.ErrorDeletingReportInvalidReportID, estimateNoteID);
                }
            }

            return Json(errorMessage, JsonRequestBehavior.AllowGet);
        }

        public void SaveNavigationLog(int userID, string controlButtonID)
        {
            if (!string.IsNullOrEmpty(controlButtonID))
            {
                //NavigationLogger.LogNavigation(userID, controlButtonID);
            }
        }

        private void ImageSave(Image img, string filePath)
        {
            ImageCodecInfo imageCodecInfo = GetImageCodeInfo("image/jpeg");
            img.Save(filePath, imageCodecInfo, GetEncodeParamsWithCompression(95));
        }

        private ImageCodecInfo GetImageCodeInfo(string mime)
        {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
            for (int i = 0; i <= encoders.Length; i++)
            {
                if (encoders[i].MimeType == mime) return encoders[i];
            }
            return null;
        }

        private EncoderParameters GetEncodeParamsWithCompression(long compression)
        {
            EncoderParameters ep = new EncoderParameters(1);
            ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, compression);
            return ep;
        }

        private string GetDiskSize(string diskpath)
        {
            var length = new System.IO.FileInfo(diskpath).Length;
            Decimal size = Decimal.Divide(length, 1024);
            return String.Format("{0:##.##} KB", size);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.IO;
using System.Security.Cryptography;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using Proestimator.ViewModel;
using Proestimator.ViewModel.CustomReports;
using Proestimator.ViewModel.Contracts;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.Models;
using ProEstimatorData.Models.SubModel;
using ProEstimatorData.DataModel.Contracts;
using ProEstimatorData.DataModel.Settings;

using ProEstimator.Business.Logic;
using System.Drawing;

using Ghostscript.NET.Rasterizer;
using Proestimator.Helpers;
using ProEstimatorData.Helpers;
using ProEstimator.Business.Model.Account.Commands;
using ProEstimator.Business.Model;
using ProEstimator.Business.Payments.StripeCommands;
using ProEstimator.Business.Payments;
using Proestimator.ViewModelMappers.Contracts;
using Proestimator.ViewModel.SendEstimate;
using static Proestimator.Controllers.SendEstimateController;
using System.Threading.Tasks;

namespace Proestimator.Controllers
{
    public class SettingsController : SiteController
    {

        private IStripeService _stripeService;

        public SettingsController(IStripeService stripeService)
        {
            _stripeService = stripeService;
        }

        #region Estimators

        [HttpGet]
        [Route("{userID}/settings/estimators")]
        public ActionResult Estimators(int userID)
        {
            EstimatorsVM vmodel = new EstimatorsVM();
            vmodel.LoginID = ActiveLogin.LoginID;

            ViewBag.NavID = "settings";
            ViewBag.settingsTopMenuID = 0;
            ViewBag.HasMUContract = ActiveLogin.HasMultiUserContract;

            return View(vmodel);
        }

        public JsonResult GetEstimator(int userID, int loginID, int estimatorID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                Estimator estimator = Estimator.Get(estimatorID, GetActiveLoginID(userID));
                if (estimator != null && estimator.LoginID == loginID)
                {
                    EstimatorInfo info = new EstimatorInfo();
                    info.FirstName = estimator.FirstName;
                    info.LastName = estimator.LastName;
                    info.Email = estimator.Email;
                    info.Phone = estimator.Phone;
                    info.DefaultEstimator = estimator.DefaultEstimator;

                    return Json(info, JsonRequestBehavior.AllowGet);
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public class EstimatorInfo
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public bool DefaultEstimator { get; set; }
        }

        public JsonResult DeleteEstimator(int userID, int loginID, int estimatorID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                Estimator estimator = Estimator.Get(estimatorID, GetActiveLoginID(userID));
                if (estimator != null && estimator.LoginID == loginID)
                {
                    DBAccess db = new DBAccess();
                    db.ExecuteNonQuery("DeleteEstimatorsData", new SqlParameter("EstimatorID", estimatorID));
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetEstimators([DataSourceRequest] DataSourceRequest request, int userID, int loginID)
        {
            List<EstimatorVM> estimatorList = new List<EstimatorVM>();

            if (IsUserAuthorized(userID))
            {
                List<Estimator> estimators = Estimator.GetByLogin(loginID, GetActiveLoginID(userID));
                foreach (Estimator estimator in estimators)
                {
                    estimatorList.Add(new EstimatorVM(estimator));
                }
            }

            return Json(estimatorList.ToDataSourceResult(request));
        }

        public JsonResult SaveEstimator(int userID, int loginID, int estimatorID, string firstName, string lastName, string email,
            string phone, bool defaultEstimator)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                Estimator estimator = new Estimator();

                if (estimatorID > 0)
                {
                    estimator = Estimator.Get(estimatorID, GetActiveLoginID(userID));
                }
                else
                {
                    estimator.LoginID = loginID;
                }

                if (estimator != null && estimator.LoginID == loginID)
                {
                    estimator.FirstName = firstName;
                    estimator.LastName = lastName;
                    estimator.Email = email;
                    estimator.Phone = phone;
                    estimator.DefaultEstimator = defaultEstimator;

                    SaveResult saveResult = estimator.Save(GetActiveLoginID(userID));
                    return Json(saveResult.ErrorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(@Proestimator.Resources.ProStrings.InvalidLoginIDEstimateID, JsonRequestBehavior.AllowGet);
        }

        public bool ReOrderEstimators(List<string> data, int loginID, int userID)
        {
            try
            {
                List<Estimator> estimators = Estimator.GetByLogin(loginID, GetActiveLoginID(userID));
                int index = 0;
                foreach (var estimatorId in data)
                {
                    Estimator estimator = estimators.FirstOrDefault(x => x.ID == int.Parse(estimatorId));
                    if (estimator != null)
                    {
                        estimator.OrderNumber = index++;
                        estimator.Save(GetActiveLoginID(userID));
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {

            }

            return false;
        }

        #endregion

        #region AccountPreferences 

        [HttpGet]
        [Route("{userID}/settings/account-preferences")]
        public ActionResult AccountPreferences(int userID)
        {
            AccountPreferencesVM model = new AccountPreferencesVM();

            ProEstimatorData.DataModel.LoginInfo loginInfo = ProEstimatorData.DataModel.LoginInfo.GetByID(ActiveLogin.LoginID);
            model.LoginID = ActiveLogin.LoginID;
            model.UserID = userID;

            model.ShowAppraiser = loginInfo.Appraiser;
            model.ShowLaborTimes = loginInfo.ShowLaborTimeWO;

            model.VehicleOptionsInFooter = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "VehicleOptionsInFooter", "ReportOptions", (true).ToString()).ValueString);
            model.IncludePartNotesOnPdfLineEntry = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "IncludePartNotesOnPdfLineEntry", "ReportOptions", (false).ToString()).ValueString);
            model.PrintShopInfo = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "PrintShopInfo", "ReportOptions", (true).ToString()).ValueString);
            model.PrintInspectionDate = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "PrintInspectionDate", "ReportOptions", (true).ToString()).ValueString);
            model.AddPartsShowAllYears = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "ShowAllYears", "AddPartsOptions", (false).ToString()).ValueString);
            model.ProAdvisorEnabled = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "ProAdvisorEnabled", "AddPartsOptions", (true).ToString()).ValueString);
            model.PrintRepairDays = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "PrintRepairDays", "ReportOptions", (true).ToString()).ValueString);
            model.PrintHeaderInfoOnEstimateReport = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "PrintHeaderInfoOnEstimateReport", "ReportOptions", (true).ToString()).ValueString);
            model.FontSizeDetails = (FontSize)InputHelper.GetInteger(SiteSettings.Get(ActiveLogin.LoginID, "FontSizeDetails", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString);
            model.FontSizeHeader = (FontSize)InputHelper.GetInteger(SiteSettings.Get(ActiveLogin.LoginID, "FontSizeHeader", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString);
            model.FontSizeHeaders = (FontSize)InputHelper.GetInteger(SiteSettings.Get(ActiveLogin.LoginID, "FontSizeHeaders", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString);
            model.FontSizeLines = (FontSize)InputHelper.GetInteger(SiteSettings.Get(ActiveLogin.LoginID, "FontSizeLines", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString);
            model.FontSizeTableHeaders = (FontSize)InputHelper.GetInteger(SiteSettings.Get(ActiveLogin.LoginID, "FontSizeTableHeaders", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString);
            model.FontSizeTotals = (FontSize)InputHelper.GetInteger(SiteSettings.Get(ActiveLogin.LoginID, "FontSizeTotals", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString);
            model.DownloadPDF = (PDFDownloadSetting)InputHelper.GetInteger(SiteSettings.Get(ActiveLogin.LoginID, "DownloadPDF", "ReportOptions", ((int)PDFDownloadSetting.OpenNewTab).ToString()).ValueString);
            model.PrintEmailAddressInHeader = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "PrintEmailAddressInHeader", "ReportOptions", (true).ToString()).ValueString);
            model.ManualEstimatePrintSortAscendingOrder = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "ManualEstimatePrintSortAscendingOrder", "ReportOptions", (true).ToString()).ValueString);
            model.UseLegacyPartsSectionDropdown = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "UseLegacyPartsSectionDropdown", "AddPartsOptions", (false).ToString()).ValueString);

            //model.DisableProAdvisor = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "SiteSettings", "DisableProAdvisor", "False").ValueString);

            model.SelectedTimeZone = InputHelper.GetInteger(SiteSettings.Get(ActiveLogin.LoginID, "TimeZone", "ReportOptions", "0").ValueString);

            model.EmailAutoCcOn = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "AutoCcOn", "EmailPreferences", "1").ValueString);

            model.Culture = ActiveLogin.LanguagePreference;

            model.HasProAdvisorContract = ActiveLogin.HasProAdvisorContract;
            model.ShowChatIconDesktop = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "ShowChatIconDesktop", "ProgramPreferences", "1").ValueString);
            model.ShowChatIconMobile = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "ShowChatIconMobile", "ProgramPreferences", "0").ValueString);
            model.AllowEstimateVehicleModsWithLineItems = InputHelper.GetBoolean(SiteSettings.Get(ActiveLogin.LoginID, "AllowEstimateVehicleModsWithLineItems", "ProgramPreferences", "0").ValueString);

            ViewBag.NavID = "settings";
            ViewBag.settingsTopMenuID = 1;
            ViewBag.HasMUContract = ActiveLogin.HasMultiUserContract;

            return View(model);
        }

        [HttpPost]
        [Route("{userID}/settings/account-preferences")]
        public ActionResult AccountPreferences(AccountPreferencesVM model, string Culture)
        {
            LoginInfo loginInfo = LoginInfo.GetByID(model.LoginID);

            loginInfo.Appraiser = model.ShowAppraiser;
            loginInfo.ShowLaborTimeWO = model.ShowLaborTimes;

            SaveResult loginInfoSave = loginInfo.Save();

            // Save Site Settings
            int activeLoginID = ActiveLogin.ID;

            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "VehicleOptionsInFooter", "ReportOptions", model.VehicleOptionsInFooter.ToString());
            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "IncludePartNotesOnPdfLineEntry", "ReportOptions", model.IncludePartNotesOnPdfLineEntry.ToString());
            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "PrintShopInfo", "ReportOptions", model.PrintShopInfo.ToString());
            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "PrintInspectionDate", "ReportOptions", model.PrintInspectionDate.ToString());
            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "PrintRepairDays", "ReportOptions", model.PrintRepairDays.ToString());

            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "ShowAllYears", "AddPartsOptions", model.AddPartsShowAllYears.ToString());

            if (ActiveLogin.HasProAdvisorContract)
            {
                SiteSettings.SaveSetting(activeLoginID, model.LoginID, "ProAdvisorEnabled", "AddPartsOptions", model.ProAdvisorEnabled.ToString());
            }  

            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "UseLegacyPartsSectionDropdown", "AddPartsOptions", model.UseLegacyPartsSectionDropdown.ToString());
            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "PrintHeaderInfoOnEstimateReport", "ReportOptions", model.PrintHeaderInfoOnEstimateReport.ToString());

            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "FontSizeDetails", "ReportFonts", (int)model.FontSizeDetails);
            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "FontSizeHeader", "ReportFonts", (int)model.FontSizeHeader);
            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "FontSizeHeaders", "ReportFonts", (int)model.FontSizeHeaders);
            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "FontSizeLines", "ReportFonts", (int)model.FontSizeLines);
            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "FontSizeTableHeaders", "ReportFonts", (int)model.FontSizeTableHeaders);
            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "FontSizeTotals", "ReportFonts", (int)model.FontSizeTotals);

            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "TimeZone", "ReportOptions", model.SelectedTimeZone);

            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "AutoCcOn", "EmailPreferences", model.EmailAutoCcOn.ToString());

            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "DownloadPDF", "ReportOptions", (int)model.DownloadPDF);

            // SiteSettings.SaveSetting(activeLoginID, model.LoginID, "SiteSettings", "DisableProAdvisor", model.DisableProAdvisor.ToString());

            // Validate input
            model.Culture = CultureHelper.GetImplementedCulture(model.Culture);

            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "Culture", "LanguagePreferences", model.Culture);
            ActiveLogin.LanguagePreference = model.Culture;
            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "ShowChatIconDesktop", "ProgramPreferences", model.ShowChatIconDesktop.ToString());
            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "ShowChatIconMobile", "ProgramPreferences", model.ShowChatIconMobile.ToString());
            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "AllowEstimateVehicleModsWithLineItems", "ProgramPreferences", model.AllowEstimateVehicleModsWithLineItems.ToString());

            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "PrintEmailAddressInHeader", "ReportOptions", model.PrintEmailAddressInHeader.ToString());
            SiteSettings.SaveSetting(activeLoginID, model.LoginID, "ManualEstimatePrintSortAscendingOrder", "ReportOptions", model.ManualEstimatePrintSortAscendingOrder.ToString());

            return DoRedirect("/" + model.UserID + "/settings/account-preferences");
        }

        public JsonResult SaveFontSizeAccountPreferences(int userID, int loginID, int partsSectionTreeFontSize)
        {
            SiteSettings.SaveSetting(GetActiveLoginID(userID), loginID, "PartsSectionTreeFontSize", "AddParts", partsSectionTreeFontSize);

            return Json(new FunctionResult(true, "OK"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult ChangeAutoRenew(int userID, int loginID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                bool turnOnAutoPay = false;
                LoginAutoRenew current = LoginAutoRenew.GetLastForLogin(loginID);

                if (current != null && current.Enabled)
                {
                    LoginAutoRenew.Insert(loginID, false);
                }
                else
                {
                    LoginAutoRenew.Insert(loginID, true);
                    turnOnAutoPay = ContractManager.TurnOnAutoPay(loginID, false, false);
                }

                EmailManager.SendContractAutoRenewChange(loginID);

                AutoRenewResult autoRenewResult = new AutoRenewResult(loginID);
                autoRenewResult.TurnOnAutoPay = turnOnAutoPay;
                return Json(autoRenewResult, JsonRequestBehavior.AllowGet);
            }

            return Json(new AutoRenewResult("Unauthorized"), JsonRequestBehavior.AllowGet);
        }

        public class AutoRenewResult : FunctionResult
        {
            public bool AutoRenewOn { get; set; }
            public List<AutoRenewDetailVM> AutoRenewDetails { get; set; }
            public bool TurnOnAutoPay { get; set; }

            public AutoRenewResult(string errorMessage)
                : base(errorMessage)
            {

            }

            public AutoRenewResult(int loginID)
            {
                List<LoginAutoRenew> autoRenews = LoginAutoRenew.GetForLogin(loginID);

                AutoRenewDetails = new List<AutoRenewDetailVM>();
                foreach (LoginAutoRenew autoRenew in autoRenews)
                {
                    AutoRenewDetails.Add(new AutoRenewDetailVM(autoRenew));
                }

                if (autoRenews.Count > 0)
                {
                    AutoRenewOn = autoRenews[0].Enabled;
                }
            }
        }

        #endregion

        #region Company Profile

        [HttpGet]
        [Route("{userID}/settings/company-profile")]
        public ActionResult CompanyProfile(int userID)
        {
            CompanyProfileVM model = new CompanyProfileVM();

            List<State> stateListColl = State.StatesList;
            model.States = new SelectList(stateListColl, "Code", "Description");
            State stateOnEmptyCode = stateListColl.Where(eachState => (string.Compare(eachState.Code, "", true) == 0)).FirstOrDefault<State>();
            stateListColl.Remove(stateOnEmptyCode);
            stateListColl.Insert(0, new State("", "-- Select State --"));

            LoginInfo loginInfo = LoginInfo.GetByID(ActiveLogin.LoginID);

            model.LoginID = ActiveLogin.LoginID;

            model.CompanyName = loginInfo.CompanyName;
            model.CompanyType = loginInfo.CompanyType;
            model.LogoImagePath = loginInfo.GetLogoPath();
            model.IsLocked = loginInfo.ProfileLocked;

            model.FederalTaxID = loginInfo.FederalTaxID;
            model.PrintFederalTaxID = loginInfo.PrintFederalTaxID;
            model.UseTaxID = loginInfo.UseTaxID;

            model.LicenseNumber = loginInfo.LicenseNumber;
            model.PrintLicenseNumber = loginInfo.PrintLicenseNumber;

            model.BarNumber = loginInfo.BarNumber;
            model.PrintBarNumber = loginInfo.PrintBarNumber;

            model.RegistrationNumber = loginInfo.RegistrationNumber;
            model.PrintRegistration = loginInfo.PrintRegistrationNumber;

            Contact contact = Contact.GetContact(loginInfo.ContactID);

            if (contact != null)
            {
                model.FirstName = contact.FirstName;
                model.LastName = contact.LastName;
                model.Email = contact.Email;
                model.Phone = InputHelper.GetNumbersOnly(contact.Phone);
                model.PhoneTwo = InputHelper.GetNumbersOnly(contact.Phone2);
                model.PhNumberType1 = !string.IsNullOrEmpty(contact.PhoneNumberType1) ? contact.PhoneNumberType1.Trim() : string.Empty;
                model.PhNumberType2 = !string.IsNullOrEmpty(contact.PhoneNumberType2) ? contact.PhoneNumberType2.Trim() : string.Empty;
                model.HeaderContact = loginInfo.HeaderContact;

                model.Fax = InputHelper.GetNumbersOnly(contact.Fax);

                ProEstimatorData.DataModel.Address address = ProEstimatorData.DataModel.Address.GetForContact(contact.ContactID);
                model.Address1 = address.Line1;
                model.Address2 = address.Line2;
                model.City = address.City;
                model.State = address.State;

                State stateOnCode = stateListColl.Where(eachState => (string.Compare(eachState.Code, model.State, true) == 0)).FirstOrDefault<State>();
                State stateOnDescription = stateListColl.Where(eachState => (string.Compare(eachState.Description, model.State, true) == 0)).FirstOrDefault<State>();

                if (stateOnCode != null)
                    model.State = stateOnCode.Code;

                if (stateOnDescription != null)
                    model.State = stateOnDescription.Code;

                if ((stateOnCode == null) && (stateOnDescription == null))
                    model.State = string.Empty;

                model.Zip = address.Zip;
            }

            ViewBag.NavID = "settings";
            ViewBag.settingsTopMenuID = 2;
            ViewBag.HasMUContract = ActiveLogin.HasMultiUserContract;

            model.Serialized = model.ToTrackable();

            return View(model);
        }

        public JsonResult RemoveCustomIcon(int userID, int loginID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                LoginInfo loginInfo = LoginInfo.GetByID(loginID);
                loginInfo.LogoImageType = "";
                loginInfo.Save();

                return Json(new JsonData(true, "", loginInfo.GetLogoPath()), JsonRequestBehavior.AllowGet);
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("{userID}/settings/company-profile")]
        public ActionResult CompanyProfile(CompanyProfileVM model, HttpPostedFileBase file)
        {
            StringBuilder errorBuilder = new StringBuilder();

            try
            {
                LoginInfo loginInfo = LoginInfo.GetByID(model.LoginID);
                if (!loginInfo.ProfileLocked && !model.IsLocked)
                {
                    Contact contact = Contact.GetContact(loginInfo.ContactID);

                    contact.FirstName = model.FirstName;
                    contact.LastName = model.LastName;
                    contact.Email = model.Email;
                    contact.Phone = model.Phone;
                    contact.Phone2 = model.PhoneTwo;
                    contact.PhoneNumberType1 = model.PhNumberType1;
                    contact.PhoneNumberType2 = model.PhNumberType2;
                    contact.Fax = model.Fax;
                    loginInfo.HeaderContact = model.HeaderContact;

                    SaveResult contactSave = contact.Save(ActiveLogin.ID);
                    if (!contactSave.Success)
                    {
                        errorBuilder.AppendLine(contactSave.ErrorMessage);
                    }

                    Address address = Address.GetForContact(contact.ContactID);
                    address.Line1 = model.Address1;
                    address.Line2 = model.Address2;
                    address.City = model.City;
                    address.State = model.State;
                    address.Zip = model.Zip;
                    SaveResult addressSave = address.Save(ActiveLogin.ID, ActiveLogin.LoginID);
                    if (!addressSave.Success)
                    {
                        errorBuilder.AppendLine(addressSave.ErrorMessage);
                    }

                    loginInfo.CompanyName = model.CompanyName;
                    loginInfo.CompanyType = model.CompanyType;
                }

                if (file != null && file.FileName != "")
                {
                    string contentType = file.ContentType.ToLower();

                    if (contentType.StartsWith("image/") || contentType == "application/pdf")
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                        string extension = Path.GetExtension(file.FileName).Replace(".", "");

                        string imageFolder = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), "CompanyLogos");
                        Directory.CreateDirectory(imageFolder);

                        if (contentType.StartsWith("image/"))
                        {
                            string path = Path.Combine(imageFolder, loginInfo.ID.ToString() + "." + extension);

                            if(Path.GetFullPath(path).StartsWith(imageFolder, StringComparison.OrdinalIgnoreCase))
                            {
                                file.SaveAs(path);

                                loginInfo.LogoImageType = extension;

                                model.LogoImagePath = loginInfo.GetLogoPath();
                            }
                        }
                        else if (contentType == "application/pdf")
                        {
                            // Save the PDF to disk
                            string pdfPath = Path.Combine(imageFolder, loginInfo.ID.ToString() + ".pdf");
                            file.SaveAs(pdfPath);

                            // Save the first page of the PDF as an image thumbnail
                            try
                            {
                                int desired_x_dpi = 96;
                                int desired_y_dpi = 96;

                                Image logoSmallImage;

                                using (var rasterizer = new GhostscriptRasterizer())
                                {
                                    ErrorLogger.LogError("Getting pdf from: " + pdfPath, "Logo upload PdfRasterizer");

                                    rasterizer.Open(pdfPath);
                                    ErrorLogger.LogError("Page Count: " + rasterizer.PageCount, "UploadImage PdfRasterizer");
                                    logoSmallImage = rasterizer.GetPage(desired_x_dpi, desired_y_dpi, 1);
                                }

                                if (logoSmallImage != null)
                                {
                                    logoSmallImage = ScaleImage(logoSmallImage, 800, 800);

                                    logoSmallImage.Save(Path.Combine(imageFolder, loginInfo.ID.ToString() + ".png"));

                                    loginInfo.LogoImageType = "png";

                                    model.LogoImagePath = loginInfo.GetLogoPath();
                                }
                            }
                            catch (System.Exception ex)
                            {
                                ErrorLogger.LogError(ex, model.LoginID, "SettingsController Logo upload PdfRasterizer");
                                errorBuilder.AppendLine("Error saving data: " + ex.Message);
                            }
                        }
                    }
                    else
                    {
                        //return Json(bulkUploadImageResult, JsonRequestBehavior.AllowGet);
                        //result.ErrorMessage = "Only Image and PDF files can be uploaded.";
                    }
                }

                loginInfo.FederalTaxID = model.FederalTaxID;
                loginInfo.LicenseNumber = model.LicenseNumber;
                loginInfo.BarNumber = model.BarNumber;
                loginInfo.RegistrationNumber = model.RegistrationNumber;

                loginInfo.PrintFederalTaxID = model.PrintFederalTaxID;
                loginInfo.PrintLicenseNumber = model.PrintLicenseNumber;
                loginInfo.PrintBarNumber = model.PrintBarNumber;
                loginInfo.PrintRegistrationNumber = model.PrintRegistration;
                loginInfo.UseTaxID = model.UseTaxID;

                SaveResult saveResult = loginInfo.Save(ActiveLogin.ID);
                if (!saveResult.Success)
                {
                    errorBuilder.AppendLine(saveResult.ErrorMessage);
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, model.LoginID, "SettingsController CompanyProfile");
                errorBuilder.AppendLine("Error saving data: " + ex.Message);
            }

            string errors = errorBuilder.ToString();

            if (string.IsNullOrEmpty(errors))
            {
                return DoRedirect("/" + ActiveLogin.SiteUserID + "/settings/company-profile");
            }
            else
            {
                model.ErrorMessage = errors;
                ViewBag.NavID = "settings";
                ViewBag.settingsTopMenuID = 2;
                ViewBag.HasMUContract = ActiveLogin.HasMultiUserContract;

                model.States = new SelectList(State.StatesList, "Code", "Description");

                return View(model);
            }
        }

        public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            if (ratio < 1)
            {
                var newWidth = (int)(image.Width * ratio);
                var newHeight = (int)(image.Height * ratio);

                var newImage = new Bitmap(newWidth, newHeight);

                using (var graphics = Graphics.FromImage(newImage))
                {
                    graphics.DrawImage(image, 0, 0, newWidth, newHeight);
                }

                return newImage;
            }
            else
            {
                return image;
            }
        }

        #endregion

        #region Contract

        [HttpGet]
        [Route("{userID}/settings/contract")]
        public ActionResult ContractPage(int userID)
        {
            ViewBag.NavID = "settings";
            ViewBag.settingsTopMenuID = 3;
            ViewBag.HasMUContract = ActiveLogin.HasMultiUserContract;

            ContractTabVMMapper mapper = new ContractTabVMMapper();
            ContractTabVM vm = mapper.Map(new ContractTabVMMapperConfiguration() { LoginID = ActiveLogin.LoginID });

            return View(vm);
        }

        [HttpPost]
        [Route("{userID}/settings/contract")]
        public ActionResult ContractPage(ContractTabVM vm, string redirectDataField)
        {
            if (!string.IsNullOrEmpty(redirectDataField) && !redirectDataField.Contains("settings/billing") && !redirectDataField.Contains("settings/contract"))
            {
                Contract activeContract = Contract.GetActive(vm.LoginID);
                if (activeContract != null)
                {
                    List<Invoice> allInvoices = Invoice.GetForContract(activeContract.ID, true);
                    List<ContractAddOn> addOns = ContractAddOn.GetForContract(activeContract.ID);

                    foreach (ContractAddOn addOn in addOns)
                    {
                        DeleteAddOn(addOn, allInvoices);
                    }
                }
            }
            return DoRedirect("/" + ActiveLogin.SiteUserID + "/settings/contract");
        }

        private void DeleteAddOn(ContractAddOn addOn, List<Invoice> listInvoice)
        {
            if (addOn != null && addOn.PriceLevel.PaymentAmount > 0)
            {
                List<Invoice> addOnInvoices = listInvoice.Where(o => o.AddOnID == addOn.ID).ToList();
                if (addOnInvoices != null && addOnInvoices.Count > 0 && !addOnInvoices.Exists(o => o.Paid) && !addOnInvoices.Exists(o => o.Notes.Contains("from previous billing cycle")))
                {
                    FunctionResult result = addOn.DeletePermanent();
                    if(result.Success)
                    {
                        EmailManager.SendSelectAddOnEmail(addOnInvoices[0].LoginID, addOn.ContractID, addOn.AddOnType.Type, addOn.PriceLevel.ContractTerms.TermDescription);                     
                    }
                }
            }
        }

        public JsonResult CreateAddOn(int userID, int loginID, int contractID, int priceLevelID, int contractTypeID, int qty, bool multiAddon)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                Contract activeContract = Contract.GetActive(loginID);

                if (activeContract != null && contractID == activeContract.ID)
                {
                    FunctionResult result = ContractManager.CreateContractAddOn(activeContract, priceLevelID, contractTypeID, DateTime.Now, qty);
                    if (result.Success)
                    {
                        List<ContractAddOn> addOns = ContractAddOn.GetForContract(activeContract.ID);
                        ContractAddOn addOn = addOns.FirstOrDefault(o => o.AddOnType.ID == contractTypeID);
                        if (multiAddon)
                        {
                            addOns = addOns.Where(o => o.AddOnType.ID == contractTypeID).ToList();
                            addOn = addOns.FirstOrDefault(o => o.PriceLevel.ID == priceLevelID && o.Active);
                        }

                        bool needPageRefresh = false;

                        // In the special case of an add on being free, we need to refresh permissions and the page.
                        if (addOn.PriceLevel.PaymentAmount == 0)
                        {
                            _siteLoginManager.RefreshInvoiceInformationForAccount(ActiveLogin.LoginID);
                            needPageRefresh = true;
                        }

                        if (addOn == null)
                        {
                            return Json(new FunctionResult(false, "Something went wrong, your add on was not created.  Please contact support if the problem continues."), JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            if (multiAddon)
                            {
                                if (addOns != null && addOns.Count > 0)
                                {
                                    return Json(new AddOnResult(addOn, addOns, needPageRefresh), JsonRequestBehavior.AllowGet);   
                                }
                                else
                                {
                                    return Json(new FunctionResult(false, "Something went wrong, your add on was not created.  Please contact support if the problem continues."), JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                return Json(new CreateAddOnResult(addOn.ID, addOn.PriceLevel.ContractTerms.TermDescription, AddOnNeedPayment(addOn), needPageRefresh), JsonRequestBehavior.AllowGet);
                            }
                        }
                    }

                }
            }

            return Json(new FunctionResult(false, "Invalid user."), JsonRequestBehavior.AllowGet);
        }

        public class CreateAddOnResult : FunctionResult
        {
            public int AddOnID { get; set; }
            public string AddOnTerms { get; set; }
            public bool NeedsPayment { get; set; }
            public bool NeedsPageRefresh { get; set; }

            public CreateAddOnResult(int addOnID, string addOnTerms, bool needsPayment, bool needPageRefresh)
                : base()
            {
                AddOnID = addOnID;
                AddOnTerms = addOnTerms;
                NeedsPayment = needsPayment;
                NeedsPageRefresh = needPageRefresh;
            }
        }

        public class AddOnResult : FunctionResult
        {
            public int AddOnID { get; set; }
            public bool NeedsPageRefresh { get; set; }
            public List<AddOnDetail> AddOnDetails { get; set; }
            public int TypeID { get; set; }
            public AddOnResult(ContractAddOn addOn, List<ContractAddOn> addOns, bool needPageRefresh)
                : base()
            {
                AddOnID = addOn.ID;
                NeedsPageRefresh = needPageRefresh;
                List<Invoice> allInvoices = Invoice.GetForContract(addOn.ContractID, true);
                AddOnDetails = new List<AddOnDetail>();
                foreach (ContractAddOn contractAddOn in addOns)
                {
                    AddOnDetails.Add(new AddOnDetail(contractAddOn, addOn.ID == contractAddOn.ID, allInvoices));
                }
                TypeID = addOn.AddOnType.ID;
            }
        }

        public class AddOnDetail
        {
            public int AddOnID { get; set; }
            public string AddOnTerms { get; set; }
            public bool NeedsPayment { get; set; }

            public AddOnDetail(ContractAddOn addOn, bool yes, List<Invoice> allInvoices)
            {
                AddOnID = addOn.ID;
                decimal paidAmount = GetFirstPaidAddOnInvoiceActualPrice(addOn, allInvoices);
                AddOnTerms = addOn.Quantity.ToString() + (addOn.Quantity > 1 ? " AddOns x " : " AddOn x ") + (yes ? addOn.PriceLevel.ContractTerms.TermDescription : addOn.PriceLevel.ContractTerms.NumberOfPayments + " x " + (paidAmount > 0 ? paidAmount.ToString("C") : addOn.PriceLevel.PaymentAmount.ToString("C")));
                NeedsPayment = ContractManager.InvoiceDeletable(addOn.ID).Count > 0;
            }

            private bool NeedPayment(ContractAddOn addOn)
            {
                bool needPayment = false;

                if (addOn.PriceLevel.PaymentAmount > 0)
                {
                    List<Invoice> addOnInvoices = Invoice.GetForContractAddOn(addOn.ID);
                    if (addOnInvoices.Count > 0 && !addOnInvoices.Exists(o => o.Notes.Contains("from previous billing cycle") || o.Paid))
                    {
                        needPayment = true;
                    }
                }

                return needPayment;
            }

            private decimal GetFirstPaidAddOnInvoiceActualPrice(ContractAddOn addOn, List<Invoice> allInvoices)
            {
                foreach (Invoice invoice in allInvoices)
                {
                    if (invoice.AddOnID == addOn.ID && invoice.Paid && (!invoice.Notes.ToLower().Contains("prorated") || allInvoices.Count == 1))
                    {
                        int qty = addOn.Quantity;
                        if (invoice.Notes.StartsWith("For"))
                        {
                            string[] notes = invoice.Notes.Split(' ');
                            if (notes.Length > 1 && InputHelper.GetDecimal(notes[1], 0) > 0)
                            {
                                qty = (int)InputHelper.GetDecimal(notes[1], 0);
                            }
                        }
                        return invoice.InvoiceTotal / qty;
                    }
                }

                return 0;
            }
        }

        private bool AddOnNeedPayment(ContractAddOn addOn)
        {
            bool needPayment = false;

            if (addOn.PriceLevel.PaymentAmount > 0)
            {
                List<Invoice> addOnInvoices = Invoice.GetForContractAddOn(addOn.ID);
                if (addOnInvoices.Count > 0 && !addOnInvoices.Exists(o => o.Notes.Contains("from previous billing cycle")))
                {
                    needPayment = true;
                }
            }

            return needPayment;
        }

        public JsonResult DeleteAddOn(int userID, int loginID, int contractID, int addOnID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                Contract activeContract = Contract.GetActive(loginID);

                if (activeContract != null && contractID == activeContract.ID)
                {
                    ContractAddOn addOn = ContractAddOn.Get(addOnID);
                    if (addOn != null && addOn.ContractID == activeContract.ID)
                    {
                        FunctionResult deleteResult = addOn.DeletePermanent();
                        if (deleteResult.Success)
                        {
                            return Json(new DeleteAddOnFunctionResult(addOn.AddOnType.ID), JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            return Json(new FunctionResult(false, "Invalid data, could not delete the add on.  Please contact support for help."), JsonRequestBehavior.AllowGet);
        }

        public class DeleteAddOnFunctionResult : FunctionResult
        {
            public int AddOnTypeID { get; set; }

            public DeleteAddOnFunctionResult(int addOnTypeID)
            {
                AddOnTypeID = addOnTypeID;
            }

        }

        public JsonResult DeleteInvoices(int userID, int loginID, int contractID, int addOnID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                Contract activeContract = Contract.GetActive(loginID);

                if (activeContract != null && contractID == activeContract.ID)
                {
                    ContractAddOn addOn = ContractAddOn.Get(addOnID);
                    if (addOn != null && addOn.ContractID == activeContract.ID)
                    {
                        ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                        FunctionResult result = ContractManager.RemoveContractAddOn(loginID, addOn.ID, addOn.Quantity, activeLogin.ID);
                        if (result.Success == false)
                        {
                            ErrorLogger.LogError("AddOn ID: " + addOn.ID + " Error: " + result.ErrorMessage, loginID, 0, "Admin RemoveContractAddOn SaveAddOnDetails");
                        }
                    }
                    List<ContractAddOn> addOns = ContractAddOn.GetForContract(activeContract.ID).Where(o => o.AddOnType.ID == addOn.AddOnType.ID).ToList();
                    return Json(new AddOnResult(addOn, addOns, false), JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new FunctionResult(false, "Invalid data, could not delete the add on.  Please contact support for help."), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Billing

        [HttpGet]
        [Route("{userID}/settings/billing")]
        public ActionResult Billing(int userID)
        {
            BillingTabVM vm = PrepareBillingTabVM();

            return View(vm);
        }

        [HttpPost]
        [Route("{userID}/settings/billing")]
        public ActionResult Billing(BillingTabVM vm)
        {
            // If a stripe token was created, save credit card info
            string token = Request.Form["stripeToken"];
            string emailAddress = Request.Form["stripeEmail"];

            if (!string.IsNullOrEmpty(token) || !string.IsNullOrEmpty(emailAddress))
            {
                ErrorLogger.LogError("Token: " + token + "  EmailAddress: " + emailAddress, ActiveLogin.LoginID, 0, "Billing Stripe");

                FunctionResultObj<StripeInfo> result = _stripeService.ProcessStripeToken(token, emailAddress, ActiveLogin.LoginID);
                if (!result.Success)
                {
                    ErrorLogger.LogError("Error: " + result.ErrorMessage, ActiveLogin.LoginID, 0, "Billing Stripe");
                    vm = PrepareBillingTabVM();
                    vm.Message = result.ErrorMessage;

                    return View(vm);
                }
            }

            return DoRedirect("/" + ActiveLogin.SiteUserID + "/settings/billing");
        }

        private BillingTabVM PrepareBillingTabVM()
        {
            BillingTabVM vm = new BillingTabVM();

            ViewBag.NavID = "settings";
            ViewBag.settingsTopMenuID = 4;
            ViewBag.HasMUContract = ActiveLogin.HasMultiUserContract;

            vm.LoginID = ActiveLogin.LoginID;

            // Get all contracts including old and future ones
            List<Contract> allContracts = Contract.GetAllForLogin(ActiveLogin.LoginID);

            ContractVMMapper contractMapper = new ContractVMMapper();

            foreach (var contract in allContracts.OrderBy(o => o.ExpirationDate))
            {
                vm.Contracts.Add(contractMapper.Map(new ViewModelMappers.Contracts.ContractVMMapperConfiguration() { Contract = contract }));
            }

            foreach (Contract contract in allContracts)
            {
                if (contract.EffectiveDate > DateTime.Now)
                {
                    List<Invoice> invoices = Invoice.GetForContract(contract.ID);
                    foreach (Invoice invoice in invoices)
                    {
                        if (invoice.AddOnID == 0 && invoice.PaymentNumber <= 1 && invoice.EarlyRenewal)
                        {
                            if (invoice.DatePaid != null && invoice.PaymentID > 0 && (!vm.CurrentContractIsEarlyRenewal || (DateTime)invoice.DatePaid < vm.EarlyRenewalStamp))
                            {
                                vm.CurrentContractIsEarlyRenewal = true;
                                vm.EarlyRenewalStamp = (DateTime)invoice.DatePaid;
                            }
                            if (invoice.EarlyRenewalStamp != null && (!vm.CurrentContractIsEarlyRenewal || (DateTime)invoice.EarlyRenewalStamp < vm.EarlyRenewalStamp))
                            {
                                vm.CurrentContractIsEarlyRenewal = true;
                                vm.EarlyRenewalStamp = (DateTime)invoice.EarlyRenewalStamp;
                            }
                        }
                    }
                }
            }

            Contract activeContract = Contract.GetActive(ActiveLogin.LoginID);
            if (activeContract != null)
            {
                vm.ForceAutoPay = activeContract.ContractPriceLevel.ContractTerms.ForceAutoPay;

                if (activeContract.IgnoreAutoPay)
                {
                    vm.ForceAutoPay = false;
                }

                // If the active contract will exire soon, let the user renew
                if (activeContract.DaysUntilExpiration <= ContractManager.MaxRenewalWindow && allContracts.FirstOrDefault(o => o.EffectiveDate.Date >= activeContract.ExpirationDate.Date) == null)
                {
                    vm.ShowContractRenewalButton = true;
                    vm.ContractExpirationDays = activeContract.DaysUntilExpiration;
                }

                if (!vm.CurrentContractIsEarlyRenewal)
                {
                    // If the active contract was an early renewal, let the user know
                    List<Invoice> invoices = Invoice.GetForContract(activeContract.ID);
                    foreach (Invoice invoice in invoices)
                    {
                        if (invoice.AddOnID == 0 && invoice.PaymentNumber <= 1 && invoice.EarlyRenewal)
                        {
                            if (invoice.DatePaid != null && invoice.PaymentID > 0 && (!vm.CurrentContractIsEarlyRenewal || (DateTime)invoice.DatePaid < vm.EarlyRenewalStamp))
                            {
                                vm.CurrentContractIsEarlyRenewal = true;
                                vm.EarlyRenewalStamp = (DateTime)invoice.DatePaid;
                            }
                            if (invoice.EarlyRenewalStamp != null && (!vm.CurrentContractIsEarlyRenewal || (DateTime)invoice.EarlyRenewalStamp < vm.EarlyRenewalStamp))
                            {
                                vm.CurrentContractIsEarlyRenewal = true;
                                vm.EarlyRenewalStamp = (DateTime)invoice.EarlyRenewalStamp;
                            }
                        }
                    }
                }
            }
            
            // If there is an in progress contract show the button to change the contract
            Contract inProgressContract = Contract.GetInProgress(ActiveLogin.LoginID);
            if (inProgressContract != null)
            {
                vm.ShowChangeContractButton = true;
            }

            // If there is a current web-est contract, show a message and hide the create button
            DateTime? webEstExpiration = ContractManager.GetWebEstExpiration(ActiveLogin.LoginID);
            if (webEstExpiration.HasValue && webEstExpiration.Value > DateTime.Now.AddDays(30))
            {
                vm.HasActiveWebEstContract = true;
            }

            // Get the current invoice total
            List<Invoice> dueInvoices = ContractManager.GetInvoicesToBePaid(ActiveLogin.LoginID);
            decimal invoiceTotal = 0;

            if (dueInvoices.Count > 0)
            {
                vm.NeedsPayment = true;
            }

            InvoiceVMMapper invoiceMapper = new InvoiceVMMapper();

            foreach (Invoice invoice in dueInvoices)
            {
                vm.DueInvoices.Add(invoiceMapper.Map(new InvoiceVMMapperConfiguration() { Invoice = invoice }));

                if (invoice.DueDate <= DateTime.Now)
                {
                    invoiceTotal += invoice.InvoiceTotal;
                }
            }

            if (invoiceTotal > 0)
            {
                vm.InvoiceAmount = invoiceTotal;
            }

            // If there are no contracts, check if there is a trial and set up the trial expiration message
            if (activeContract == null)
            {
                Trial trial = Trial.GetActive(ActiveLogin.LoginID);
                if (trial != null && trial.EndDate >= DateTime.Now.Date)
                {
                    vm.TrialExpirationMessage = string.Format(@Proestimator.Resources.ProStrings.TrialWillExpireOn, trial.EndDate.ToShortDateString());
                }
            }

            // Load info for display on the form
            StripeInfo stripeInfo = StripeInfo.GetForLogin(ActiveLogin.LoginID);
            if (stripeInfo != null && !string.IsNullOrEmpty(stripeInfo.StripeCardID))
            {
                vm.HasSavedPaymentInfo = true;
                vm.CardExpiration = stripeInfo.CardExpiration;
                vm.Last4 = stripeInfo.CardLast4;
                vm.AutoPaySelected = stripeInfo.AutoPay;

                if (vm.ForceAutoPay && !stripeInfo.AutoPay && (activeContract != null && !activeContract.IgnoreAutoPay))
                {
                    bool result = new TurnOnAutoPay(ActiveLogin.LoginID, "ForcedOnLoad").Execute();
                    if (result)
                    {
                        vm.AutoPaySelected = true;
                    }
                }

                if (stripeInfo.CardError)
                {
                    vm.CardHasError = true;
                    vm.CardErrorMessage = stripeInfo.ErrorMessage;
                }
            }
            else
            {
                vm.HasSavedPaymentInfo = false;
            }

            // Fill the auto renew history
            List<LoginAutoRenew> autoRenews = LoginAutoRenew.GetForLogin(ActiveLogin.LoginID);
            vm.AutoRenewOn = false;

            vm.AutoRenewDetails = new List<AutoRenewDetailVM>();
            foreach (LoginAutoRenew autoRenew in autoRenews)
            {
                vm.AutoRenewDetails.Add(new AutoRenewDetailVM(autoRenew));
            }

            if (autoRenews.Count > 0)
            {
                vm.AutoRenewOn = autoRenews[0].Enabled;
            }

            vm.StripeKey = ConfigurationManager.AppSettings.Get("StripePublishableKey").ToString();

            return vm;
        }

        public ActionResult GetInvoicesForContractOrAddOn(
              [DataSourceRequest] DataSourceRequest request
            , int userID
            , int loginID
            , int contractID
            , int addOnID
        )
        {
            List<InvoiceVM> invoiceList = new List<InvoiceVM>();

            if (IsUserAuthorized(userID))
            {
                // Make sure the contract belongs to the login
                Contract contract = Contract.Get(contractID);

                if (contract != null && contract.LoginID == loginID)
                {
                    List<Invoice> invoices = Invoice.GetForContract(contract.ID, true);

                    if (addOnID > 0)
                    {
                        invoices = invoices.Where(o => o.AddOnID == addOnID).ToList();
                    }

                    List<Invoice> invoicesDelete = ContractManager.InvoiceDeletable(addOnID);
                    InvoiceVMMapper invoiceMapper = new InvoiceVMMapper();

                    foreach (Invoice invoice in invoices.OrderBy(o => o.AddOnID).ThenBy(o => o.DueDate))
                    {
                        invoiceList.Add(invoiceMapper.Map(new InvoiceVMMapperConfiguration() { Invoice = invoice, Deletable = invoicesDelete.FirstOrDefault(o => o.ID == invoice.ID) == null ? false : true }));
                    }
                }
            }

            return Json(invoiceList.ToDataSourceResult(request));
        }

        public async Task<JsonResult> CreateContractReportAttachment(int userID, int loginID, int contractID, string reportType)
        {
            CreateReportJson reportJson = new CreateReportJson();

            try
            {
                if (IsUserAuthorized(userID))
                {
                    ReportFunctionResult result = null;

                    // Make sure the contract belongs to the login
                    Contract contract = Contract.Get(contractID);

                    if (contract != null && contract.LoginID == loginID)
                    {
                        if (reportType == "ContractInvoices")
                        {
                            ReportGenerator generator = new ReportGenerator();
                            result = await generator.GenerateReport(contract.LoginID, contractID, reportType);
                            result.ReportFullName = ProEstimatorData.DataModel.Report.GetReportNameForContract(contractID, reportType) + ".pdf";
                            //result.ReportFullName = "ContractInvoices.pdf";
                        }

                        if (result != null)
                        {
                            if (result.Success)
                            {
                                reportJson.Report = new ReportVM(result.Report.ID, result.Report.ReportType.Tag, result.ReportFullName);
                                reportJson.ErrorMessage = result.ErrorMessage;      // There could be extra message even if the report was successful
                            }
                            else
                            {
                                reportJson.Success = false;
                                reportJson.ErrorMessage = result.ErrorMessage;
                            }
                        }
                        else
                        {
                            reportJson.Success = false;
                            reportJson.ErrorMessage = @Proestimator.Resources.ProStrings.NoResultFromReportGenerator;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, GetActiveLoginID(userID), "CreateReportAttachment GenerateReport");
            }

            return Json(reportJson, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("{userID}/{loginID}/{contractID}/view-contract-report-attachment/{attachment}/{filename}")]
        public ActionResult ViewContractReportAttachment(int userID, int loginID, int contractID, string attachment, string filename = "")
        {
            // Make sure the contract belongs to the login
            Contract contract = Contract.Get(contractID);
            if (contract != null && contract.LoginID == loginID)
            {
                int idInt = InputHelper.GetInteger(attachment);
                if (idInt > 0)
                {
                    ProEstimatorData.DataModel.Report report = ProEstimatorData.DataModel.Report.Get(idInt);

                    if (report != null && report.ReportContractBelongsToLogin(ActiveLogin.LoginID))
                    {
                        string diskPath = report.GetContractReportDiskPath();
                        if (!string.IsNullOrEmpty(diskPath) && System.IO.File.Exists(diskPath))
                        {
                            var fileStream = new FileStream(diskPath, FileMode.Open, FileAccess.Read);
                            if (report.GetFileExtension() == "pdf")
                            {
                                PDFDownloadSetting download = (PDFDownloadSetting)InputHelper.GetInteger(SiteSettings.Get(ActiveLogin.LoginID, "DownloadPDF", "ReportOptions", ((int)PDFDownloadSetting.OpenNewTab).ToString()).ValueString);

                                if (download == PDFDownloadSetting.Download)
                                {
                                    Response.Headers.Add("Content-Disposition", "attachment");
                                }

                                //return new FileStreamResult(fileStream, "application/pdf") { FileDownloadName = report.FileName + "." + report.GetFileExtension() };
                                return new FileStreamResult(fileStream, "application/pdf");
                                //return File(fileStream, "application/pdf", report.FileName);
                            }
                            else
                            {
                                return File(diskPath, report.GetContentType(), report.FileName + "." + report.GetFileExtension());
                            }
                        }
                    }
                }
            }

            //filename from querystring or estimate id from session is not valid...show the error message instead of blank screen.
            return Content("Error: report not found.");
        }

        public JsonResult ChangeAutoPay(int userID, int loginID, bool isAutoPay)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                if (isAutoPay)
                {
                    new TurnOnAutoPay(loginID, "UserClicked").Execute();
                }
                else
                {
                    new TurnOffAutoPay(loginID, "UserClicked").Execute();
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("{userID}/settings/delete-cc")]
        public ActionResult DeleteCC(int userID)
        {
            if (IsUserAuthorized(userID))
            {
                _stripeService.DeleteStripeCreditCard(ActiveLogin.LoginID);
            }

            return Redirect("/" + userID + "/settings/billing");
        }

        public JsonResult DeleteCCInfo(int userID, int loginID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                string result = "";

                FunctionResult functionResult = _stripeService.DeleteStripeCreditCard(ActiveLogin.ID);
                if (!functionResult.Success)
                {
                    result = functionResult.ErrorMessage;
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json(@Proestimator.Resources.ProStrings.UnauthorizedLogin, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ClearCCError(int userID, int loginID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                StripeInfo stripeInfo = StripeInfo.GetForLogin(loginID);
                stripeInfo.ErrorMessage = "";
                stripeInfo.CardError = false;

                SaveResult saveResult = stripeInfo.Save();

                return Json(saveResult.ErrorMessage, JsonRequestBehavior.AllowGet);
            }

            return Json(@Proestimator.Resources.ProStrings.UnauthorizedLogin, JsonRequestBehavior.AllowGet);
        }

        [Route("{userID}/settings/view-signature/{id}")]
        public ActionResult ViewReport(int userID, string id)
        {
            ContractDigitalSignature digitalSignature = ContractDigitalSignature.Get(InputHelper.GetInteger(id));
            if (digitalSignature != null && digitalSignature.LoginID == ActiveLogin.LoginID)
            {
                string diskPath = DigitalSignaturePrintManager.GetDiskPath(digitalSignature);

                if (!DigitalSignaturePrintManager.DoesPrintExist(digitalSignature))
                {
                    FunctionResult result = DigitalSignaturePrintManager.CreateReport(digitalSignature);
                    if (!result.Success)
                    {
                        return Content("Error: " + result.ErrorMessage);
                    }
                }

                if (!string.IsNullOrEmpty(diskPath) && System.IO.File.Exists(diskPath))
                {
                    var fileStream = new FileStream(diskPath, FileMode.Open, FileAccess.Read);
                    var fsResult = new FileStreamResult(fileStream, "application/pdf");
                    return fsResult;
                }
            }

            //filename from querystring or estimate id from session is not valid...show the error message instead of blank screen.
            return Content(@Proestimator.Resources.ProStrings.SomethingWentWrongOpenGenerateReport);
        }

        public ActionResult ViewContract(int id)
        {
            ContractDigitalSignature digitalSignature = ContractDigitalSignature.Get(id);
            if (digitalSignature != null)
            {
                string diskPath = DigitalSignaturePrintManager.GetDiskPath(digitalSignature);

                if (!DigitalSignaturePrintManager.DoesPrintExist(digitalSignature))
                {
                    FunctionResult result = DigitalSignaturePrintManager.CreateReport(digitalSignature);
                    if (!result.Success)
                    {
                        return Content("Error: " + result.ErrorMessage);
                    }
                }

                if (!string.IsNullOrEmpty(diskPath) && System.IO.File.Exists(diskPath))
                {
                    var fileStream = new FileStream(diskPath, FileMode.Open, FileAccess.Read);
                    var fsResult = new FileStreamResult(fileStream, "application/pdf");
                    return fsResult;
                }
            }

            //filename from querystring or estimate id from session is not valid...show the error message instead of blank screen.
            return Content(@Proestimator.Resources.ProStrings.SomethingWentWrongOpenGenerateReport);
        }

        #endregion

        #region User Accounts

        [HttpGet]
        [Route("{userID}/settings/users")]
        public ActionResult UserAccounts(int userID)
        {
            if (!PermissionManager.HasPermission("Admin"))
            {
                return Redirect("/" + userID);
            }

            UserAccountsPageVM vm = new UserAccountsPageVM();
            vm.LoginID = ActiveLogin.LoginID;

            int logins = 1;
            Contract activeContract = Contract.GetActive(ActiveLogin.LoginID);
            if (activeContract != null)
            {
                List<ContractAddOn> muAddOns = ContractAddOn.GetForContract(activeContract.ID).Where(o => o.AddOnType.ID == 8 && o.Active && o.HasPayment).ToList();
                muAddOns.ForEach(o => { logins += o.Quantity; });
                List<ContractAddOnTrial> muTrialAddOns = ContractAddOnTrial.GetForContract(activeContract.ID).Where(o => o.AddOnType.ID == 8 && o.StartDate < DateTime.Now && o.EndDate > DateTime.Now).ToList();
                logins += muTrialAddOns.Count;
            }
            vm.MaxUsers = logins;

            ViewBag.NavID = "settings";
            ViewBag.settingsTopMenuID = 7;
            ViewBag.HasMUContract = ActiveLogin.HasMultiUserContract;

            return View(vm);
        }

        public ActionResult GetSiteUsers([DataSourceRequest] DataSourceRequest request, int userID, int loginID, bool deleted = false)
        {
            List<SiteUserVM> gridRows = new List<SiteUserVM>();

            if (IsUserAuthorized(userID))
            {
                List<SiteUser> siteUsers = SiteUser.GetForLogin(loginID, deleted);
                foreach (SiteUser siteUser in siteUsers)
                {
                    gridRows.Add(new SiteUserVM(siteUser));
                }
            }

            return Json(gridRows.ToDataSourceResult(request));
        }

        public JsonResult GetSiteUser(int userID, int loginID, int targetUserID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                SiteUser siteUser = SiteUser.Get(targetUserID);
                if (siteUser != null && siteUser.LoginID == loginID)
                {
                    return Json(new SiteUserFunctionResult(siteUser), JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new SiteUserFunctionResult("Invalid site user."), JsonRequestBehavior.AllowGet);
        }

        public class SiteUserFunctionResult : FunctionResult
        {
            public SiteUserVM SiteUser { get; set; }

            // public List<SiteUserPermissionVM> Permissions { get; set; }

            public SiteUserFunctionResult(SiteUser siteUser, string extraMessage = "")
                : base()
            {
                SiteUser = new SiteUserVM(siteUser);

                //Permissions = new List<SiteUserPermissionVM>();

                //List<SitePermission> permissions = SitePermission.GetAll();
                //SitePermissionsManager manager = new SitePermissionsManager(siteUser.ID);

                //foreach (SitePermission permission in permissions)
                //{
                //    Permissions.Add(new SiteUserPermissionVM(permission, manager.HasPermission(permission.Tag)));
                //}

                ErrorMessage = extraMessage;
            }

            public SiteUserFunctionResult(string errorMessage)
                : base(errorMessage)
            {

            }
        }

        public ActionResult GetPermissions([DataSourceRequest] DataSourceRequest request, int userID, int siteUserID)
        {
            List<SiteUserPermissionVM> Permissions = new List<SiteUserPermissionVM>();

            if (IsUserAuthorized(userID))
            {
                List<SitePermission> permissions = SitePermission.GetAll();
                SitePermissionsManager manager = new SitePermissionsManager(siteUserID);

                foreach (SitePermission permission in permissions)
                {
                    Permissions.Add(new SiteUserPermissionVM(permission, siteUserID == 0 ? false : manager.HasPermission(permission.Tag)));
                }
            }
            return Json(Permissions.ToDataSourceResult(request));
        }

        public JsonResult SaveSiteUser(int userID, int loginID, int siteUserID, string email, string name, string password, string permissions)     // , string permissions
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                SiteUser siteUser = SiteUser.Get(siteUserID);

                if (siteUser == null)
                {
                    siteUser = new SiteUser();
                    siteUser.LoginID = loginID;
                }

                if (siteUser.LoginID == loginID)
                {
                    siteUser.EmailAddress = email;
                    siteUser.Name = name;

                    ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                    SaveResult saveResult = siteUser.Save(activeLogin.ID);
                    if (saveResult.Success)
                    {
                        // Update permissions
                        SitePermissionsManager manager = new SitePermissionsManager(siteUser.ID);

                        string[] permissionsList = permissions.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        foreach (string permission in permissionsList)
                        {
                            string[] permissionPieces = permission.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            string tag = permissionPieces[0];
                            bool isChecked = InputHelper.GetBoolean(permissionPieces[1]);

                            // Don't let the active user turn off admin permission for themself
                            if (userID == siteUserID && tag == "Admin")
                            {
                                isChecked = true;
                            }

                            manager.SavePermission(tag, isChecked);
                        }

                        // Update the password if one was passed
                        string extraMessage = "";

                        if (!string.IsNullOrEmpty(password) && siteUser.Password != password)
                        {
                            FunctionResult result = _siteLoginManager.ChangeUserPassword(activeLogin.ID, siteUser.ID, password);

                            if (result.Success)
                            {
                                extraMessage = Proestimator.Resources.ProStrings.Message_PasswordChanged;
                            }
                            else
                            {
                                return Json(new SiteUserFunctionResult(result.ErrorMessage), JsonRequestBehavior.AllowGet);
                            }
                        }

                        return Json(new SiteUserFunctionResult(siteUser, extraMessage), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        SiteUserFunctionResult functionResult = new SiteUserFunctionResult(siteUser, saveResult.ErrorMessage);
                        functionResult.Success = false;

                        //string[] permissionsList = permissions.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        //foreach (string permission in permissionsList)
                        //{
                        //    string[] permissionPieces = permission.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        //    string tag = permissionPieces[0];
                        //    bool isChecked = InputHelper.GetBoolean(permissionPieces[1]);

                        //    SitePermission sitePermission = SitePermission.GetForTag(tag);
                        //    functionResult.Permissions.Add(new SiteUserPermissionVM(sitePermission, isChecked));
                        //}

                        return Json(functionResult, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            return Json(new SiteUserFunctionResult("Invalid user information."), JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteSiteUser(int userID, int loginID, int siteUserID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                SiteUser siteUser = SiteUser.Get(siteUserID);

                if (siteUser != null && siteUser.LoginID == loginID)
                {
                    siteUser.IsDeleted = !siteUser.IsDeleted;

                    SaveResult saveResult = siteUser.Save();
                    if (saveResult.Success)
                    {
                        return Json(new SiteUserFunctionResult(siteUser), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new SiteUserFunctionResult(saveResult.ErrorMessage), JsonRequestBehavior.AllowGet);
                    }
                }
            }

            return Json(new SiteUserFunctionResult("Invalid user information."), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Technicians

        [HttpGet]
        [Route("{userID}/settings/technicians")]
        public ActionResult Technicians(int userID)
        {
            TechnicianVM vmodel = new TechnicianVM();

            vmodel.LoginID = ActiveLogin.LoginID;

            // Fill the Labor Type List
            List<SimpleListItem> laborTypeList = ProEstHelper.GetLaborTypeList();
            vmodel.LaborTypeList = new SelectList(laborTypeList, "Value", "Text");

            ViewBag.NavID = "settings";
            ViewBag.settingsTopMenuID = 5;
            ViewBag.HasMUContract = ActiveLogin.HasMultiUserContract;

            return View(vmodel);
        }

        public JsonResult GetTechnician(int userID, int technicianID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                Technician estimator = Technician.Get(technicianID);

                if (estimator != null && estimator.LoginID == activeLogin.LoginID)
                {
                    TechnicianVM technicianVM = new TechnicianVM();
                    technicianVM.FirstName = estimator.FirstName;
                    technicianVM.LastName = estimator.LastName;
                    technicianVM.LaborTypeID = estimator.LaborTypeID;
                    technicianVM.LaborTypeText = estimator.LaborTypeText;

                    return Json(technicianVM, JsonRequestBehavior.AllowGet);
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteTechnician(int userID, int technicianID, Boolean restoreDeleteTechnician)
        {
            CacheActiveLoginID(userID);

            string errorMessage = "";

            if (IsUserAuthorized(userID))
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                Technician technician = Technician.Get(technicianID);

                if (technician != null && technician.LoginID == activeLogin.LoginID)
                {
                    if (technician.ID == technicianID)
                    {
                        if (restoreDeleteTechnician == true)
                            technician.Restore();
                        else
                            technician.Delete();
                    }
                }
            }

            return Json(errorMessage, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTechnicians([DataSourceRequest] DataSourceRequest request, int userID, int loginID, string showDeletedTechnicians)
        {
            List<TechnicianVM> technicianList = new List<TechnicianVM>();

            if (IsUserAuthorized(userID))
            {
                List<Technician> technicians = Technician.GetByLogin(loginID, showDeletedTechnicians);

                foreach (Technician technician in technicians)
                {
                    technicianList.Add(new TechnicianVM(technician));
                }
            }

            return Json(technicianList.ToDataSourceResult(request));
        }

        public JsonResult SaveTechnician(int userID, int technicianID, string firstName, string lastName, int laborTypeID, string laborTypeText)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                Technician technician = new Technician();

                if (technicianID > 0)
                {
                    technician = Technician.Get(technicianID);
                }
                else
                {
                    technician.LoginID = activeLogin.LoginID;
                }

                if (technician != null && technician.LoginID == activeLogin.LoginID)
                {
                    technician.FirstName = firstName;
                    technician.LastName = lastName;
                    technician.LaborTypeID = laborTypeID;
                    technician.LaborTypeText = laborTypeText;

                    SaveResult saveResult = technician.Save(GetActiveLoginID(userID));
                    return Json(saveResult.ErrorMessage, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(@Proestimator.Resources.ProStrings.InvalidLoginIDEstimateID, JsonRequestBehavior.AllowGet);
        }

        public bool ReOrderTechnicians(List<string> data, int userID)
        {
            try
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                List<Technician> technicians = Technician.GetByLogin(activeLogin.LoginID);
                int index = 0;
                foreach (var technicianId in data)
                {
                    Technician technician = technicians.FirstOrDefault(x => x.ID == int.Parse(technicianId));
                    if (technician != null)
                    {
                        technician.OrderNumber = index++;
                        technician.Save(GetActiveLoginID(userID));
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {

            }

            return false;
        }

        #endregion

        #region Email Templates

        [HttpGet]
        [Route("{userID}/settings/email-template")]
        public ActionResult EmailTemplate(int userID)
        {
            EmailTemplateVM vmodel = new EmailTemplateVM();

            vmodel.LoginID = ActiveLogin.LoginID;

            ViewBag.NavID = "settings";
            ViewBag.settingsTopMenuID = 6;

            return View(vmodel);
        }

        public ActionResult GetTemplates([DataSourceRequest] DataSourceRequest request, int loginID, bool showDeleted)
        {
            var templateList = new List<EmailTemplateVM>();
            var templates = EmailCustomTemplate.GetForLogin(loginID, showDeleted);

            foreach (EmailCustomTemplate template in templates)
            {
                templateList.Add(new EmailTemplateVM(template));
            }
            
            return Json(templateList.ToDataSourceResult(request));
        }

        public JsonResult SaveTemplate(int userID, int templateID, string template, string name, string description, bool isDefault)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                var activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                var emailTemplate = new EmailCustomTemplate();

                if (templateID > 0)
                {
                    emailTemplate = EmailCustomTemplate.Get(templateID);
                }

                if (emailTemplate == null)
                {
                    emailTemplate.LoginID = activeLogin.LoginID;
                }

                emailTemplate.LoginID = activeLogin.LoginID;
                emailTemplate.Name = name;
                emailTemplate.Description = description;
                emailTemplate.Template = template;
                emailTemplate.IsDefault = isDefault;

                SaveResult saveResult = emailTemplate.Save(GetActiveLoginID(userID));
                return Json(saveResult.ErrorMessage, JsonRequestBehavior.AllowGet);
            }

            return Json(@Proestimator.Resources.ProStrings.InvalidData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTemplate(int userID, int templateID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                var activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                var emailTemplate = EmailCustomTemplate.Get(templateID);
                if (emailTemplate != null && emailTemplate.LoginID == activeLogin.LoginID)
                {
                    var emailTemplateVM = new EmailTemplateVM();
                    emailTemplateVM.Name = emailTemplate.Name;
                    emailTemplateVM.Description = emailTemplate.Description;
                    emailTemplateVM.Template = emailTemplate.Template;
                    emailTemplateVM.IsDeleted = emailTemplate.IsDeleted;
                    emailTemplateVM.IsDefault = emailTemplate.IsDefault;
                    emailTemplateVM.CreatedDate = emailTemplate.CreatedDate;
                    emailTemplateVM.ModifiedDate = emailTemplate.ModifiedDate;

                    return Json(emailTemplateVM, JsonRequestBehavior.AllowGet);
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteTemplate(int userID, int templateID, bool restoreDeleteTemplate)
        {
            CacheActiveLoginID(userID);

            var errorMessage = "";

            if (IsUserAuthorized(userID))
            {
                var activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                var emailTemplate = EmailCustomTemplate.Get(templateID);
                if (emailTemplate != null && emailTemplate.LoginID == activeLogin.LoginID)
                {
                    emailTemplate.SetIsDeleted(activeLogin.ID, !restoreDeleteTemplate);
                }
            }

            return Json(errorMessage, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}
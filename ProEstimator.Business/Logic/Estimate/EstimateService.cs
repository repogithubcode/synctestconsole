using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;
using System.Data.SqlClient;

using ProEstimator.Business.ILogic;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Profiles;
using ProEstimatorData.DataModel.Contracts;
using ProEstimatorData.Models;

using Parameter = Telerik.Reporting.Parameter;
using Report = ProEstimatorData.DataModel.Report;

using Ghostscript.NET.Rasterizer;
using ProEstimator.Business.Model.Financing;
using System.Web.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ProEstimatorData.DataModel.PDR;
using ProEstimatorData.DataModel.ProAdvisor;
using Proestimator.ViewModel;
using ProEstimator.Business.Model;
using Proestimator.Models.LicensePlateReader;
using ProEstimatorData.Models.SubModel;
using ProEstimator.Business.ProAdvisor;
using Kendo.Mvc.UI;

namespace ProEstimator.Business.Logic
{
    public class EstimateService : IEstimateService
    {
        private IProAdvisorProfileService _proAdvisorProfileService;

        public EstimateService(IProAdvisorProfileService proAdvisorProfileService)
        {
            _proAdvisorProfileService = proAdvisorProfileService;
        }

        public Estimate CreateNewEstimate(ActiveLogin activeLogin)
        {
            LoginInfo loginInfo = LoginInfo.GetByID(activeLogin.LoginID);
            loginInfo.LastEstimateNumber++;
            loginInfo.Save();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", activeLogin.LoginID));
            parameters.Add(new SqlParameter("EstimateNumber", loginInfo.LastEstimateNumber));
            parameters.Add(new SqlParameter("ActiveLoginID", activeLogin.ID));

            DBAccess db = new DBAccess();
            DBAccessIntResult newEstimateResult = db.ExecuteWithIntReturn("Estimate_CreateNew", parameters);

            if (newEstimateResult.Success)
            {
                Estimate estimate = new Estimate(newEstimateResult.Value);

                // Attach a blank customer record
                Customer newCustomer = new Customer();
                newCustomer.LoginID = activeLogin.LoginID;
                newCustomer.Save(activeLogin.ID);

                estimate.CustomerID = newCustomer.ID;

                // Default Rate Profile
                if (loginInfo.UseDefaultRateProfile == false)
                {
                    List<RateProfile> rateProfiles = RateProfile.GetAllForLogin(activeLogin.LoginID);

                    if (rateProfiles != null && rateProfiles.Count == 1)
                    {
                        if (rateProfiles[0].IsDefault == false)
                        {
                            rateProfiles[0].SetAsDefaultProfile(); // set customer profiles flag set to 1
                        }

                        // following updates flag in logininfo table for for UseDefaultRateProfile
                        loginInfo.UseDefaultRateProfile = !loginInfo.UseDefaultRateProfile;
                        loginInfo.Save();

                        loginInfo = LoginInfo.GetByID(activeLogin.LoginID);
                        if (loginInfo.UseDefaultRateProfile)
                        {
                            RateProfile defaultProfile = RateProfile.GetAllForLogin(activeLogin.LoginID).FirstOrDefault(o => o.IsDefault);
                            if (defaultProfile != null)
                            {
                                estimate.CustomerProfileID = RateProfileManager.CopyProfile(activeLogin.LoginID, defaultProfile.ID, estimate.EstimateID);
                                
                                // Set the Credit Card fee and flag based the values on the rate profile
                                estimate.CreditCardFeePercentage = defaultProfile.CreditCardFeePercentage;
                                estimate.ApplyCreditCardFee = defaultProfile.ApplyCreditCardFee;
                                estimate.TaxedCreditCardFee = defaultProfile.TaxedCreditCardFee;
                            }
                        }
                    }
                }

                // ProAdvisor rate profile
                if (((SiteActiveLogin)activeLogin).HasProAdvisorContract)
                {
                    List<ProAdvisorPresetProfile> addOnPresetProfiles = _proAdvisorProfileService.GetAllProfilesForLogin(activeLogin.LoginID, false);
                    bool useDefault = _proAdvisorProfileService.UseDefaultProfile(activeLogin.LoginID);

                    if (useDefault == false || (addOnPresetProfiles != null && addOnPresetProfiles.Count == 0))
                    {
                        if (addOnPresetProfiles != null && addOnPresetProfiles.Count == 0)
                        {
                            _proAdvisorProfileService.SetUseDefaultProfile(activeLogin.ID, activeLogin.LoginID, !useDefault);
                        }

                        if ((_proAdvisorProfileService.UseDefaultProfile(activeLogin.LoginID))
                             || (addOnPresetProfiles != null && addOnPresetProfiles.Count == 0))
                        {
                            ProAdvisorPresetProfile defaultProfile = _proAdvisorProfileService.GetDefaultProfile(activeLogin.LoginID);
                            if (defaultProfile != null)
                            {
                                estimate.AddOnProfileID = defaultProfile.ID;
                            }
                            else
                            {
                                estimate.AddOnProfileID = 1;
                            }
                        }
                    }
                }

                estimate.Save(activeLogin.ID);

                // For success box, log the new estimate differently for the first 7 days and for after the first 7 days
                Contract activeContract = Contract.GetActive(activeLogin.LoginID);
                if (activeContract != null && activeContract.EffectiveDate > DateTime.Now.AddDays(-7))
                {
                    SuccessBoxFeatureLog.LogFeature(activeLogin.LoginID, SuccessBoxModule.EstimateWriting, "New estimate first week", activeLogin.ID);
                }
                else
                {
                    SuccessBoxFeatureLog.LogFeature(activeLogin.LoginID, SuccessBoxModule.EstimateWriting, "New estimate", activeLogin.ID);
                }

                return estimate;
            }

            return null;
        }

        public int CopyEstimate(int activeLoginID, int estimateID, string estimateName, bool newCustomer, bool copyInsuranceInformation, bool copyClaimantInformation, bool copyVehicleInformation, bool copyAttachedImages, bool copyLineItems, bool copyLatestItemsOnly)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("AdminInfoID", estimateID));
            parameters.Add(new SqlParameter("EstimateName", estimateName));
            parameters.Add(new SqlParameter("CopyInsurance", copyInsuranceInformation));
            parameters.Add(new SqlParameter("CopyClaimant", copyClaimantInformation));
            parameters.Add(new SqlParameter("CopyVehicle", copyVehicleInformation));
            parameters.Add(new SqlParameter("CopyImages", copyAttachedImages));
            parameters.Add(new SqlParameter("CopyLineItems", copyLineItems));
            parameters.Add(new SqlParameter("LatestInfoOnly", copyLatestItemsOnly));

            DBAccess db = new DBAccess();
            DBAccessIntResult intResult = db.ExecuteWithIntReturn("CopyEstimate", parameters);
            if (intResult.Success)
            {
                Estimate copyEstimate = new Estimate(estimateID);
                Customer copyCustomer = Customer.Get(copyEstimate.CustomerID);
                if (newCustomer || copyCustomer == null || (copyCustomer.Contact.NotValid() && copyCustomer.Address.NotValid()))
                {
                    Estimate estimate = new Estimate(intResult.Value);

                    Customer customer = new Customer();
                    customer.LoginID = estimate.CreatedByLoginID;
                    customer.Save(activeLoginID);

                    estimate.CustomerID = customer.ID;
                    estimate.Save(activeLoginID);
                }

                return intResult.Value;
            }

            return 0;
        }

        public async Task<ReportFunctionResult> MakeQuickPrintEstimateReport(int userID, int loginID, int estimateID, int activeLoginID)
        {
            Estimate.RefreshProcessedLines(estimateID);

            Estimate estimate = new Estimate(estimateID);
            if (estimate != null && estimate.CreatedByLoginID == loginID)
            {
                bool printInspectionDate = InputHelper.GetBoolean(SiteSettings.Get(loginID, "PrintInspectionDate", "ReportOptions", (true).ToString()).ValueString);
                bool printPnLNotes = InputHelper.GetBoolean(SiteSettings.Get(loginID, "IncludePartNotesOnPdfLineEntry", "ReportOptions", (true).ToString()).ValueString);

                bool printIncludeImages = true;
                bool printHideHourlyRates = false;
                ProEstimatorData.DataModel.Profiles.PrintSettings printSettings = ProEstimatorData.DataModel.Profiles.PrintSettings.GetForProfile(estimate.CustomerProfileID);
                if (printSettings != null)
                {
                    printIncludeImages = !printSettings.NoPhotos;
                    printHideHourlyRates = printSettings.Dollars;
                }

                ProEstimatorData.DataModel.Report report = await CreateEstimateReport(estimate.EstimateID, "Estimate", loginID, activeLoginID, printHideHourlyRates, null, printIncludeImages, false, false, false, printPnLNotes, printInspectionDate);

                return new ReportFunctionResult(report);
            }
            else
            {
                return new ReportFunctionResult("Error generating report, bad estimate or account details passed.");
            }
        }

        public async Task<ProEstimatorData.DataModel.Report> CreateEstimateReport(int estimateID, string reportType, int loginID, int activeLoginID, bool hideHours = false, int? supplementVersion = null, bool includeImages = true, bool onlySupplementImages = false, bool onlyCheckedImages = false, bool pdrOnly = false, bool PrintPnLNotes = false, bool PrintInspectionDate = false
                                        , bool attachImagesWithEmail = false, bool printSupplementPhotosOnly = false, bool? printHeaderInfoOnEstimateReport = null, bool PrintRepairDays = false, bool IncludeLoanApplicationLink = false, string selectedTechnicians = "")
        {
            Estimate estimate = new Estimate(estimateID);
            LoginInfo loginInfo = LoginInfo.GetByID(loginID);
            PrintSettings printSettings = PrintSettings.GetForProfile(estimate.CustomerProfileID);

            // If there is no supplement for the estimate, pass -1 as the supplement level to the report creator so it uses the estimate report without a supplement section
            if (estimate.LockLevel == 0)
            {
                supplementVersion = -1;
            }

            if (printSupplementPhotosOnly && estimate.LockLevel > 0)
            {
                supplementVersion = estimate.LockLevel;
                onlySupplementImages = true;
            }

            // Set the image size based on the rate profile selection
            int imageSize = 2000;
            int imageSizePDF = 2000;

            if (printSettings != null)
            {
                if (printSettings.GraphicsQuality == 0)
                {
                    imageSize = 500;
                    imageSizePDF = 1000;
                }
                else if (printSettings.GraphicsQuality == 1)
                {
                    imageSize = 1000;
                    imageSizePDF = 1000;
                }
            }

            // If there is no data in the current supplement use the last version
            DBAccess db = new DBAccess();
            DBAccessIntResult lastSupplementResult = db.ExecuteWithIntReturn("Estimate_GetHighestUsedEstimate", new System.Data.SqlClient.SqlParameter("AdminInfoID", estimateID));

            int lastSupplement = 0;
            if (lastSupplementResult.Success)
            {
                lastSupplement = lastSupplementResult.Value;
            }

            int lockLevel = estimate.LockLevel;
            if (lockLevel > lastSupplement)
            {
                lockLevel = lastSupplement;
            }

            List<Telerik.Reporting.Parameter> parameters = new List<Telerik.Reporting.Parameter>();

            parameters.Add(new Telerik.Reporting.Parameter("EOROnly", (supplementVersion.HasValue && supplementVersion == -1)));

            if (supplementVersion.HasValue && supplementVersion > lastSupplement)
            {
                supplementVersion = lastSupplement;
            }

            List<string> newImagePaths = new List<string>();

            List<EstimationImage> images = null;

            if (includeImages || attachImagesWithEmail)
            {
                newImagePaths = CreateImageByResolution(imageSize, imageSizePDF, estimateID, loginID, onlySupplementImages ? supplementVersion.Value : -1, onlyCheckedImages, out images);
            }

            // Adding the initial parameter values
            parameters.Add(new Telerik.Reporting.Parameter("ContentDirectory", ConfigurationManager.AppSettings.Get("UserContentPath").ToString()));
            if (imageSize > 0)
            {
                parameters.Add(new Telerik.Reporting.Parameter("ImageSizeParameter", imageSize));
                parameters.Add(new Telerik.Reporting.Parameter("ImageSizeParameterPDF", imageSizePDF));
            }
            parameters.Add(new Telerik.Reporting.Parameter("PrintRepairDays", PrintRepairDays ? 1 : 0));
            parameters.Add(new Telerik.Reporting.Parameter("IncludeImages", includeImages));
            parameters.Add(new Telerik.Reporting.Parameter("PdrOnly", pdrOnly ? 1 : 0));
            parameters.Add(new Telerik.Reporting.Parameter("PrintPnLNotes", PrintPnLNotes ? 1 : 0));
            parameters.Add(new Telerik.Reporting.Parameter("PrintInspectionDate", PrintInspectionDate ? 1 : 0));
            parameters.Add(new Telerik.Reporting.Parameter("SelectedTechnicians", selectedTechnicians));

            parameters.Add(new Telerik.Reporting.Parameter("IncludeLoanApplicationLink", IncludeLoanApplicationLink));

            if (IncludeLoanApplicationLink)
            {
                var monthlyAmountAndloanAppLink = await FinancingService.Instance.GetFinancingMonthlyAmountAndLoanAppLink(loginID, estimate);
                parameters.Add(new Telerik.Reporting.Parameter("FinancingAsLowAsMonthlyAmount", monthlyAmountAndloanAppLink.Item1));
                parameters.Add(new Telerik.Reporting.Parameter("FinancingLoanApplicationLink", monthlyAmountAndloanAppLink.Item2));
            }
            else
            {
                parameters.Add(new Telerik.Reporting.Parameter("FinancingAsLowAsMonthlyAmount", string.Empty));
                parameters.Add(new Telerik.Reporting.Parameter("FinancingLoanApplicationLink", string.Empty));
            }

            if (printHeaderInfoOnEstimateReport == null)
            {
                parameters.Add(new Telerik.Reporting.Parameter("PrintHeaderInfoOnEstimateReport", InputHelper.GetBoolean(SiteSettings.Get(loginID, "PrintHeaderInfoOnEstimateReport", "ReportOptions", true.ToString()).ValueString)));
            }
            else
            {
                if (printHeaderInfoOnEstimateReport == true)
                {
                    parameters.Add(new Telerik.Reporting.Parameter("PrintHeaderInfoOnEstimateReport", 1));
                }
                else
                {
                    parameters.Add(new Telerik.Reporting.Parameter("PrintHeaderInfoOnEstimateReport", 0));
                }
            }

            int imageSupplementVersion = onlySupplementImages ? supplementVersion.Value : -1;
            if (imageSupplementVersion < -1)
            {
                imageSupplementVersion = -1;
            }
            parameters.Add(new Telerik.Reporting.Parameter("ImageSupplement", imageSupplementVersion));
            parameters.Add(new Telerik.Reporting.Parameter("ImageOnlyChecked", onlyCheckedImages));

            parameters.Add(new Telerik.Reporting.Parameter("ImageDirectory", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images") + "\\"));
            parameters.Add(new Telerik.Reporting.Parameter("Supplement", supplementVersion.HasValue && supplementVersion.Value > -1 ? supplementVersion.Value : lockLevel));
            parameters.Add(new Telerik.Reporting.Parameter("SupplementOnly", supplementVersion.HasValue && supplementVersion.Value > 0 ? true : false));
            parameters.Add(new Telerik.Reporting.Parameter("HeaderText", string.IsNullOrEmpty(estimate.ReportTextHeader) ? "Estimate" : estimate.ReportTextHeader));
            parameters.Add(new Telerik.Reporting.Parameter("EstimatorOrAppraiser", loginInfo.Appraiser ? "Appraiser" : "Estimator"));

            DateTime timeStamp = DateTime.Now.AddHours(InputHelper.GetInteger(SiteSettings.Get(loginID, "TimeZone", "ReportOptions", "0").ValueString));
            parameters.Add(new Telerik.Reporting.Parameter("TimeStamp", timeStamp.ToShortDateString() + " " + timeStamp.ToShortTimeString()));

            // Add font size setting values
            parameters.Add(new Telerik.Reporting.Parameter("FontSizeDetails", InputHelper.GetInteger(SiteSettings.Get(loginID, "FontSizeDetails", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));
            parameters.Add(new Telerik.Reporting.Parameter("FontSizeHeader", InputHelper.GetInteger(SiteSettings.Get(loginID, "FontSizeHeader", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));
            parameters.Add(new Telerik.Reporting.Parameter("FontSizeHeaders", InputHelper.GetInteger(SiteSettings.Get(loginID, "FontSizeHeaders", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));
            parameters.Add(new Telerik.Reporting.Parameter("FontSizeLines", InputHelper.GetInteger(SiteSettings.Get(loginID, "FontSizeLines", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));
            parameters.Add(new Telerik.Reporting.Parameter("FontSizeTableHeaders", InputHelper.GetInteger(SiteSettings.Get(loginID, "FontSizeTableHeaders", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));
            parameters.Add(new Telerik.Reporting.Parameter("FontSizeTotals", InputHelper.GetInteger(SiteSettings.Get(loginID, "FontSizeTotals", "ReportFonts", ((int)FontSize.Medium).ToString()).ValueString)));

            parameters.Add(new Telerik.Reporting.Parameter("VehicleOptionsInFooter", InputHelper.GetBoolean(SiteSettings.Get(loginID, "VehicleOptionsInFooter", "ReportOptions", true.ToString()).ValueString)));
            parameters.Add(new Telerik.Reporting.Parameter("PrintShopInfo", InputHelper.GetBoolean(SiteSettings.Get(loginID, "PrintShopInfo", "ReportOptions", true.ToString()).ValueString)));

            // Get the rate profile for the current estimate to pass parameter values
            if (printSettings != null)
            {
                bool showPhotosSetting = !printSettings.NoPhotos;
                bool showDPFs = showPhotosSetting;

                if (showPhotosSetting)
                {
                    // Hide photos or PDFs page if there aren't any
                    List<System.Data.SqlClient.SqlParameter> imageParameters = new List<System.Data.SqlClient.SqlParameter>();
                    imageParameters.Add(new SqlParameter("AdminInfoID", estimateID));
                    imageParameters.Add(new SqlParameter("Supplement", supplementVersion < -1 ? -1 : supplementVersion));
                    imageParameters.Add(new SqlParameter("OnlyChecked", onlyCheckedImages));
                    imageParameters.Add(new SqlParameter("PDFs", 0));

                    DBAccessTableResult imageCountTable = db.ExecuteWithTable("sp_GetAllImagesPaths", imageParameters);
                    if (imageCountTable.DataTable.Rows.Count == 0)
                    {
                        showPhotosSetting = false;
                    }

                    imageParameters.Clear();
                    imageParameters.Add(new SqlParameter("AdminInfoID", estimateID));
                    imageParameters.Add(new SqlParameter("Supplement", supplementVersion < -1 ? -1 : supplementVersion));
                    imageParameters.Add(new SqlParameter("OnlyChecked", onlyCheckedImages));
                    imageParameters.Add(new SqlParameter("PDFs", 1));

                    DBAccessTableResult pdfCountTable = db.ExecuteWithTable("sp_GetAllImagesPaths", imageParameters);
                    if (pdfCountTable.DataTable.Rows.Count == 0)
                    {
                        showDPFs = false;
                    }
                }

                parameters.Add(new Telerik.Reporting.Parameter("ShowHeaderLogo", !printSettings.NoHeaderLogo));
                parameters.Add(new Telerik.Reporting.Parameter("ShowInsuranceSection", !printSettings.NoInsuranceSection));
                parameters.Add(new Telerik.Reporting.Parameter("ShowPhotoPage", showPhotosSetting));
                parameters.Add(new Telerik.Reporting.Parameter("ShowPDFPage", showDPFs));
                parameters.Add(new Telerik.Reporting.Parameter("ShowExternalNotes", printSettings.PrintPublicNotes));
                parameters.Add(new Telerik.Reporting.Parameter("ShowInternalNotes", printSettings.PrintPrivateNotes));
                parameters.Add(new Telerik.Reporting.Parameter("UseLargerFonts", printSettings.UseBigLetters));
                parameters.Add(new Telerik.Reporting.Parameter("ShowEstimateNumber", printSettings.EstimateNumber));
                parameters.Add(new Telerik.Reporting.Parameter("ShowID", printSettings.AdminInfoID));
                parameters.Add(new Telerik.Reporting.Parameter("ShowPaymentInfo", printSettings.PrintPaymentInfo));
                parameters.Add(new Telerik.Reporting.Parameter("ShowAlternatingGrayBars", printSettings.GreyBars));
                parameters.Add(new Telerik.Reporting.Parameter("ShowEstimator", InputHelper.GetBoolean(printSettings.PrintEstimator.ToString())));
                parameters.Add(new Telerik.Reporting.Parameter("HeaderLabels", printSettings.ContactOption == "Label" ? false : true));
                parameters.Add(new Telerik.Reporting.Parameter("HideHours", hideHours));
                parameters.Add(new Telerik.Reporting.Parameter("NoFooterDateTimeStamp", printSettings.NoFooterDateTimeStamp ? 1 : 0));

                bool showVendors = printSettings.PrintVendors;
                if (showVendors)
                {
                    // Set ShowVendors to false if there are no vendors to show
                    List<SqlParameter> vendorParameters = new List<SqlParameter>();
                    vendorParameters.Add(new SqlParameter("AdminInfoID", estimateID));
                    vendorParameters.Add(new SqlParameter("SupplementVersion", supplementVersion.HasValue && supplementVersion.Value > 0 ? supplementVersion.Value : lockLevel));
                    DBAccessTableResult vendorsTable = db.ExecuteWithTable("EstimateReport_GetPartSourceVendors", vendorParameters);
                    if (vendorsTable.DataTable.Rows.Count == 0)
                    {
                        showVendors = false;
                    }
                }

                parameters.Add(new Telerik.Reporting.Parameter("ShowVendors", showVendors));
            }

            ReportGenerator reportGenerator = new ReportGenerator();
            ReportFunctionResult reportResult = reportGenerator.GenerateReport(estimateID, reportType, parameters, onlyCheckedImages);

            if (attachImagesWithEmail)
            {
                int reportID = reportResult.Report.ID;

                // Create a Image Attachment record
                foreach (EstimationImage eachEstimationImage in images)
                {
                    try
                    {
                        if (eachEstimationImage.Include || !onlyCheckedImages)
                        {
                            ImageAttachment imageAttachment = new ImageAttachment();
                            imageAttachment.ReportID = reportID;
                            imageAttachment.EstimationImage.ID = eachEstimationImage.ID;
                            imageAttachment.Save();
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogError(ex, 0, "AttachImagesWithEmail");
                    }
                }
            }

            if (includeImages)
            {
                try
                {
                    foreach (string path in newImagePaths)
                    {
                        System.IO.File.Delete(path);
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, 0, "DeleteScaledImagePaths");
                }
            }

            SuccessBoxFeatureLog.LogFeature(loginID, SuccessBoxModule.EstimateWriting, "User printed an estimate", activeLoginID);

            return reportResult.Report;
        }

        public List<string> CreateImageByResolution(int imageSize, int imageSizePDF, int estimateID, int loginID, int supplement,
                                                        bool onlyChecked, out List<EstimationImage> images)
        {
            images = new List<EstimationImage>();

            List<string> newPaths = new List<string>();

            if (imageSize == 0)
            {
                return newPaths;
            }

            images = EstimationImage.GetForEstimate(estimateID);

            string imageFolder = Path.Combine(GetEstimateDiskPath(estimateID, loginID), "Images");

            foreach (EstimationImage image in images)
            {
                bool useImage = true;

                if (supplement > -1 && image.SupplementVersion != supplement)
                {
                    useImage = false;
                }

                if (onlyChecked && !image.Include)
                {
                    useImage = false;
                }

                if (useImage)
                {
                    if (image.FileType == "jpg" || image.FileType == "jpeg" || image.FileType == "png")
                    {
                        string diskPath = image.GetDiskPath(loginID, false);
                        //string editPath = image.GetEditDiskPath(loginID, true, true);////
                        string editPath = image.GetEditDiskPath(loginID, false, true);////
                        if (File.Exists(editPath))
                        {
                            diskPath = editPath;
                        }
                        if (File.Exists(diskPath))
                        {
                            Image diskImage = Image.FromFile(diskPath, true);
                            Image imageNew = ScaleImage(diskImage, imageSize, imageSize);
                            string newPath = Path.Combine(imageFolder, image.ID + "_" + imageSize + "." + image.FileType);
                            newPaths.Add(newPath);

                            ImageSave(imageNew, newPath);
                        }
                    }
                    else if (image.FileType == "pdf")
                    {
                        using (var rasterizer = new GhostscriptRasterizer())
                        {
                            string diskPath = image.GetDiskPath(loginID, false);
                            if (File.Exists(diskPath))
                            {
                                rasterizer.Open(diskPath);
                                for (int i = 1; i <= image.PDFPageCount; i++)
                                {
                                    Image pdfPageImage = null;
                                    pdfPageImage = rasterizer.GetPage(96, 96, i);

                                    // following would be needed for SMS (This is like upload image)
                                    #region "Thumbnail Image"

                                    Image thumbnailImage;
                                    string filePath = string.Empty;

                                    //if(i > 1)
                                    //{
                                    filePath = Path.Combine(imageFolder, image.ID.ToString() + "_thumb_" + i + ".jpg");
                                    //}
                                    //else
                                    //{
                                    //    filePath = Path.Combine(imageFolder, image.ID.ToString() + "_thumb" + ".jpg");
                                    //}

                                    if (!System.IO.File.Exists(filePath))
                                    {
                                        thumbnailImage = EstimateService.ScaleImage(pdfPageImage, 600, 800);
                                        ImageSave(thumbnailImage, filePath);
                                    }

                                    #endregion

                                    // following would be needed for SMS (This is like upload image)
                                    #region "Large Image"

                                    Image largeImage;
                                    string largeFilePath = string.Empty;

                                    //if (i > 1)
                                    //{
                                    largeFilePath = Path.Combine(imageFolder, image.ID.ToString() + "_" + i + ".jpg");
                                    //}
                                    //else
                                    //{
                                    //    largeFilePath = Path.Combine(imageFolder, image.ID.ToString() + ".jpg");
                                    //}

                                    if (!System.IO.File.Exists(filePath))
                                    {
                                        largeImage = EstimateService.ScaleImage(pdfPageImage, 2000, 2000);
                                        ImageSave(largeImage, filePath);
                                    }

                                    #endregion

                                    #region "Estimation Report Quality/Size"

                                    //filePath = image.GetPDFImageEditDiskPath(loginID, true, i, true);////
                                    filePath = image.GetPDFImageEditDiskPath(loginID, false, i, true);////
                                    if (File.Exists(filePath))
                                    {
                                        pdfPageImage = Image.FromFile(filePath, true);
                                    }

                                    Image pdfPageResized = ScaleImage(pdfPageImage, imageSizePDF, imageSizePDF);

                                    string newPath = string.Empty;
                                    //if (i > 1)
                                    //{
                                    newPath = Path.Combine(imageFolder, image.ID + "_" + i + "_" + imageSizePDF + ".jpg");
                                    //}
                                    //else
                                    //{
                                    //    newPath = Path.Combine(imageFolder, image.ID + "_" + imageSize + ".jpg");
                                    //}
                                    ImageSave(pdfPageResized, newPath);

                                    #endregion

                                    newPaths.Add(newPath);
                                }
                            }
                        }
                    }
                }
            }

            return newPaths;
        }

        public async Task<DecodeLicensePlateImageResults> DecodeLicensePlateImage(HttpPostedFile file, string readFailureMessage)
        {
            var results = new DecodeLicensePlateImageResults
            {
                ErrorMessage = readFailureMessage
            };

            try
            {
                var fileSizeLimit = InputHelper.GetInteger(ConfigurationManagerUtilities.GetAppSetting("LicensePlateReaderApiFileSizeLimit", "0"));
                var repeatApiCallDelayMS = InputHelper.GetInteger(ConfigurationManagerUtilities.GetAppSetting("LicensePlateReaderApiRepeatCallMinDelayMS", "0"));
                var repeatWithRotations = InputHelper.GetInteger(ConfigurationManagerUtilities.GetAppSetting("LicensePlateReaderApiRepeatWithRotations", "0"));
                var minScorePlateNumber = InputHelper.GetDouble(ConfigurationManagerUtilities.GetAppSetting("LicensePlateReaderApiMinScorePlateNumber", "0.82"));
                var minScorePlateState = InputHelper.GetDouble(ConfigurationManagerUtilities.GetAppSetting("LicensePlateReaderApiMinScorePlateState", "0.63"));

                var request = new PlateReaderRequest();
                var image = Image.FromStream(file.InputStream, true, true) as Bitmap;
                PlateReaderResponse currentResponse = null;
                var responseList = new List<PlateReaderResponse>();
                bool tryAnotherAngle = false;
                DateTime timeOfApiRequest = DateTime.UtcNow;

                // Get the response from the api, rotating 90 degrees after each request, until we get a response with a high confidence rate
                // Add each of these responses to an array and then, if we do not get a good confidence on any, just use the highest one
                do
                {
                    // Get the string representation of the image - and ensure it is resized if it eclipses the file size limit (imposed by the api)
                    request.Upload = ImageUtilities.GetImageAsBase64String(image, fileSizeLimit > 0 && file.InputStream.Length > fileSizeLimit,
                        Math.Min(500, image.Width / 4), Math.Min(500, image.Height / 4));

                    var numMSBetwenCalls = responseList.Count == 0 ? 0 : Convert.ToInt32((DateTime.UtcNow - timeOfApiRequest).TotalMilliseconds);
                    timeOfApiRequest = DateTime.UtcNow;

                    // Add a delay based on numMSBetwenCalls - e.g. make it 1500 ms - numMSBetwenCalls
                    //      ensuring we always wait AT LEAST 1.5 seconds between api calls
                    // NOTE: The free tier for this API service limits 1 call per second. The paid tier is 8 cps.
                    //       Once we switch to the paid tier, we can simply change the config settings to not add a delay  
                    if (responseList.Count > 0 && repeatApiCallDelayMS > numMSBetwenCalls)
                    {
                        ErrorLogger.LogError($"ADDING DELAY: {repeatApiCallDelayMS - numMSBetwenCalls}", "EstimateService DecodeLicensePlateImage");
                        await Task.Delay(repeatApiCallDelayMS - numMSBetwenCalls);
                    }

                    // Send the image to the api and get back the plate number and state
                    currentResponse = await LicensePlateReaderService.ReadLicensePlateImage(request);
                    responseList.Add(currentResponse ?? new PlateReaderResponse());

                    //ErrorLogger.LogError($"#{responseList.Count}, API Gap: {numMSBetwenCalls}, OL: {fileSizeLimit > 0 && file.InputStream.Length > fileSizeLimit}...
                    //  {(currentResponse == null ? "-NO RESP-" : $"{currentResponse.PlateNumberScore}/{currentResponse.PlateStateScore}")}",  "EstimateService DecodeLicensePlateImage");

                    // Rotate for the next api call (if the confidence score is not high enough)
                    tryAnotherAngle = responseList.Count <= repeatWithRotations && currentResponse != null && (currentResponse.PlateNumberScore < minScorePlateNumber || currentResponse.PlateStateScore < minScorePlateState);
                    if (tryAnotherAngle)
                        image.RotateFlip(RotateFlipType.Rotate90FlipNone);

                } while (tryAnotherAngle);

                // Get highest item from list based on confidence scores
                var response = responseList.OrderByDescending(x => x.TotalScore).FirstOrDefault();

                if (response?.Results?.FirstOrDefault() != null && !string.IsNullOrEmpty(response?.Results?[0]?.Plate))
                {
                    results.ErrorMessage = "";
                    results.LicensePlateNumber = response.Results[0].Plate.ToUpper();
                    results.CorrectPlateNumberConfidenceScore = response.Results[0].Score;
                    if (!string.IsNullOrEmpty(response.Results[0].Region?.Code))
                    {
                        results.LicensePlateStateAbbreviation = response.Results[0].Region.Code.Replace("us-", "").ToUpper();
                        results.CorrectPlateStateConfidenceScore = response.Results[0].Region.Score;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "EstimateService DecodeLicensePlateImage");
            }

            return results;
        }

        public bool IsEstimateTotalLoss(int estimateID)
        {
            bool isTotalLoss = false;

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("sp_GetHeaderInfo", new SqlParameter("AdminInfoID", estimateID));
            if (tableResult.Success)
            {
                DataRow row = tableResult.DataTable.Rows[0];
                double totalLostPercent = InputHelper.GetDouble(row["TotalLossPercent"].ToString());
                double vehicleValue = InputHelper.GetDouble(row["VehicleValue"].ToString());
                double estimateTotal = InputHelper.GetDouble(row["GrandTotal"].ToString());

                if (totalLostPercent > 0 && vehicleValue > 0)
                {
                    if (estimateTotal > vehicleValue * (totalLostPercent / 100.0))
                    {
                        isTotalLoss = true;
                    }
                }
            }

            return isTotalLoss;
        }

        public static string GetEstimateDiskPath(int estimateID, int loginID)
        {
            return Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), loginID.ToString(), estimateID.ToString());
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

        public List<SimpleListItem> GetSections(int estimateID, int vehicleID, bool ignoreYearFilter = false)
        {
            string acCapacities = GetACCapacitiesNote(estimateID);
            bool acCapacitiesAdded = false;

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("VehicleID", vehicleID));
            parameters.Add(new SqlParameter("IgnoreYearFilter", ignoreYearFilter));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("GetSectionsListByVehicle", parameters);

            List<SimpleListItem> result = new List<SimpleListItem>();

            if (tableResult.Success)
            {
                result.Add(new SimpleListItem("---Select Section", "-1"));
                result.Add(new SimpleListItem("Mitchell P Pages", "-3"));

                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    int sectionKey = InputHelper.GetInteger(row["SectionKey"].ToString());
                    string sectionName = row["SectionName"].ToString();

                    if (sectionKey > 0)
                    {
                        result.Add(new SimpleListItem(sectionName, sectionKey.ToString()));

                        if (!string.IsNullOrEmpty(acCapacities) && sectionName.StartsWith("A/C") && !acCapacitiesAdded)
                        {
                            result.Add(new SimpleListItem("A/C Refrigerant Capacities", "-2"));
                            acCapacitiesAdded = true;
                        }
                    }
                }
            }

            if (!acCapacitiesAdded && !string.IsNullOrEmpty(acCapacities))
            {
                result.Add(new SimpleListItem("A/C Refrigerant Capacities", "-2"));
            }

            return result;
        }

        public List<DropDownTreeItemModel> GetSectionsListByVehicleforDropDownTreeItem(int estimateID, int vehicleID, bool ignoreYearFilter = false)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("VehicleID", vehicleID));
            parameters.Add(new SqlParameter("IgnoreYearFilter", ignoreYearFilter));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("GetSectionsListByVehicleTreeView", parameters);

            List<DropDownTreeItemModel> result = FillVehicleDropDownTreeItemModel(estimateID, tableResult);

            return result;
        }

        private List<DropDownTreeItemModel> FillVehicleDropDownTreeItemModel(int estimateID, DBAccessTableResult tableResult)
        {
            string acCapacities = GetACCapacitiesNote(estimateID);
            bool acCapacitiesAdded = false;

            List<DropDownTreeItemModel> result = new List<DropDownTreeItemModel>();

            if (tableResult.Success)
            {
                result.Add(new DropDownTreeItemModel() { Text = "Please select a parts section", Value = "-1" });

                DataView view = new DataView(tableResult.DataTable);
                DataTable distinctCategories = view.ToTable(true, "Category");

                foreach (DataRow distinctRow in distinctCategories.Rows)
                {
                    string category = Convert.ToString(distinctRow["Category"]);

                    if (!string.IsNullOrEmpty(category))
                    {
                        DataRow[] sameCategoryTableRowsArr = tableResult.DataTable.Select("Category = '" + category + "'");
                        Boolean hasChildren = sameCategoryTableRowsArr.Length > 1 ? true : false;

                        DropDownTreeItemModel dropDownTreeItemModel = null;

                        if (hasChildren)
                        {
                            dropDownTreeItemModel = new DropDownTreeItemModel()
                            {
                                Text = Convert.ToString(sameCategoryTableRowsArr[0]["Category"]),
                                Id = Guid.NewGuid().ToString()
                            };

                            // dropDownTreeItemModel.Value should be null for parent node
                            dropDownTreeItemModel.Value = null;
                            dropDownTreeItemModel.HasChildren = hasChildren;
                            int dropDownTreeItemModelLength = sameCategoryTableRowsArr.Length;

                            List<DropDownTreeItemModel> dropDownTreeItemModelColl = new List<DropDownTreeItemModel>(dropDownTreeItemModelLength);

                            for (int rowIndex = 0; rowIndex < dropDownTreeItemModelLength; rowIndex++)
                            {
                                DataRow row = sameCategoryTableRowsArr[rowIndex];

                                int sectionKey = InputHelper.GetInteger(row["SectionKey"].ToString());
                                string subcategory = row["Subcategory"].ToString();

                                if (sectionKey > 0)
                                {
                                    dropDownTreeItemModelColl.Add(new DropDownTreeItemModel()
                                    {
                                        Text = subcategory,
                                        Id = sectionKey.ToString()
                                    });
                                }
                            }

                            dropDownTreeItemModel.Items = dropDownTreeItemModelColl;
                        }
                        else
                        {
                            dropDownTreeItemModel = new DropDownTreeItemModel()
                            {
                                Text = Convert.ToString(sameCategoryTableRowsArr[0]["Category"]),
                                Id = Convert.ToString(sameCategoryTableRowsArr[0]["SectionKey"])
                            };
                        }

                        result.Add(dropDownTreeItemModel);

                        if (!string.IsNullOrEmpty(acCapacities) && category.StartsWith("A/C") && !acCapacitiesAdded)
                        {
                            DropDownTreeItemModel dropDownTreeItemModelAC = new DropDownTreeItemModel()
                            {
                                Text = Convert.ToString("A/C Refrigerant Capacities").ToUpper(),
                                Id = "-2"
                            };
                            result.Add(dropDownTreeItemModelAC);
                            acCapacitiesAdded = true;
                        }
                    }
                }
            }

            if (!acCapacitiesAdded && !string.IsNullOrEmpty(acCapacities))
            {
                result.Add(new DropDownTreeItemModel() { Text = "A/C Refrigerant Capacities", Value = "-2" });
            }

            return result;
        }

        public List<TreeViewItemModel> GetSectionsListByVehicleforTreeViewItem(int estimateID, int vehicleID, bool ignoreYearFilter = false)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("VehicleID", vehicleID));
            parameters.Add(new SqlParameter("IgnoreYearFilter", ignoreYearFilter));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("GetSectionsListByVehicleTreeView", parameters);

            List<TreeViewItemModel> result = FillVehicleTreeViewItemModel(estimateID, tableResult);

            return result;
        }

        public List<TreeViewItemModel> FillVehicleTreeViewItemModel(int estimateID, DBAccessTableResult tableResult)
        {
            string acCapacities = GetACCapacitiesNote(estimateID);
            bool acCapacitiesAdded = false;

            List<TreeViewItemModel> result = new List<TreeViewItemModel>();

            if (tableResult.Success)
            {
                result.Add(new TreeViewItemModel() { Text = "---Select Section", Id = "-1" });

                DataView view = new DataView(tableResult.DataTable);
                DataTable distinctCategories = view.ToTable(true, "Category");

                foreach (DataRow distinctRow in distinctCategories.Rows)
                {
                    string category = Convert.ToString(distinctRow["Category"]);

                    if (!string.IsNullOrEmpty(category))
                    {
                        DataRow[] sameCategoryTableRowsArr = tableResult.DataTable.Select("Category = '" + category + "'");
                        Boolean hasChildren = sameCategoryTableRowsArr.Length > 1 ? true : false;

                        TreeViewItemModel treeViewItemModel = null;

                        if (hasChildren)
                        {
                            treeViewItemModel = new TreeViewItemModel()
                            {
                                Text = Convert.ToString(sameCategoryTableRowsArr[0]["Category"])
                            };

                            // dropDownTreeItemModel.Value should be null for parent node
                            treeViewItemModel.Id = null;
                            treeViewItemModel.HasChildren = hasChildren;
                            int treeViewItemModelLength = sameCategoryTableRowsArr.Length;

                            List<TreeViewItemModel> treeViewItemModelColl = new List<TreeViewItemModel>(treeViewItemModelLength);

                            for (int rowIndex = 0; rowIndex < treeViewItemModelLength; rowIndex++)
                            {
                                DataRow row = sameCategoryTableRowsArr[rowIndex];

                                int sectionKey = InputHelper.GetInteger(row["SectionKey"].ToString());
                                string subcategory = row["Subcategory"].ToString();

                                if (sectionKey > 0)
                                {
                                    treeViewItemModelColl.Add(new TreeViewItemModel()
                                    {
                                        Text = subcategory,
                                        Id = sectionKey.ToString()
                                    });
                                }
                            }

                            treeViewItemModel.Items = treeViewItemModelColl;
                        }
                        else
                        {
                            treeViewItemModel = new TreeViewItemModel()
                            {
                                Text = Convert.ToString(sameCategoryTableRowsArr[0]["Category"]),
                                Id = Convert.ToString(sameCategoryTableRowsArr[0]["SectionKey"])
                            };
                        }

                        result.Add(treeViewItemModel);

                        if (!string.IsNullOrEmpty(acCapacities) && category.StartsWith("A/C") && !acCapacitiesAdded)
                        {
                            TreeViewItemModel treeViewItemModelAC = new TreeViewItemModel()
                            {
                                Text = Convert.ToString("A/C Refrigerant Capacities").ToUpper(),
                                Id = "-2"
                            };
                            result.Add(treeViewItemModelAC);
                            acCapacitiesAdded = true;
                        }
                    }
                }
            }

            if (!acCapacitiesAdded && !string.IsNullOrEmpty(acCapacities))
            {
                result.Add(new TreeViewItemModel() { Text = "A/C Refrigerant Capacities", Id = "-2" });
            }

            return result;
        }

        public string GetACCapacitiesNote(int estimateID)
        {
            DBAccess db = new DBAccess();
            DBAccessStringResult result = db.ExecuteWithStringReturn("GetACCapacities", new SqlParameter("AdminInfoID", estimateID));

            if (result.Success)
            {
                return result.Value;
            }

            return "";
        }

        public class ReportAsImageFunctionResult : FunctionResult
        {

            public Image Image { get; private set; }
            public string DiskPath { get; private set; }
            public string WebPath { get; private set; }

            public ReportAsImageFunctionResult(bool success, string errorMessage, Image image, string diskPath, string webPath)
                : base(success, errorMessage)
            {
                Image = image;
                DiskPath = diskPath;
                WebPath = webPath;
            }

        }

        public ReportAsImageFunctionResult SaveReportAsImage(Report report)
        {
            bool success = false;
            string errorMessage = "";
            string diskPath = "";
            string webPath = "";
            Image combinedImage = null;

            Estimate estimate = new Estimate(report.EstimateID);

            try
            {
                int desired_x_dpi = 96;
                int desired_y_dpi = 96;

                string contentPath = ConfigurationManager.AppSettings.Get("UserContentPath").ToString();
                string inputPdfPath = report.GetDiskPath();

                // Render all report pages as a list of images
                List<Image> images = new List<Image>();

                using (var rasterizer = new GhostscriptRasterizer())
                {
                    rasterizer.Open(inputPdfPath);

                    for (var pageNumber = 1; pageNumber <= rasterizer.PageCount; pageNumber++)
                    {
                        images.Add(rasterizer.GetPage(desired_x_dpi, desired_y_dpi, pageNumber));
                    }
                }

                combinedImage = CombineReportImages(images);
                //diskPath = Path.Combine(contentPath, estimate.CreatedByLoginID.ToString(), estimate.EstimateID.ToString(), "Reports", string.IsNullOrEmpty(report.FileName) ? report.ID.ToString() : report.FileName + ".jpg");
                //webPath = Path.Combine(ConfigurationManager.AppSettings["BaseURL"], "UserContent", estimate.CreatedByLoginID.ToString(), estimate.EstimateID.ToString(), "Reports", report.FileName + ".jpg").Replace("\\", "/");

                diskPath = Path.Combine(contentPath, "sms", report.ID.ToString() + report.FileName + ".jpg");
                webPath = Path.Combine(ConfigurationManager.AppSettings["BaseURL"], "UserContent", "sms", report.ID.ToString() + report.FileName + ".jpg").Replace("\\", "/");

                ImageSave(combinedImage, diskPath);

                success = true;
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, estimate.CreatedByLoginID, "EstimateService SaveReportAsImage");
                errorMessage = ex.Message;
            }

            return new ReportAsImageFunctionResult(success, errorMessage, combinedImage, diskPath, webPath);
        }

        public Image CombineReportImages(List<Image> images)
        {
            if (images == null || images.Count == 0)
            {
                return null;
            }

            try
            {
                int height = 0;
                int width = images[0].Width;

                foreach (Image image in images)
                {
                    height += image.Height;
                }

                Bitmap combinedImage = new Bitmap(width, height);
                Graphics graphics = Graphics.FromImage(combinedImage);
                graphics.Clear(Color.White);

                int drawY = 0;

                foreach (Image image in images)
                {
                    graphics.DrawImage(image, new Point(0, drawY));
                    drawY += image.Height;
                }

                graphics.Dispose();

                return combinedImage;
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "EstimateService CombineReportImages");
            }

            return null;
        }

        /// <summary>
        /// Save teh image info to the database, and save two copies of the image, one full size and one thumbnail
        /// </summary>
        public UploadImageResult SaveImage(int loginID, int estimateID, Image image, string fileName, string extension, EstimationImage estimationImage = null)
        {
            UploadImageResult result = new UploadImageResult();
            result.Success = false;

            extension = extension.Replace(".", "");

            try
            {
                if (image != null)
                {
                    // Create the database record of the file.  We need an ID from this record to save the file to disk.
                    if (estimationImage == null)
                    {
                        estimationImage = new EstimationImage();
                        estimationImage.AdminInfoID = estimateID;
                        estimationImage.FileName = fileName;
                        estimationImage.FileType = extension;

                        Estimate estimate = new Estimate(estimateID);
                        estimationImage.SupplementVersion = estimate.LockLevel;
                        estimationImage.Save();
                    }

                    string imageFolder = Path.Combine(GetEstimateDiskPath(estimateID, loginID), "Images");
                    Directory.CreateDirectory(imageFolder);

                    int orientationID = 0x0112;
                    if (image.PropertyIdList.Contains(orientationID))
                    {
                        var orientationData = image.GetPropertyItem(orientationID);
                        RotateFlipType flipType = GetRotateFlipTypeByExifOrientationData(orientationData.Value[0]);

                        if (flipType != RotateFlipType.RotateNoneFlipNone)
                        {
                            image.RotateFlip(flipType);
                        }
                    }

                    Image imageLarge = ScaleImage(image, 2000, 2000);
                    Image imageSmall = ScaleImage(image, 600, 800);

                    ImageSave(imageLarge, Path.Combine(imageFolder, estimationImage.ID.ToString() + "." + extension));
                    ImageSave(imageSmall, Path.Combine(imageFolder, estimationImage.ID.ToString() + "_thumb." + extension));

                    result.Success = true;
                    result.ImageID = estimationImage.ID;
                    result.NewImagePath = estimationImage.GetWebPath(loginID, true);
                }
                else
                {
                    result.ErrorMessage = "No file attached.";
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = "Error uploading image: " + ex.Message;
            }

            return result;
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

        public UploadImageResult UploadImage(int loginID, int estimateID, System.Web.HttpPostedFile file)
        {
            UploadImageResult result = new UploadImageResult();
            result.Success = false;

            try
            {
                if (file != null)
                {
                    string contentType = file.ContentType.ToLower();

                    string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                    string extension = Path.GetExtension(file.FileName).Replace(".", "");

                    if (contentType.StartsWith("image/") || contentType == "application/pdf" || extension == "heic")
                    {
                        // Create the database record of the file.  We need an ID from this record to save the file to disk.
                        EstimationImage newImage = new EstimationImage();
                        newImage.AdminInfoID = estimateID;
                        newImage.FileName = file.FileName;
                        newImage.FileType = extension == "heic" ? "jpg" : extension;

                        Estimate estimate = new Estimate(estimateID);
                        newImage.SupplementVersion = estimate.LockLevel;

                        SaveResult saveResult = newImage.Save();
                        if (saveResult.Success)
                        {
                            string imageFolder = Path.Combine(GetEstimateDiskPath(estimateID, loginID), "Images");
                            Directory.CreateDirectory(imageFolder);

                            if (contentType.StartsWith("image/"))
                            {
                                Image image = Image.FromStream(file.InputStream, true, true);
                                return SaveImage(loginID, estimateID, image, file.FileName, extension, newImage);
                            }
                            else if (contentType == "application/pdf")
                            {
                                // Save the PDF to disk
                                string pdfPath = Path.Combine(imageFolder, newImage.ID.ToString() + ".pdf");
                                file.SaveAs(pdfPath);

                                // Save the first page of the PDF as an image thumbnail
                                try
                                {
                                    int desired_x_dpi = 96;
                                    int desired_y_dpi = 96;

                                    Image rasterizerImage;
                                    Image thumbnailImage;
                                    Image largeImage;

                                    using (var rasterizer = new GhostscriptRasterizer())
                                    {
                                        ErrorLogger.LogError("Getting pdf from: " + pdfPath, "UploadImage PdfRasterizer");

                                        rasterizer.Open(pdfPath);
                                        ErrorLogger.LogError("Page Count: " + rasterizer.PageCount, "UploadImage PdfRasterizer");
                                        rasterizerImage = rasterizer.GetPage(desired_x_dpi, desired_y_dpi, 1);

                                        newImage.PDFPageCount = rasterizer.PageCount;
                                        newImage.Save();
                                    }

                                    if (rasterizerImage != null)
                                    {
                                        thumbnailImage = ScaleImage(rasterizerImage, 600, 800);
                                        ImageSave(thumbnailImage, Path.Combine(imageFolder, newImage.ID.ToString() + "_thumb.jpg"));

                                        largeImage = ScaleImage(rasterizerImage, 2000, 2000);
                                        ImageSave(largeImage, Path.Combine(imageFolder, newImage.ID.ToString() + ".jpg"));

                                        result.Success = true;
                                        result.ImageID = newImage.ID;
                                        result.NewImagePath = newImage.GetWebPath(loginID, true);
                                    }

                                    if (newImage.PDFPageCount > 1)
                                    {
                                        using (var rasterizer = new GhostscriptRasterizer())
                                        {
                                            ErrorLogger.LogError("Saving PDF images by each page: " + pdfPath, "UploadImage PdfRasterizer");

                                            rasterizer.Open(pdfPath);
                                            int pageCount = rasterizer.PageCount;

                                            for (int pageIndex = 2; pageIndex <= pageCount; pageIndex++)
                                            {
                                                ErrorLogger.LogError("Page Index: " + pageIndex, "UploadImage PdfRasterizer");
                                                rasterizerImage = rasterizer.GetPage(desired_x_dpi, desired_y_dpi, pageIndex);

                                                if (rasterizerImage != null)
                                                {
                                                    thumbnailImage = ScaleImage(rasterizerImage, 600, 800);
                                                    ImageSave(thumbnailImage, Path.Combine(imageFolder, newImage.ID.ToString() + "_thumb_" + pageIndex + ".jpg"));

                                                    largeImage = ScaleImage(rasterizerImage, 2000, 2000);
                                                    ImageSave(largeImage, Path.Combine(imageFolder, newImage.ID.ToString() + "_" + pageIndex + ".jpg"));
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (System.Exception ex)
                                {
                                    ErrorLogger.LogError(ex, loginID, "EstimateService SaveReportAsImage");
                                    result.ErrorMessage = ex.Message;
                                }
                            }
                            else if (extension == "heic")
                            {
                                string heicPath = Path.Combine(imageFolder, newImage.ID.ToString() + ".heic");
                                file.SaveAs(heicPath);
                                try
                                {
                                    file.InputStream.Position = 0;
                                    using (ImageMagick.MagickImage i = new ImageMagick.MagickImage(file.InputStream))
                                    {
                                        i.Format = ImageMagick.MagickFormat.Jpg;
                                        using (MemoryStream memStream = new MemoryStream(i.ToByteArray()))
                                        {
                                            Image convertImage = Image.FromStream(memStream, true, true);
                                            return SaveImage(loginID, estimateID, convertImage, file.FileName, "jpg", newImage);
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    ErrorLogger.LogError(e, loginID, "EstimateService UploadImage MagickImage");
                                    result.ErrorMessage = e.Message;
                                }
                            }
                        }
                    }
                    else
                    {
                        result.ErrorMessage = "Only Image and PDF files can be uploaded.";
                    }
                }
                else
                {
                    result.ErrorMessage = "No file attached.";
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = "Error uploading image: " + ex.Message;
            }

            return result;
        }

        private RotateFlipType GetRotateFlipTypeByExifOrientationData(int orientation)
        {
            switch (orientation)
            {
                case 1:
                default:
                    return RotateFlipType.RotateNoneFlipNone;
                case 2:
                    return RotateFlipType.RotateNoneFlipX;
                case 3:
                    return RotateFlipType.Rotate180FlipNone;
                case 4:
                    return RotateFlipType.Rotate180FlipX;
                case 5:
                    return RotateFlipType.Rotate90FlipX;
                case 6:
                    return RotateFlipType.Rotate90FlipNone;
                case 7:
                    return RotateFlipType.Rotate270FlipX;
                case 8:
                    return RotateFlipType.Rotate270FlipNone;
            }
        }
    }

    public class UploadImageResult
    {
        public bool Success { get; set; }
        public int ImageID { get; set; }
        public string ErrorMessage { get; set; }
        public string NewImagePath { get; set; }
        public bool Include { get; set; }
        public ImageExtraInfo ImageInfo { get; set; }

        public UploadImageResult()
        {
            Success = false;
            ImageID = 0;
            ErrorMessage = "";
            NewImagePath = "";
            Include = false;
        }
    }

    public class BulkUploadImageResult
    {
        public bool Success { get; set; }
        public UploadImageResult[] UploadImageResultArray { get; set; }
        public string ErrorMessage { get; set; }

        public BulkUploadImageResult()
        {
            Success = false;
            UploadImageResultArray = null;
            ErrorMessage = "";
        }
    }
}
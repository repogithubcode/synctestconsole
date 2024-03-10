using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Drawing;

using Ghostscript.NET.Rasterizer;

using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using ProEstimatorData;

using ProEstimatorData.Models.SubModel;
using Proestimator.ViewModel.SendEstimate;
using ProEstimator.Business.Logic;
using Proestimator.Helpers;
using Proestimator.ViewModel.Printing;

using System.IO;
using System.Configuration;
using Proestimator.Resources;

namespace Proestimator.ViewModel
{
    public class ReportCreator
    {

        /// <summary>True when showing on the Send page, false on the Print page.</summary>
        public bool SendPage { get; set; }

        public List<ReportTypeVM> LetterTypes { get; set; }
        public List<DropDownData> CustomReports { get; set; }

        public SelectList LaborTypeList { get; set; }
        public string LaborTypeID { get; set; }

        public string PartsOrderType { get; set; }
        public List<DropDownData> PartsOrderTypeList { get; set; }

        public string SelectedReportHeader { get; set; }
        public SelectList ReportHeaderSelections { get; set; }

        public string ReportType { get; set; }
        public List<DropDownData> EstimateReportTypes { get; set; }

        public int MaxSupplement { get; set; }

        public List<DropDownData> ImageSizeSelection { get; set; }
        public string ImageSize { get; set; }

        public List<ImageVM> Images { get; set; }

        public string PrintDescription { get; set; }

        public bool IncludeImages { get; set; }
        public bool HideHourlyRates { get; set; }
        public bool PdrOnly { get; set; }
        public bool PrintPnLNotes { get; set; }
        public bool PrintHeaderInfo { get; set; }

        public bool HasEMSContract { get; set; }

        public int LoginID { get; set; }
        public string EmsRedirectPage { get; set; }

        public bool IsTrial { get; set; }

        public bool PrintInspectionDate { get; set; }

        public bool AttachImages { get; set; }

        public bool PrintSupplementPhotosOnly { get; set; }

        public WorkOrderReportVM WorkOrderReportVM { get; set; }

        public bool PrintLaborHours { get; set; }
        public bool PrintRepairDays { get; set; }

        public List<DropDownData> PDRTechnicians { get; set; }
        public string PDRTechnician { get; set; }

        public SelectList PartOrderHeaderList { get; set; }
        public string PartOrderHeader { get; set; }

        public bool IncludeLoanApplicationLink { get; set; }
        public bool IsFinancingMerchantApproved { get; set; }

        public bool PrintTechnicians { get; set; }

        public bool ShowEMSVersionSelection { get; set; }

        public bool IncludeCustomerInfo { get; set; }

        public bool PrintLaborDiscount { get; set; }

        public ReportCreator()
        {

        }

        public ReportCreator(int loginID, int estimateID, bool hasEMSContract, bool isTrial, bool sendPage = false)
        {
            SendPage = sendPage;
            LoginID = loginID;

            IsTrial = isTrial;

            LetterTypes = new List<ReportTypeVM>();
            CustomReports = new List<DropDownData>();
            PDRTechnicians = new List<DropDownData>();

            // Set up the list of report types that can be added
            string cultureName = InputHelper.GetString(SiteSettings.Get(loginID, "Culture", "LanguagePreferences", "en-us").ValueString);
            foreach (ReportType reportType in ProEstimatorData.DataModel.ReportType.GetAll())
            {
                bool showType = true;

                // Hacky - only show the PDR work order report if there is pdr on the estimate
                if (reportType.Tag == "PDRWorkOrderReport")
                {
                    ProEstimatorData.DataModel.PDR.PDR_EstimateData pdrEstimateData = ProEstimatorData.DataModel.PDR.PDR_EstimateData.GetForEstimate(estimateID);
                    if (pdrEstimateData == null)
                    {
                        showType = false;
                    }

                    List<Technician> pdrTechs = Technician.GetByLogin(loginID).Where(o => o.LaborTypeID == -2 || o.LaborTypeID == 0).ToList();
                    PDRTechnicians.Add(new DropDownData("", "Select"));

                    foreach (Technician tech in pdrTechs)
                    {
                        PDRTechnicians.Add(new DropDownData(tech.FirstName + " " + tech.LastName, tech.FirstName + " " + tech.LastName));
                    }
                }

                if (showType)
                {
                    reportType.Text = ProStrings.ResourceManager.GetString("Reports_" + reportType.Tag) ?? string.Empty;
                    LetterTypes.Add(new ReportTypeVM(reportType.Text, reportType.Tag, reportType.MultiLanguage));
                }
            }

            List<CustomReportTemplate> customTemplates = CustomReportTemplate.GetForLogin(loginID)
                                                        .Where(o => o.IsActive && !o.IsDeleted && !string.IsNullOrEmpty(o.Template) && o.CustomReportTemplateType == CustomReportTemplateType.Report)
                                                        .OrderBy(o => o.Name).ToList();
            if (customTemplates.Count > 0)
            {
                LetterTypes.Add(new ReportTypeVM(Proestimator.Resources.ProStrings.Reports_CustomReports, "CustomReport"));

                foreach (CustomReportTemplate template in customTemplates)
                {
                    CustomReports.Add(new DropDownData(template.ID.ToString(), template.Name));
                }
            }

            // Fill the Labor Type List
            List<SimpleListItem> laborTypeList = ProEstHelper.GetLaborTypeList(estimateID);
            LaborTypeList = new SelectList(laborTypeList, "Value", "Text");
            LaborTypeID = "All";

            // Fill the list of part source types to show
            PartsOrderTypeList = new List<DropDownData>();
            PartsOrderTypeList.Add(new DropDownData("All", "All"));
            PartsOrderTypeList.Add(new DropDownData("After", "A/M"));
            PartsOrderTypeList.Add(new DropDownData("LKQ", "LKQ"));
            PartsOrderTypeList.Add(new DropDownData("Reman", "Reconditioned"));
            PartsOrderTypeList.Add(new DropDownData("OEM", "OEM"));

            PartsOrderType = "All";

            // Fill the estimate report types if the estimate is in a supplement
            EstimateReportTypes = new List<DropDownData>();

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("Estimate_GetHighestUsedEstimate", new System.Data.SqlClient.SqlParameter("AdminInfoID", estimateID));

            if (result.Success && result.Value > 0)
            {
                EstimateReportTypes.Add(new DropDownData("-2", "Full"));

                EstimateReportTypes.Add(new DropDownData("0", "Original Only"));
                EstimateReportTypes.Add(new DropDownData("-1", "EOR Only"));

                for (int i = 1; i <= result.Value; i++)
                {
                    EstimateReportTypes.Add(new DropDownData(i.ToString(), "Supplement " + i.ToString()));
                }
            }

            // Fill the report headers list
            List<SelectListItem> reportHeaders = new List<SelectListItem>();
            List<SelectListItem> partOrderHeaders = new List<SelectListItem>();
            foreach (ReportHeader header in ReportHeader.ReportHeadersList)
            {
                if (header.Type == "Estimate")
                {
                    reportHeaders.Add(new SelectListItem() { Text = header.Header, Value = header.Header });
                }
                else if (header.Type == "Parts Order")
                {
                    partOrderHeaders.Add(new SelectListItem() { Text = header.Header, Value = header.Header });
                }
            }
            ReportHeaderSelections = new SelectList(reportHeaders, "Value", "Text");
            if (partOrderHeaders.Count > 0)
            {
                PartOrderHeader = partOrderHeaders[0].Value;
            }
            PartOrderHeaderList = new SelectList(partOrderHeaders, "Value", "Text");

            ImageSizeSelection = new List<DropDownData>();
            ImageSizeSelection.Add(new DropDownData("0", "original Image"));
            ImageSizeSelection.Add(new DropDownData("2000", "2000X2000"));
            ImageSizeSelection.Add(new DropDownData("1500", "1500X1500"));
            ImageSizeSelection.Add(new DropDownData("1000", "1000X1000"));
            ImageSizeSelection.Add(new DropDownData("500", "500X500"));
            ImageSizeSelection.Add(new DropDownData("250", "250X250"));
            ImageSizeSelection.Add(new DropDownData("125", "125X125"));

            Estimate estimate = new Estimate(estimateID);

            ImageSize = estimate.ImageSize ?? "500";
            MaxSupplement = estimate.LockLevel;

            SelectedReportHeader = estimate.ReportTextHeader;

            if (string.IsNullOrEmpty(SelectedReportHeader))
            {
                if (estimate.CurrentJobStatusID == 2)
                {
                    SelectedReportHeader = "Repair Order";
                }
                else if (estimate.CurrentJobStatusID == 3)
                {
                    SelectedReportHeader = "Closed Repair Order";
                }
                else
                {
                    SelectedReportHeader = "Estimate";
                }
            }

            DBAccessIntResult highestUsedResult = db.ExecuteWithIntReturn("Estimate_GetHighestUsedEstimate", new System.Data.SqlClient.SqlParameter("AdminInfoID", estimateID));

            int lastSupplement = 0;
            if (highestUsedResult.Success)
            {
                lastSupplement = highestUsedResult.Value;
            }

            PrintDescription = estimate.Description;

            ProEstimatorData.DataModel.Profiles.PrintSettings printSettings = ProEstimatorData.DataModel.Profiles.PrintSettings.GetForProfile(estimate.CustomerProfileID);
            if (printSettings != null)
            {
                IncludeImages = !printSettings.NoPhotos;
                HideHourlyRates = printSettings.Dollars;
                PrintTechnicians = printSettings.PrintTechnicians;
            }
            else
            {
                IncludeImages = true;
                HideHourlyRates = false;
                PrintTechnicians = true;
            }

            HasEMSContract = hasEMSContract;

            PrintHeaderInfo = InputHelper.GetBoolean(SiteSettings.Get(loginID, "PrintHeaderInfoOnEstimateReport", "ReportOptions", true.ToString()).ValueString);
            if (!HasEMSContract)
            {
                // There might be created but un paid for EMS contract
                Contract activeContract = Contract.GetActive(loginID);
                if (activeContract != null)
                {
                    List<ContractAddOn> addOns = ContractAddOn.GetForContract(activeContract.ID);
                    ContractAddOn emsAddOn = addOns.FirstOrDefault(o => o.AddOnType.ID == 5);
                    if (emsAddOn != null)
                    {
                        if (!emsAddOn.Active)
                        {
                            HasEMSContract = true;    // Still go to the EMS page where we will show a message about the contract being deactivated
                        }
                        else
                        {
                            EmsRedirectPage = "customer-invoice";
                        }
                    }
                    else
                    {
                        EmsRedirectPage = "pick-addon/" + activeContract.ID;
                    }
                }
                else
                {
                    EmsRedirectPage = estimateID + "/contact-webest";
                }
            }

            PrintPnLNotes = InputHelper.GetBoolean(SiteSettings.Get(loginID, "IncludePartNotesOnPdfLineEntry", "ReportOptions", (true).ToString()).ValueString);
            PrintInspectionDate = InputHelper.GetBoolean(SiteSettings.Get(loginID, "PrintInspectionDate", "ReportOptions", (true).ToString()).ValueString);
            PrintRepairDays = InputHelper.GetBoolean(SiteSettings.Get(loginID, "PrintRepairDays", "ReportOptions", (true).ToString()).ValueString);

            LoginInfo loginInfo = LoginInfo.GetByID(loginID);
            PrintLaborHours = loginInfo.ShowLaborTimeWO;

            Images = GetAllImagePaths(loginID, estimateID);

            // Determine if the Financing functionality should be included
            var merchantInfo = FinancingService.Instance.GetWisetackMerchantData(loginID);
            IsFinancingMerchantApproved = false; // !string.IsNullOrEmpty(merchantInfo?.MerchantID) && merchantInfo?.Status == "APPLICATION_APPROVED";

            // Default the IncludeLoanApplicationLink checkbox to checked if the merchant is approved
            IncludeLoanApplicationLink = false; // IsFinancingMerchantApproved;

            var emsBetaIds = ConfigurationManagerUtilities.GetAppSetting("EMSBetaIds");
            ShowEMSVersionSelection = emsBetaIds == "*" || emsBetaIds.Contains($",{loginID},");
        }

        #region Image Paths

        // This is copied from the Estimate Controller.  Need to refactor so it's not duplicated.

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

        /// <summary>
        /// Files for an estimate are stored on disk in a folder like C:/UserContent/LoginID/EstimateID.  This function returns that as a path.
        /// </summary>
        public string GetEstimateDiskPath(int loginID, int estimateID)
        {
            return Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), loginID.ToString(), estimateID.ToString());
        }

        #endregion

    }
}
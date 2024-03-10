using Kendo.Mvc.UI;
using Proestimator.ViewModel;
using ProEstimatorData.DataModel;
using ProEstimatorData.Models.SubModel;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Web;

namespace ProEstimator.Business.Logic
{
    public interface IEstimateService
    {
        Image CombineReportImages(List<Image> images);
        int CopyEstimate(int activeLoginID, int estimateID, string estimateName, bool newCustomer, bool copyInsuranceInformation, bool copyClaimantInformation, bool copyVehicleInformation, bool copyAttachedImages, bool copyLineItems, bool copyLatestItemsOnly);
        Task<Report> CreateEstimateReport(int estimateID, string reportType, int loginID, int activeLoginID, bool hideHours = false, int? supplementVersion = null, bool includeImages = true, bool onlySupplementImages = false, bool onlyCheckedImages = false, bool pdrOnly = false, bool PrintPnLNotes = false, bool PrintInspectionDate = false, bool attachImagesWithEmail = false, bool printSupplementPhotosOnly = false, bool? printHeaderInfoOnEstimateReport = null, bool PrintRepairDays = false, bool IncludeLoanApplicationLink = false, string selectedTechnicians = "");
        List<string> CreateImageByResolution(int imageSize, int imageSizePDF, int estimateID, int loginID, int supplement, bool onlyChecked, out List<EstimationImage> images);
        Estimate CreateNewEstimate(ActiveLogin activeLogin);
        Task<DecodeLicensePlateImageResults> DecodeLicensePlateImage(HttpPostedFile file, string readFailureMessage);
        Task<ReportFunctionResult> MakeQuickPrintEstimateReport(int userID, int loginID, int estimateID, int activeLoginID);
        UploadImageResult SaveImage(int loginID, int estimateID, Image image, string fileName, string extension, EstimationImage estimationImage = null);
        EstimateService.ReportAsImageFunctionResult SaveReportAsImage(Report report);
        UploadImageResult UploadImage(int loginID, int estimateID, HttpPostedFile file);
        bool IsEstimateTotalLoss(int estimateID);
        List<SimpleListItem> GetSections(int estimateID, int vehicleID, bool ignoreYearFilter = false);
        string GetACCapacitiesNote(int estimateID);

        List<DropDownTreeItemModel> GetSectionsListByVehicleforDropDownTreeItem(int estimateID, int vehicleID, bool ignoreYearFilter = false);
        List<TreeViewItemModel> GetSectionsListByVehicleforTreeViewItem(int estimateID, int vehicleID, bool ignoreYearFilter = false);
    }
}
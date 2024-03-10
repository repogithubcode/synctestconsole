using System.Collections.Generic;
using System.Data;
using System.Linq;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmSalesRep : IModelMap<VmSalesRep>, IDataTableMap<VmSalesRep>
    {
        public int ID;
        public int DT_RowId;

        public VmSalesRep ToModel(DataRow row)
        {
            var model = new VmSalesRep();
            model.DT_RowId = (int)row["SalesRepID"];
            model.ID = (int)row["SalesRepID"];
            model.Number = row["SalesNumber"].SafeString();
            model.FirstName = row["FirstName"].SafeString();
            model.LastName = row["LastName"].SafeString();
            model.Email = row["Email"].SafeString();
            model.UserName = row["UserName"].SafeString();
            model.Password = row["Password"].SafeString();

            model.InvoiceTab = (bool)row["pInvoiceTab"];
            model.OrgMaintTab = (bool)row["pOrgMaintTab"];
            model.SalesRepMaint = (bool)row["pSalesRepMaint"];
            model.SalesBoard = (bool)row["pSalesBoard"];
            model.ServerLogsTab = (bool)row["pServerLogsTab"];
            model.LoginFailureTab = (bool)row["pLoginFailureTab"];
            model.ErrorsTab = (bool)row["pErrorsTab"];
            model.CurrentSessionsTab = (bool)row["pCurrentSessionsTab"];
            model.LinkingTab = (bool)row["pLinkingTab"];
            model.PaymentReport = (bool)row["pPaymentReport"];
            model.ForcastedRevReport = (bool)row["pForcastedRevReport"];
            model.RenewalReport = (bool)row["pRenewalReport"];
            model.RoyaltyReport = (bool)row["pRoyaltyReport"];
            model.ExpectedRenReport = (bool)row["pExpectedRenReport"];
            model.ShopActivityReport = (bool)row["pShopActivityReport"];
            model.WebsiteAccessReport = (bool)row["pWebsiteAccessReport"];
            model.UnusedContracts = (bool)row["pUnusedContracts"];
            model.EstimatesByShop = (bool)row["pEstimatesByShop"];
            model.UserMaintLoginInfo = (bool)row["pUserMaintLoginInfo"];
            model.UserMaintOrgInfo = (bool)row["pUserMaintOrgInfo"];
            model.UserMaintContactInfo = (bool)row["pUserMaintContactInfo"];
            model.UserMaintSalesRep = (bool)row["pUserMaintSalesRep"];
            model.EditPermissions = (bool)row["pEditPermissions"];
            model.EditBonusGoals = (bool)row["pEditBonusGoals"];
            model.PromoMaintenance = (bool)row["pPromoMaintenance"];
            model.ExtensionReport = (bool)row["pExtensionReport"];
            model.UserMaintImpersonate = (bool)row["pUserMaintImpersonate"];
            model.LoginAttempts = (bool)row["pLoginAttempts"];
            model.UserMaintCreate = (bool)row["pUserMaintCreate"];
            model.Import = (bool)row["pImport"];
            model.ImportEst = (bool)row["pImportEst"];
            var renewalPermission = row["pDeleteRenewal"].SafeBool();
            model.RenewalReportDelete = renewalPermission == null ? false : (bool)renewalPermission;

            if (row.Table.Columns.Contains("isSalesRep"))
            {
                model.IsSalesRep = (bool)row["isSalesRep"];
            }

            if (row.Table.Columns.Contains("Active"))
            {
                model.Active = (bool)row["Active"];
            }

            model.CarFax = (bool)row["pCarFax"];
            model.SuccessBox = (bool)row["pSuccessBox"];

            model.PartsNowManager = (bool) row["pPartsNowManager"];

            var documentsManagerDownload = row["pDocumentsManagerDownload"].SafeBool();
            model.DocumentsManagerDownload = documentsManagerDownload == null ? false : (bool)documentsManagerDownload;

            var documentsManagerUpload = row["pDocumentsManagerUpload"].SafeBool();
            model.DocumentsManagerUpload = documentsManagerUpload == null ? false : (bool)documentsManagerUpload;

            var trialSetup = row["pTrialSetup"].SafeBool();
            model.TrialSetup = trialSetup == null ? false : (bool)trialSetup;

            model.AddOns = ProEstimatorData.InputHelper.GetBoolean(row["pAddOns"].ToString());

            return model;
        }

        public bool IsSalesRep { get; set; }
        public bool Active { get; set; }
        public bool Import { get; set; }

        public bool ImportEst { get; set; }

        public bool LinkingTab { get; set; }

        public bool PaymentReport { get; set; }

        public bool CurrentSessionsTab { get; set; }

        public bool ErrorsTab { get; set; }

        public bool LoginFailureTab { get; set; }

        public bool ServerLogsTab { get; set; }

        public bool SalesBoard { get; set; }

        public bool SalesRepMaint { get; set; }

        public bool OrgMaintTab { get; set; }

        public bool ForcastedRevReport { get; set; }

        public bool RenewalReport { get; set; }

        public bool RoyaltyReport { get; set; }

        public bool ExpectedRenReport { get; set; }

        public bool ShopActivityReport { get; set; }

        public bool WebsiteAccessReport { get; set; }

        public bool UnusedContracts { get; set; }

        public bool EstimatesByShop { get; set; }

        public bool UserMaintLoginInfo { get; set; }

        public bool UserMaintOrgInfo { get; set; }

        public bool UserMaintContactInfo { get; set; }

        public bool UserMaintSalesRep { get; set; }

        public bool EditPermissions { get; set; }

        public bool EditBonusGoals { get; set; }

        public bool PromoMaintenance { get; set; }

        public bool ExtensionReport { get; set; }

        public bool UserMaintImpersonate { get; set; }

        public bool LoginAttempts { get; set; }

        public bool UserMaintCreate { get; set; }

        public bool InvoiceTab { get; set; }
        public bool RenewalReportDelete { get; set; }

        public bool SuccessBox { get; set; }
        public bool CarFax { get; set; }

        public bool PartsNowManager { get; set; }
        public bool AddOns { get; set; }

        public bool DocumentsManagerUpload { get; set; }
        public bool DocumentsManagerDownload { get; set; }

        public bool TrialSetup { get; set; }

        public string Password { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string Number { get; set; }

        public VmSalesRep MapFromDataTableRow(Dictionary<string, Dictionary<string, string>> row)
        {
            var model = new VmSalesRep();
            int myKey;
            if (int.TryParse(row.FirstOrDefault().Key, out myKey))
            {
                model.ID = myKey;
            }
            model.FirstName = row.FirstOrDefault().Value["FirstName"];
            model.LastName = row.FirstOrDefault().Value["LastName"];
            model.Email = row.FirstOrDefault().Value["Email"];
            model.UserName = row.FirstOrDefault().Value["UserName"];
            model.Password = row.FirstOrDefault().Value["Password"];

            return model;
        }

        public Dictionary<string, Dictionary<string, string>> MapToDataTableRow()
        {
            var model = this;
            var row = new Dictionary<string, Dictionary<string, string>>();
            row[model.ID.ToString()] = new Dictionary<string, string>()
            {
                {"FirstName", model.FirstName},
                {"LastName", model.LastName},
                {"Email", model.Email},
                {"UserName", model.UserName},
                {"Password", model.Password},
                {"Number", model.Number},
                {"DT_RowId", model.DT_RowId.ToString()},
                {"ID", model.ID.ToString()}
            };

            return row;
        }
    }
}

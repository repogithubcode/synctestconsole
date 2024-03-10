namespace ProEstimator.Business
{
    internal static class AdminConstants
    {
        internal static string LogStripePayment
        {
            get { return "LogStripePayment"; }
        }
        internal static string Update_ConversionComplete
        {
            get { return "[Admin].[Update_ConversionComplete]"; }
        }

        internal static string GET_USER
        {
            get { return "[Admin].SearchUser"; }
        }

        internal static string GET_USER_INFO
        {
            get { return "[Admin].GetUserInfo"; }
        }

        internal static string GET_NUMBER_OF_LOGINS
        {
            get { return "[Admin].[GetNoOfLogins]"; }
        }

        internal static string ORG_SEARCH
        {
            get { return "[Admin].SearchOrg"; }
        }

        internal static string ORG_CREATE
        {
            get { return "[Admin].CreateOrg"; }
        }

        internal static string GET_CODE_LIST
        {
            get { return "[Admin].[Getcodelist]"; }
        }

        internal static string GET_LEVEL_TERMS
        {
            get { return "[Admin].[GetLevelTerms]"; }
        }

        internal static string GET_PROMO_BY_ID
        {
            get { return "[Admin].[GetPromobyId]"; }
        }

        internal static string GET_CUSTOMER_CONTACTS
        {
            get { return "[Admin].[GetLoginContacts]"; }
        }

        internal static string GET_ORG
        {
            get { return "[Admin].[GetOrganizationInfo]"; }
        }

        internal static string GET_ORG_SINGLE
        {
            get { return "[Admin].GetOrganization_Single"; }
        }

        internal static string Update_AutoPay
        {
            get { return "[Admin].[Update_AutoPay]"; }
        }

        internal static string GetContract_AddOn
        {
            get { return "[Admin].[GetContract_AddOn]"; }
        }

        internal static string Delete_Contract
        {
            get { return "[Admin].[DeleteContract]"; }
        }

        internal static string InsertContract
        {
            get { return "[Admin].[InsertContract]"; }
        }

        internal static string DeleteInvoice
        {
            get { return "[Admin].[DeleteInvoice]"; }
        }

        internal static string InsertInvoice_Custom
        {
            get { return "[Admin].[InsertInvoice_Custom]"; }
        }

        internal static string GetSalesReps
        {
            get { return "[Admin].[GetSalesReps]"; }
        }

        internal static string GetDefaultProfile
        {
            get { return "[Admin].[GetDefaultProfile]"; }
        }

        internal static string GetInvoiceTypes
        {
            get { return "[Admin].[GetInvoiceTypes]"; }
        }

        internal static string UpdateSalesRepSql
        {
            get { return "[Admin].UpdateSalesRep"; }
        }

        internal static string InsertSalesRepSql
        {
            get { return "[Admin].[InsertSalesRep]"; }
        }

        internal static string DeleteSalesRep
        {
            get { return "[Admin].[DeleteSalesRep]"; }
        }

        internal static string UpdateSalesRepPermissionSql
        {
            get { return "[Admin].[UpdateSalesRepPermission]"; }
        }

        internal static string GetExtensionHistorySql
        {
            get { return "[Admin].[GetExtensionHistory]"; }
        }

        internal static string UpdateSelectInvoice
        {
            get { return "[Admin].[UpdateSelectInvoice]"; }
        }

        internal static string UpdateSelectRenewal
        {
            get { return "[Admin].[UpdateSelectRenewal]"; }
        }

        internal static string USER_INFO_TABLE
        {
            get { return "UserInfo"; }
        }

        internal static string NO_OF_LOGINS_TABLE
        {
            get { return "NoOfLogins"; }
        }

        internal static string NEW_GET_CONTRACT
        {
            get { return "[admin].[GetContract]"; }
        }

        internal static string CONTRACT_TABLE
        {
            get { return "Contract"; }
        }

        internal static string PHONE_ENTRY
        {
            get { return "Phone"; }
        }

        internal static string STATE_ENTRY
        {
            get { return "USState"; }
        }

        internal static string GetCreditCardPayments
        {
            get { return "GetPayments_CC"; }
        }
        internal static string GetStripePayments
        {
            get { return "GetPayments_Stripe"; }
        }
        internal static string GetCheckPayments
        {
            get { return "GetPayments_Check"; }
        }

        internal static string GetAutoPayments
        {
            get { return "GetPayments_NoPaymentInfo"; }
        }

        internal static int STATE_GROUP
        {
            get { return 48; }
        }

        internal static int PHONE_GROUP
        {
            get { return 11; }
        }

        internal static string GetWebestImports
        {
            get { return "[Admin].[Get_WebestImports]"; }
        }
        internal static string GetStripeAuto
        {
            get { return "[Admin].[Get_AutoPay]"; }
        }
        internal static string InsertStripeAuto
        {
            get { return "[Admin].[Insert_StripeAuto]"; }
        }
        internal static string UpdateStripeAuto
        {
            get { return "[Admin].[Update_AutoPay]"; }
        }
        internal static string DataMigration_Estimate
        {
            get { return "DataMigration_Estimate"; }
        }
        internal static string DataMigration_Contracts
        {
            get { return "DataMigration_Contracts"; }
        }

        internal static string GET_SmsHistory
        {
            get { return "[Admin].[GET_SmsHistory]"; }
        }
        internal static string GET_PdrReport
        {
            get { return "[Admin].[GET_PdrReport]"; }
        }
        internal static string GET_RenewalReport
        {
            get { return "[Admin].[GetRenewals]"; }
        }
        internal static string GET_TRIAL_REPORT
        {
            get { return "[Admin].[GET_TRIAL_REPORT]"; }
        }

        internal static string Insert_SmsHistory
        {
            get { return "[Admin].[Insert_SmsHistory]"; }
        }

        internal static string GET_PARTS_NOW_CLIENTS
        {
            get { return "[Admin].[GET_PARTS_NOW_CLIENTS]"; }
        }

        internal static string UPDATE_PARTS_NOW_CLIENT
        {
            get { return "[Admin].[UPDATE_PARTS_NOW_CLIENT]"; }
        }

        public static string SET_DEFAULT_RATE_PROFILE
        {
            get { return "[Admin].[SET_DEFAULT_RATE_PROFILE]"; }
        }

        public static string INSERT_IMPORT
        {
            get { return "[Admin].[InsertImport]"; }
        }

        public static string UPDATE_IMPORT
        {
            get { return "[dbo].[UpdateImport]"; }
        }

        public static string GET_IMPORT_MESSAGE_BY_STATUS
        {
            get { return "[GET_IMPORT_MESSAGE_BY_STATUS]"; }
        }

        public static string GET_MigrateImages
        {
            get { return "[Admin].[MigrateImages]"; }
        }

        public static string LOG_EXCEPTION
        {
            get { return "[Admin].[LOG_EXCEPTION]"; }
        }

        public static string DISABLE_WEBEST
        {
            get { return "[Admin].[DISABLE_WEBEST]"; }
        }

        public static string DataMigration_EstimatesForLogin
        {
            get { return "[Admin].[DataMigration_EstimatesForLogin]"; }
        }

        //[Admin].[DataMigration_GetEstimateMigrationMetricForLogin]
        public static string DataMigration_GetEstimateMigrationMetricForLogin
        {
            get { return "[Admin].[DataMigration_GetEstimateMigrationMetricForLogin]"; }
        }

        public static string MIGRATE_VENDOR
        {
            get { return "[MIGRATE_VENDORForLogin]"; }
        }

        public static string DataMigration_PrevCheck
        {
            get { return "[DataMigration_PrevCheck]"; }
        }

        //public static string GET_QUEUE_EXCEPTIONS { get; set; }
        public static string GET_QUEUE_EXCEPTIONS
        {
            get { return "[Admin].[GET_QUEUE_EXCEPTIONS]"; }
        }

        public static string SelectRenewalReportSettings
        {
            get { return "[Admin].[SelectRenewalReportSettings]"; }
        }

        public static string RenewalReportSettingsTable
        {
            get { return "RenewalReportSettingsTable"; }
        }

        public static string InsertRenewalReportSettings
        {
            get { return "[Admin].[InsertRenewalReportSettings]"; }
        }

        public static string UpdateRenewalReportSettings
        {
            get { return "[Admin].[UpdateRenewalReportSettings]"; }
        }

        public static string Updaterenewalgoal
        {
            get { return "[Admin].[Updaterenewalgoal]"; }
        }
        public static string Insertrenewalgoal
        {
            get { return "[Admin].[Insertrenewalgoal]"; }
        }

        public static string Selectrenewalgoal
        {
            get { return "[Admin].[Selectrenewalgoal]"; }
        }

        public static string SelectrenewalgoalForActive
        {
            get { return "[Admin].[SelectrenewalgoalForActive]"; }
        }

        public static string SaveForecast
        {
            get { return "[Admin].[SaveForecast]"; }
        }

        public static string SelectTotalSold
        {
            get { return "[Admin].[SelectTotalSold]"; }
        }

        public static string SelectTotalSoldForActive
        {
            get { return "[Admin].[SelectTotalSoldForActive]"; }
        }

        public static string UpdateRenewalReport_PE
        {
            get { return "[Admin].[UpdateRenewalReport_PE]"; }
        }

        public static string UpdateRenewalReport_WE
        {
            get { return "[Admin].[UpdateRenewalReport_WE]"; }
        }

        public static string LoginsConversionCompleteInsert
        {
            get { return "[dbo].[LoginsConversionCompleteInsert]"; }
        }

        public static string DeleteRenewal
        {
            get { return "[Admin].[DeleteRenewal]"; }
        }
    }
}
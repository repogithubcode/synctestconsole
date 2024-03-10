using ProEstimator.Business.ILogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Extension;
using ProEstimatorData.DataAdmin;
using System.Data;

namespace ProEstimator.Admin.ViewModel
{
    public class TrialRecordVM 
    {
        public int Id { get; set; }
        public int SalesRepID { get; set; }
        public string CompanyName { get; set; }
        public string Name { get; set; }
        public int ContractID { get; set; }
        public string TrialStartDate { get; set; }
        public string TrialEndDate { get; set; }
        public int EstimateCount { get; set; }
        public string ContractType { get; set; }
        public bool Trial { get; set; }
        public bool HasConverted { get; set; }

        public TrialRecordVM(TrialReport trialReport)
        {
            Id = trialReport.Id;
            SalesRepID = trialReport.SalesRepID;
            CompanyName = trialReport.CompanyName;
            Name = trialReport.Name;
            ContractID = trialReport.ContractID;
            TrialStartDate = trialReport.TrialStartDate;
            TrialEndDate = trialReport.TrialEndDate;
            EstimateCount = trialReport.EstimateCount;
            ContractType = trialReport.ContractType;
            Trial = trialReport.Trial;
        }

        public static DataTable ToDataTable(List<TrialRecordVM> listRecordVMs)
        {
            DataRow dr = null;
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("Login Id");
            dataTable.Columns.Add("Company Name");
            dataTable.Columns.Add("Sales Rep");
            dataTable.Columns.Add("Contract ID");
            dataTable.Columns.Add("Trial Start Date");
            dataTable.Columns.Add("Trial End Date");
            dataTable.Columns.Add("Estimate Count");
            dataTable.Columns.Add("Contract Type");
            dataTable.Columns.Add("Has Converted");

            foreach (TrialRecordVM trialRecordVM in listRecordVMs)
            {
                dr = dataTable.NewRow();

                dr["Login Id"] = trialRecordVM.Id;
                dr["Company Name"] = trialRecordVM.CompanyName;
                dr["Sales Rep"] = trialRecordVM.Name;
                dr["Contract ID"] = trialRecordVM.ContractID;
                dr["Trial Start Date"] = trialRecordVM.TrialStartDate;
                dr["Trial End Date"] = trialRecordVM.TrialEndDate;
                dr["Estimate Count"] = trialRecordVM.EstimateCount;
                dr["Contract Type"] = trialRecordVM.ContractType;
                dr["Has Converted"] = trialRecordVM.HasConverted;

                dataTable.Rows.Add(dr);
            }

            return dataTable;
        }  
    }
}

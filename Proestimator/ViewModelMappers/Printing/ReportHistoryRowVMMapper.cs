using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using ProEstimatorData;

using Proestimator.ViewModel.Printing;

namespace Proestimator.ViewModelMappers.Printing
{
    public class ReportHistoryRowVMMapper : IVMMapper<ReportHistoryRowVM>
    {
        public ReportHistoryRowVM Map(MappingConfiguration configuration)
        {
            ReportHistoryRowVMMapperConfiguration config = configuration as ReportHistoryRowVMMapperConfiguration;

            ReportHistoryRowVM vm = new ReportHistoryRowVM();

            vm.ReportID = InputHelper.GetInteger(config.Row["ReportID"].ToString());
            vm.MailID = InputHelper.GetInteger(config.Row["EmailID"].ToString());
            vm.TimeStamp = InputHelper.GetDateTime(config.Row["DateCreated"].ToString()).AddHours(config.TimezoneOffset);
            vm.FileName = InputHelper.GetString(config.Row["FileName"].ToString());
            vm.CustomTemplateID = InputHelper.GetInteger(config.Row["CustomTemplateID"].ToString());
            vm.IsDeleted = InputHelper.GetBoolean(config.Row["IsDeleted"].ToString());
            vm.ReportType = InputHelper.GetString(config.Row["ReportType"].ToString());
            vm.ReportTypeTag = InputHelper.GetString(config.Row["ReportTypeTag"].ToString());
            vm.Notes = InputHelper.GetString(config.Row["Notes"].ToString());

            ProEstimatorData.DataModel.ReportType reportType = ProEstimatorData.DataModel.ReportType.GetAll().FirstOrDefault(o => o.Tag == vm.ReportTypeTag);

            if (reportType != null && vm.ReportType == vm.ReportTypeTag)
            {
                vm.ReportType = reportType.Text;
            }
            else if (vm.ReportTypeTag == "Custom")
            {
                vm.ReportType = config.Row["CustomReportName"].ToString();
            }

            if (vm.IsDeleted)
            {
                vm.DeleteRestoreImgName = "restore.gif";
            }
            else
            {
                vm.DeleteRestoreImgName = "delete.gif";
            }

            // Fix the file name, remove the _# part and add .pdf, for display in the grid
            vm.FileName = vm.FileName.Substring(0, vm.FileName.LastIndexOf("_")) + "." + (reportType != null && reportType.Text == "EMS" ? "zip" : "pdf");

            return vm;
        }
    }

    public class ReportHistoryRowVMMapperConfiguration : MappingConfiguration
    {
        public DataRow Row { get; set; }
        public int TimezoneOffset { get; set; }
    }
}
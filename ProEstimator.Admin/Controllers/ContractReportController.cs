using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.Web.Mvc;
using ProEstimator.Admin.ViewModel.ContractReport;
using ProEstimatorData.DataModel.Contracts;
using Proestimator.Admin.ViewModelMappers.ContractReport;

namespace ProEstimator.Admin.Controllers
{
    public class ContractReportController : AdminController
    {
        [Route("Reports/Contract")]
        public ActionResult Index()
        {
            ContractReportVM contractReportVM = new ContractReportVM();

            contractReportVM.SalesRepDDL = new SelectList(GetSalesRepList(), "Value", "Text");
            contractReportVM.SelectedSalesRep = "0";

            return View(contractReportVM);
        }

        private IEnumerable<SelectListItem> GetSalesRepList()
        {
            List<SelectListItem> salesRepSelectList = new List<SelectListItem>();
            List<ProEstimatorData.DataModel.SalesRep> salesReps = ProEstimatorData.DataModel.SalesRep.GetAll().Where(o => o.IsSalesRep && o.IsActive).ToList();
            foreach (ProEstimatorData.DataModel.SalesRep salesRep in salesReps)
            {
                salesRepSelectList.Add(new SelectListItem() { Value = salesRep.SalesRepID.ToString(), Text = salesRep.SalesNumber + " - " + salesRep.FirstName + " " + salesRep.LastName });
            }
            // Add an empty for the New drop down
            List<SelectListItem> allSalesRepSelectList = salesRepSelectList.ToList();
            allSalesRepSelectList.Insert(0, new SelectListItem() { Value = "0", Text = "--ALL--" });

            return allSalesRepSelectList;
        }

        public ActionResult GetContractReport([DataSourceRequest] DataSourceRequest request, string loginIDFilter, string rangeStartFilter, string rangeEndFilter, string salesRepFilter)
        {
            List<ContractGridVM> contractGridVMs = new List<ContractGridVM>();
            List<Contract> contracts = Contract.GetContracts(loginIDFilter, rangeStartFilter, rangeEndFilter, salesRepFilter);
            ContractGridVMMapper mapper = new ContractGridVMMapper();
            foreach (Contract contract in contracts)
            {
                contractGridVMs.Add(mapper.Map(new ContractGridVMMapperConfiguration() { Contract = contract }));
            }
            return Json(contractGridVMs.ToDataSourceResult(request));
        }
    }
}
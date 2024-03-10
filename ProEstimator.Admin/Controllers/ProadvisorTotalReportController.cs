using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using ProEstimator.Admin.ViewModel;
using ProEstimator.Admin.ViewModel.Logins;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

using ProEstimator.Business.Logic;
using ProEstimatorData.DataModel.Admin;

namespace ProEstimator.Admin.Controllers
{
    public class ProadvisorTotalReportController : AdminController
    {
        [Route("Reports/ProadvisorTotalReport")]
        public ActionResult Index()
        {
            ProadvisorTotalVM proadvisorTotalVM = new ProadvisorTotalVM();

            proadvisorTotalVM.SalesRepDDL = new SelectList(GetSalesRepList(), "Value", "Text");
            proadvisorTotalVM.SalesRep = "-1";

            return View(proadvisorTotalVM);
        }

        public ActionResult GetProadvisorTotalReport([DataSourceRequest] DataSourceRequest request, string loginIDFilter, string organizationFilter, int salesrepFilter,
                                                string rangeStartFilter, string rangeEndFilter)
        {
            List<ProadvisorTotalVM> proadvisorTotalVMs = new List<ProadvisorTotalVM>();

            // LoginID
            int loginID = 0;
            Boolean loginIdTryParseVal = Int32.TryParse(loginIDFilter, out loginID);

            List<ProadvisorTotal> proadvisorTotals = ProadvisorTotal.GetForFilter(loginID, organizationFilter ?? "", salesrepFilter, rangeStartFilter,rangeEndFilter);

            foreach (ProadvisorTotal proadvisorTotal in proadvisorTotals)
            {
                proadvisorTotalVMs.Add(new ProadvisorTotalVM(proadvisorTotal));
            }

            return Json(proadvisorTotalVMs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<SelectListItem> GetSalesRepList()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem() { Text = "Please Choose", Value = "-1" });

            List<SalesRep> reps = SalesRep.GetSpecial();
            foreach(SalesRep rep in reps)
            {
                selectListItems.Add(new SelectListItem() { Text = rep.FirstName + " " + rep.LastName, Value = rep.SalesRepID.ToString() });
            }

            return selectListItems;
        }

        private void AddRepIfFound(List<SalesRep> allReps, List<SelectListItem> listItems, int repID)
        {
            SalesRep rep = allReps.FirstOrDefault(o => o.SalesRepID == repID);
            if (rep != null)
            {
                listItems.Add(new SelectListItem() { Text = rep.FirstName + " " + rep.LastName, Value = rep.SalesRepID.ToString() });
            }
        }
    }
}
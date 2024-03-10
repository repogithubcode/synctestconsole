using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Proestimator.ViewModel.OptOut;
using ProEstimator.Business.OptOut;
using ProEstimatorData;

namespace Proestimator.Controllers
{
    public class OptOutController : Controller
    {

        private IOptOutService _optOutService;

        public OptOutController(IOptOutService optOutService)
        {
            _optOutService = optOutService;
        }

        public ActionResult Index(string link)
        {
            AutoPayPageVM vm = new AutoPayPageVM();

            FunctionResult result = _optOutService.ProcessOptOutLink(link);
            if (result.Success)
            {
                vm.SuccessMessage = result.ErrorMessage;
            }
            else
            {
                vm.ErrorMessage = result.ErrorMessage;
            }

            return View(vm);
        }
    }
}
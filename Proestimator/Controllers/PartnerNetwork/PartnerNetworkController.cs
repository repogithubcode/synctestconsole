using ProEstimator.DataRepositories.PartnerNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Proestimator.ViewModel.PartnerNetwork;

namespace Proestimator.Controllers.PartnerNetwork
{
    public class PartnerNetworkController : SiteController
    {
        private IPartnerNetworkRepository _partnerNetworkRepository;

        public PartnerNetworkController(IPartnerNetworkRepository partnerNetworkRepository)
        {
            _partnerNetworkRepository = partnerNetworkRepository;
        }

        [HttpGet]
        [Route("{userID}/partner-network")]
        public ActionResult Index()
        {
            PartnerNetworkPageVM vm = new PartnerNetworkPageVM();
            vm.Partners = _partnerNetworkRepository.GetAll();

            return View(vm);
        }

        public JsonResult LogClick(int userID, int partnerID)
        {
            int activeLoginID = GetActiveLoginID(userID);
            _partnerNetworkRepository.InsertClick(activeLoginID, partnerID);
            return Json("", JsonRequestBehavior.AllowGet);
        }
    }
}
using System;
using Proestimator.Helpers;
using ProEstimator.Business.Logic;
using ProEstimator.Business.Model;
using ProEstimatorData.Models.EditorTemplateModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

using ProEstimatorData.DataModel;

namespace Proestimator.Controllers
{
    [AllowAnonymous]
    public class PartsNowApiController : ApiController
    {
        private PartsNowService _service;

        PartsNowApiController()
        {
            _service = new PartsNowService();
        }

        [AllowAnonymous]
        [HttpPost]
        //[Route("api/getPartsNow")]
        public async Task<IHttpActionResult> GetPartsNow(int estimateID)
        {
            try
            {
                Estimate estimate = new Estimate(estimateID);
                Customer customer = Customer.Get(estimate.CustomerID);

                //string mode = (meMode == "Preset" ? "Presets" : "LineItems");
                string mode = "LineItems";
                List<ManualEntryListItem> list = ManualEntryHelper.getManualEntryList(mode, estimateID);
                var request = new PnWorkflowRequest();
                request.LoginID = estimate.CreatedByLoginID;
                request.Id = estimateID;
                request.List = list;
                request.CustomerName = customer.Contact.FirstName + " " + customer.Contact.LastName;
                var model = await _service.PnWorkFlow(request, estimateID);

                return Json(model);
            }
            catch(Exception ex)
            {
                ProEstimatorData.ErrorLogger.LogError(ex.Message, "GetPartsNow");
                return Json("");
            }
        }
    }
}

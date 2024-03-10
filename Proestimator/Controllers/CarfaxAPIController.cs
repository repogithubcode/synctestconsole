using Proestimator.ViewModel;
using ProEstimator.Business.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Proestimator.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class CarfaxController : ApiController
    {
        CarfaxService carfaxService = new CarfaxService();

        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody] DecodeRequestVM request)
        {
            var result = await carfaxService.GetVinByPlateAndState(request.Plate, request.State);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}

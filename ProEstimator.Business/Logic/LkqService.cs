using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ProEstimator.Business.Extension;
using ProEstimator.Business.Type;
using ProEstimatorData;
using service = ProEstimator.Business.LkqService;
using model = ProEstimator.Business.Model;

namespace ProEstimator.Business.Logic
{
    public class LkqService
    {
        private QuoteRequestMap _quoteRequstMap { get; set; }
        private RepairFacilityRequestMap _repairRequestMap { get; set; }
        private OrderRequestMap _orderRequestMap { get; set; }
        private const string LOG_LKQQUOTE = "LogLkqQuote";
        private DBAccess _data;
        private const string LogLkqProcessStatus = "LkqLog_Insert";
        private const string SUCCESS_MESSAGE = "Success";
        private const string FAILURE_MESSAGE = "Failure";

        public LkqService()
        {
            _quoteRequstMap = new QuoteRequestMap();
            _repairRequestMap = new RepairFacilityRequestMap();
            _orderRequestMap = new OrderRequestMap();
            _data = new DBAccess();
        }

        ///TODO: Identify elements of quote response required for the UI to be included in the model.QuoteResponse class to simplify service usage
        public async Task<model.QuoteResponse> GetQuote(model.QuoteRequest item)
        {
            var result = new model.QuoteResponse();
            using (var client = new service.QuotesClient())
            {
                client.Open();
                //var user = _quoteRequstMap.Convert<service.UserRequest>(item);
                //var estimate = _quoteRequstMap.Convert<service.Estimate>(item);
                var user = item.ToModel(new service.UserRequest());
                var estimate = item.ToModel(new service.Estimate());

                var response = await client.GetQuoteAsync(user, estimate);
                if (response.IsSuccessful)
                {
                    result.Parts = response.Value.AvailableParts;
                }
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("shopId", estimate.RepairFacilityId));
                parameters.Add(new SqlParameter("request", estimate.Serialize()));
                parameters.Add(new SqlParameter("response", response.Serialize()));
                var db = new DBAccess();
                db.ExecuteNonQuery(LOG_LKQQUOTE, parameters);
            }

            return result;
        }

        public async Task<model.RepairResponse> SaveFacility(model.RepairRequest item)
        {
            var result = new model.RepairResponse();
            using (var client = new service.QuotesClient())
            {
                client.Open();

                if (item.RegistrationType == RegistrationType.Register)
                {
                    //var user = _repairRequestMap.Convert<service.UserInformation>(item);
                    //var facility = _repairRequestMap.Convert<service.RegisterRepairFacilityRequest>(item);
                    var user = item.ToModel(new service.UserInformation());
                    var facility = item.ToModel(new service.RegisterRepairFacilityRequest());
                    facility.UserRequestInfo = user;
                    var response = await client.RegisterRepairFacilityAsync(facility);
                }
                else
                {
                    //var user = _repairRequestMap.Convert<service.UserInformation>(item);
                    //var facility = _repairRequestMap.Convert<service.UnRegisterRepairFacilityRequest>(item);
                    var user = item.ToModel(new service.UserInformation());
                    var facility = item.ToModel(new service.UnRegisterRepairFacilityRequest());
                    facility.UserRequestInfo = user;
                    var response = await client.UnRegisterRepairFacilityAsync(facility);
                }
            }

            return result;
        }

        public async Task<model.OrderResponse> SaveOrder(model.OrderRequest item)
        {
            var result = new model.OrderResponse();
            using (var client = new service.QuotesClient())
            {
                client.Open();
                //var user = _orderRequestMap.Convert<service.UserRequest>(item);
                //var purchaseOrder = _orderRequestMap.Convert<service.PurchaseOrderRequest>(item);
                var user = item.ToModel(new service.UserRequest());
                var purchaseOrder = item.ToModel(new service.PurchaseOrderRequest());
                var response = await client.ProcessOrderAsync(user, purchaseOrder);
                if (response.IsSuccessful)
                {
                    result.PurchaseOrderDetails = response.Value.PurchaseOrderDetails;
                    _data.ExecuteNonQuery(LOG_LKQQUOTE, GetPartOrderParams(true, item));
                }
                else
                {
                    _data.ExecuteNonQuery("", GetPartOrderParams(false, item));
                }
            }

            return result;
        }

        private List<SqlParameter> GetPartOrderParams(bool state, model.OrderRequest request)
        {
            var result = new List<SqlParameter>();
            result.Add(new SqlParameter("EstimateId", request.EstimateId));
            result.Add(new SqlParameter("Type", state ? SUCCESS_MESSAGE : FAILURE_MESSAGE));

            return result;
        }
    }
}

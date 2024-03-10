using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData;

using ProEstimator.Business.Extension;
using ProEstimator.Business.Logic;
using ProEstimator.Business.Model.Admin;

using Newtonsoft.Json;

namespace ProEstimator.Admin.Controllers
{
    public class LoginSessionReportController : ApiController
    {
        private readonly AdminService _adminService = new AdminService();

        // get api/loginsessionreport/GetCurrentSessions
        //[HttpGet]
        //public HttpResponseMessage GetCurrentSessions()
        //{
        //    var model = SessionService.GetLoginCurrentSessions();
        //    return Request.CreateResponse(HttpStatusCode.OK, model);
        //}

        // GET api/loginsessionreport/GetFailureReport
        [HttpGet]
        public HttpResponseMessage GetFailureReport(string LoginNameFilter, string OrganizationFilter, string LoginIdFilter)
        {
            var model = GetLoginFailureReport(LoginNameFilter ?? "", OrganizationFilter ?? "", LoginIdFilter ?? "");
            return Request.CreateResponse(HttpStatusCode.OK, model);

        }

        public static List<VmLoginFailureReport> GetLoginFailureReport(string LoginNameFilter, string OrganizationFilter, string loginIdFilter)
        {
            var dt = GetLoginFailed(LoginNameFilter, OrganizationFilter, loginIdFilter);
            var result = (from DataRow row in dt.Rows select row.ToModel<VmLoginFailureReport>()).ToList();

            if (string.IsNullOrEmpty(LoginNameFilter) && string.IsNullOrEmpty(OrganizationFilter))
            {
                result = result.OrderByDescending(item => item.ID).ToList(); //check it once
            }

            return result;
        }

        public static DataTable GetLoginFailed(string LoginNameFilter, string OrganizationFilter, string loginIdFilter)
        {
            var result = new DataTable();
            DBAccess db = new DBAccess();
            List<SqlParameter> parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter("LoginNameFilter", LoginNameFilter));
            parameters.Add(new SqlParameter("OrganizationFilter", OrganizationFilter));

            var item = db.ExecuteWithTable("getLoginFailures", parameters);

            if (item != null && item.DataTable != null)
            {
                item.DataTable.TableName = "getLoginFailures";
                return item.DataTable;
            }
            return result;
        }
    }
}

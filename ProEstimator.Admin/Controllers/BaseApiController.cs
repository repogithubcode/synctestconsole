using ProEstimator.Business.Logic;
using ProEstimator.Business.Logic.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ProEstimator.Admin.Controllers
{
    public class BaseApiController : ApiController
    {
        public AdminBusinessServices AdminRepoServices = new AdminBusinessServices();

    }
    public class AdminBusinessServices
    {

        private SalesBoardReportService _salesBoardReportService;

        private ErrorLogReportService _errorLogReportService;

        public FeedBackBugReportService _feedBackBugReportService;

        public SalesBoardReportService SalesBoardReportService
        {
            get
            {
                return _salesBoardReportService ?? (_salesBoardReportService = new SalesBoardReportService());
            }
        }

        public ErrorLogReportService ErrorLogReportService
        {
            get
            {
                return _errorLogReportService ?? (_errorLogReportService = new ErrorLogReportService());
            }
        }

        public FeedBackBugReportService FeedBackBugReportService
        {
            get
            {
                return _feedBackBugReportService ?? (_feedBackBugReportService = new FeedBackBugReportService());
            }
        }
    }
}
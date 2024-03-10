using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel
{
    public class EstimatorVM
    {
        public int ID { get; private set; }
        public int EstimateID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int LoginID { get; set; }
        public int OrderNumber { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }
        public string DefaultEstimator { get; set; }

        public EstimatorVM()
        {

        }

        public EstimatorVM(Estimator estimator)
        {
            ID = estimator.ID;
            EstimateID = estimator.EstimateID;
            FirstName = estimator.FirstName;
            LastName = estimator.LastName;
            LoginID = estimator.LoginID;
            OrderNumber = estimator.OrderNumber;
            Email = estimator.Email;
            Phone = estimator.Phone;
            DefaultEstimator = estimator.DefaultEstimator ? "Yes" : "";
        }

    }
}
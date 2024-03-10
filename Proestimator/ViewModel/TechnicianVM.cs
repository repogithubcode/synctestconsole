using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel;
using System.Web.Mvc;

namespace Proestimator.ViewModel
{
    public class TechnicianVM
    {
        public int ID { get; private set; }
        public int EstimateID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int LoginID { get; set; }
        public int OrderNumber { get; set; }
        public int UserID { get; set; }

        public SelectList LaborTypeList { get; set; }

        public int LaborTypeID { get; set; }
        public string LaborTypeText { get; set; }

        public DateTime? TimeStamp { get; set; }
        public Boolean IsDeleted { get; set; }

        public TechnicianVM()
        {

        }

        public TechnicianVM(Technician technician)
        {
            ID = technician.ID;
            EstimateID = technician.EstimateID;
            FirstName = technician.FirstName;
            LastName = technician.LastName;
            LoginID = technician.LoginID;
            OrderNumber = technician.OrderNumber;
            LaborTypeID = technician.LaborTypeID;
            LaborTypeText = technician.LaborTypeText;

            TimeStamp = technician.TimeStamp;
            IsDeleted = technician.IsDeleted;
        }

    }
}
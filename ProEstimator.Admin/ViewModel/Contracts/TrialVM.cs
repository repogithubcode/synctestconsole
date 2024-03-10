using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Admin.ViewModel.Contracts
{
    public class TrialVM
    {
        public int ID { get; set; }
        public string DateCreated { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool HasEMS { get; set; }
        public bool HasFrameData { get; set; }
        public bool HasQBExporter { get; set; }
        public bool HasProAdvisor { get; set; }
        public bool HasImages { get; set; }
        public bool HasCustomReports { get; set; }
        public bool HasBundle { get; set; }

        public string ErrorMessage { get; set; }

        public bool HasMultiUser { get; set; }

        public TrialVM(Trial trial)
        {
            ID = trial.ID;
            DateCreated = trial.CreationStamp.ToShortDateString();
            StartDate = trial.StartDate.ToShortDateString();
            EndDate = trial.EndDate.ToShortDateString();
            IsActive = trial.Active;
            IsDeleted = trial.IsDeleted;
            HasEMS = trial.HasEMS;
            HasFrameData = trial.HasFrameData;
            HasQBExporter = trial.HasQBExport;
            HasProAdvisor = trial.HasProAdvisor;
            HasImages = trial.HasImages;
            HasCustomReports = trial.HasCustomReports;
            HasBundle = trial.HasBundle;
            HasMultiUser = trial.HasMultiUser;

            ErrorMessage = "";
        }

    }
}
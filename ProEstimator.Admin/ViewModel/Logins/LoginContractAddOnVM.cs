using System.Collections.Generic;
using System.Linq;

using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Admin.ViewModel.Logins
{
    public class LoginContractAddOnVM
    {
        public string AddOnName { get; set; }
        public bool HasAddOn { get; set; }
        public bool HasPermission { get; set; }
        public bool IsActiveTrial { get; set; }

        public LoginContractAddOnVM() { }

        private LoginContractAddOnVM(string name, bool hasAddOn, bool isActiveTrial, bool bundle, bool permission)
        {
            AddOnName = name;
            HasAddOn = hasAddOn;
            HasPermission = permission && (hasAddOn || isActiveTrial || bundle);
            IsActiveTrial = isActiveTrial;
        }

        public static List<LoginContractAddOnVM> GetLoginContractAddOnVMList(Trial trial, bool permission)
        {
            List<LoginContractAddOnVM> loginContractAddOnVMs = new List<LoginContractAddOnVM>();

            loginContractAddOnVMs.Add(new LoginContractAddOnVM("Frame Data", false, trial.HasFrameData, false, permission));
            loginContractAddOnVMs.Add(new LoginContractAddOnVM("EMS Communiations Package", false, trial.HasEMS, false, permission));
            loginContractAddOnVMs.Add(new LoginContractAddOnVM("Multi User", false, trial.HasMultiUser, false, permission));
            loginContractAddOnVMs.Add(new LoginContractAddOnVM("QB Exporter", false, trial.HasQBExport, trial.HasBundle, permission));
            loginContractAddOnVMs.Add(new LoginContractAddOnVM("Pro Advisor", false, trial.HasProAdvisor, trial.HasBundle, permission));
            loginContractAddOnVMs.Add(new LoginContractAddOnVM("Image Editing", false, trial.HasImages, trial.HasBundle, permission));
            loginContractAddOnVMs.Add(new LoginContractAddOnVM("Custom Reports", false, trial.HasCustomReports, trial.HasBundle, permission));
            loginContractAddOnVMs.Add(new LoginContractAddOnVM("Bundle", false, trial.HasBundle, false, permission));

            return loginContractAddOnVMs;
        }

        public static List<LoginContractAddOnVM> GetLoginContractAddOnVMList(List<ContractAddOn> addOns, List<ContractAddOnTrial> trialAddOns, bool permission)
        {
            List<LoginContractAddOnVM> loginContractAddOnVMs = new List<LoginContractAddOnVM>();

            bool hasBundle = addOns.FirstOrDefault(o => o.AddOnType.ID == 12) != null || trialAddOns.FirstOrDefault(o => o.AddOnType.ID == 2) != null;

            loginContractAddOnVMs.Add(new LoginContractAddOnVM("Frame Data", addOns.FirstOrDefault(o => o.AddOnType.ID == 2) != null, trialAddOns.FirstOrDefault(o => o.AddOnType.ID == 2) != null, false, permission));
            loginContractAddOnVMs.Add(new LoginContractAddOnVM("EMS Communiations Package", addOns.FirstOrDefault(o => o.AddOnType.ID == 5) != null, trialAddOns.FirstOrDefault(o => o.AddOnType.ID == 5) != null, false, permission));
            loginContractAddOnVMs.Add(new LoginContractAddOnVM("Multi User", addOns.FirstOrDefault(o => o.AddOnType.ID == 8) != null, trialAddOns.FirstOrDefault(o => o.AddOnType.ID == 8) != null, false, permission));
            loginContractAddOnVMs.Add(new LoginContractAddOnVM("QB Exporter", addOns.FirstOrDefault(o => o.AddOnType.ID == 9) != null, trialAddOns.FirstOrDefault(o => o.AddOnType.ID == 9) != null, hasBundle, permission));
            loginContractAddOnVMs.Add(new LoginContractAddOnVM("Pro Advisor", addOns.FirstOrDefault(o => o.AddOnType.ID == 10) != null, trialAddOns.FirstOrDefault(o => o.AddOnType.ID == 10) != null, hasBundle, permission));
            loginContractAddOnVMs.Add(new LoginContractAddOnVM("Image Editing", addOns.FirstOrDefault(o => o.AddOnType.ID == 11) != null, trialAddOns.FirstOrDefault(o => o.AddOnType.ID == 11) != null, hasBundle, permission));
            loginContractAddOnVMs.Add(new LoginContractAddOnVM("Custom Reports", addOns.FirstOrDefault(o => o.AddOnType.ID == 13) != null, trialAddOns.FirstOrDefault(o => o.AddOnType.ID == 13) != null, hasBundle, permission));
            loginContractAddOnVMs.Add(new LoginContractAddOnVM("Bundle", addOns.FirstOrDefault(o => o.AddOnType.ID == 12) != null, trialAddOns.FirstOrDefault(o => o.AddOnType.ID == 12) != null, false, permission));

            return loginContractAddOnVMs;
        }
    }
}
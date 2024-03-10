using ProEstimator.Business.Model;
using ProEstimatorData.DataModel.ProAdvisor;
using ProEstimatorData.DataModel;
using ProEstimatorData.Models.SubModel;
using ProEstimatorData;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Logic;
using ProEstimator.Business.Panels;
using ProEstimator.DataRepositories.ProAdvisor;
using System.Data.SqlClient;
using System.Threading;

namespace ProEstimator.Business.ProAdvisor
{
    public class ProAdvisorService : IProAdvisorService
    {
        private IEstimateService _estimateService;
        private IPanelService _panelService;
        private IProAdvisorPresetRepository _proAdvisorPresetRepository;
        

        public ProAdvisorService(IEstimateService estimateService, IPanelService panelService, IProAdvisorPresetRepository proAdvisorPresetRepository)
        {
            _estimateService = estimateService;
            _panelService = panelService;
            _proAdvisorPresetRepository = proAdvisorPresetRepository;
        }

        public List<ProAdvisorRecommendation> GetRecommendations(Estimate estimate, int sectionKey, string sectionDescription, string addAction)
        {
            List<ProAdvisorRecommendation> returnList = new List<ProAdvisorRecommendation>();

            // Only apply Add Ons to estimates created after the feature was added to the site
            if (estimate.EstimateID < InputHelper.GetInteger(ConfigurationManager.AppSettings["FirstAdminInfoIDForAddOns"]))
            {
                return returnList;
            }

            Vehicle vehicle = ProEstimatorData.DataModel.Vehicle.GetByEstimate(estimate.EstimateID);
            if (vehicle == null)
            {
                return returnList;
            }

            List<SimpleListItem> sections = _estimateService.GetSections(estimate.EstimateID, vehicle.VehicleID, true);

            SimpleListItem section = sections.FirstOrDefault(o => o.Value == sectionKey.ToString());
            if (section != null)
            {
                List<Panel> panelMatches = _panelService.GetForSection(section.Text);

                foreach (Panel panel in panelMatches)
                {
                    if (_panelService.IsPrimaryPanelMatch(panel, sectionDescription))
                    {
                        List<ProAdvisorPreset> presets = _proAdvisorPresetRepository.GetForRuleAndProfile(estimate.EstimateID, panel.PrimaryPanelLinkRuleID, estimate.AddOnProfileID, addAction);

                        foreach (ProAdvisorPreset preset in presets)
                        {
                            returnList.Add(new ProAdvisorRecommendation(preset));
                        }

                        break;
                    }
                }
            }

            return returnList;
        }

        public FunctionResult AddPresetToEstimate(int estimateID, int presetID, int parentLineID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("EstimateID", estimateID));
            parameters.Add(new SqlParameter("PresetID", presetID));
            parameters.Add(new SqlParameter("ParentLineID", parentLineID));

            DBAccess db = new DBAccess();
            return db.ExecuteNonQuery("AddOnPreset_AddToEstimate", parameters);
        }

        public double GetAddOnTotalForLogin(int loginID)
        {
            DBAccess db = new DBAccess();

            DBAccessStringResult result = db.ExecuteWithStringReturn("ProAdvisorTotalCache_GetForLogin", new SqlParameter("LoginID", loginID));
            if (result.Success)
            {
                return InputHelper.GetDouble(result.Value);
            }

            return 0;
        }

        public void RefreshTotalForEstimate(int estimateID)
        {
            var thread = new Thread(() => CallRefreshTotalForEstimate(estimateID));
            thread.Start();
        }

        private void CallRefreshTotalForEstimate(int estimateID)
        {
            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("ProAdvisorEstimateTotalCache_Update", new SqlParameter("EstimateID", estimateID));
        }

    }
}

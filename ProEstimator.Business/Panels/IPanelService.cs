using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.LinkRules;

namespace ProEstimator.Business.Panels
{
    public interface IPanelService
    {
        List<Panel> GetMainPanels();
        List<Panel> GetLinkedPanels(int panelID);
        Panel GetPanel(int id);
        List<Panel> GetForSection(string sectionDescription);
        FunctionResult SavePanelDetails(int activeLoginID, int panelID, string panelName, int sortOrder, bool symmetry, int sectionLinkRuleID, int primaryPanelLinkRuleID, string linkedPanelIDs);
        bool IsPrimaryPanelMatch(Panel panel, string partDescription);
        FunctionResult DeleteRuleFromPanel(int activeLoginID, int panelID, LinkRuleType ruleType);
    }
}

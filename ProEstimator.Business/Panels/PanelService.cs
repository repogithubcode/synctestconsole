using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Panels.LinkRules;
using ProEstimator.DataRepositories.Panels;
using ProEstimator.DataRepositories.Panels.LinkRules;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.LinkRules;

namespace ProEstimator.Business.Panels
{
    public class PanelService : IPanelService
    {
        private IPanelRepository _panelRepository;
        private ILinkRuleRepository _linkRuleRepository;
        private ILinkRuleService _linkRuleService;

        public PanelService(IPanelRepository panelRepository, ILinkRuleRepository linkRuleRepository, ILinkRuleService linkRuleService)
        {
            _panelRepository = panelRepository;
            _linkRuleRepository = linkRuleRepository;
            _linkRuleService = linkRuleService;
        }

        public List<Panel> GetMainPanels()
        {
            return _panelRepository.GetAllPanels();
        }

        public List<Panel> GetLinkedPanels(int panelID)
        {
            return _panelRepository.GetLinkedPanels(panelID);
        }

        public Panel GetPanel(int id)
        {
            return _panelRepository.GetByID(id);
        }

        /// <summary>
        /// Pass a section description from the Mitchel database, and return a list of Panels that it matches.
        /// </summary>
        public List<Panel> GetForSection(string sectionDescription)
        {
            List<Panel> returnPanels = new List<Panel>();

            List<Panel> allPanels = _panelRepository.GetAllPanels();
            List<LinkRule> allLinkRules = _linkRuleRepository.GetAll();

            foreach (Panel panel in allPanels)
            {
                if (IsSectionMatch(panel, sectionDescription, allLinkRules))
                {
                    returnPanels.Add(panel);
                }
            }

            return returnPanels;
        }

        private bool IsSectionMatch(Panel panel, string sectionDescription, List<LinkRule> allLinkRules)
        {
            if (panel != null && panel.SectionLinkRuleID > 0)
            {
                LinkRule linkRule = allLinkRules.FirstOrDefault(o => o.ID == panel.SectionLinkRuleID);
                if (linkRule != null)
                {
                    bool ruleMatch = _linkRuleService.RuleMatchCheck(linkRule, sectionDescription);
                    if (ruleMatch)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public FunctionResult SavePanelDetails(int activeLoginID, int panelID, string panelName, int sortOrder, bool symmetry, int sectionLinkRuleID, int primaryPanelLinkRuleID, string linkedPanelIDs)
        {
            Panel panel = _panelRepository.GetByID(panelID);
            if (panel == null)
            {
                new SaveResult("Invalid Panel ID");
            }

            panel.PanelName = panelName;
            panel.SortOrder = sortOrder;
            panel.Symmetry = symmetry;
            panel.SectionLinkRuleID = sectionLinkRuleID;
            panel.PrimaryPanelLinkRuleID = primaryPanelLinkRuleID;
            
            FunctionResult saveResult = _panelRepository.Save(panel, activeLoginID);

            if (saveResult.Success == true && !string.IsNullOrEmpty(linkedPanelIDs))
            {
                string[] parts = linkedPanelIDs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(part => part.Trim())
                              .ToArray();

                int[] linkedIDs = new int[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                {
                    linkedIDs[i] = InputHelper.GetInteger(parts[i]);
                }

                _panelRepository.SaveLinkedPanels(panelID, linkedIDs);
            }

            return saveResult;
        }

        public bool IsPrimaryPanelMatch(Panel panel, string partDescription)
        {
            if (panel.PrimaryPanelLinkRuleID > 0)
            {
                LinkRule linkRule = _linkRuleRepository.Get(panel.PrimaryPanelLinkRuleID);
                return _linkRuleService.RuleMatchCheck(linkRule, partDescription);
            }

            return false;
        }

        public FunctionResult DeleteRuleFromPanel(int activeLoginID, int panelID, LinkRuleType ruleType)
        {
            Panel panel = GetPanel(panelID);

            if (panel != null)
            {
                if (ruleType == LinkRuleType.SectionContainer && panel.SectionLinkRuleID > 0)
                {
                    LinkRule linkRule = _linkRuleService.GetRule(panel.SectionLinkRuleID);
                    if (linkRule != null)
                    {
                        linkRule.Deleted = true;
                        _linkRuleRepository.Save(linkRule, activeLoginID);
                    }

                    panel.SectionLinkRuleID = 0;
                    _panelRepository.Save(panel, activeLoginID);
                    return new FunctionResult();
                }
                else if (ruleType == LinkRuleType.PrimaryPanel && panel.PrimaryPanelLinkRuleID > 0)
                {
                    LinkRule linkRule = _linkRuleService.GetRule(panel.PrimaryPanelLinkRuleID);
                    if (linkRule != null)
                    {
                        linkRule.Deleted = true;
                        _linkRuleRepository.Save(linkRule, activeLoginID);
                    }

                    panel.PrimaryPanelLinkRuleID = 0;
                    _panelRepository.Save(panel, activeLoginID);
                    return new FunctionResult();
                }
                else if (ruleType == LinkRuleType.PrimarySection && panel.PrimarySectionLinkRuleID > 0)
                {
                    LinkRule linkRule = _linkRuleService.GetRule(panel.PrimarySectionLinkRuleID);
                    if (linkRule != null)
                    {
                        linkRule.Deleted = true;
                        _linkRuleRepository.Save(linkRule, activeLoginID);
                    }

                    panel.PrimarySectionLinkRuleID = 0;
                    _panelRepository.Save(panel, activeLoginID);
                    return new FunctionResult();
                }
            }

            return new FunctionResult("Unable to delete panel from rule.");
        }
    }
}

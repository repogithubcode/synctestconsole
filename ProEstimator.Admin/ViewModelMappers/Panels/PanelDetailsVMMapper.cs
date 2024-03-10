using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.Contracts;
using ProEstimator.Admin.ViewModel.Contracts;
using Proestimator.Admin.ViewModelMappers;
using ProEstimator.Admin.ViewModel.LinkRules;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.LinkRules;
using ProEstimator.Business.Panels.LinkRules;

namespace ProEstimator.Admin.ViewModelMappers.Panels
{
    public class PanelDetailsVMMapper : IVMMapper<PanelDetailsVM>
    {
        public PanelDetailsVM Map(MappingConfiguration mappingConfiguration)
        {
            PanelDetailsVMMapperConfiguration config = mappingConfiguration as PanelDetailsVMMapperConfiguration;

            PanelDetailsVM vm = new PanelDetailsVM();

            vm.ID = config.Panel.ID;
            vm.PanelName = config.Panel.PanelName;
            vm.SortOrder = config.Panel.SortOrder;
            vm.Symmetry = config.Panel.Symmetry;
            vm.SectionLinkRuleID = config.Panel.SectionLinkRuleID;
            vm.PrimarySectionLinkRuleID = config.Panel.PrimarySectionLinkRuleID;
            vm.PrimaryPanelLinkRuleID = config.Panel.PrimaryPanelLinkRuleID;

            vm.SectionLinkSummary = "-empty-";
            vm.PrimarySectionLinkSummary = "-empty-";
            vm.PrimaryPanelLinkSummary = "-empty-";

            if (vm.SectionLinkRuleID > 0)
            {
                LinkRule sectionLinkRule = config.LinkRuleService.GetRule(vm.SectionLinkRuleID);
                string summaryText = config.LinkRuleService.GetRuleSummaryText(sectionLinkRule);
                if (!string.IsNullOrEmpty(summaryText))
                {
                    vm.SectionLinkSummary = summaryText;
                }
            }

            if (vm.PrimarySectionLinkRuleID > 0)
            {
                LinkRule sectionLinkRule = config.LinkRuleService.GetRule(vm.PrimarySectionLinkRuleID);
                string summaryText = config.LinkRuleService.GetRuleSummaryText(sectionLinkRule); 
                if (!string.IsNullOrEmpty(summaryText))
                {
                    vm.PrimarySectionLinkSummary = summaryText;
                }
            }

            if (vm.PrimaryPanelLinkRuleID > 0)
            {
                LinkRule partLinkRule = config.LinkRuleService.GetRule(vm.PrimaryPanelLinkRuleID);
                string summaryText = config.LinkRuleService.GetRuleSummaryText(partLinkRule); 
                if (!string.IsNullOrEmpty(summaryText))
                {
                    vm.PrimaryPanelLinkSummary = summaryText;
                }
            }

            return vm;
        }
    }

    public class PanelDetailsVMMapperConfiguration : MappingConfiguration
    {
        public ILinkRuleService LinkRuleService { get; set; }
        public Panel Panel { get; set; }

    }
}
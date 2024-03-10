using System.Collections.Generic;
using ProEstimatorData;
using ProEstimatorData.DataModel;

namespace ProEstimator.DataRepositories.Panels
{
    public interface IPanelRepository
    {
        FunctionResult Save(Panel panel, int activeLoginID);
        List<Panel> GetAllPanels();
        Panel GetByID(int id);
        FunctionResult SavePanelLink(int parentPanelID, int linkedPanelID);
        FunctionResult DeletePanelLink(int parentPanelID, int linkedPanelID);
        List<Panel> GetLinkedPanels(int parentPanelID);
        FunctionResult SaveLinkedPanels(int panelID, int[] linkedPanelIDs);

    }
}

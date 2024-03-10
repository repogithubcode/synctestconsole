using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProEstimatorData;
using ProEstimatorData.DataModel;

namespace ProEstimator.DataRepositories.Panels
{
    public class PanelRepository : IPanelRepository
    {
        /// <summary>
        /// Returns all "root" panels, ones that do not have a parent.
        /// </summary>
        public List<Panel> GetAllPanels()
        {
            List<Panel> allPanels = new List<Panel>();

            // Load panels from the db
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Panels_GetAll");

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                allPanels.Add(InstantiatePanel(row));
            }

            return allPanels.OrderBy(o => o.SortOrder).ToList();
        }

        public Panel GetByID(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Panels_Get", new SqlParameter("ID", id));

            if (tableResult.Success)
            {
                return InstantiatePanel(tableResult.DataTable.Rows[0]);
            }

            return null;
        }        

        public FunctionResult Save(Panel panel, int activeLoginID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", panel.ID));
            parameters.Add(new SqlParameter("PanelName", panel.PanelName));
            parameters.Add(new SqlParameter("SortOrder", panel.SortOrder));
            parameters.Add(new SqlParameter("Symmetry", panel.Symmetry));

            parameters.Add(new SqlParameter("SectionLinkRuleID", panel.SectionLinkRuleID));
            parameters.Add(new SqlParameter("PrimarySectionLinkRuleID", panel.PrimarySectionLinkRuleID));
            parameters.Add(new SqlParameter("PrimaryPanelLinkRuleID", panel.PrimaryPanelLinkRuleID));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("Panels_Save", parameters);

            if (result.Success)
            {
                if (panel.ID == 0 && result.Value > 0)
                {
                    ((IIDSetter)panel).ID = result.Value;
                    ChangeLogManager.LogChange(activeLoginID, "Panel", panel.ID, 0, parameters, panel.RowAsLoaded);
                }
            }

            return new FunctionResult(result.Success, result.ErrorMessage); ;
        }

        private Panel InstantiatePanel(DataRow row)
        {
            Panel panel = new Panel();

            ((IIDSetter)panel).ID = InputHelper.GetInteger(row["ID"].ToString());
            panel.PanelName = row["PanelName"].ToString();
            panel.SortOrder = InputHelper.GetInteger(row["SortOrder"].ToString());
            panel.Symmetry = InputHelper.GetBoolean(row["Symmetry"].ToString());

            panel.SectionLinkRuleID = InputHelper.GetInteger(row["SectionLinkRuleID"].ToString());
            panel.PrimarySectionLinkRuleID = InputHelper.GetInteger(row["PrimarySectionLinkRuleID"].ToString());
            panel.PrimaryPanelLinkRuleID = InputHelper.GetInteger(row["PrimaryPanelLinkRuleID"].ToString());

            panel.RowAsLoaded = row;

            return panel;
        }

        public FunctionResult SavePanelLink(int parentPanelID, int linkedPanelID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("PanelID", parentPanelID));
            parameters.Add(new SqlParameter("LinkedPanelID", linkedPanelID));

            DBAccess db = new DBAccess();
            return db.ExecuteNonQuery("Panels_SaveLink", parameters);
        }

        public FunctionResult DeletePanelLink(int parentPanelID, int linkedPanelID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("PanelID", parentPanelID));
            parameters.Add(new SqlParameter("LinkedPanelID", linkedPanelID));

            DBAccess db = new DBAccess();
            return db.ExecuteNonQuery("Panels_DeleteLink", parameters);
        }

        public List<Panel> GetLinkedPanels(int parentPanelID)
        {
            List<Panel> allPanels = new List<Panel>();

            // Load panels from the db
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("Panels_GetLinked", new SqlParameter("ParentPanelID", parentPanelID));

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                allPanels.Add(InstantiatePanel(row));
            }

            return allPanels.OrderBy(o => o.SortOrder).ToList();
        }

        public FunctionResult SaveLinkedPanels(int panelID, int[] linkedPanelIDs)
        {
            DBAccess db = new DBAccess();

            // First delete the existing links.
            db.ExecuteNonQuery("Panels_DeleteLinks", new SqlParameter("PanelID", panelID));

            foreach(int linkedPanelID in linkedPanelIDs)
            {
                FunctionResult saveResult = SavePanelLink(panelID, linkedPanelID);
                if (saveResult.Success == false)
                {
                    return saveResult;
                }
            }

            return new FunctionResult();
        }
    }
}

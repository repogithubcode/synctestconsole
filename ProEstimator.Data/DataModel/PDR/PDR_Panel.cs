using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel.PDR
{
    public class PDR_Panel
    {
        public int ID { get; private set; }
        public string PanelName { get; private set; }
        public string Orientation { get; private set; }
        public bool ShowEuro { get; private set; }
        public int SortOrder { get; private set; }
        public int LinkedPanelID { get; private set; }

        public bool IsLeftSide
        {
            get { return PanelName.Contains("Left"); }
        }

        public bool IsRightSide
        {
            get { return PanelName.Contains("Right"); }
        }

        public PDR_Panel(DataRow row)
        {
            ID = InputHelper.GetInteger(row["PanelID"].ToString());
            PanelName = row["PanelName"].ToString();
            Orientation = row["Orientation"].ToString();
            ShowEuro = InputHelper.GetBoolean(row["ShowEuro"].ToString());
            SortOrder = InputHelper.GetInteger(row["SortOrder"].ToString());
            LinkedPanelID = InputHelper.GetInteger(row["LinkedPanelID"].ToString());
        }

        public static List<PDR_Panel> GetAll()
        {
            FillCache();
            return _panels;
        }

        public static PDR_Panel GetByID(int id)
        {
            FillCache();
            return _panels.FirstOrDefault(o => o.ID == id);
        }

        public static PDR_Panel GetByLinkedPanelID(int panelID)
        {
            return GetAll().FirstOrDefault(o => o.LinkedPanelID == panelID);
        }

        private static void FillCache()
        {
            lock(_loadLock)
            {
                if (_panels == null || _panels.Count == 0)
                {
                    DBAccess db = new DBAccess();
                    DBAccessTableResult tableResult = db.ExecuteWithTable("PDR_Panel_GetAll");
                    if (tableResult.Success)
                    {
                        _panels = new List<PDR_Panel>();

                        foreach (DataRow row in tableResult.DataTable.Rows)
                        {
                            _panels.Add(new PDR_Panel(row));
                        }
                    }
                }
            }
        }

        private static object _loadLock = new object();
        private static List<PDR_Panel> _panels = null;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.PDR
{
    public class PDR_CategoryMapItem
    {

        public PDR_Panel PDR_Panel { get; private set; }
        public string CategoryName { get; private set; }
        public Side Side { get; private set; }

        public PDR_CategoryMapItem(DataRow row)
        {
            int panelID = InputHelper.GetInteger(row["PanelID"].ToString());
            PDR_Panel = PDR_Panel.GetAll().FirstOrDefault(o => o.ID == panelID);

            CategoryName = row["CategoryName"].ToString();

            string sideString = row["Side"].ToString().ToLower();
            if (sideString == "l")
            {
                Side = Side.Left;
            }
            else if (sideString == "r")
            {
                Side = PDR.Side.Right;
            }
            else
            {
                Side = PDR.Side.NA;
            }
        }

        private static object _loadLock = new object();
        private static List<PDR_CategoryMapItem> _list = null;

        public static List<PDR_CategoryMapItem> GetMapItems()
        {
            lock(_loadLock)
            {
                if (_list == null)
                {
                    DBAccess dbAccess = new DBAccess();
                    DBAccessTableResult result = dbAccess.ExecuteWithTableForQuery("SELECT * FROM PDR_CategoryMap");

                    if (result.Success)
                    {
                        _list = new List<PDR_CategoryMapItem>();

                        foreach (DataRow row in result.DataTable.Rows)
                        {
                            _list.Add(new PDR_CategoryMapItem(row));
                        }
                    }
                    else
                    {
                        ErrorLogger.LogError("PDR_GetMapItems got no results from database.", 0, 0, "PDR_CategoryMapItem GetMapItems");
                    }
                }
            }

            return _list;
        }
    }

    public enum Side
    {
        Left,
        Right,
        NA
    }
}

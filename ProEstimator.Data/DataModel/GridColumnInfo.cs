using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class GridColumnInfo
    {
        public int ID { get; private set; }
        public string GridControlID { get; private set; }
        public string Name { get; private set; }
        public string HeaderText { get; private set; }

        public GridColumnInfo(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            GridControlID = InputHelper.GetString(row["GridControlID"].ToString());
            Name = InputHelper.GetString(row["ColumnName"].ToString());
            HeaderText = InputHelper.GetString(row["HeaderText"].ToString());
        }

        public static GridColumnInfo Get(int id)
        {
            return GridColumnInfoList.FirstOrDefault(o => o.ID == id);
        }

        private static object _loadLock = new object();
        private static List<GridColumnInfo> _gridColumnInfoList;

        public static List<GridColumnInfo> GridColumnInfoList
        {
            get
            {
                lock(_loadLock)
                {
                    if (_gridColumnInfoList == null || _gridColumnInfoList.Count == 0)
                    {
                        DBAccess db = new DBAccess();
                        DBAccessTableResult tableResult = db.ExecuteWithTable("sp_GetGridColumnInfo");

                        if (tableResult.Success)
                        {
                            _gridColumnInfoList = new List<GridColumnInfo>();

                            foreach (DataRow row in tableResult.DataTable.Rows)
                            {
                                _gridColumnInfoList.Add(new GridColumnInfo(row));
                            }
                        }
                    }
                }

                return _gridColumnInfoList;
            }
        }
    }
}

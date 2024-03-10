using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class State
    {

        public string Code { get; private set; }
        public string Description { get; private set; }

        public State(DataRow row)
        {
            Code = row["Code"].ToString();
            Description = row["Description"].ToString();
        }

        public State(string code, string description)
        {
            Code = code;
            Description = description;
        }

        private static object _loadLock = new object();
        private static List<State> _statesList;

        public static List<State> StatesList
        {
            get
            {
                lock(_loadLock)
                {
                    if (_statesList == null || _statesList.Count == 0)
                    {
                        DBAccess db = new DBAccess();
                        DBAccessTableResult tableResult = db.ExecuteWithTable("sp_GetStates");
                        if (tableResult.Success)
                        {
                            _statesList = new List<State>();

                            foreach (DataRow row in tableResult.DataTable.Rows)
                            {
                                _statesList.Add(new State(row));
                            }
                        }
                    }
                }

                return _statesList;
            }
        }
    }
}

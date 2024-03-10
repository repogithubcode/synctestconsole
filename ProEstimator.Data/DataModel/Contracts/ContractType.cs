using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ProEstimatorData.DataModel.Contracts
{
    public class ContractType
    {

        public int ID { get; private set; }
        public string Type { get; private set; }
        public string Description { get; private set; }
        public string ProductCode { get; private set; }

        /// <summary>
        /// Returns True if this is an add on that can be included in the Bundle add on.
        /// </summary>
        public bool IsBundlable
        {
            get
            {
                List<int> ids = new List<int>() { 9, 10, 11, 13 }; // QB Exporter, Pro Advisor, Image Editor, Enterprise Reporting
                return ids.Contains(ID);
            }
        }

        public ContractType(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ContractTypeID"].ToString());
            Type = row["ContractType"].ToString();
            ProductCode = row["ProductCode"].ToString();
            Description = row["ProductDescription"].ToString(); 
        }

        public static ContractType Get(int id)
        {
            LoadAll();
            return _types.FirstOrDefault(o => o.ID == id);
        }

        public static List<ContractType> GetAll()
        {
            LoadAll();
            return _types;
        }

        private static void LoadAll()
        {
            if (_types == null || _types.Count == 0)
            {
                lock(_loadLock)
                {
                    DBAccess db = new DBAccess();
                    DBAccessTableResult result = db.ExecuteWithTable("GetContractTypes");
                    if (result.Success)
                    {
                        _types = new List<ContractType>();

                        foreach (DataRow row in result.DataTable.Rows)
                        {
                            _types.Add(new ContractType(row));
                        }
                    }
                }
            }
        }

        private static object _loadLock = new object();
        private static List<ContractType> _types;
    }
}

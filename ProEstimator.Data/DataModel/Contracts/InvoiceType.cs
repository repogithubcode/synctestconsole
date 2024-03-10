using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ProEstimatorData.DataModel.Contracts
{
    public class InvoiceType
    {
        public int ID { get; private set; }
        public string Type { get; private set; }

        public InvoiceType(DataRow row)
        {
            ID = InputHelper.GetInteger(row["InvoiceTypeID"].ToString());
            Type = row["InvoiceType"].ToString();
        }

        public static InvoiceType Get(int id)
        {
            LoadData();
            return _data.FirstOrDefault(o => o.ID == id);
        }

        public static List<InvoiceType> GetAll()
        {
            LoadData();
            return _data;
        }

        private static void LoadData()
        {
            lock(_loadLock)
            {
                if (_data == null || _data.Count == 0)
                {
                    DBAccess db = new DBAccess();
                    DBAccessTableResult result = db.ExecuteWithTable("GetAllInvoiceTypes");
                    if (result.Success)
                    {
                        _data = new List<InvoiceType>();

                        foreach (DataRow row in result.DataTable.Rows)
                        {
                            _data.Add(new InvoiceType(row));
                        }
                    }
                }
            }
        }

        private static object _loadLock = new object();
        private static List<InvoiceType> _data;
    }
}

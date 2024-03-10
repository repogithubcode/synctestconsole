using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ProEstimatorData.DataModel.Contracts
{
    // TODO - Ezra - 10/16/2019
    // Something is broken with this caching, every so often we would see the payment amount * 100.
    // Not sure how that happened, but changing it for now to load from the db every time it's needed.

    // 11/16/2021 - looking at sp call counts, this one is really hight.  Turned on the caching again and move the _lock inside the check.
    // Also added .ToList() in the GetAll function, I think that was causing the bug above, passing the list by reference instead of a copy

    public class ContractPriceLevel
    {

        public int ID { get; private set; }
        public int PriceLevel { get; private set; }
        public ContractTerms ContractTerms { get; private set; }
        public decimal PaymentAmount { get; private set; }
        public bool Active { get; private set; }

        public ContractPriceLevel(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ContractPriceLevelID"].ToString());
            PriceLevel = InputHelper.GetInteger(row["PriceLevel"].ToString());
            ContractTerms = ContractTerms.Get(InputHelper.GetInteger(row["ContractTermID"].ToString()));
            PaymentAmount = InputHelper.GetDecimal(row["PaymentAmount"].ToString());
            Active = InputHelper.GetBoolean(row["Active"].ToString());
        }

        public static ContractPriceLevel Get(int id)
        {
            List<ContractPriceLevel> priceLevels = GetAll();
            return priceLevels.FirstOrDefault(o => o.ID == id);
            //LoadAll();
            //return _data.FirstOrDefault(o => o.ID == id);
        }

        public static List<ContractPriceLevel> GetAll()
        {
            //List<ContractPriceLevel> priceLevels = new List<ContractPriceLevel>();

            //DBAccess db = new DBAccess();
            //DBAccessTableResult result = db.ExecuteWithTable("GetAllContractPriceLevels");
            //if (result.Success)
            //{
            //    priceLevels = new List<ContractPriceLevel>();

            //    foreach (DataRow row in result.DataTable.Rows)
            //    {
            //        priceLevels.Add(new ContractPriceLevel(row));
            //    }
            //}

            //return priceLevels;

            LoadAll();
            return _data.ToList(); ;
        }

        private static void LoadAll()
        {
            if (_data == null)
            {
                lock (_loadLock)
                {
                    _data = new List<ContractPriceLevel>();

                    DBAccess db = new DBAccess();
                    DBAccessTableResult result = db.ExecuteWithTable("GetAllContractPriceLevels");
                    if (result.Success)
                    {
                        foreach (DataRow row in result.DataTable.Rows)
                        {
                            _data.Add(new ContractPriceLevel(row));
                        }
                    }
                }
            }
        }

        private static object _loadLock = new object();
        private static List<ContractPriceLevel> _data;
    }
}

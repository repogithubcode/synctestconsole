using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ProEstimatorData.DataModel.Contracts
{
    public class ContractTerms
    {

        public int ID { get; private set; }
        public int ContractTypeID { get; private set; }
        public string TermDescription { get; private set; }
        public int ContractLength { get; private set; }
        public int NumberOfPayments { get; private set; }
        public decimal DepositAmount { get; private set; }
        public int PreviousContractTermID { get; private set; }
        public bool ForceAutoPay { get; private set; }
        public bool Active { get; private set; }

        public bool IsUpgrade
        {
            get
            {
                return TermDescription.StartsWith("Upgrade");
            }
        }

        public ContractTerms(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ContractTermID"].ToString());
            ContractTypeID = InputHelper.GetInteger(row["ContractTypeID"].ToString());
            TermDescription = row["TermDescription"].ToString();
            ContractLength = InputHelper.GetInteger(row["ContractLength"].ToString());
            NumberOfPayments = InputHelper.GetInteger(row["NumberOfPayments"].ToString());
            DepositAmount = InputHelper.GetDecimal(row["DepositAmount"].ToString());
            PreviousContractTermID = InputHelper.GetInteger(row["PreviousContractTermID"].ToString());
            ForceAutoPay = InputHelper.GetBoolean(row["ForceAutoPay"].ToString());
            Active = InputHelper.GetBoolean(row["Active"].ToString());
        }

        public static ContractTerms Get(int id)
        {
            //LoadAll();
            return _terms.FirstOrDefault(o => o.ID == id);
        }

        public static List<ContractTerms> GetAll()
        {
            //LoadAll();
            return _terms;
        }

        public static void LoadAll()
        {
            //lock(_loadLock)
            //{
            //    if (_terms == null || _terms.Count == 0)
            //    {
            _terms = new List<ContractTerms>();

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetAllContractTerms");
            if (result.Success)
            {
                        

                foreach (DataRow row in result.DataTable.Rows)
                {
                    _terms.Add(new ContractTerms(row));
                }
            }
            //    }
            //}
        }

        //private static object _loadLock = new object();
        private static List<ContractTerms> _terms;
    }
}
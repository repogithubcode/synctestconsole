using System.Collections.Generic;
using System.Data;

namespace ProEstimatorData.DataModel.LinkRules
{
    public class SectionDetailsResult : FunctionResult
    {
        public List<SectionDetails> Details { get; set; }

        public SectionDetailsResult(string errorMessage)
            : base(false, errorMessage)
        {

        }

        public SectionDetailsResult(DBAccessTableResult tableData)
        {
            Details = new List<SectionDetails>();

            foreach(DataRow row in tableData.DataTable.Rows)
            {
                Details.Add(new SectionDetails(row));
            }
        }
    }

}

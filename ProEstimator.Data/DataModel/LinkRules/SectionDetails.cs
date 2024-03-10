using System.Data;

namespace ProEstimatorData.DataModel.LinkRules
{
    public class SectionDetails
    {
        public int Header { get; set; }
        public int Section { get; set; }
        public int SectionKey { get; set; }
        public string SectionName { get; set; }

        public SectionDetails(DataRow row)
        {
            Header = InputHelper.GetInteger(row["nheader"].ToString());
            Section = InputHelper.GetInteger(row["nsection"].ToString());
            SectionName = InputHelper.GetString(row["Subcategory"].ToString());

            SectionKey = (Header * 256) + Section;
        }
    }
}

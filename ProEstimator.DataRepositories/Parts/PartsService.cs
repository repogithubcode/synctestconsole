using ProEstimatorData;
using ProEstimatorData.DataModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.DataRepositories.Parts
{
    public class PartsService : IPartsService
    {

        public List<SectionPartInfo> GetPartsForSection(int estimateID, int sectionKey, bool allYears)
        {
            int nHeader = sectionKey / 256;
            int nSection = sectionKey % 256;

            return GetPartsForSection(estimateID, nHeader, nSection, allYears);
        }

        public List<SectionPartInfo> GetPartsForSection(int estimateID, int header, int section, bool allYears)
        {
            List<SectionPartInfo> partInfoRecords = new List<SectionPartInfo>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("nHeader", header));
            parameters.Add(new SqlParameter("nSection", section));
            parameters.Add(new SqlParameter("AdminInfoID", estimateID));
            parameters.Add(new SqlParameter("AllYears", allYears));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("GetSectionParts", parameters);

            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    partInfoRecords.Add(new SectionPartInfo(row, header, section));
                }
            }

            return partInfoRecords;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProEstimatorData;

namespace ProEstimator.DataRepositories.PartnerNetwork
{
    public class PartnerNetworkRepository : IPartnerNetworkRepository
    {
        public List<ProEstimatorData.DataModel.PartnerNetwork> GetAll()
        {
            List<ProEstimatorData.DataModel.PartnerNetwork> partnerNetworks = new List<ProEstimatorData.DataModel.PartnerNetwork>();

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("PartnerNetwork_GetAll");
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    ProEstimatorData.DataModel.PartnerNetwork partner = InstantiatePartnerNetwork(row);
                    partnerNetworks.Add(partner);
                }
            }

            return partnerNetworks;
        }

        private ProEstimatorData.DataModel.PartnerNetwork InstantiatePartnerNetwork(DataRow row)
        {
            ProEstimatorData.DataModel.PartnerNetwork partner = new ProEstimatorData.DataModel.PartnerNetwork();

            ((IIDSetter)partner).ID = InputHelper.GetInteger(row["ID"].ToString());
            partner.Name = InputHelper.GetString(row["Name"].ToString());
            partner.Tagline = InputHelper.GetString(row["Tagline"].ToString());
            partner.Summary = InputHelper.GetString(row["Summary"].ToString());
            partner.Link = InputHelper.GetString(row["Link"].ToString());
            partner.SortOrder = InputHelper.GetInteger(row["SortOrder"].ToString());
            partner.Active = InputHelper.GetBoolean(row["Active"].ToString());

            return partner;
        }

        public void InsertClick(int partnerID, int activeLoginID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("PartnerID", partnerID));
            parameters.Add(new SqlParameter("ActiveLoginID", activeLoginID));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("PartnerNetworkClick_Insert", parameters);
        }
    }
}

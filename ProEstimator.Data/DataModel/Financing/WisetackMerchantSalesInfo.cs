using System.Data;

namespace ProEstimatorData.DataModel.Financing
{
    // Data returned from stored proc "Wisetack_MerchantSalesInfo_Get" for a given LoginID
    public class WisetackMerchantSalesInfo
    {
        public int TtmSalesUnits { get; set; }
        public double TtmSales { get; set; }

        public WisetackMerchantSalesInfo()
        {
        }

        public WisetackMerchantSalesInfo(DataRow row)
        {
            TtmSalesUnits = InputHelper.GetInteger(row["TtmSalesUnits"].ToString());
            TtmSales = InputHelper.GetDouble(row["TtmSales"].ToString());
        }
    }
}

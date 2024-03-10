using System.Data;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class PhoneLookup : VmOption, IModelMap<PhoneLookup>
    {
        public PhoneLookup ToModel(DataRow row)
        {
            var result = new PhoneLookup();
            result.Description = row["Description"].SafeString();
            result.Type = row["Code"].SafeString();

            return result;
        }
    }

    public class SalesRepLookup : VmOption, IModelMap<SalesRepLookup>
    {
        public SalesRepLookup ToModel(DataRow row)
        {
            var result = new SalesRepLookup();
            result.Description = row["FullDescription"].SafeString();
            result.Type = ((int) row["SalesRepID"]).ToString();

            return result;
        }
    }

    public class SalesRepPermissionLookup : VmOption, IModelMap<SalesRepPermissionLookup>
    {
        public SalesRepPermissionLookup ToModel(DataRow row)
        {
            var result = new SalesRepPermissionLookup();
            result.Description = row["FullName"].SafeString();
            result.Type = ((int)row["SalesRepID"]).ToString();
            result.Deleted = (bool) row["deleted"];

            return result;
        }

        public bool Deleted { get; set; }
    }

    public class ResellerLookup : VmOption, IModelMap<ResellerLookup>
    {
        public ResellerLookup ToModel(DataRow row)
        {
            var result = new ResellerLookup();
            result.Description = row["FullDescription"].SafeString();
            result.Type = ((int)row["SalesRepID"]).ToString();

            return result;
        }
    }

    public class InvoiceTypeLookup : VmOption, IModelMap<InvoiceTypeLookup>
    {
        public InvoiceTypeLookup ToModel(DataRow row)
        {
            var result = new InvoiceTypeLookup();
            result.Description = row["FullDescription"].SafeString();
            result.Type = ((int)row["ID"]).ToString();

            return result;
        }
    }
    public class LevelTermsLookup : VmOption, IModelMap<LevelTermsLookup>
    {
        public LevelTermsLookup ToModel(DataRow row)
        {
            var result = new LevelTermsLookup();
            result.Description = row["LevelTerm"].SafeString();
            result.Type = ((int)row["ContractPriceLevelID"]).ToString();

            return result;
        }
    }

    public class PromosLookup : VmOption, IModelMap<PromosLookup>
    {
        public PromosLookup ToModel(DataRow row)
        {
            var result = new PromosLookup();
            result.Description = row["PromoDescription"].SafeString();
            result.Type = ((int)row["PromoID"]).ToString();

            return result;
        }
    }

    public class InvoicesLookup : VmOption, IModelMap<InvoicesLookup>
    {
        public InvoicesLookup ToModel(DataRow row)
        {
            var result = new InvoicesLookup();
            result.Description = row["InvoiceDescription"].SafeString();
            result.Type = ((int)row["InvoiceID"]).ToString();

            return result;
        }
    }
    public class SubscriptionLookup : VmOption, IModelMap<SubscriptionLookup>
    {
        public SubscriptionLookup ToModel(DataRow row)
        {
            var result = new SubscriptionLookup();
            result.Description = row["DateRange"].SafeString();
            result.Type = ((int)row["ContractID"]).ToString();

            return result;
        }
    }
    public class GetFrameDataLevelTermsLookup : VmOption, IModelMap<GetFrameDataLevelTermsLookup>
    {
        public GetFrameDataLevelTermsLookup ToModel(DataRow row)
        {
            var result = new GetFrameDataLevelTermsLookup();
            result.Description = row["TermDescription"].SafeString();
            result.Type = ((int)row["ContractPriceLevelID"]).ToString();

            return result;
        }
    }
}
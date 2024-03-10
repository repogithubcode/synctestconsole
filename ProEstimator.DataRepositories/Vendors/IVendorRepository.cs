using System.Collections.Generic;

using ProEstimatorData.DataModel;

namespace ProEstimator.DataRepositories.Vendors
{
    public interface IVendorRepository
    {
        Vendor Get(int id);
        List<Vendor> GetUniversalList(int loginID);
        List<Vendor> GetPrivateList(int loginID, VendorType vendorType);
        List<Vendor> GetAllForType(int loginID, VendorType vendorType);
        SaveResult Save(Vendor vendor, int activeLoginID);
        SaveResult Delete(int vendorID, int activeLoginID);
        SaveResult ToggleSelection(int loginID, int vendorID, int activeLoginID);
        List<Vendor> GetAllPrivateVendors();
    }
}

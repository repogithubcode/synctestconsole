using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using ProEstimator.DataRepositories.Vendors;
using ProEstimatorData;
using ProEstimatorData.DataModel;

namespace Proestimator.Controllers.Vendors.Commands
{
    public class CleanVendorsTableCommand : CommandBase
    {

        private IVendorRepository _vendorRepository;
        private int _activeLoginID;

        public CleanVendorsTableCommand(IVendorRepository vendorRepository, int activeLoginID)
        {
            _vendorRepository = vendorRepository;
            _activeLoginID = activeLoginID;
        }

        public override bool Execute()
        {
            List<Vendor> vendors = _vendorRepository.GetAllPrivateVendors();

            List<Vendor> usedVendors = new List<Vendor>();

            foreach (Vendor vendor in vendors)
            {
                Vendor existingMatch = usedVendors.FirstOrDefault(o => o.LoginsID == vendor.LoginsID && o.CompanyIDCode == vendor.CompanyIDCode && o.Type == vendor.Type);

                if (existingMatch == null)
                {
                    usedVendors.Add(vendor);
                }
                else
                {
                    // The vendor is already in the used list.  If this one has more info filled out use it instead, otherwise delete 
                    int existingLength = VendorInfoLength(existingMatch);
                    int vendorLength = VendorInfoLength(vendor);

                    if (vendorLength > existingLength)
                    {
                        usedVendors.Remove(existingMatch);

                        _vendorRepository.Delete(existingMatch.ID, _activeLoginID);

                        usedVendors.Add(vendor);
                    }
                    else
                    {
                        _vendorRepository.Delete(vendor.ID, _activeLoginID);
                    }
                }
            }

            return true;
        }

        private int VendorInfoLength(Vendor vendor)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(vendor.CompanyName);
            builder.Append(vendor.FirstName);
            builder.Append(vendor.LastName);
            builder.Append(vendor.Email);
            builder.Append(vendor.MobilePhone);
            builder.Append(vendor.WorkPhone);
            builder.Append(vendor.Address1);
            builder.Append(vendor.Address2);
            builder.Append(vendor.City);
            builder.Append(vendor.State);
            builder.Append(vendor.Zip);
            builder.Append(vendor.Type);
            builder.Append(vendor.FaxNumber);
            builder.Append(vendor.FederalTaxID);
            builder.Append(vendor.LicenseNumber);
            builder.Append(vendor.BarNumber);
            builder.Append(vendor.RegistrationNumber);

            return builder.ToString().Length;
        }

    }
}
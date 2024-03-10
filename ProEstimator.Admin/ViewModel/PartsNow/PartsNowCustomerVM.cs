using ProEstimator.Business.ILogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Extension;
using ProEstimator.Business.Model.Admin;

namespace ProEstimator.Admin.ViewModel
{
    public class PartsNowCustomerVM
    {
        public int Id { get; set; }
        public string companyname { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string phone1 { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public bool Active { get; set; }

        public int DT_RowId;

        public PartsNowCustomerVM()
        {

        }

        public PartsNowCustomerVM(VmPartsNowCustomer vmPartsNowCustomer)
        {
            this.Id = vmPartsNowCustomer.Id;
            this.DT_RowId = vmPartsNowCustomer.DT_RowId;
            this.companyname = vmPartsNowCustomer.companyname;
            this.address1 = vmPartsNowCustomer.address1;
            this.address2 = vmPartsNowCustomer.address2;
            this.city = vmPartsNowCustomer.city;
            this.state = vmPartsNowCustomer.state;
            this.zip = vmPartsNowCustomer.zip;
            this.phone1 = vmPartsNowCustomer.phone1;
            this.firstname = vmPartsNowCustomer.firstname;
            this.lastname = vmPartsNowCustomer.lastname;
            this.Active = vmPartsNowCustomer.PartsNow;
        }

        public VmPartsNowCustomer ToVmSalesRep(PartsNowCustomerVM partsNowCustomerVM)
        {
            VmPartsNowCustomer vmPartsNowCustomer = new VmPartsNowCustomer();

            vmPartsNowCustomer.Id = partsNowCustomerVM.Id;
            vmPartsNowCustomer.DT_RowId = partsNowCustomerVM.DT_RowId;
            vmPartsNowCustomer.companyname = partsNowCustomerVM.companyname;
            vmPartsNowCustomer.address1 = partsNowCustomerVM.address1;
            vmPartsNowCustomer.address2 = partsNowCustomerVM.address2;
            vmPartsNowCustomer.city = partsNowCustomerVM.city;
            vmPartsNowCustomer.state = partsNowCustomerVM.state;
            vmPartsNowCustomer.zip = partsNowCustomerVM.zip;
            vmPartsNowCustomer.phone1 = partsNowCustomerVM.phone1;
            vmPartsNowCustomer.firstname = partsNowCustomerVM.firstname;
            vmPartsNowCustomer.lastname = partsNowCustomerVM.lastname;
            vmPartsNowCustomer.PartsNow = partsNowCustomerVM.Active;

            return vmPartsNowCustomer;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Admin;
using ProEstimator.Business.Model.Admin;

namespace ProEstimator.Admin.ViewModel
{
    public class SalesRepVM
    {
        public int SalesRepID { get; set; }
        public bool IsSalesRep { get; set; }
        public bool Active { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Number { get; set; }

        public int SelectedSalesRepPermission { get; set; }

        public int SessionSalesRepID { get; set; }

        public SalesRepVM() { }

        public SalesRepVM(ProEstimatorData.DataModel.SalesRep salesRep)
        {
            SalesRepID = salesRep.SalesRepID;
            Number = salesRep.SalesNumber;
            FirstName = salesRep.FirstName;
            LastName = salesRep.LastName;
            UserName = salesRep.UserName;
            Password = salesRep.Password;
            Email = salesRep.Email;
            IsSalesRep = salesRep.IsSalesRep;
            Active = salesRep.IsActive;
        }

        public void ToSalesRepVM(VmSalesRep vmSalesRep)
        {
            this.SalesRepID = vmSalesRep.ID;
            this.Number = vmSalesRep.Number;
            this.FirstName = vmSalesRep.FirstName;
            this.LastName = vmSalesRep.LastName;
            this.UserName = vmSalesRep.UserName;
            this.Password = vmSalesRep.Password;
            this.Email = vmSalesRep.Email;
            this.IsSalesRep = vmSalesRep.IsSalesRep;
            this.Active = vmSalesRep.Active;
        }

        public VmSalesRep ToVmSalesRep(SalesRepVM salesRepVM)
        {
            VmSalesRep vmSalesRep = new VmSalesRep();

            vmSalesRep.ID = this.SalesRepID;
            vmSalesRep.Number = this.Number;
            vmSalesRep.FirstName = this.FirstName;
            vmSalesRep.LastName = this.LastName;
            vmSalesRep.UserName = this.UserName;
            vmSalesRep.Password = this.Password;
            vmSalesRep.Email = this.Email;
            vmSalesRep.IsSalesRep = this.IsSalesRep;
            vmSalesRep.Active = this.Active;

            return vmSalesRep;
        }
    }
}
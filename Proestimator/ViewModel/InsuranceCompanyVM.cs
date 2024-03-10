using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel
{
    public class InsuranceCompanyVM
    {

        public int ID { get; set; }
        public string Name { get; set; }

        public List<InsuranceCompanyEmployeeVM> Adjusters { get; set; }
        public List<InsuranceCompanyEmployeeVM> ClaimReps { get; set; }

        public InsuranceCompanyVM(InsuranceCompany insuranceCompany)
        {
            ID = insuranceCompany.ID;
            Name = insuranceCompany.Name;

            Adjusters = new List<InsuranceCompanyEmployeeVM>();
            List<InsuranceCompanyEmployee> adjusters = InsuranceCompanyEmployee.GetForCompany(insuranceCompany.ID, InsuranceCompanyEmployeeType.Adjuster);
            adjusters.ForEach(o => Adjusters.Add(new InsuranceCompanyEmployeeVM(o)));

            ClaimReps = new List<InsuranceCompanyEmployeeVM>();
            List<InsuranceCompanyEmployee> claimReps = InsuranceCompanyEmployee.GetForCompany(insuranceCompany.ID, InsuranceCompanyEmployeeType.ClaimRep);
            claimReps.ForEach(o => ClaimReps.Add(new InsuranceCompanyEmployeeVM(o)));
        }
    }

    public class InsuranceCompanyEmployeeVM
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Extension { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }

        public bool IsDeleted { get; set; }
        public string DeleteRestoreImgName { get; set; }

        public InsuranceCompanyEmployeeVM(InsuranceCompanyEmployee employee)
        {
            ID = employee.ID;
            FirstName = employee.FirstName;
            LastName = employee.LastName;
            Phone = employee.Phone;
            Extension = employee.Extension;
            Fax = employee.Fax;
            Email = employee.Email;
            IsDeleted = employee.IsDeleted;
            FullName = FirstName + ' ' + LastName;

            if (IsDeleted)
            {
                DeleteRestoreImgName = "restore.gif";
            }
            else
            {
                DeleteRestoreImgName = "delete.gif";
            }
        }
    }


}